using System;
using Harmony;

namespace CSLTwitchCitizens
{
    [HarmonyPatch(typeof(CitizenAI), "GenerateCitizenName", new Type[]{typeof(uint), typeof(byte)})]
    public class GenerateCitizenNamePatch
    {
        /// <see cref="CitizenAI.GenerateCitizenName"/>
        static bool Prefix(ref string __result, uint citizenID, byte family)
        {
            // __result = "Some Name";
            return true;
        }
    }
}
