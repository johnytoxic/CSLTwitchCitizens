using ColossalFramework.UI;
using ICities;
using UnityEngine;

namespace CSLTwitchCitizens
{
    public class TwitchCitizensMod: IUserMod
    {
        public string Name => "Twitch Citizens";
        public string Description => "Integrate your Twitch.tv viewers into Cities: Skylines";

        public delegate void OnSettingsChanged(object sender, string access_token, string broadcaster_id);
        public static event OnSettingsChanged TwitchSettingsChanged;

        public static string TwitchAccessToken
        {
            get => PlayerPrefs.GetString("TwitchCitizensMod_AccessToken", "");
            set => PlayerPrefs.SetString("TwitchCitizensMod_AccessToken", value);
        }

        public static string TwitchBroadcasterName
        {
            get => PlayerPrefs.GetString("TwitchCitizensMod_BroadcasterName", "");
            set => PlayerPrefs.SetString("TwitchCitizensMod_BroadcasterName", value);
        }

        public static string TwitchBroadcasterID
        {
            get => PlayerPrefs.GetString("TwitchCitizensMod_BroadcasterID", "");
            set => PlayerPrefs.SetString("TwitchCitizensMod_BroadcasterID", value);
        }

        public void OnSettingsUI(UIHelperBase helper)
        {
            UILabel errorLabel = null;
            UITextField broadcasterNameTextField = null;

            void UpdateBroadcaster(string access_token)
            {
                var api = new TwitchAPI(access_token);
                api.GetBroadcaster((broadcaster, ex) =>
                {
                    if (ex != null)
                    {
                        errorLabel.text = ex.Message;
                        broadcasterNameTextField.text = "";
                    }
                    else
                    {
                        TwitchBroadcasterID = broadcaster.ID;
                        TwitchBroadcasterName = broadcaster.Name;

                        errorLabel.text = "";
                        broadcasterNameTextField.text = TwitchBroadcasterName;

                        TwitchSettingsChanged.Invoke(this, TwitchAccessToken, TwitchAccessToken);
                    }
                });
            }

            UIHelperBase settingsGroup = helper.AddGroup("Twitch Citizens Settings");
            var accessTokenTextField = (UITextField)settingsGroup.AddTextfield(
                "Twitch Access Token",
                TwitchAccessToken,
                (v) => {
                    TwitchAccessToken = v;
                    UpdateBroadcaster(v);
                }
            );

            var textfieldContainer = accessTokenTextField.GetComponent<UIComponent>().parent as UIPanel;

            string[] labels =
            {
                "Please authorize this mod to access a list of viewers.",
                "Do not share this secret with anyone and do not show it on-stream!",
                "Visit https://johnytoxic.github.io/CSLTwitchCitizens/ to obtain the access token."
            };

            foreach (var msg in labels)
            {
                textfieldContainer.AddUIComponent<UILabel>().text = msg;
            }

            settingsGroup.AddSpace(48);
            var openSiteButton = settingsGroup.AddButton("Open in Browser", () =>
            {
                System.Diagnostics.Process.Start("https://johnytoxic.github.io/CSLTwitchCitizens/");
            });

            settingsGroup.AddSpace(24);
            broadcasterNameTextField = (UITextField) settingsGroup.AddTextfield("Channel", TwitchBroadcasterName, v => {});
            broadcasterNameTextField.readOnly = true;
            errorLabel = (broadcasterNameTextField.GetComponent<UIComponent>().parent).AddUIComponent<UILabel>();
            errorLabel.textColor = Color.red;

            if (!string.IsNullOrEmpty(TwitchAccessToken)
                && string.IsNullOrEmpty(TwitchBroadcasterID))
            {
                UpdateBroadcaster(TwitchAccessToken);
            }
        }
    }
}
