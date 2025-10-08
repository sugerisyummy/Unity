// EnemyTargetButton.cs — verbose debug 2025-10-06
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace CyberLife.Combat
{
    [RequireComponent(typeof(RectTransform))]
    public class EnemyTargetButton : MonoBehaviour, IPointerClickHandler
    {
        public CombatUIController ui;
        public Combatant enemy;
        public bool bindExistingButton = true;

        void Awake()
        {
            if (ui == null) ui = FindObjectOfType<CombatUIController>();

            var btn = GetComponent<Button>();
            if (bindExistingButton && btn != null)
            {
                btn.onClick.RemoveListener(OnClickProxy);
                btn.onClick.AddListener(OnClickProxy);
                Debug.Log($"[UI] EnemyTargetButton bound to Button on {name}");
            }
            else
            {
                Debug.Log($"[UI] EnemyTargetButton (no Button) on {name}");
            }
        }

        public void OnPointerClick(PointerEventData eventData) => OnClickProxy();

        private void OnClickProxy()
        {
            if (enemy == null) { Debug.LogWarning($"[UI] EnemyTargetButton: enemy null on {name}"); return; }
            if (ui == null) { Debug.LogWarning($"[UI] EnemyTargetButton: ui null on {name}"); return; }

            Debug.Log($"[UI] EnemyTargetButton click → {enemy.name}");
            ui.SelectTarget(enemy);
        }
    }
}
