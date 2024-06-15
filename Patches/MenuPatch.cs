using BepInEx.Bootstrap;
using GamemodeManager.GameFile;
using GamemodeManager.NotStolenUtils;
using HarmonyLib;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Object = UnityEngine.GameObject;
using Newtonsoft.Json;
using GamemodeManagerAPI.Mods;
using GamemodeManager.UI;
using Steamworks;
using GUI = GamemodeManager.UI.GUI;

namespace GamemodeManager.Patches
{
    [HarmonyPatch(typeof(MenuManager))]
    internal class MenuPatch
    {
        static Object panel__gamemodes;
        static Object panel__host;
        static Object panel__gamemodecreate;
        static Object panel__edit;

        public static Object panel__spawn;

        static List<string> selectedGamemodes = new List<string>();
        static Dictionary<string, Object> gamemodesUI = new Dictionary<string, Object>();
        static MenuManager instance_;
        static Object lastGamemodeObj;

        public static List<BepInEx.PluginInfo> allInstalledMods = new List<BepInEx.PluginInfo>();
        public static List<string> allInstalledGUIDS = new List<string> ();

        static List<string> allowedMods = new List<string>();

        public static UnityAction<Gamemode> selectedNewGamemode;

        public static string selectedGamemode = "";

        [HarmonyPostfix]
        [HarmonyPatch("Start")]
        private static void StartPatch(MenuManager __instance)
        {
            Plugin.Log.LogInfo("WOAH! it's a menu :)");

            StartOfRoundPatch.localgamemode = new Gamemode("", false);

            foreach (var plugin in Chainloader.PluginInfos)
            {
                var val = plugin.Value;
                allInstalledMods.Add(val);
                allInstalledGUIDS.Add(val.Metadata.GUID);
            }
            InjectButton("Gamemodes", () =>
            {
                AddAllGamemodes();
                panel__gamemodes.gameObject.SetActive(true);
                Plugin.Log.LogInfo("A");
            });
            instance_ = __instance;
            __instance.StartCoroutine(DelayedMainMenuInjection());

            
        }

        public static BepInEx.PluginInfo getPluginByGUID(string guid)
        {
            return Chainloader.PluginInfos[guid];
        }

        private static IEnumerator DelayedMainMenuInjection()
        {
            yield return new WaitForSeconds(0);
            CreateUI();
        }

        private static void InjectButton(string text, UnityAction click)
        {
            var menuContainer = Object.Find("MenuContainer");
            var mainButtonsTransform = menuContainer?.transform.Find("MainButtons");
            var quitButton = mainButtonsTransform?.Find("QuitButton")?.gameObject;

            if (menuContainer == null || mainButtonsTransform == null || quitButton == null) return;

            LethalConfig__MenuUtil.InjectMenu(menuContainer.transform, mainButtonsTransform, quitButton, text, click);
        }

