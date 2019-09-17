﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using ColossalFramework.UI;
using Harmony;
using ICities;
using UnityEngine;

namespace CSLTwitchCitizens
{
    public class TwitchCitizensMod: IUserMod
    {
        public string Name => "Twitch Citizens";
        public string Description => "Integrate your Twitch.tv viewers into Cities: Skylines";

        private TwitchChattersJob _job;

        private const string TwitchChannelNamePrefKey = "TwitchCitizensMod_TwitchChannelName";
        private string TwitchChannelName
        {
            get => PlayerPrefs.GetString(TwitchChannelNamePrefKey, "");
            set => PlayerPrefs.SetString(TwitchChannelNamePrefKey, value);
        }

        public void OnEnabled()
        {
            PatchMethods();

            MaybeStartPollingTwitchChatters();
        }

        public void OnDisabled()
        {
            _job?.Stop();
        }

        public void OnSettingsUI(UIHelperBase helper)
        {
            UIHelperBase settingsGroup = helper.AddGroup("Twitch Citizens Settings");
            var channelNameTextField = (UITextField) settingsGroup.AddTextfield(
                "Twitch Channel Name",
                TwitchChannelName,
                (v) => { },
                HandleTwitchChannelChanged
            );

            var textfieldContainer = channelNameTextField.GetComponent<UIComponent>().parent as UIPanel;
            var channelNameHint = textfieldContainer.AddUIComponent<UILabel>();
            channelNameHint.text = "(e.g. \"paradoxinteractive\", like in \"https://www.twitch.tv/paradoxinteractive\")";
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

        private void MaybeStartPollingTwitchChatters()
        {
            if (!string.IsNullOrEmpty(TwitchChannelName))
            {
                _job = new TwitchChattersJob(TwitchChannelName);
                _job.ChattersUpdated += HandleChattersUpdated;
                _job.Start();
            }
            else
            {
                _job?.Stop();
            }
        }

        private void HandleTwitchChannelChanged(string channelName)
        {
            TwitchChannelName = channelName;

            // TODO: validate channel
            MaybeStartPollingTwitchChatters();
        }
    }
}
