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
#if DEBUG
                    UnityEngine.Debug.LogWarning("Shot");
#endif
                    Stats.AddValue("Bullets shot");
                }
            }
        }

    }
}