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
            if (enemy == null) enemy = GetComponent<Combatant>();

            if (bindExistingButton)
            {
                var btn = GetComponent<Button>();
                if (btn != null)
                {
                    btn.onClick.RemoveListener(OnClickProxy);
                    btn.onClick.AddListener(OnClickProxy);
                }
            }
        }

        void OnClickProxy() { if (ui && enemy) ui.SelectTarget(enemy); }

        public void OnPointerClick(PointerEventData eventData) => OnClickProxy();
    }
}
