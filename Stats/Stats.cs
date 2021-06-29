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

        public static Stats Instance;

        private static GameObject MenuBase;
        private static GameObject StatBase;
        private static GameObject ButtonBase;

        public readonly ConfigFile customConfig = new ConfigFile(Path.Combine(Paths.ConfigPath, "Stats.cfg"), true);

        internal static readonly CultureInfo cultureInfo = new CultureInfo("nl-NL");

        private bool firstTime = true;

        internal static readonly List<CardItem> CardObjects = new List<CardItem>();
        internal static readonly List<StatValue> StatObjects = new List<StatValue>();

        internal static Player localPlayer;

        public Stats()
        {
            var uiStats = AssetUtils.LoadAssetBundleFromResources("statui", typeof(Stats).Assembly);
            if (uiStats == null)
            {
                UnityEngine.Debug.LogError("Couldn't find UIStats?");
            }
            var statsText = uiStats.LoadAsset<GameObject>("Stats_Text");
            var baseObjects = uiStats.LoadAsset<GameObject>("BaseObjects");
            MenuBase = uiStats.LoadAsset<GameObject>("EmptyMenuBase");
            var stats = uiStats.LoadAsset<GameObject>("Stats");
            var cards = uiStats.LoadAsset<GameObject>("Cards");
            
            StatBase = baseObjects.transform.Find("Group/Grid/StatBaseObject").gameObject;
            ButtonBase = baseObjects.transform.Find("Group/Grid/ButtonBaseObject").gameObject;
            
            GameObject _statsText;
            GameObject _stats;
            GameObject _cards;
            
            On.MainMenuHandler.Awake += (orig, self) =>
            {
                CardObjects.Clear();
                this.ExecuteAfterSeconds(firstTime ? 1f : 0f, () =>
                {
                    // Create main menu text
                    _statsText = Instantiate(statsText,
                        MainMenuHandler.instance.transform.Find("Canvas/ListSelector/Main/Group"));
                    _statsText.transform.SetSiblingIndex(4);

                    // Create stats menu
                    _stats = Instantiate(stats, MainMenuHandler.instance.transform.Find("Canvas/ListSelector"));

                    // Create cards menu
                    _cards = Instantiate(cards, MainMenuHandler.instance.transform.Find("Canvas/ListSelector"));
                    CreateCardMenu(_cards);

                    // Create button for main menu text
                    _statsText.GetComponent<Button>().onClick.AddListener(() => ClickMenuButton(_stats));

                    // Create click action for cards button
                    _stats.transform.Find("Group/Grid/Cards").GetComponent<Button>().onClick
                        .AddListener(() => ClickCards(_cards));

                    // Create back esc key
                    _stats.transform.Find("Group").GetComponent<GoBack>().target = MainMenuHandler.instance.transform
                        .Find("Canvas/ListSelector/Main").GetComponent<ListMenuPage>();

                    _cards.transform.Find("Group").GetComponent<GoBack>().target = _stats.GetComponent<ListMenuPage>();


                    // Creat back button
                    _stats.transform.Find("Group/Back").GetComponent<Button>().onClick.AddListener(
                        ClickBack(MainMenuHandler.instance.transform.Find("Canvas/ListSelector/Main")
                            .GetComponent<ListMenuPage>()));

                    _cards.transform.Find("Group/Back").GetComponent<Button>().onClick
                        .AddListener(ClickBack(_stats.transform.GetComponent<ListMenuPage>()));

                    
                    var gameMenu = CreateMenu("Game", 50, _stats);
                    var gamesPlayed = CreateStat("Games played", "Games", "How many games you have played", gameMenu);
                    var gamesWon = CreateStat("Games won", "Games", "How many games you have won", gameMenu);
                    var gamesLost = CreateStat("Games lost", "Games", "How many games you have lost", gameMenu);
                    var gamesWonPercentage = CreateStat("Games won percentage", "Games", "How many games you have won by percentage", gameMenu, 0,
                        () =>
                        {
                            StatValue RWP = null;
                            foreach (var stat in StatObjects)
                            {
                                if (stat._statName == "Games won percentage")
                                {
                                    RWP = stat;
                                }
                            }

                            if (gamesWon.GetComponent<StatValue>().amount.Value == 0 ||
                                gamesPlayed.GetComponent<StatValue>().amount.Value == 0 || RWP == null) return;
                            RWP.amount.Value = (gamesWon.GetComponent<StatValue>().amount.Value /
                                                gamesPlayed.GetComponent<StatValue>().amount.Value) * 100;
                        });
                    
                    var roundsPlayed = CreateStat("Rounds played", "Games", "How many rounds you have played", gameMenu);
                    var roundsWon = CreateStat("Rounds won", "Games", "How many rounds you have won", gameMenu);
                    var roundsLost = CreateStat("Rounds lost", "Games", "How many rounds you have lost", gameMenu);
                    var roundsWonPercentage = CreateStat("Rounds won percentage", "Games", "How many rounds you have won by percentage", gameMenu, 0,
                        () =>
                        {
                            StatValue RWP = null;
                            foreach (var stat in StatObjects)
                            {
                                if (stat._statName == "Rounds won percentage")
                                {
                                    RWP = stat;
                                }
                            }

                            if (roundsWon.GetComponent<StatValue>().amount.Value == 0 ||
                                roundsPlayed.GetComponent<StatValue>().amount.Value == 0 || RWP == null) return;
                            RWP.amount.Value = (roundsWon.GetComponent<StatValue>().amount.Value /
                                                 roundsPlayed.GetComponent<StatValue>().amount.Value) * 100;
                        });
                    
                    var GunMenu = CreateMenu("Gun and Player", 50, _stats);
                    var bulletsShot = CreateStat("Bullets shot", "Gun", "How many bullets you have shot", GunMenu);
                    var totalDamage = CreateStat("Total damage", "Gun", "How much damage you have done total", GunMenu);
                    CreateStat("Average damage done per shot", "Gun", "How much damage you have done average per bullet",
                        GunMenu, 2,
                        () =>
                        {
                            StatValue AVDPS = null;
                            foreach (var stat in StatObjects)
                            {
                                if (stat._statName == "Average damage done per shot")
                                {
                                    AVDPS = stat;
                                }
                            }

                            if (totalDamage.GetComponent<StatValue>().amount.Value == 0 ||
                                bulletsShot.GetComponent<StatValue>().amount.Value == 0 || AVDPS == null) return;
                            AVDPS.amount.Value = totalDamage.GetComponent<StatValue>().amount.Value /
                                                 bulletsShot.GetComponent<StatValue>().amount.Value;
                        });
                    var recievedDamage = CreateStat("Total damage received", "Player", "How much damage you have received", GunMenu);

                    CreateStat("Blocks", "Block", "How many times you blocked", GunMenu).transform.SetSiblingIndex(3);
                    CreateStat("Average damage done per game", "Games", "How much damage you have done per game average", GunMenu,2 , () =>
                    {
                        StatValue AVDPG = null;
                        foreach (var stat in StatObjects)
                        {
                            if (stat._statName == "Average damage done per game")
                            {
                                AVDPG = stat;
                            }
                        }
                        
                        if (totalDamage.GetComponent<StatValue>().amount.Value == 0 ||
                            gamesPlayed.GetComponent<StatValue>().amount.Value == 0 || AVDPG == null) return;
                        AVDPG.amount.Value = totalDamage.GetComponent<StatValue>().amount.Value /
                                             gamesPlayed.GetComponent<StatValue>().amount.Value;
                    });
                    CreateStat("Average damage received per game", "Games", "How much damage you have received per game average", GunMenu,2 , () =>
                    {
                        StatValue AVDRPG = null;
                        foreach (var stat in StatObjects)
                        {
                            if (stat._statName == "Average damage received per game")
                            {
                                AVDRPG = stat;
                            }
                        }
                        
                        if (recievedDamage.GetComponent<StatValue>().amount.Value == 0 ||
                            gamesPlayed.GetComponent<StatValue>().amount.Value == 0 || AVDRPG == null) return;
                        AVDRPG.amount.Value = recievedDamage.GetComponent<StatValue>().amount.Value /
                                             gamesPlayed.GetComponent<StatValue>().amount.Value;
                    });

                    


                    //var testMenu = CreateMenu("TestMenu", 100, _stats);
                    //CreateStat("testMenuStat", "testing", "", testMenu, () => {UnityEngine.Debug.Log("Updated testMenuStat");});
                });
                
                firstTime = false;

                orig(self);
            };

        }

        private static void UpdateValues(GameObject obj)
        {
            foreach (var stat in obj.GetComponentsInChildren<StatValue>(true))
            {
                stat.UpdateValue();
            }
        }

        internal static void AddValue(string valueName, int amount = 1)
        {
            var listSelector = MainMenuHandler.instance.transform.Find("Canvas/ListSelector");
            var values = listSelector.gameObject.GetComponentsInChildren<StatValue>(true);

            foreach (var value in values)
            {
                if (string.Equals(valueName, value._statName, StringComparison.OrdinalIgnoreCase))
                {
                    value.amount.Value += amount;
                }
            }
        }

        internal static void AddCardPickedValue(CardInfo cardInfo)
        {
            foreach(var card in CardObjects)
            {
                if (string.Equals(cardInfo.cardName, card.cardName, StringComparison.OrdinalIgnoreCase))
                {
                    card.pickedAmountConfig.Value++;
                }
            }
        }
        
        internal static void AddCardSeenValue(CardInfo cardInfo)
        {
            foreach(var card in CardObjects)
            {
                if (string.Equals(cardInfo.cardName, card.cardName, StringComparison.OrdinalIgnoreCase))
                {
                    card.seenAmountConfig.Value++;
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
        private static GameObject CreateStat(string Name, string Section, string Description, GameObject parent,int RoundDecimals = 0 , Action updateAction = null)
        {
            parent = parent.transform.Find("Group/Grid/").gameObject;
            var _statBase = Instantiate(StatBase, parent.transform);
            // ReSharper disable once Unity.IncorrectMonoBehaviourInstantiation
            // ReSharper disable once UnusedVariable
            var statValue = new StatValue(_statBase, Name, Section, Description, RoundDecimals , out var obj);
            StatObjects.Add(obj.GetComponent<StatValue>());
            obj.GetComponent<StatValue>().AddUpdateAction(updateAction);
            return(obj);
        }

        private static GameObject CreateMenu(string Name, int size, GameObject parent)
        {
            var obj = Instantiate(MenuBase, MainMenuHandler.instance.transform.Find("Canvas/ListSelector"));
            obj.name = Name;

            // Assign back objects
            var goBackObject = parent.GetComponent<ListMenuPage>();
            obj.GetComponentInChildren<GoBack>(true).target = goBackObject;
            obj.transform.Find("Group/Back").gameObject.GetComponent<Button>().onClick.AddListener(ClickBack(goBackObject));

            // Create button to menu
            var button = Instantiate(ButtonBase, parent.transform.Find("Group/Grid"));
            button.GetComponent<ListMenuButton>().setBarHeight = size;
            button.name = Name;
            button.GetComponent<RectTransform>().sizeDelta = new Vector2(button.GetComponent<RectTransform>().sizeDelta.x, size+12);
            var uGUI = button.GetComponentInChildren<TextMeshProUGUI>();
            uGUI.text = Name;
            uGUI.fontSize = size;
            button.GetComponent<Button>().onClick.AddListener(() => ClickMenuButton(obj.GetComponent<ListMenuPage>().gameObject));

            return obj;
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
            foreach (var cardInstance in (CardInfo[]) Traverse.Create(Unbound.Instance).Field("defaultCards").GetValue())
            {
                var _card = Instantiate(CardBase, cards.transform.Find("Group/Grid/Scroll View Default/Viewport/Content"));
                
                _card.Find("CardBase").GetComponent<TextMeshProUGUI>().text = cardInstance.cardName.ToLower();
                _card.Find("Picked/CardBase_Text").GetComponent<TextMeshProUGUI>().text = "ERROR";
                _card.Find("Seen/CardBase_Text").GetComponent<TextMeshProUGUI>().text = "ERROR";
                var cardObject = _card.gameObject;
                var cardItem = cardObject.AddComponent<CardItem>();
                CardObjects.Add(cardItem);
                cardObject.SetActive(true);
            }

            foreach (Transform customCard in Unbound.Instance.canvas.transform.Find("Card Toggle Menu(Clone)/Mod Cards/Content"))
            {
                var _card = Instantiate(CardBase, cards.transform.Find("Group/Grid/Scroll View Modded/Viewport/Content"));
                
                _card.Find("CardBase").GetComponent<TextMeshProUGUI>().text = customCard.GetComponentInChildren<TextMeshProUGUI>().text.ToLower();
                _card.Find("Picked/CardBase_Text").GetComponent<TextMeshProUGUI>().text = "ERROR";
                _card.Find("Seen/CardBase_Text").GetComponent<TextMeshProUGUI>().text = "ERROR";
                var cardObject = _card.gameObject;
                var cardItem = cardObject.AddComponent<CardItem>();
                CardObjects.Add(cardItem);
                cardObject.SetActive(true);
            }
            

            void ActivateAll()
            {
                foreach (var card in CardObjects)
                {
                    card.gameObject.SetActive(true);
                }
            }
            
            var searchBar = cards.transform.Find("Group/Grid/InputField");
            searchBar.GetComponent<TMP_InputField>().onValueChanged.AddListener( value =>
            {
                foreach (var card in CardObjects)
                {
                    if (value == "")
                    {
                        ActivateAll();
                        return;
                    }
                    if (!card.cardName.Contains(value))
                    {
                        card.gameObject.SetActive(false);
                    }
                    else
                    {
                        card.gameObject.SetActive(true);
                    }
                }
            });
            searchBar.GetComponent<TMP_InputField>().onDeselect.AddListener(value =>
            {
                searchBar.GetComponent<TMP_InputField>().text = "";
                ActivateAll();
            });
            
            
        }

        private static void ClickMenuButton(GameObject listMenuPage)
        {
            UpdateValues(listMenuPage);
            listMenuPage.GetComponent<ListMenuPage>().Open();
        }

        private static void ClickCards(GameObject cards)
        {
            UpdateValues(cards);
            foreach(var card in CardObjects)
            {
                card.UpdateValue();
            }
            cards.GetComponent<ListMenuPage>().Open();
        }

        private static UnityAction ClickBack(ListMenuPage backObject)
        {
            return backObject.Open;
        }
    }
}