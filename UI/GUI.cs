using GamemodeManager.GameFile;
using GamemodeManager.Mods;
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

        public static void AddGUIInteractivity_GamemodeEdit(Transform edit, Dictionary<string, string> data)
        {
            var panel = edit.Find("Panel");

            var allowedMods = JsonConvert.DeserializeObject<List<string>>(data["allowedMods"]);

            //TODO

            #region Add mods
            var allAddedMods = new List<string>();
            var allInstalledMods = MenuPatch.allInstalledMods;
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
                if (!ModManager.mods.Contains(mod.Instance) || mod.Metadata.GUID == "funfoxrr.GamemodeManager" || allAddedMods.Contains(mod.Metadata.GUID)) { continue; }
                if (allowedMods.Contains(mod.Metadata.GUID))
                {
                    ex.Find("Selected").GetComponent<Toggle>().isOn = true;
                }
                Plugin.Log.LogInfo(mod.Metadata.GUID + " found");
                ex.Find("By").GetComponent<TextMeshProUGUI>().text = mod.Metadata.GUID.Split('.')[0];
                ex.Find("Name").GetComponent<TextMeshProUGUI>().text = mod.Metadata.Name;
                ex.Find("Selected").GetComponent<Toggle>().onValueChanged.AddListener((value) => {
                    var allowed = JsonConvert.DeserializeObject<List<string>>(data["allowedMods"]);
                    if (value)
                        allowed.Add(mod.Metadata.GUID);
                    else
                        allowed.Remove(mod.Metadata.GUID);
                    data["allowedMods"] = JsonConvert.SerializeObject(allowed);
                });
                allAddedMods.Add(mod.Metadata.GUID);
                ex.gameObject.SetActive(true);
            }
            #endregion

            var copyGUID = panel.Find("copy");
            copyGUID.GetComponent<Button>().onClick.RemoveAllListeners();
            copyGUID.GetComponent<Button>().onClick.AddListener(() => {
                GUIUtility.systemCopyBuffer = data["GUID"];
                OpenNotification("Success", "GUID Copied to clipboard.");
            });

            var apply = panel.Find("Apply");
            apply.GetComponent<Button>().interactable = true;
            apply.GetComponent<Button>().onClick.RemoveAllListeners();
            apply.GetComponent<Button>().onClick.AddListener(() => {
                GameModeFile.RemoveGamemode(data["GUID"]);
                var guid = data["GUID"];
                var newdata = data;
                if (newdata.ContainsKey("GUID")) newdata.Remove("GUID");
                GameModeFile.CreateGamemodeRaw(guid, newdata);
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
