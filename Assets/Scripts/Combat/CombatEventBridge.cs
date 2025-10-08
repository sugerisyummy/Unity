// Assets/Scripts/Combat/CombatEventBridge.cs
using UnityEngine;

namespace CyberLife.Combat
{
    public class CombatEventBridge : MonoBehaviour
    {
        public CombatManager manager;
        public CombatPageController page;
        public CombatResultRouter router; // 有就接，沒有可空

        void Awake() { if (!manager) manager = GetComponent<CombatManager>(); }
        void OnEnable()  { if (manager) manager.OnCombatEnd += Handle; }
        void OnDisable() { if (manager) manager.OnCombatEnd -= Handle; }

        void Handle(CombatOutcome outcome)
        {
            Debug.Log("[Bridge] Combat end -> " + outcome);
            if (router) router.Route(outcome);
            else if (page) page.BackToStory();
        }
    }
}
