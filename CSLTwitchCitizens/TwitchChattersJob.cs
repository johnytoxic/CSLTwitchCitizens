using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using ColossalFramework.HTTP;

namespace CSLTwitchCitizens
{
    public class TwitchChattersJob
    {
        public string AccessToken;
        public string BroadcasterID;
        public int UpdateInterval = 5 * 60 * 1000; // 5 minutes

        public delegate void OnChattersUpdated(object sender, string[] chatters);
        public event OnChattersUpdated ChattersUpdated;

        private Timer _timer;

        public TwitchChattersJob(string accessToken, string broadcasterID)
        {
            AccessToken = accessToken;
            BroadcasterID = broadcasterID;
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
            // see https://dev.twitch.tv/docs/api/reference/#get-chatters
            var request = new Request("GET", $"https://api.twitch.tv/helix/chat/chatters?broadcaster_id={BroadcasterID}&moderator_id={BroadcasterID}");
            request.AddHeader("Authorization", $"Bearer {AccessToken}");
            request.AddHeader("Client-Id", "nseaqiq9r9k4kf6lorq69djkxizozt");
            request.AddHeader("Accept", "application/json");

            request.Send(req =>
            {
                var res = req.response;
                var chatters = res.Object?["data"] as ArrayList;

                if (chatters == null || chatters.Count == 0)
                {
                    // TODO: handle error (empty response)
                    return;
                }

                var chattersNames = new List<string>(chatters.Count);
                foreach (Hashtable chatter in chatters)
                {
                    chattersNames.Add(chatter["user_name"] as string);
                }

                ChattersUpdated?.Invoke(this, chattersNames.ToArray());
            });
        }
    }
}
