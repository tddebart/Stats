using HarmonyLib;

namespace Stats.Patches
{
    public class PlayerPatch
    {
        [HarmonyPatch(typeof(Player)),HarmonyPatch("Start")]
        private class Patch_DoAttack
        {
            // ReSharper disable once UnusedMember.Local
            private static void Postfix(Player __instance)
            {
                if (__instance.data.view.IsMine && !__instance.GetComponent<PlayerAPI>().enabled && Stats.localPlayer == null)
                {
                    Stats.localPlayer = __instance;
                }
            }
        }

    }
}