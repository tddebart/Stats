using HarmonyLib;

namespace Stats.Patches
{
    public static class BlockPatch
    {
        [HarmonyPatch(typeof(Block),"Spawn")]
        private class Patch_Start
        {
            // ReSharper disable once UnusedMember.Local
            private static void Postfix(Block __instance)
            {
                var player = __instance.gameObject.GetComponent<Player>();
                if (player.data.view.IsMine && !player.GetComponent<PlayerAPI>().enabled)
                {
#if DEBUG
                    UnityEngine.Debug.LogWarning("Blocked");
#endif
                    Stats.AddValue("Blocks");
                }
            }
        }


    }
}