using GamemodeManager.GameFile;
using GamemodeManagerAPI.Mods;
using GamemodeManager.Patches;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Object = UnityEngine.GameObject;
using GamemodeManager.Util;
using static UnityEngine.EventSystems.EventTrigger;
using GameNetcodeStuff;

namespace GamemodeManager.UI
{
    internal class GUI
    {
        public static Object OpenNotification(string title, string desc, Action onClick=null)
        {
            var menuContainer = Object.Find("MenuContainer");
            var notification = Object.Instantiate(Plugin.assets.LoadAsset<GameObject>("GamemodeNotification"), menuContainer.transform);
            var notifcationObj = notification.transform.Find("Panel").gameObject;
            notifcationObj.transform.Find("TitleText").GetComponentInChildren<TextMeshProUGUI>().text = title;
            notifcationObj.transform.Find("DescriptionText").GetComponentInChildren<TextMeshProUGUI>().text = desc;
            notifcationObj.transform.Find("Close").GetComponent<Button>().onClick.AddListener(() =>
            {
                onClick?.Invoke();
                notification.SetActive(false);
                Object.Destroy(notification.gameObject);
            });
            notification.SetActive(true);
            return notification;
        }

        public static void PromptNotification(string title, string placeholder, Action<string> onSubmit)
        {
            var menuContainer = Object.Find("MenuContainer");
            var notification = Object.Instantiate(Plugin.assets.LoadAsset<GameObject>("GamemodeInputNotification"), menuContainer.transform);
            var notifcationObj = notification.transform.Find("Panel").gameObject;
            notifcationObj.transform.Find("DescriptionText").GetComponentInChildren<TextMeshProUGUI>().text = title;
            notifcationObj.transform.Find("Value").Find("Text Area").Find("Placeholder").GetComponent<TextMeshProUGUI>().text = placeholder;
            notifcationObj.transform.Find("Ok").GetComponent<Button>().onClick.AddListener(() =>
            {
                onSubmit.Invoke(notifcationObj.transform.Find("Value").GetComponent<TMP_InputField>().text);
                notification.SetActive(false);
                Object.Destroy(notification.gameObject);
            });
            notifcationObj.transform.Find("Cancel").GetComponent<Button>().onClick.AddListener(() =>
            {
                onSubmit.Invoke("");
                notification.SetActive(false);
                Object.Destroy(notification.gameObject);
            });

            notification.SetActive(true);
        }
        public static void AddSpawnUIFunctionality(Transform spawnUI)
        {
            EnemyType selected = null;
            var container = spawnUI.Find("Panel");
            var closeBtn = container.Find("Close").GetComponent<Button>();
            var spawn = container.Find("Spawn").GetComponent<Button>();
            var randomEnemy = container.Find("Random").GetComponent<Button>();

            var spawnAmount = container.Find("Amount").GetComponent<TMP_InputField>();
            var spawnLocationType = container.Find("LocationType").GetComponent<TMP_Dropdown>();
            var x = container.Find("X").GetComponent<TMP_InputField>();
            var y = container.Find("Y").GetComponent<TMP_InputField>();
            var z = container.Find("Z").GetComponent<TMP_InputField>();
            var playerToSpawnOn = container.Find("TeleportToPlayer").GetComponent<TMP_Dropdown>();

            // Add all entites
            var content = container.Find("MonsterList").Find("Viewport").Find("Content");
            var entites = EnemyUtil.GetAllEnemies();
            entites.ForEach((enemy) =>
            {
                var EnemyName = enemy.Name;
                var EnemyType = enemy.Type;
                Sprite attemptedSprite = Plugin.assets.LoadAsset<Sprite>(EnemyName.ToLower() + ".png");
                Plugin.Log.LogInfo(EnemyName.ToLower() + ".png");

                var clone = GameObject.Instantiate(content.Find("MonsterEx"), content);
                if (attemptedSprite != null)
                {
                    clone.Find("BG").GetComponent<Image>().sprite = attemptedSprite;

                }
                clone.Find("Text (TMP)").GetComponent<TextMeshProUGUI>().text = EnemyName;
                clone.gameObject.SetActive(true);

                clone.GetComponent<Button>().onClick.AddListener(() =>
                {
                    selected = EnemyType;
                });
            });

            randomEnemy.onClick.AddListener(() =>
            {
                selected = entites[UnityEngine.Random.Range(0, entites.Count)].Type;
            });
            
            var allPlayers = StartOfRound.Instance.allPlayerScripts;
            for (int i = 0; i < allPlayers.Length; i++)
            {
                var plr = allPlayers[i];

                List<TMP_Dropdown.OptionData> list = new List<TMP_Dropdown.OptionData>
                {
                    new TMP_Dropdown.OptionData(plr.playerUsername)
                };
                playerToSpawnOn.AddOptions(list);
            }
            spawnLocationType.onValueChanged.AddListener((val) =>
            {
                if (val == 0)
                {
                    x.gameObject.SetActive(true);
                    y.gameObject.SetActive(true);
                    z.gameObject.SetActive(true);
                    playerToSpawnOn.gameObject.SetActive(false);
                } else if (val == 1)
                {
                    x.gameObject.SetActive(false);
                    y.gameObject.SetActive(false);
                    z.gameObject.SetActive(false);
                    playerToSpawnOn.gameObject.SetActive(true);
                } else
                {
                    x.gameObject.SetActive(false);
                    y.gameObject.SetActive(false);
                    z.gameObject.SetActive(false);
                    playerToSpawnOn.gameObject.SetActive(false);
                }
            });
            closeBtn.onClick.AddListener(() =>
            {
                GameObject.Destroy(spawnUI.gameObject);
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            });
            spawn.onClick.AddListener(() =>
            {
                //                                        This is only inside enemies
                // RoundManagerPatch.roundManagerInstance.SpawnEnemyOnServer(RoundManagerPatch.roundManagerInstance.playersManager.localPlayerController.gameObject.transform.position, 0, int.Parse(enemyID.text));

                Transform pos = RoundManager.Instance.playersManager.localPlayerController.transform;

                switch (spawnLocationType.value)
                {
                    case 0:
                        {
                            // Coordinate
                            pos.position = new Vector3(float.Parse(x.text), float.Parse(y.text), float.Parse(z.text));
                            break;
                        }
                    case 1:
                        {
                            // Player
                            var playerIdx = playerToSpawnOn.value;
                            PlayerControllerB player = null;
                            for (int i = 0; i < StartOfRound.Instance.allPlayerScripts.Length; i++)
                            {
                                var plr = StartOfRound.Instance.allPlayerScripts[i];
                                if (plr.playerUsername == playerToSpawnOn.options[playerIdx].text)
                                {
                                    player = plr;
                                }
                            }

                            if (!player)
                            {
                                HUDManager.Instance.DisplayTip("Whoops", "Something went wrong finding player. Spawning on local player.");
                                break;
                            }

                            pos = player.transform;

                            break;
                        }
                    case 2:
                        {
                            // Ship
                            var ship = GameObject.Find("ShipInside").transform;
                            pos = ship;
                            break;
                        }
                    case 3:
                        {

                            // Main inside
                            break;
                        }
                    case 4:
                        {
                            // Main outside
                            break;
                        }
                    case 5:
                        {
                            // Drop ship
                            break;
                        }


                    default:
                        break;
                }

                for (int i = 0; i < int.Parse(spawnAmount.text); i++)
                {
                    EnemyUtil.SpawnEnemy(selected, pos);
                }
                

                GameObject.Destroy(spawnUI.gameObject);
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            });
        }
        public static void AddGUIInteractivity_GamemodeEdit(Transform edit, Dictionary<string, object> data)
        {
            var panel = edit.Find("Panel");

            var allowedMods = JsonConvert.DeserializeObject<List<string>>((string)data["allowedMods"]);

            //TODO

            #region Add mods
            var allAddedMods = new List<string>();
            var allInstalledMods = ModManager.mods;
            var panel__ = panel.Find("Mods").Find("allMods").Find("Viewport").Find("Content");


            #region Remove mod objs
            for (int i = 0; i < panel__.childCount; i++)
            {
                var child = panel__.GetChild(i);
                if (child.name == "Mod_EX")
                {
                    continue;
                }
                Object.Destroy(child.gameObject);
            }
            #endregion

            for (int i = 0; i < allInstalledMods.Count; i++)
            {
                var ex = Object.Instantiate(panel__.Find("Mod_EX"), panel__);
                var mod = allInstalledMods[i];
                if (allowedMods.Contains(mod.Info.Metadata.GUID))
                {
                    ex.Find("Selected").GetComponent<Toggle>().isOn = true;
                }
                Plugin.Log.LogInfo(mod.Info.Metadata.GUID + " found");
                ex.Find("By").GetComponent<TextMeshProUGUI>().text = mod.Info.Metadata.GUID.Split('.')[0];
                ex.Find("Name").GetComponent<TextMeshProUGUI>().text = mod.Info.Metadata.Name;
                ex.Find("Selected").GetComponent<Toggle>().onValueChanged.AddListener((value) => {
                    var allowed = JsonConvert.DeserializeObject<List<string>>((string)data["allowedMods"]);
                    if (value)
                        allowed.Add(mod.Info.Metadata.GUID);
                    else
                        allowed.Remove(mod.Info.Metadata.GUID);
                    data["allowedMods"] = JsonConvert.SerializeObject(allowed);
                });
                allAddedMods.Add(mod.Info.Metadata.GUID);
                ex.gameObject.SetActive(true);
            }
            #endregion

            var copyGUID = panel.Find("copy");
            copyGUID.GetComponent<Button>().onClick.RemoveAllListeners();
            copyGUID.GetComponent<Button>().onClick.AddListener(() => {
                GUIUtility.systemCopyBuffer = (string)data["GUID"];
                OpenNotification("Success", "GUID Copied to clipboard.");
            });

            var apply = panel.Find("Apply");
            apply.GetComponent<Button>().interactable = true;
            apply.GetComponent<Button>().onClick.RemoveAllListeners();
            apply.GetComponent<Button>().onClick.AddListener(() => {
                GameModeFile.RemoveGamemode((string)data["GUID"]);
                var guid = data["GUID"];
                var newdata = data;
                if (newdata.ContainsKey("GUID")) newdata.Remove("GUID");
                GameModeFile.CreateGamemodeRaw((string)guid, newdata);
                OpenNotification("Success", "Settings applied! Close and reopen to apply again.");
                apply.GetComponent<Button>().interactable = false;
            });

        }

