using HarmonyLib;

namespace Stats.Patches
{
    public class CharacterDataPatch
    {
        [HarmonyPatch(typeof(CharacterData)),HarmonyPatch("Update")]
        private class Patch_Update
        {
            // ReSharper disable once UnusedMember.Local
            private static void Postfix(CharacterData __instance)
            {
                if (Stats.HighestActivated)
                {
                    if (__instance.maxHealth > Stats.HighestHealth)
                    {
#if DEBUG
                        UnityEngine.Debug.LogWarning("New Highest health: " + (__instance.maxHealth).ToString("N0"));
#endif
                        Stats.HighestHealth = __instance.maxHealth;
                        Stats.SetValue("Health", (int)__instance.maxHealth);
                    }
                }
            }
        }

    }
}