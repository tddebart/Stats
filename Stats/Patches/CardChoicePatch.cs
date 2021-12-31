using HarmonyLib;
using Stats.Extensions;
using UnityEngine;

namespace Stats.Patches
{
    public class CardChoicePatch
    {
        [HarmonyPatch(typeof(CardChoice)),HarmonyPatch("Update")]
        private class Patch_Update
        {
            // ReSharper disable once UnusedMember.Local
            private static void Postfix(CardChoice __instance)
            {
                if (__instance.pickrID == -1) return;
                
                if(Input.GetKeyDown(KeyCode.Tab)) {
                    UnityEngine.Debug.Log("Tab: " + __instance.pickrID);
                    UnityEngine.Debug.Log(PlayerManager.instance.GetPlayerWithID(__instance.pickrID).data.maxHealth);

                    Stats.drawCardUI = true;
                    Stats.playerCard = PlayerManager.instance.GetPlayerWithID(__instance.pickrID);
                }

                if (Input.GetKeyUp(KeyCode.Tab))
                {
                    Stats.drawCardUI = false;
                }
            }
        }

    }
}