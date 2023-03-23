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

            TwitchCitizensMod.TwitchSettingsChanged += HandleSettingsChanged;
        }

        public override void OnReleased()
        {
            TwitchCitizensMod.TwitchSettingsChanged -= HandleSettingsChanged;

            _job?.Stop();
        }

        public override void OnLevelLoaded(LoadMode mode)
        {
            MaybeStartPollingTwitchChatters(TwitchCitizensMod.TwitchAccessToken, TwitchCitizensMod.TwitchBroadcasterID);
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

        private void HandleSettingsChanged(object sender, string accessToken, string broadcasterID)
        {
            MaybeStartPollingTwitchChatters(accessToken, broadcasterID);
        }

        private void MaybeStartPollingTwitchChatters(string accessToken, string broadcasterID)
        {
            if (!string.IsNullOrEmpty(accessToken) && !string.IsNullOrEmpty(broadcasterID))
            {
                _job = new TwitchChattersJob(new TwitchAPI(accessToken), broadcasterID);
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
