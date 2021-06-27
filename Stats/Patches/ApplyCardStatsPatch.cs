using HarmonyLib;
using Photon.Pun;
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
#endif
                    Stats.AddCardPickedValue(__instance.GetComponent<CardInfo>());
                }
            }
        }
        
        [HarmonyPatch(typeof(CardChoice)),HarmonyPatch("Spawn")]
        private class Patch_AwakeCardInfo
        {
            // ReSharper disable once UnusedMember.Local
            private static void Postfix(GameObject objToSpawn, int ___pickrID)
            {
                if (PhotonNetwork.OfflineMode)
                {
#if DEBUG
                    UnityEngine.Debug.LogWarning("Spawned card: " + objToSpawn.GetComponent<CardInfo>().cardName.ToLower() + " with id: " + ___pickrID);
#endif
                    Stats.AddCardSeenValue(objToSpawn.GetComponent<CardInfo>());
                    return;
                }
                
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