using BepInEx.Configuration;
using TMPro;
using UnityEngine;

namespace Stats
{
    public class CardItem : MonoBehaviour
    {
        public int pickedAmountConfig
        {
            get => PlayerPrefs.GetInt("bossSloth.stats" + "cards_picked" + cardName, 0);
            set => PlayerPrefs.SetInt("bossSloth.stats" + "cards_picked" + cardName, value);
        }
        public int seenAmountConfig
        {
            get => PlayerPrefs.GetInt("bossSloth.stats" + "cards_seen" + cardName, 0);
            set => PlayerPrefs.SetInt("bossSloth.stats" + "cards_seen" + cardName, value);
        }

        public string cardName;
        private readonly TextMeshProUGUI pickedAmount;
        private readonly TextMeshProUGUI seenAmount;

        public CardItem()
        {
            cardName = transform.Find("CardBase").GetComponent<TextMeshProUGUI>().text;
            pickedAmount = transform.Find("Picked/CardBase_Text").GetComponent<TextMeshProUGUI>();
            seenAmount = transform.Find("Seen/CardBase_Text").GetComponent<TextMeshProUGUI>();
            
            this.gameObject.name = cardName;
        }

        public void UpdateValue()
        {
            pickedAmount.text = pickedAmountConfig.ToString("N0", Stats.cultureInfo);
            seenAmount.text = seenAmountConfig.ToString("N0", Stats.cultureInfo);
        }
    }
}