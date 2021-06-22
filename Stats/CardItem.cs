using BepInEx.Configuration;
using TMPro;
using UnityEngine;

namespace Stats
{
    public class CardItem : MonoBehaviour
    {
        // ReSharper disable once FieldCanBeMadeReadOnly.Local
        public ConfigEntry<int> amount;

        public string cardName;
        private readonly TextMeshProUGUI cardAmount;

        public CardItem()
        {
            cardName = transform.Find("CardBase").GetComponent<TextMeshProUGUI>().text;
            cardAmount = transform.Find("Procedural Image/CardBase_Text").GetComponent<TextMeshProUGUI>();
            
            this.gameObject.name = cardName;
            
            amount = Stats.Instance.customConfig.Bind("Cards", cardName, 0, "Amount of times you have gotten " + cardName);
        }

        public void UpdateValue()
        {
            cardAmount.text = amount.Value.ToString("N0", Stats.cultureInfo);
        }
    }
}