        private static void CreateUI()
        {
            var menuContainer = Object.Find("MenuContainer");
            if (menuContainer == null)
            {
                Plugin.Log.LogWarning("Couldn't find the menu :(");
                return;
            }
            var create = Plugin.assets.LoadAsset<Object>("GamemodeCreate");
            panel__gamemodecreate = Object.Instantiate(create, menuContainer.transform);
            UIFunctions_Create();
            panel__gamemodecreate.SetActive(false);
            var edit = Plugin.assets.LoadAsset<Object>("GamemodeEditPanel");
            panel__edit = Object.Instantiate(edit, menuContainer.transform);
            UIEdit_Create();
            panel__edit.SetActive(false);
            var panel = Plugin.assets.LoadAsset<GameObject>("GamemodesPanel");
            panel__gamemodes = Object.Instantiate(panel, menuContainer.transform);
            CreateUIFunctionality();
            panel__gamemodes.SetActive(false);
            var spawn = Plugin.assets.LoadAsset<GameObject>("SpawnGUI");
            spawn.SetActive(false);
            panel__spawn = spawn;

            var sidePanel_host = Plugin.assets.LoadAsset<Object>("GamemodeHostPanel");
            panel__host = Object.Instantiate(sidePanel_host, menuContainer.transform.Find("LobbyHostSettings"));
            panel__host.GetComponent<RectTransform>().localPosition = new Vector3(-250.8683f, -7.29f, 2.9804f);
            panel__host.transform.Find("AllGamemodes").Find("Viewport").Find("Content").Find("GamemodeEX").gameObject.SetActive(false);
            panel__host.SetActive(true);

            

            if (menuContainer.transform.Find("MainButtons") != null)
            {
                menuContainer.transform.Find("MainButtons").Find("HostButton").GetComponent<Button>().onClick.AddListener(() =>
                {
                    var saves = menuContainer.transform.Find("LobbyHostSettings").Find("FilesPanel");

                    saves.gameObject.SetActive(true);
                    selectedGamemode = "";
                    panel__host.SetActive(GameModeFile.InstalledGamemodeLength() != 0);
                    if (panel__host.activeSelf)
                    {
                        var content = panel__host.transform.Find("AllGamemodes").Find("Viewport").Find("Content");
                        // TODO: LOAD GAMEMODES!

                        for (int i = 0; i < content.childCount; i++)
                        {
                            var child = content.GetChild(i);
                            if (child.name == "GamemodeEX")
                            {
                                continue;
                            }
                            Object.Destroy(child.gameObject);
                        }
                        for (int i = 0; i < GameModeFile.GetAllGamemodes().Count; i++)
                        {
                            string _gamemode = GameModeFile.GetAllGamemodes()[i];
                            var gdata = GameModeFile.GetGamemode(_gamemode);
                            var allMods = JsonConvert.DeserializeObject<List<string>>((string)gdata.gamemode.data["allowedMods"]);
                            
                            var gamemode = Object.Instantiate(content.Find("GamemodeEX"), content);
                            for (int ii = 0; ii < allMods.Count; ii++)
                            {
                                var mod = allMods[ii];
                                if (!allInstalledGUIDS.Contains(mod))
                                {
                                    Plugin.Log.LogWarning("WARNING! Gamemode: " + _gamemode + " has a non-downloaded mod. MOD GUID: " + mod);
                                    Object.Destroy(gamemode.gameObject);
                                }
                            }
                            gamemode.name = _gamemode;
                            gamemode.Find("name").GetComponent<TextMeshProUGUI>().text = _gamemode;
                            gamemode.gameObject.SetActive(true);

                            //TODO: Use gamemode
                            gamemode.Find("use").GetComponent<Button>().onClick.AddListener(() =>
                            {

                                if (selectedGamemode == _gamemode)
                                {
                                    //Selected
                                    gamemode.GetComponent<Image>().color = new Color(0.5283019f, 0, 0, 1);
                                    selectedNewGamemode.Invoke(new Gamemode("", false));
                                    content.Find(selectedGamemode).Find("use").GetComponentInChildren<TextMeshProUGUI>().text = "Use";
                                    saves.gameObject.SetActive(true);
                                    selectedGamemode = "";

                                }
                                else
                                {
                                    // Not selected yet
                                    var clr = new Color(1, 0.0519f, 0, 0.0353f);
                                    saves.gameObject.SetActive(false);

                                    if (selectedGamemode != "")
                                    {
                                        content.Find(selectedGamemode).GetComponent<Image>().color = new Color(0.5283019f, 0, 0, 1);
                                        content.Find(selectedGamemode).Find("use").GetComponentInChildren<TextMeshProUGUI>().text = "Use";
                                    }

                                    gamemode.GetComponent<Image>().color = new Color(0.2329722f, 0.3962264f, 0, 1);
                                    Gamemode gamemode__ = GameModeFile.GetGamemode(_gamemode).gamemode;
                                    gamemode__.active = true;
                                    selectedNewGamemode.Invoke(gamemode__);
                                    gamemode.Find("use").GetComponentInChildren<TextMeshProUGUI>().text = "Unuse";
                                    selectedGamemode = _gamemode;
                                }

                            });
                        }
                    }
                });
            }


            var allAddedMods = new List<string>();
            var panel__ = panel__gamemodecreate.transform.Find("Panel").Find("SidePanel").Find("allMods").Find("Viewport").Find("Content");
            for (int i = 0; i < ModManager.mods.Count; i++)
            {
                var ex = Object.Instantiate(panel__.Find("Mod_EX"), panel__);
                var mod = ModManager.mods[i];
                if (allAddedMods.Contains(mod.Info.Metadata.GUID)) { continue; }
                Plugin.Log.LogInfo(mod.Info.Metadata.GUID + " found");
                ex.Find("By").GetComponent<TextMeshProUGUI>().text = mod.Info.Metadata.GUID.Split('.')[0];
                ex.Find("Name").GetComponent<TextMeshProUGUI>().text = mod.Info.Metadata.Name;
                ex.Find("Selected").GetComponent<Toggle>().onValueChanged.AddListener((value) => {
                    if (value)
                        allowedMods.Add(mod.Info.Metadata.GUID);
                    else
                        allowedMods.Remove(mod.Info.Metadata.GUID);
                });
                allAddedMods.Add(mod.Info.Metadata.GUID);
                ex.gameObject.SetActive(true);
            }
        }

        

