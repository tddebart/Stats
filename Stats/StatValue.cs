using System;
using BepInEx.Configuration;
using TMPro;
using UnboundLib;
using UnityEngine;

namespace Stats
{
    public class StatValue : MonoBehaviour
    {
        // ReSharper disable once FieldCanBeMadeReadOnly.Local
        public ConfigEntry<float> amount;

        public string _statName;
        
        private TextMeshProUGUI statAmount;

        private Action updateAction; 
        
        private int RoundDecimals;

        public StatValue(GameObject obj ,string StatName, string Section, string Description, int _RoundDecimals, out GameObject _obj)
        {
            var gameObj = obj.AddComponent<StatValue>();
            _obj = gameObj.gameObject;

            _statName = StatName;
            RoundDecimals = _RoundDecimals;
            
            gameObj.RoundDecimals = _RoundDecimals;
            gameObj._statName = StatName;
            
            gameObj.name = StatName;
            
            gameObj.transform.Find("StatBase").GetComponent<TextMeshProUGUI>().text = StatName;
            gameObj.statAmount = gameObj.transform.Find("StatBase_Text").GetComponent<TextMeshProUGUI>();

            gameObj.amount = Stats.Instance.customConfig.Bind(Section, StatName, 0f, Description);

            obj.SetActive(true);
        }

        public void AddUpdateAction(Action action)
        {
            this.updateAction = (Action)Delegate.Combine(this.updateAction, action);
        }

        public void UpdateValue()
        {
            if (updateAction != null) updateAction();
            statAmount.text = amount.Value.ToString("N" + RoundDecimals, Stats.cultureInfo);
            
            statAmount.transform.SetXPosition(0);
        }
    }
}