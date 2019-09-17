using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using ColossalFramework.HTTP;

namespace CSLTwitchCitizens
{
    public class TwitchChattersJob
    {
        public string ChannelName;
        public int UpdateInterval = 5 * 60 * 1000; // 5 minutes

        public delegate void OnChattersUpdated(object sender, string[] chatters);
        public event OnChattersUpdated ChattersUpdated;

        private Timer _timer;

        public TwitchChattersJob(string channelName)
        {
            ChannelName = channelName;
        }

        ~TwitchChattersJob()
        {
            Stop();
        }

        public void Start()
        {
            _timer = new Timer(DoUpdate, null, 0, UpdateInterval);
        }

        public void Stop()
        {
            _timer.Dispose();
        }

        private void DoUpdate(object state)
        {
            var request = new Request("GET", $"https://tmi.twitch.tv/group/user/{ChannelName}/chatters");
            request.AddHeader("Client-ID", "anonymous");
            request.AddHeader("Accept", "application/json");

            request.Send(req =>
            {
                var chattersResponse = req.response?.Object;
                if (chattersResponse == null)
                {
                    // TODO: error handling
                    return;
                }

                var chattersCount = (int) chattersResponse["chatter_count"];

                if (chattersCount > 0)
                {
                    var chatters = new List<string>(chattersCount);
                    foreach (DictionaryEntry group in (Hashtable) chattersResponse["chatters"])
                    {
                        chatters.AddRange(((ArrayList)group.Value).Cast<string>().ToList());
                    }

                    ChattersUpdated?.Invoke(this, chatters.ToArray());
                }
            });
        }
    }
}
