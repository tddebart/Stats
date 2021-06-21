using HarmonyLib;

namespace Stats.Patches
{
    public class GunPatch
    {
        [HarmonyPatch(typeof(Gun)),HarmonyPatch("DoAttack")]
        private class Patch_DoAttack
        {
            // ReSharper disable once UnusedMember.Local
            private static void Postfix(Player ___player)
            {
                if (___player.data.view.IsMine && !___player.GetComponent<PlayerAPI>().enabled)
                {
                    UnityEngine.Debug.LogWarning("shot");
                    Stats.AddValueOld(Stats.Value.Shoots);
                }
            }
        }

    }
}