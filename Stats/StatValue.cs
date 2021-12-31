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
        public float amount
        {
            get => PlayerPrefs.GetFloat("bossSloth.stats" + section + _statName, 0);
            set => PlayerPrefs.SetFloat("bossSloth.stats" + section + _statName, value);
        }

        public string customAmount = "FUCK";

        public string _statName;
        
        private TextMeshProUGUI statAmount;

        private Action<StatValue> updateAction; 
        
        private int RoundDecimals;
        
        private string section;

        public StatValue(GameObject obj ,string StatName, string Section, int _RoundDecimals, out GameObject _obj)
        {
            var gameObj = obj.AddComponent<StatValue>();
            _obj = gameObj.gameObject;

            _statName = StatName;
            section = Section;
            RoundDecimals = _RoundDecimals;
            customAmount = "FUCK";
            
            gameObj.RoundDecimals = _RoundDecimals;
            gameObj._statName = StatName;
            gameObj.section = Section;
            gameObj.customAmount = "FUCK";
            
            gameObj.name = StatName;
            
            gameObj.transform.Find("StatBase").GetComponent<TextMeshProUGUI>().text = StatName;
            gameObj.statAmount = gameObj.transform.Find("StatBase_Text").GetComponent<TextMeshProUGUI>();

            obj.SetActive(true);
        }

        public void AddUpdateAction(Action<StatValue> action)
        {
            this.updateAction = (Action<StatValue>)Delegate.Combine(this.updateAction, action);
        }

        public void UpdateValue()
        {
            if (updateAction != null) updateAction(this);
            statAmount.text = customAmount == "FUCK" ? amount.ToString("N" + RoundDecimals, Stats.cultureInfo) : customAmount;
            
            statAmount.transform.SetXPosition(0);
        }
    }
}