        static IEnumerator closeGUI()
        {
            yield return new WaitForSeconds(1f);
            Object.Destroy(lastGamemodeObj);
        }

        private static void AddAllGamemodes()
        {
            var panel = panel__gamemodes.transform.Find("Panel");
            var scroll = panel.Find("AllGamemodes");
            var content = scroll.Find("Viewport").Find("Content");
            scroll.Find("NothingFound").gameObject.SetActive(true);
            for (int i = 0; i < content.childCount; i++)
            {
                var child = content.GetChild(i);
                if (child.name == "Gamemode_EX")
                {
                    continue;
                }
                Object.Destroy(child.gameObject);
            }
            for (int i = 0; i < GameModeFile.GetAllGamemodes().Count; i++)
            {
                scroll.Find("NothingFound").gameObject.SetActive(false);
                string _gamemode = GameModeFile.GetAllGamemodes()[i];
                var gamemode = Object.Instantiate(content.Find("Gamemode_EX"), content);

                var gdata = GameModeFile.GetGamemode(_gamemode);
                var allMods = JsonConvert.DeserializeObject<List<string>>((string)gdata.gamemode.data["allowedMods"]);
                for (int ii = 0; ii < allMods.Count; ii++)
                {
                    var mod = allMods[ii];
                    if (!allInstalledGUIDS.Contains(mod))
                    {
                        gamemode.Find("Problem").gameObject.SetActive(true);
                        gamemode.Find("Problem").GetComponent<Button>().onClick.AddListener(() =>
                        {
                            GUI.OpenNotification("Error", "Some mods enabled aren't installed.");
                        });
                    }
                }

                gamemode.name = "Gamemode_" + i;
                gamemode.Find("Name").GetComponent<TextMeshProUGUI>().text = _gamemode.Split('.')[1];
                gamemode.Find("By").GetComponent<TextMeshProUGUI>().text = "By: " + _gamemode.Split('.')[0];
                gamemode.gameObject.SetActive(true);
                gamemode.Find("Edit").GetComponent<Button>().onClick.AddListener(() =>
                {
                    //Plugin.Log.LogInfo("edit gamemode: " + _gamemode);
                    panel__edit.transform.Find("Panel").Find("TitleText").GetComponentInChildren<TextMeshProUGUI>().text = _gamemode;

                    Dictionary<string, object> datatata = GameModeFile.GetGamemode(_gamemode).results;
                    datatata.Add("GUID", _gamemode);

                    GUI.AddGUIInteractivity_GamemodeEdit(panel__edit.transform, datatata);


                    panel__gamemodes.SetActive(false);
                    panel__edit.SetActive(true);
                });
                gamemode.Find("Remove").GetComponent<Button>().onClick.AddListener(() =>
                {
                    GameModeFile.RemoveGamemode(_gamemode);
                    gamemode.GetComponent<Animator>().Play("CloseAnim");

                    lastGamemodeObj = gamemode.gameObject;
                    instance_.StartCoroutine(closeGUI());

                    Object.Destroy(gamemode);
                    if (content.childCount == 0)
                    {
                        content.parent.parent.Find("NothingFound").gameObject.SetActive(true);
                    }
                    gamemodesUI.Remove(_gamemode);
                    selectedGamemodes.Remove(_gamemode);
                });
                gamemode.Find("Clone").GetComponent<Button>().onClick.AddListener(() =>
                {
                    Plugin.Log.LogInfo("clone gamemode: " + _gamemode);
                    
                });
                gamemode.Find("Selected").GetComponent<Toggle>().onValueChanged.AddListener((bool val) =>
                {
                    if (val)
                    {
                        selectedGamemodes.Add(_gamemode);
                        gamemodesUI.Add(_gamemode, gamemode.gameObject);
                    }
                    else
                    {
                        selectedGamemodes.Remove(_gamemode);
                        gamemodesUI.Remove(_gamemode);
                    }
                });
            }
        }

