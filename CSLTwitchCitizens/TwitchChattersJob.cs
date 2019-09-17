using System.Collections.Generic;
using System.Linq;
using System.Threading;
using ColossalFramework.HTTP;
using Newtonsoft.Json;

namespace CSLTwitchCitizens
{
    public class TwitchChattersJob
    {
        private struct ChattersResponse
        {
            [JsonProperty(PropertyName = "chatters_count")]
            public int Count { get; set; }

            [JsonProperty(PropertyName = "chatters")]
            public Dictionary<string, List<string>> Chatters { get; set; }
        }

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
                if (req.response == null)
                {
                    // TODO: handle error
                    return;
                }

                var res = req.response.Text;
                var chattersResponse =
                    JsonConvert.DeserializeObject<ChattersResponse>(res);

                string[] chatters = chattersResponse.Count > 0
                    ? chattersResponse.Chatters.Values.SelectMany(c => c).ToArray()
                    : new string[] { };

                ChattersUpdated?.Invoke(this, chatters);
            });
        }
    }
}
