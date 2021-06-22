using HarmonyLib;

namespace Stats.Patches
{
    public class GM_ArmsRacePatch
    {
        [HarmonyPatch(typeof(GM_ArmsRace)),HarmonyPatch("StartGame")]
        private class Patch_DoAttack
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

    }
}