        private static void UIEdit_Create()
        {
            var panel = panel__edit.transform.Find("Panel");
            panel.Find("Close").GetComponent<Button>().onClick.AddListener(() =>
            {
                GUI.OpenConfirmation("Discard", "Remove ALL unsaved changes?", "Yes", "Keep editing", (obj) =>
                {
                    panel__edit.SetActive(false);
                    panel__gamemodes.SetActive(true);
                });
                
            });
        }

        private static void CreateUIFunctionality()
        {
            var panel = panel__gamemodes.transform.Find("Panel");
            panel.transform.Find("Close").GetComponent<Button>().onClick.AddListener(() =>
            {
                panel__gamemodes.SetActive(false);
            });
            panel.transform.Find("Create").GetComponent<Button>().onClick.AddListener(() =>
            {
                //HUDManager.FillImageWithSteamProfile(panel__gamemodecreate.transform.Find("Panel").Find("RawImage").GetComponent<RawImage>(), SteamClient.SteamId);
                panel__gamemodes.SetActive(false);
                panel__gamemodecreate.SetActive(true);
                //GameModeFile.CreateGamemode("funfoxrr", "gamemode", new Dictionary<string, string>());
            });
        }
    
        private static void resetUI_Create()
        {
            var panel = panel__gamemodecreate.transform.Find("Panel");
            var modName = panel.Find("ModeName").GetComponent<TMP_InputField>();

            var side = panel.Find("SidePanel");

            var sharedEdit = side.Find("allowEdit").GetComponent<Toggle>();
            var cloning = side.Find("allowCloning").GetComponent<Toggle>();

            modName.text = "";
            sharedEdit.isOn = true;
            cloning.isOn = true;
        }


