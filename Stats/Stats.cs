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

namespace Stats
{
    [BepInDependency("com.willis.rounds.unbound")]
    [BepInPlugin(ModId, ModName, Version)]
    [BepInProcess("Rounds.exe")]
    public class Stats : BaseUnityPlugin
    {
        public enum Value
        {
            Shoots,
            Blocks,
        }

        public static Stats Instance;
        
        private const string ModId = "BossSloth.rounds.Stats";
        private const string ModName = "Stats";
        public const string Version = "0.1.0";

        private readonly ConfigFile customConfig = new ConfigFile(Path.Combine(Paths.ConfigPath, "Stats.cfg"), true);

        private bool firstTime = true;


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
                
                this.ExecuteAfterSeconds(firstTime ? 1f : 0, () =>
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
                    
                    // Create button for cards button
                    _stats.transform.Find("Group/Grid/Cards").GetComponent<Button>().onClick.AddListener(() => ClickCards(_cards));
                    
                    // Create back esc key
                    _stats.transform.Find("Group").GetComponent<GoBack>().target = MainMenuHandler.instance.transform.Find("Canvas/ListSelector/Main").GetComponent<ListMenuPage>();
                    _cards.transform.Find("Group").GetComponent<GoBack>().target = _stats.GetComponent<ListMenuPage>();
                    
                    
                    // Creat back button
                    _stats.transform.Find("Group/Back").GetComponent<Button>().onClick.AddListener(ClickBack(MainMenuHandler.instance.transform.Find("Canvas/ListSelector/Main")));
                    _cards.transform.Find("Group/Back").GetComponent<Button>().onClick.AddListener(ClickBack(_stats.transform));
                });
                
                firstTime = false;

                orig(self);
            };

        }

        private static void UpdateValuesMain(GameObject _stats)
        {
            var shootsText = _stats.transform.Find("Group/Grid/Shoots/Procedural Image/Shoots_Text");
            shootsText.GetComponent<TextMeshProUGUI>().text = shoots.Value.ToString();
            
            var blockText = _stats.transform.Find("Group/Grid/Blocks/Procedural Image/Blocks_Text");
            blockText.GetComponent<TextMeshProUGUI>().text = blocks.Value.ToString();
        }
        
        private static void UpdateValuesCards(GameObject _cards)
        {
            var shootsText = _cards.transform.Find("Group/Grid/Shoots/Procedural Image/Shoots_Text");
            shootsText.GetComponent<TextMeshProUGUI>().text = shoots.Value.ToString();
            
            var blockText = _cards.transform.Find("Group/Grid/Blocks/Procedural Image/Blocks_Text");
            blockText.GetComponent<TextMeshProUGUI>().text = blocks.Value.ToString();
        }

        internal static void UpdateValue(Value value)
        {
            if (value == Value.Shoots)
            {
                shoots.Value++;
            } else if (value == Value.Blocks)
            {
                blocks.Value++;
            }
        }

        private void Update()
        {
        }

        private void Start()
        {
            var harmony = new Harmony(ModId);
            harmony.PatchAll();

            Instance = this;
        }

        private static void CreateCardMenu(GameObject cards)
        {
            var CardBase = cards.transform.Find("Group/Grid/CardBaseObject");
            foreach (var cardInstance in CardChoice.instance.cards)
            {
                var _card = Instantiate(CardBase, cards.transform.Find("Group/Grid/Scroll View/Viewport/Content"));
                _card.Find("CardBase").GetComponent<TextMeshProUGUI>().text = cardInstance.cardName.ToLower();
                _card.Find("Procedural Image/CardBase_Text").GetComponent<TextMeshProUGUI>().text = "10";
                _card.gameObject.SetActive(true);
            }
            
        }
        
        private static void ClickStats(GameObject stats)
        {
            stats.GetComponent<ListMenuPage>().Open();
            UpdateValuesMain(stats);
        }
        
        private static void ClickCards(GameObject cards)
        {
            cards.GetComponent<ListMenuPage>().Open();
            UpdateValuesCards(cards);
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