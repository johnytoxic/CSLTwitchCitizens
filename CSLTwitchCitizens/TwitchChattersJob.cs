using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TwitchLib.Api;

namespace CSLTwitchCitizens
{
    public class TwitchChattersJob
    {
        public string ChannelName;
        public int UpdateInterval = 5 * 60 * 1000; // 5 minutes

        public event EventHandler<string[]> ChattersUpdated;

        private TwitchAPI _api;
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
            _api = new TwitchAPI();
            _api.Settings.ClientId = "anonymous";

            _timer = new Timer(DoUpdate, null, 0, UpdateInterval);
        }

        public void Stop()
        {
            _timer.Dispose();

            _api = null;
        }

        private async void DoUpdate(object state)
        {
            var chattersResponse = await _api.Undocumented.GetChattersAsync(ChannelName);
            var chatters = chattersResponse.Select(c => c.Username).ToArray();

            ChattersUpdated?.Invoke(this, chatters);
        }
    }
}
