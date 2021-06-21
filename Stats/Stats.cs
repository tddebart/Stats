using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using Jotunn.Utils;
using TMPro;
using UnboundLib;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
// ReSharper disable UseStringInterpolation
// ReSharper disable FormatStringProblem

namespace Stats
{
    [BepInDependency("com.willis.rounds.unbound")]
    [BepInPlugin(ModId, ModName, Version)]
    [BepInProcess("Rounds.exe")]
    public class Stats : BaseUnityPlugin
    {
        private const string ModId = "BossSloth.rounds.Stats";
        private const string ModName = "Stats";
        public const string Version = "0.1.0";
        
        public enum Value
        {
            Shoots,
            Blocks,
        }

        public static Stats Instance;

        public readonly ConfigFile customConfig = new ConfigFile(Path.Combine(Paths.ConfigPath, "Stats.cfg"), true);

        internal static readonly CultureInfo cultureInfo = new CultureInfo("nl-NL");

        private bool firstTime = true;

        internal static readonly List<CardItem> CardObjects = new List<CardItem>();

        private static ConfigEntry<int> shoots;
        private static ConfigEntry<int> blocks;

        public Stats()
        {
            shoots = customConfig.Bind("Gun", "shoots", 0, "How many bullets you have shot");
            blocks = customConfig.Bind("Block", "blocks", 0, "How many times you blocked");
            
            var uiStats = AssetUtils.LoadAssetBundleFromResources("statui", typeof(Stats).Assembly);
            if (uiStats == null)
            {
                UnityEngine.Debug.LogError("Couldn't find UIStats?");
            }
            var statsText = uiStats.LoadAsset<GameObject>("Stats_Text");
            var stats = uiStats.LoadAsset<GameObject>("Stats");
            var cards = uiStats.LoadAsset<GameObject>("Cards");

            GameObject _statsText;
            GameObject _stats;
            GameObject _cards;
            
            On.MainMenuHandler.Awake += (orig, self) =>
            {
                CardObjects.Clear();
                this.ExecuteAfterSeconds(firstTime ? 1f : 0f, () =>
                {
                    // Create main menu text
                    _statsText = Instantiate(statsText, MainMenuHandler.instance.transform.Find("Canvas/ListSelector/Main/Group"));
                    _statsText.transform.SetSiblingIndex(4);
                    
                    // Create stats menu
                    _stats = Instantiate(stats, MainMenuHandler.instance.transform.Find("Canvas/ListSelector"));
                    
                    // Create cards menu
                    _cards = Instantiate(cards, MainMenuHandler.instance.transform.Find("Canvas/ListSelector"));
                    CreateCardMenu(_cards);

                    // Create button for main menu text
                    _statsText.GetComponent<Button>().onClick.AddListener(() => ClickStats(_stats));
                    
                    // Create click action for cards button
                    _stats.transform.Find("Group/Grid/Cards").GetComponent<Button>().onClick.AddListener(() => ClickCards(_cards));
                    
                    // Create back esc key
                    _stats.transform.Find("Group").GetComponent<GoBack>().target = MainMenuHandler.instance.transform.Find("Canvas/ListSelector/Main").GetComponent<ListMenuPage>();
                    _cards.transform.Find("Group").GetComponent<GoBack>().target = _stats.GetComponent<ListMenuPage>();
                    
                    
                    // Creat back button
                    _stats.transform.Find("Group/Back").GetComponent<Button>().onClick.AddListener(ClickBack(MainMenuHandler.instance.transform.Find("Canvas/ListSelector/Main")));
                    _cards.transform.Find("Group/Back").GetComponent<Button>().onClick.AddListener(ClickBack(_stats.transform));
                    
                    CreateStat("Testing", "testing", "", _stats);
                    CreateStat("testing", "testing", "", _stats);
                });
                
                firstTime = false;

                orig(self);
            };

        }

        private static void UpdateValuesMain(GameObject _stats)
        {
            var shootsText = _stats.transform.Find("Group/Grid/Shoots/Procedural Image/Shoots_Text");
            shootsText.GetComponent<TextMeshProUGUI>().text = shoots.Value.ToString("N0", cultureInfo);
            
            var blockText = _stats.transform.Find("Group/Grid/Blocks/Procedural Image/Blocks_Text");
            blockText.GetComponent<TextMeshProUGUI>().text = blocks.Value.ToString("N0", cultureInfo);
        }

        private static void UpdateValues(GameObject obj)
        {
            foreach (var stat in obj.GetComponentsInChildren<StatValue>(true))
            {
                stat.UpdateValue();
            }
        }
        
