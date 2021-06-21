using HarmonyLib;

namespace Stats.Patches
{
    public class BlockPatch
    {
        [HarmonyPatch(typeof(Block),"DoBlock")]
        private class Patch_RPCA_DoBlock
        {
            // ReSharper disable once UnusedMember.Local
            private static void Postfix()
            {
                UnityEngine.Debug.LogWarning("block1");
                // var player = __state.gameObject.GetComponent<Player>();
                // if (player.data.view.IsMine && !player.GetComponent<PlayerAPI>().enabled)
                // {
                //     UnityEngine.Debug.LogWarning("block2");
                //     Stats.UpdateValue(Stats.Value.Blocks);
                // }
            }
        }
        
                
        [HarmonyPatch(typeof(Block),"Spawn")]
        private class Patch_Start
        {
            // ReSharper disable once UnusedMember.Local
            private static void Postfix(Block __instance)
            {
                var player = __instance.gameObject.GetComponent<Player>();
                if (player.data.view.IsMine && !player.GetComponent<PlayerAPI>().enabled)
                {
                    UnityEngine.Debug.LogWarning("block2");
                    Stats.UpdateValue(Stats.Value.Blocks);
                }
            }
        }


    }
}