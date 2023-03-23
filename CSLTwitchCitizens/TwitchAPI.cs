using ColossalFramework.HTTP;
using System;
using System.Collections;
using System.Collections.Generic;

namespace CSLTwitchCitizens
{
    internal class TwitchAPI
    {
        private readonly string AccessToken;

        public class BroadcasterDAO
        {
            public string ID;
            public string Name;
        }

        public TwitchAPI(string access_token)
        {
            AccessToken = access_token;
        }

        private Request prepareRequest(string path)
        {
            var request = new Request("GET", $"https://twitchapiproxy.kettenclan.de/{path}");
            request.AddHeader("Authorization", $"Bearer {AccessToken}");
            request.AddHeader("Client-Id", "nseaqiq9r9k4kf6lorq69djkxizozt");
            request.AddHeader("Accept", "application/json");

            return request;
        }

        public void GetBroadcaster(Action<BroadcasterDAO, Exception> callback)
        {
            // see https://dev.twitch.tv/docs/api/reference/#get-users
            var request = prepareRequest("helix/users");

            request.Send(req =>
            {
                var res = req.response;
                if (res.status != 200)
                {
                    if (res.status == 401)
                        callback(null, new Exception("Invalid access token"));
                    else
                        callback(null, new Exception("Unexpected API error."));

                    return;
                }

                var user = (res.Object?["data"] as ArrayList)?[0] as Hashtable;
                if (user == null)
                {
                    callback(null, new Exception("Unexpected error: API returned no data."));
                    return;
                }

                var b = new BroadcasterDAO
                {
                    ID = (string)user["id"],
                    Name = (string)user["display_name"]
                };

                callback(b, null);
            });
        }

        public void GetChatters(string broadcasterID, Action<List<string>, Exception> callback)
        {
            // see https://dev.twitch.tv/docs/api/reference/#get-chatters
            var request = prepareRequest($"helix/chat/chatters?broadcaster_id={broadcasterID}&moderator_id={broadcasterID}");

            request.Send(req =>
            {
                var res = req.response;
                if (res.status != 200)
                {
                    if (res.status == 401)
                        callback(null, new Exception("Invalid access token"));
                    else
                        callback(null, new Exception("Unexpected API error."));

                    return;
                }

                var chatters = res.Object?["data"] as ArrayList;
                if (chatters == null || chatters.Count == 0)
                {
                    callback(null, new Exception("Unexpected error: API returned no data."));
                    return;
                }

                var chattersNames = new List<string>(chatters.Count);
                foreach (Hashtable chatter in chatters)
                {
                    chattersNames.Add(chatter["user_name"] as string);
                }

                callback(chattersNames, null);
            });
        }
    }
}
