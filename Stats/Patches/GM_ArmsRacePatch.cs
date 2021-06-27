using HarmonyLib;

namespace Stats.Patches
{
    public class GM_ArmsRacePatch
    {
        [HarmonyPatch(typeof(GM_ArmsRace)),HarmonyPatch("StartGame")]
        private class Patch_StartGame
        {
            // ReSharper disable once UnusedMember.Local
            private static void Postfix()
            {
                #if DEBUG
                UnityEngine.Debug.LogWarning("Game started");
                #endif

                Stats.AddValue("Games played");
            }
        }
        
        [HarmonyPatch(typeof(GM_ArmsRace)),HarmonyPatch("RoundOver")]
        private class Patch_RoundOver
        {
            // ReSharper disable once UnusedMember.Local
            private static void Postfix(int winningTeamID, int losingTeamID)
            {

                if (Stats.localPlayer.teamID == winningTeamID)
                {
#if DEBUG
                    UnityEngine.Debug.LogWarning("Round won");
#endif
                    Stats.AddValue("Rounds won");
                } 
                else if (Stats.localPlayer.teamID == losingTeamID)
                {
#if DEBUG
                    UnityEngine.Debug.LogWarning("Round lost");
#endif
                    Stats.AddValue("Rounds lost");
                }
                
#if DEBUG
                UnityEngine.Debug.LogWarning("Round over");
#endif

                Stats.AddValue("Rounds played");
            }
        }

    }
}