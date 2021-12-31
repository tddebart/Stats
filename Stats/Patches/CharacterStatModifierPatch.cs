using HarmonyLib;
using UnityEngine;

namespace Stats.Patches
{
    public class CharacterStatModifierPatch
    {
        [HarmonyPatch(typeof(CharacterStatModifiers),"DealtDamage")]
        private class Patch_DealtDamage
        {
            // ReSharper disable once UnusedMember.Local
            private static void Postfix(CharacterStatModifiers __instance, Vector2 damage, bool selfDamage, Player damagedPlayer)
            {
                if (selfDamage || damagedPlayer == null || !__instance.GetComponent<Player>().data.view.IsMine || __instance.GetComponent<Player>().GetComponent<PlayerAPI>().enabled || __instance.GetComponent<Player>().playerID != 0) return;
                
#if DEBUG
                UnityEngine.Debug.LogWarning("Dealt damage: " + damage.magnitude.ToString("N0"));
#endif
                Stats.AddValue("Total damage done", (int)float.Parse(damage.magnitude.ToString("N0")));

                if (Stats.HighestActivated && Stats.HighestDamage < (int)float.Parse(damage.magnitude.ToString("N0")))
                {
                    Stats.HighestDamage = (int)float.Parse(damage.magnitude.ToString("N0"));
                    Stats.SetValue("Damage", (int)float.Parse(damage.magnitude.ToString("N0")));
                }
            }
        }
        
        [HarmonyPatch(typeof(CharacterStatModifiers),"WasDealtDamage")]
        private class Patch_DealtDamageFromOther
        {
            // ReSharper disable once UnusedMember.Local
            private static void Postfix(CharacterStatModifiers __instance, Vector2 damage, bool selfDamage)
            {
                if (__instance.GetComponent<Player>().data.view.IsMine && !__instance.GetComponent<Player>().GetComponent<PlayerAPI>().enabled && !selfDamage && __instance.GetComponent<Player>().playerID == 0)
                {
#if DEBUG
                    UnityEngine.Debug.LogWarning("Damage dealt to us: " + damage.magnitude.ToString("N0"));
#endif
                    
                    Stats.AddValue("Total damage received", (int)float.Parse(damage.magnitude.ToString()));
                }
            }
        } 
    }
}