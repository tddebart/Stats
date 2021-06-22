using HarmonyLib;

namespace Stats.Patches
{
    public class ApplyCardStatsPatch
    {
        [HarmonyPatch(typeof(ApplyCardStats)),HarmonyPatch("ApplyStats")]
        private class Patch_DoApplyStats
        {
            // ReSharper disable once UnusedMember.Local
            private static void Postfix(ApplyCardStats __instance, Player ___playerToUpgrade)
            {
                if (___playerToUpgrade.data.view.IsMine && !___playerToUpgrade.GetComponent<PlayerAPI>().enabled)
                {
#if DEBUG
                    UnityEngine.Debug.LogWarning("Got card: " + __instance.GetComponent<CardInfo>().cardName.ToLower());
#endif
                    Stats.AddCardValue(__instance.GetComponent<CardInfo>());
                }
            }
        }

    }
}