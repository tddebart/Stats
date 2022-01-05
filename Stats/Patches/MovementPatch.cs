using HarmonyLib;
using UnboundLib;

namespace Stats.Patches
{
    public class MovementPatch
    {
        [HarmonyPatch(typeof(PlayerJump)),HarmonyPatch("Jump")]
        private class Patch_Jump
        {
            // ReSharper disable once UnusedMember.Local
            [HarmonyPriority(Priority.First)]
            private static void Prefix(PlayerJump __instance, CharacterData ___data)
            {
                if (___data.sinceJump > 0.1f && ___data.currentJumps > 0)
                {
#if DEBUG  
                    UnityEngine.Debug.LogWarning("Jump");
#endif
                    Stats.AddValue("Jumps");
                }
            }
        }

    }
}