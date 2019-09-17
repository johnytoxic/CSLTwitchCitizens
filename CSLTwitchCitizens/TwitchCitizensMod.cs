using ColossalFramework.UI;
using ICities;
using UnityEngine;

namespace CSLTwitchCitizens
{
    public class TwitchCitizensMod: IUserMod
    {
        public string Name => "Twitch Citizens";
        public string Description => "Integrate your Twitch.tv viewers into Cities: Skylines";

        public delegate void OnTwitchChannelNameChanged(object sender, string channelName);

        public static event OnTwitchChannelNameChanged TwitchChannelNameChanged;

        private const string TwitchChannelNamePrefKey = "TwitchCitizensMod_TwitchChannelName";
        public static string TwitchChannelName
        {
            get => PlayerPrefs.GetString(TwitchChannelNamePrefKey, "");
            set
            {
                PlayerPrefs.SetString(TwitchChannelNamePrefKey, value);
                TwitchChannelNameChanged?.Invoke(null, value);
            }
        }

        public void OnSettingsUI(UIHelperBase helper)
        {
            UIHelperBase settingsGroup = helper.AddGroup("Twitch Citizens Settings");
            var channelNameTextField = (UITextField) settingsGroup.AddTextfield(
                "Twitch Channel Name",
                TwitchChannelName,
                (v) => { },
                (v) => { TwitchChannelName = v; }
            );

            var textfieldContainer = channelNameTextField.GetComponent<UIComponent>().parent as UIPanel;
            var channelNameHint = textfieldContainer.AddUIComponent<UILabel>();
            channelNameHint.text = "(e.g. \"paradoxinteractive\", like in \"https://www.twitch.tv/paradoxinteractive\")";
        }
    }
}
