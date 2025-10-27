using UnityEngine;

namespace CyberLife.Board
{
    [System.Serializable]
    public class EventCard
    {
        public string id;
        public string title;
        public string body;
        [Range(1,100)] public int weight = 10;
        public bool causesCombat;
        public int encounterId = 1;
        public int moneyDelta = 0;
        public int hpDelta = 0;
    }

    // 簡單事件池：你可在 Inspector 編輯/調整權重
    public class EventService : MonoBehaviour
    {
        public EventCard[] deck = new EventCard[]
        {
            new EventCard{ id="FIND_COINS", title="路邊撿到錢", body="+50 Money", weight=25, moneyDelta=+50 },
            new EventCard{ id="FOOD_POISON", title="食物不潔", body="-10 HP", weight=15, hpDelta=-10 },
            new EventCard{ id="STRANGER_HELP", title="神秘幫助", body="+30 Money", weight=20, moneyDelta=+30 },
            new EventCard{ id="AMBUSH", title="埋伏", body="遭遇敵人！", weight=20, causesCombat=true, encounterId=1 },
            new EventCard{ id="BOSS_SCOUT", title="強敵偵查", body="危險的腳步聲...", weight=10, causesCombat=true, encounterId=2 },
            new EventCard{ id="NOTHING", title="風平浪靜", body="什麼都沒發生", weight=10 },
        };

        public EventCard Draw()
        {
            if (deck == null || deck.Length == 0) return new EventCard{ id="NOTHING", title="...", body="..." };
            int total = 0;
            for (int i=0;i<deck.Length;i++) total += Mathf.Max(1, deck[i].weight);
            int r = Random.Range(0, total);
            for (int i=0;i<deck.Length;i++)
            {
                int w = Mathf.Max(1, deck[i].weight);
                if (r < w) return deck[i];
                r -= w;
            }
            return deck[deck.Length-1];
        }
    }
}
