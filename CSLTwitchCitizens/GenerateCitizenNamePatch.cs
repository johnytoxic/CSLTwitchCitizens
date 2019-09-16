using System;
using ColossalFramework.Globalization;
using ColossalFramework.Math;
using Harmony;

namespace CSLTwitchCitizens
{
    [HarmonyPatch(typeof(CitizenAI), "GenerateCitizenName", new Type[]{typeof(uint), typeof(byte)})]
    public class GenerateCitizenNamePatch
    {
        public static string[] CitizenNames;
        public static int NamesThreshold = 200;

        /// <see cref="CitizenAI.GenerateCitizenName"/>
        static bool Prefix(ref string __result, uint citizenID, byte family)
        {
            Randomizer randomizer = new Randomizer(citizenID);

            if (CitizenNames.Length < NamesThreshold)
            {
                // There are not enough Twitch chatters, so we need to mix them with existing names
                string localeKey = Citizen.GetGender(citizenID) == Citizen.Gender.Male
                    ? "NAME_MALE_FIRST"
                    : "NAME_FEMALE_FIRST";
                uint countNames = Locale.Count(localeKey);

                if (randomizer.Int32((uint) (countNames + CitizenNames.Length)) > CitizenNames.Length)
                {
                    // Use an existing Name
                    return true;
                }
            }

            // Select a random name from our chatters
            __result = CitizenNames[randomizer.Int32((uint) CitizenNames.Length)];
            return false;
        }
    }
}
