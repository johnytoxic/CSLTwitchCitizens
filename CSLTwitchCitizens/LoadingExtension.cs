using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Harmony;
using ICities;

namespace CSLTwitchCitizens
{
    public class LoadingExtension : LoadingExtensionBase
    {
        private TwitchChattersJob _job;

        public override void OnCreated(ILoading loading)
        {
            base.OnCreated(loading);

            PatchMethods();

            TwitchCitizensMod.TwitchChannelNameChanged += HandleChannelNameChanged;
        }

        public override void OnReleased()
        {
            TwitchCitizensMod.TwitchChannelNameChanged -= HandleChannelNameChanged;

            _job?.Stop();
        }

        public override void OnLevelLoaded(LoadMode mode)
        {
            MaybeStartPollingTwitchChatters(TwitchCitizensMod.TwitchChannelName);
        }

        public override void OnLevelUnloading()
        {
            _job?.Stop();
        }

        private void PatchMethods()
        {
            var harmony = HarmonyInstance.Create("net.johnytoxic.csltwitchcitizens");
            harmony.PatchAll(Assembly.GetExecutingAssembly());
        }

        private void HandleChattersUpdated(object sender, string[] chatters)
        {
            // merge existing chatters with new ones but keep old ones
            var currentChatters = new HashSet<string>(GenerateCitizenNamePatch.CitizenNames);
            currentChatters.UnionWith(chatters);

            GenerateCitizenNamePatch.CitizenNames = currentChatters.ToArray();
        }

        private void HandleChannelNameChanged(object sender, string channelName)
        {
            MaybeStartPollingTwitchChatters(channelName);
        }

        private void MaybeStartPollingTwitchChatters(string channelName)
        {
            if (!string.IsNullOrEmpty(channelName))
            {
                _job = new TwitchChattersJob(channelName);
                _job.ChattersUpdated += HandleChattersUpdated;
                _job.Start();
            }
            else
            {
                _job?.Stop();
            }
        }
    }
}