        private static void UIFunctions_Create()
        {
            var panel = panel__gamemodecreate.transform.Find("Panel");
            panel.Find("Close").GetComponent<Button>().onClick.AddListener(() =>
            {
                resetUI_Create();

                AddAllGamemodes();
                panel__gamemodes.SetActive(true);
                panel__gamemodecreate.SetActive(false);
            });
            panel.Find("Create").GetComponent<Button>().onClick.AddListener(() =>
            {
                var modName = panel.Find("ModeName").GetComponent<TMP_InputField>().text;

                var side = panel.Find("SidePanel");

                var sharedEdit = side.Find("allowEdit").GetComponent<Toggle>().isOn;
                var cloning = side.Find("allowCloning").GetComponent<Toggle>().isOn;
                var forceOne = side.Find("forceOneMoon").GetComponent<Toggle>().isOn;
                var data = new Dictionary<string, object>();
                data.Add("sharedEdit", sharedEdit.ToString());
                data.Add("cloning", cloning.ToString());
                data.Add("allowedMods", JsonConvert.SerializeObject(allowedMods));
                data.Add("forceOneMoon", forceOne.ToString());
                var author = "";
                if (SteamClient.IsValid == false)
                {
                    GUI.PromptNotification("Enter your name.", "Author name...", (res) =>
                    {
                        author = res;
                        var keepGoing_ = true;
                        if (modName.Contains(" "))
                        {
                            GUI.OpenNotification("Error", "Your name and mod name must not have spaces.");
                            return;
                        }
                        if (modName == "")
                        {
                            GUI.OpenNotification("Error", "A mod name is required.");
                            return;
                        }
                        Plugin.Log.LogInfo(JsonConvert.SerializeObject(allowedMods) + "  " + allowedMods.Count);
                        if (allowedMods.Count <= 0)
                        {
                            keepGoing_ = false;

                            GUI.OpenConfirmation("Hey", "You dont have any mods selected. Make a vanilla game?", "Yes", "No", (o) =>
                            {
                                GameModeFile.CreateGamemode(author, modName, data);

                                var scroll1 = side.Find("allMods").Find("Viewport").Find("Content");
                                for (int i = 0; i < scroll1.childCount; i++)
                                {
                                    var child = scroll1.GetChild(i);
                                    if (child.name == "Mod_EX")
                                    {
                                        continue;
                                    }
                                    child.Find("Selected").GetComponent<Toggle>().isOn = false;
                                }

                                resetUI_Create();

                                AddAllGamemodes();
                                panel__gamemodes.SetActive(true);
                                panel__gamemodecreate.SetActive(false);
                            });
                        }
                        if (!keepGoing_) { return; }

                        allowedMods.Clear();

                        var scroll_ = side.Find("allMods").Find("Viewport").Find("Content");
                        for (int i = 0; i < scroll_.childCount; i++)
                        {
                            var child = scroll_.GetChild(i);
                            if (child.name == "Mod_EX")
                            {
                                continue;
                            }
                            child.Find("Selected").GetComponent<Toggle>().isOn = false;
                        }

                        GameModeFile.CreateGamemode(author, modName, data);

                        resetUI_Create();

                        GUI.OpenNotification("Success", "Your gamemode, " + modName + ", has been created!", () => {
                            AddAllGamemodes();
                            panel__gamemodes.SetActive(true);
                            panel__gamemodecreate.SetActive(false);
                        });
                    });
                    return;
                }
                author = SteamClient.Name.Replace(" ", "");
                

                bool keepGoing = true;
                if (modName.Contains(" ")) {
                    GUI.OpenNotification("Error", "Your name and mod name must not have spaces.");
                    return;
                }
                if (modName == "")
                {
                    GUI.OpenNotification("Error", "A mod name is required.");
                    return;
                }
                Plugin.Log.LogInfo(JsonConvert.SerializeObject(allowedMods) + "  " + allowedMods.Count);
                if (allowedMods.Count <= 0)
                {
                    keepGoing = false;

                    GUI.OpenConfirmation("Hey", "You dont have any mods selected. Make a vanilla game?", "Yes", "No", (o) =>
                    {
                        GameModeFile.CreateGamemode(author, modName, data);

                        var scroll1 = side.Find("allMods").Find("Viewport").Find("Content");
                        for (int i = 0; i < scroll1.childCount; i++)
                        {
                            var child = scroll1.GetChild(i);
                            if (child.name == "Mod_EX")
                            {
                                continue;
                            }
                            child.Find("Selected").GetComponent<Toggle>().isOn = false;
                        }

                        resetUI_Create();

                        AddAllGamemodes();
                        panel__gamemodes.SetActive(true);
                        panel__gamemodecreate.SetActive(false);
                    });
                }
                if (!keepGoing) { return; }

                allowedMods.Clear();

                var scroll = side.Find("allMods").Find("Viewport").Find("Content");
                for (int i = 0; i < scroll.childCount; i++)
                {
                    var child = scroll.GetChild(i);
                    if (child.name == "Mod_EX")
                    {
                        continue;
                    }
                    child.Find("Selected").GetComponent<Toggle>().isOn = false;
                }

                GameModeFile.CreateGamemode(author, modName, data);

                resetUI_Create();

                GUI.OpenNotification("Success", "Your gamemode, " + modName + ", has been created!", () => {
                    AddAllGamemodes();
                    panel__gamemodes.SetActive(true);
                    panel__gamemodecreate.SetActive(false);
                });
            });
        }
    }
}
