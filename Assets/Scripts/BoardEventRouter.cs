using UnityEngine;
using UnityEngine.UI;
using CyberLife.Board;

namespace CyberLife.UI
{
    public class BoardEventRouter : MonoBehaviour
    {
        [Header("Refs")]
        public GameObject eventPanel;  // BoardPanel/UI/EventsPanel
        public Text title;
        public Text body;
        public Button okButton;        // 回棋盤
        public Button fightButton;     // 進戰鬥

        [Header("Services")]
        public EventService eventService;
        public BoardEventsBridge bridge;  // 取得 onRequestCombat()

        [Header("Player (可空)")]
        public PlayerState player;

        // 暫存本次抽到的卡
        private EventCard current;

        void Awake()
        {
            Hide();
            if (okButton) okButton.onClick.AddListener(CloseAndReturnToBoard);
            if (fightButton) fightButton.onClick.AddListener(GoFight);
        }

        // 給 Board 上的棋子落地呼叫：抽卡 + 顯示
        public void DrawAndShow()
        {
            if (!eventService) eventService = FindObjectOfType<EventService>();
            current = (eventService ? eventService.Draw() : new EventCard{ id="NOTHING", title="...", body="..." });

            // 套用數值效果（非戰鬥）
            if (!current.causesCombat && player)
            {
                player.money += current.moneyDelta;
                player.hp     += current.hpDelta;
            }

            ShowCard(current);
        }

        public void ShowCard(EventCard card)
        {
            if (!eventPanel) return;
            if (title) title.text = card.title;
            if (body)
            {
                string extra = "";
                if (card.moneyDelta != 0) extra += (card.moneyDelta>0? " +" : " ") + card.moneyDelta + " Money";
                if (card.hpDelta != 0)    extra += (extra==""? "" : "，") + (card.hpDelta>0? " +" : " ") + card.hpDelta + " HP";
                body.text = string.IsNullOrEmpty(extra) ? card.body : (card.body + "\n" + extra);
            }
            if (fightButton) fightButton.gameObject.SetActive(card.causesCombat);
            if (okButton)    okButton.gameObject.SetActive(!card.causesCombat);
            eventPanel.SetActive(true);
        }

        // 戰鬥 → 回事件（顯示勝敗），讓玩家按「返回棋盤」
        public void ShowCombatResult(bool win)
        {
            if (!eventPanel) return;
            if (title) title.text = win ? "勝利" : "敗北";
            if (body)  body.text  = win ? "你擊退了敵人。" : "你被打趴了…";
            if (fightButton) fightButton.gameObject.SetActive(false);
            if (okButton)    okButton.gameObject.SetActive(true);
            eventPanel.SetActive(true);
        }

        public void Hide()
        {
            if (eventPanel) eventPanel.SetActive(false);
        }

        public void CloseAndReturnToBoard()
        {
            Hide();
            // 交給 MenuManager 統一處理
            var mm = FindObjectOfType<MenuManager>();
            if (mm) mm.ReturnToBoard();
            else
            {
                // 備援：直接開 BoardPanel
                var boardPanel = GameObject.Find("Canvas/BoardPanel");
                if (boardPanel) boardPanel.SetActive(true);
            }
        }

        void GoFight()
        {
            if (current == null || !current.causesCombat) return;
            Hide();
            if (bridge) bridge.onRequestCombat?.Invoke(current.encounterId);
        }
    }
}
