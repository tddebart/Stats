using BepInEx.Configuration;
using TMPro;
using UnityEngine;

namespace Stats
{
    public class CardItem : MonoBehaviour
    {
        // ReSharper disable once FieldCanBeMadeReadOnly.Local
        public ConfigEntry<int> pickedAmountConfig;
        public ConfigEntry<int> seenAmountConfig;

        public string cardName;
        private readonly TextMeshProUGUI pickedAmount;
        private readonly TextMeshProUGUI seenAmount;

        public CardItem()
        {
            cardName = transform.Find("CardBase").GetComponent<TextMeshProUGUI>().text;
            pickedAmount = transform.Find("Picked/CardBase_Text").GetComponent<TextMeshProUGUI>();
            seenAmount = transform.Find("Seen/CardBase_Text").GetComponent<TextMeshProUGUI>();
            
            this.gameObject.name = cardName;
            
            pickedAmountConfig = Stats.Instance.customConfig.Bind("Cards", cardName + " picked", 0, "Amount of times you have gotten " + cardName);
            seenAmountConfig = Stats.Instance.customConfig.Bind("Cards", cardName + " seen", 0, "Amount of times you have seen " + cardName);
        }

        public void UpdateValue()
        {
            pickedAmount.text = pickedAmountConfig.Value.ToString("N0", Stats.cultureInfo);
            seenAmount.text = seenAmountConfig.Value.ToString("N0", Stats.cultureInfo);
        }
    }
}