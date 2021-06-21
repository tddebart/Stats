using BepInEx.Configuration;
using TMPro;
using UnityEngine;

namespace Stats
{
    public class StatValue : MonoBehaviour
    {
        // ReSharper disable once FieldCanBeMadeReadOnly.Local
        public ConfigEntry<int> amount;

        public string _statName;
        
        private TextMeshProUGUI statAmount;

        public StatValue(GameObject obj ,string StatName, string Section, string Description, ref GameObject _obj)
        {
            UnityEngine.Debug.LogWarning("Forech");
            var gameObj = obj.AddComponent<StatValue>();
            _obj = gameObj.gameObject;

            _statName = StatName;
            gameObj._statName = StatName;
            
            gameObj.name = StatName;
            
            gameObj.transform.Find("StatBase").GetComponent<TextMeshProUGUI>().text = StatName;
            gameObj.statAmount = gameObj.transform.Find("Procedural Image/StatBase_Text").GetComponent<TextMeshProUGUI>();
            
            gameObj.amount = Stats.Instance.customConfig.Bind(Section, StatName, 0, Description);

            obj.SetActive(true);
        }

        public void UpdateValue()
        {
            statAmount.text = amount.Value.ToString("N0", Stats.cultureInfo);
        }
    }
}