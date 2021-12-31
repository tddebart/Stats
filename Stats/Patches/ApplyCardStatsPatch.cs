using HarmonyLib;
using Photon.Pun;
using UnboundLib.GameModes;
using UnityEngine;

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
                    Stats.AddCardPickedValue(__instance.GetComponent<CardInfo>());
#endif
#if !DEBUG
                    if (GameModeManager.CurrentHandler.Name != "Sandbox")
                    {
                        Stats.AddCardPickedValue(__instance.GetComponent<CardInfo>());
                    }
#endif
                }
            }
        }
        
        [HarmonyPatch(typeof(CardChoice)),HarmonyPatch("Spawn")]
        private class Patch_AwakeCardInfo
        {
            // ReSharper disable once UnusedMember.Local
            private static void Postfix(GameObject objToSpawn, int ___pickrID)
            {
#if DEBUG
                if (PhotonNetwork.OfflineMode)
                {
                    UnityEngine.Debug.LogWarning("Spawned card: " + objToSpawn.GetComponent<CardInfo>().cardName.ToLower() + " with id: " + ___pickrID);
                    Stats.AddCardSeenValue(objToSpawn.GetComponent<CardInfo>());
                    return;
                }
#endif
                
                if (Stats.localPlayer.data.view.IsMine && !Stats.localPlayer.GetComponent<PlayerAPI>().enabled && Stats.localPlayer.playerID == ___pickrID)
                {
#if DEBUG
                    UnityEngine.Debug.LogWarning("Spawned card: " + objToSpawn.GetComponent<CardInfo>().cardName.ToLower() + " with id: " + ___pickrID);
#endif
                    Stats.AddCardSeenValue(objToSpawn.GetComponent<CardInfo>());
                }
            }
        }

    }
}