        internal static void AddValueOld(Value value)
        {
            if (value == Value.Shoots)
            {
                shoots.Value++;
            } else if (value == Value.Blocks)
            {
                blocks.Value++;
            }
        }

        internal static void AddValue(string valueName)
        {
            var listSelector = MainMenuHandler.instance.transform.Find("Canvas/ListSelector");
            var values = listSelector.gameObject.GetComponentsInChildren<StatValue>(true);

            foreach (var value in values)
            {
                if (string.Equals(valueName, value._statName, StringComparison.OrdinalIgnoreCase))
                {
                    value.amount.Value++;
                }
            }
        }

        internal static void AddCardValue(CardInfo cardInfo)
        {
            foreach(var card in CardObjects)
            {
                if (string.Equals(cardInfo.cardName, card.cardName, StringComparison.OrdinalIgnoreCase))
                {
                    card.amount.Value++;
                }
            }
        }

        private void Start()
        {
            var harmony = new Harmony(ModId);
            harmony.PatchAll();
            Instance = this;
        }

        // ReSharper disable once UnusedMethodReturnValue.Local
        private static GameObject CreateStat(string Name, string Section, string Description, GameObject location)
        {
            location = location.transform.Find("Group/Grid/").gameObject;
            var statBase = Instantiate(MainMenuHandler.instance.transform.Find("Canvas/ListSelector/Stats(Clone)/Group/Grid/StatBaseObject").gameObject, location.transform);
            GameObject obj = null;
            // ReSharper disable once Unity.IncorrectMonoBehaviourInstantiation
            // ReSharper disable once UnusedVariable
            var statValue = new StatValue(statBase, Name, Section, Description, ref obj);
            return(obj);
        }

        private static void CreateCardMenu(GameObject cards)
        {
            var scrollDefault = cards.transform.Find("Group/Grid/Scroll View Default");
            var scrollModded = cards.transform.Find("Group/Grid/Scroll View Modded");

            var defaultButton = cards.transform.Find("Group/Grid/Default");
            defaultButton.GetComponent<Button>().onClick.AddListener(() =>
            {
                scrollDefault.gameObject.SetActive(true);
                scrollModded.gameObject.SetActive(false);
            });
            var moddedButton = cards.transform.Find("Group/Grid/Modded");
            moddedButton.GetComponent<Button>().onClick.AddListener(() =>
            {
                scrollDefault.gameObject.SetActive(false);
                scrollModded.gameObject.SetActive(true);
            });
            
            var CardBase = cards.transform.Find("Group/Grid/CardBaseObject");
            foreach (var cardInstance in CardChoice.instance.cards)
            {
                var _card = Instantiate(CardBase, cards.transform.Find("Group/Grid/Scroll View Default/Viewport/Content"));
                
                _card.Find("CardBase").GetComponent<TextMeshProUGUI>().text = cardInstance.cardName.ToLower();
                _card.Find("Procedural Image/CardBase_Text").GetComponent<TextMeshProUGUI>().text = "10";
                var cardObject = _card.gameObject;
                var cardItem = cardObject.AddComponent<CardItem>();
                CardObjects.Add(cardItem);
                cardObject.SetActive(true);
            }

            foreach (Transform customCard in Unbound.Instance.canvas.transform.Find("Card Toggle Menu(Clone)/Mod Cards/Content"))
            {
                var _card = Instantiate(CardBase, cards.transform.Find("Group/Grid/Scroll View Modded/Viewport/Content"));
                
                _card.Find("CardBase").GetComponent<TextMeshProUGUI>().text = customCard.GetComponentInChildren<TextMeshProUGUI>().text.ToLower();
                _card.Find("Procedural Image/CardBase_Text").GetComponent<TextMeshProUGUI>().text = "10";
                var cardObject = _card.gameObject;
                var cardItem = cardObject.AddComponent<CardItem>();
                CardObjects.Add(cardItem);
                cardObject.SetActive(true);
            }
            
        }
        
        private void ClickStats(GameObject stats)
        {
            UpdateValuesMain(stats);
            UpdateValues(stats);
            stats.GetComponent<ListMenuPage>().Open();
        }
        
        private static void ClickCards(GameObject cards)
        {
            foreach(var card in CardObjects)
            {
                card.UpdateValue();
            }
            cards.GetComponent<ListMenuPage>().Open();
        }

        private static UnityAction ClickBack(Component backObject)
        {
            return () =>
            {
                backObject.GetComponent<ListMenuPage>().Open();
            };
        }
    }
}