        public static Object OpenConfirmation(string title, string desc, string okBtn, string cancelBtn, Action<Object> onOk=null, Action<Object> onCancel=null)
        {
            var menuContainer = Object.Find("MenuContainer");
            var notification = Object.Instantiate(Plugin.assets.LoadAsset<GameObject>("GamemodeNotification"), menuContainer.transform);
            var notifcationObj = notification.transform.Find("Panel").gameObject;
            notifcationObj.transform.Find("TitleText").GetComponentInChildren<TextMeshProUGUI>().text = title;
            notifcationObj.transform.Find("DescriptionText").GetComponentInChildren<TextMeshProUGUI>().text = desc;

            notifcationObj.transform.Find("Close").gameObject.SetActive(false);

            notifcationObj.transform.Find("Ok").gameObject.SetActive(true);
            notifcationObj.transform.Find("Ok").GetComponentInChildren<TextMeshProUGUI>().text = okBtn;

            notifcationObj.transform.Find("Cancel").gameObject.SetActive(true);
            notifcationObj.transform.Find("Cancel").GetComponentInChildren<TextMeshProUGUI>().text = cancelBtn;

            notifcationObj.transform.Find("Ok").GetComponent<Button>().onClick.AddListener(() =>
            {
                onOk?.Invoke(notifcationObj);
                notification.SetActive(false);
                Object.Destroy(notification.gameObject);
            });
            notifcationObj.transform.Find("Cancel").GetComponent<Button>().onClick.AddListener(() =>
            {
                onCancel?.Invoke(notifcationObj);
                notification.SetActive(false);
                Object.Destroy(notification.gameObject);
            });
            notification.SetActive(true);
            return notification;
        }
    }
}
