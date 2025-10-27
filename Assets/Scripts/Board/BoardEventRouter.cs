using UnityEngine;
using UnityEngine.UI;
using CyberLife.Board;

namespace CyberLife.UI
{
    public class BoardEventRouter : MonoBehaviour
    {
        [Header("Refs")]
        public GameObject eventPanel; // 放在 BoardPanel/UI/EventsPanel 之類
        public Text title;
        public Text body;
        public Button okButton;

        [Header("Money Hook (可空)")]
        public PlayerState player;

        void Awake()
        {
            Hide();
            if (okButton) okButton.onClick.AddListener(Hide);
        }

        public void ShowSimple(string key)
        {
            if (!eventPanel) return;
            // 超簡單映射，可換 ScriptableObject 或表格
            string t = "事件";
            string b = "你踩到 " + key + "，+100 金錢";
            if (title) title.text = t;
            if (body)  body.text  = b;
            if (player) player.money += 100;
            eventPanel.SetActive(true);
        }

        public void Hide()
        {
            if (eventPanel) eventPanel.SetActive(false);
        }
    }
}
