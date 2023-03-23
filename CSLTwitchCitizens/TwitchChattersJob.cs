using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using ColossalFramework.HTTP;
using UnityEngine;

namespace CSLTwitchCitizens
{
    internal class TwitchChattersJob
    {
        public int UpdateInterval = 5 * 60 * 1000; // 5 minutes

        private TwitchAPI api;
        private string BroadcasterID;

        public delegate void OnChattersUpdated(object sender, string[] chatters);
        public event OnChattersUpdated ChattersUpdated;

        private Timer _timer;

        public TwitchChattersJob(TwitchAPI api, string broadcasterID)
        {
            this.api = api;
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
            api.GetChatters(BroadcasterID, (chatters, ex) =>
            {
                if (ex != null)
                {
                    // TODO: handle error (empty response)
                    Debug.Log($"CSLTwitchCitizens: no chatters retrieved from API. Reason: {ex.Message}");
                    return;
                }

                ChattersUpdated?.Invoke(this, chatters.ToArray());
            });
        }
    }
}
