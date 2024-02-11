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
