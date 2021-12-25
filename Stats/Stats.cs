﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using Jotunn.Utils;
using TMPro;
using UnboundLib;
using UnboundLib.Utils;
using UnboundLib.Utils.UI;
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
        
        private static GameObject StatBase;
        private static GameObject CardMenuBase;

        public readonly ConfigFile customConfig = new ConfigFile(Path.Combine(Paths.ConfigPath, "Stats.cfg"), true);

        internal static readonly CultureInfo cultureInfo = new CultureInfo("nl-NL");

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
            
            var baseObjects = uiStats.LoadAsset<GameObject>("BaseObjects");
            CardMenuBase = uiStats.LoadAsset<GameObject>("Cards");
            
            StatBase = baseObjects.transform.Find("Group/Grid/StatBaseObject").gameObject;


            // Create stats menu
            Unbound.RegisterMenu("Stats", () => { }, CreateMenuUnbound, null);
        }
        
        private void CreateMenuUnbound(GameObject menu)
        {
            GameObject _cards;

            CardObjects.Clear();

            // Create cards menu
            _cards = Instantiate(CardMenuBase, MainMenuHandler.instance.transform.Find("Canvas/ListSelector"));
            CreateCardMenu(_cards);

            MenuHandler.CreateButton("Cards", menu, () =>
            {
                _cards.GetComponent<ListMenuPage>().Open();
                menu.GetComponent<ListMenuPage>().Close();
                ClickCards(_cards);
            });

            _cards.transform.Find("Group/Back").GetComponent<Button>().onClick
                .AddListener(ClickBack(menu.transform.GetComponent<ListMenuPage>()));


            var gameMenu = CreateMenu("Game", menu);
            var gamesPlayed = CreateStat("Games played", "Games", "How many games you have played", gameMenu);
            var gamesWon = CreateStat("Games won", "Games", "How many games you have won", gameMenu);
            var gamesLost = CreateStat("Games lost", "Games", "How many games you have lost", gameMenu);
            var gamesWonPercentage = CreateStat("Win percentage", "Games",
                "How many games you have won by percentage", gameMenu, 0,
                () =>
                {
                    StatValue RWP = null;
                    foreach (var stat in StatObjects)
                    {
                        if (stat._statName == "Win percentage")
                        {
                            RWP = stat;
                        }
                    }

                    if (gamesWon.GetComponent<StatValue>().amount.Value == 0 ||
                        gamesPlayed.GetComponent<StatValue>().amount.Value == 0 || RWP == null) return;
                    RWP.amount.Value = (gamesWon.GetComponent<StatValue>().amount.Value /
                                        gamesPlayed.GetComponent<StatValue>().amount.Value) * 100;
                });

            var roundsPlayed = CreateStat("Rounds played", "Games", "How many rounds you have played",
                gameMenu);
            var roundsWon = CreateStat("Rounds won", "Games", "How many rounds you have won", gameMenu);
            var roundsLost = CreateStat("Rounds lost", "Games", "How many rounds you have lost", gameMenu);
            var roundsWonPercentage = CreateStat("Rounds won percentage", "Games",
                "How many rounds you have won by percentage", gameMenu, 0,
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

            
            var GunMenu = CreateMenu("Gun and player", menu);

            var bulletsShot = CreateStat("Bullets shot", "Gun", "How many bullets you have shot", GunMenu);
            var totalDamage = CreateStat("Total damage", "Gun", "How much damage you have done total", GunMenu);
            CreateStat("Average damage done per shot", "Gun",
                "How much damage you have done average per bullet",
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
            var recievedDamage = CreateStat("Total damage received", "Player",
                "How much damage you have received", GunMenu);

            CreateStat("Blocks", "Block", "How many times you blocked", GunMenu).transform.SetSiblingIndex(3);
            CreateStat("Average damage done per game", "Games",
                "How much damage you have done per game average", GunMenu, 2, () =>
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
            CreateStat("Average damage received per game", "Games",
                "How much damage you have received per game average", GunMenu, 2, () =>
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
        }

        private static GameObject CreateMenu(string name, GameObject parent)
        {
            var menu = MenuHandler.CreateMenu(name, () => { }, parent);
            menu.transform.Find("Group/Grid/Scroll View/Viewport/Content/").GetComponent<VerticalLayoutGroup>().spacing = 50;
            
            var obj = parent.transform.Find("Group/Grid/Scroll View/Viewport/Content/");
            var button = parent.transform.Find("Group/Grid/Scroll View/Viewport/Content/" + name).GetComponent<Button>();
                button.onClick.AddListener(() => {UpdateValues(menu);});
            
            return menu;
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
            parent = parent.transform.Find("Group/Grid/Scroll View/Viewport/Content").gameObject;
            var _statBase = Instantiate(StatBase, parent.transform);
            // ReSharper disable once Unity.IncorrectMonoBehaviourInstantiation
            // ReSharper disable once UnusedVariable
            var statValue = new StatValue(_statBase, Name, Section, Description, RoundDecimals , out var obj);
            StatObjects.Add(obj.GetComponent<StatValue>());
            obj.GetComponent<StatValue>().AddUpdateAction(updateAction);
            return(obj);
        }

        private static void CreateCardMenu(GameObject cards)
        {
            var CardBase = cards.transform.Find("Group/Grid/CardBaseObject");
            foreach (var cardInstance in CardManager.cards.Select(t => t.Value.cardInfo))
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