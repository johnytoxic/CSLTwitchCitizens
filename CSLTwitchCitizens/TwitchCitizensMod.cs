using System;
using Harmony;
using ICities;
using UnityEngine;

namespace CSLTwitchCitizens
{
    public class TwitchCitizensMod: IUserMod
    {
        public string Name => "Twitch Citizens";
        public string Description => "Integrate your Twitch.tv viewers into Cities: Skylines";

        public void OnEnabled()
        {
            PatchMethods();

            var job = new TwitchChattersJob("tfue");
            job.ChattersUpdated += HandleChattersUpdated;
            job.Start();
        }

        private void PatchMethods()
        {
            var harmony = HarmonyInstance.Create("net.johnytoxic.csltwitchcitizens");

            var original = typeof(CitizenAI).GetMethod("GenerateCitizenName", new Type[] {typeof(uint), typeof(byte)});
            var prefix = typeof(GenerateCitizenNamePatch).GetMethod("Prefix");
            harmony.Patch(original, new HarmonyMethod(prefix));
        }

        private void HandleChattersUpdated(object sender, string[] chatters)
        {
            GenerateCitizenNamePatch.CitizenNames = chatters;
        }
    }
}
