using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace CyberLife.Combat
{
    public class EnemyHPBinder : MonoBehaviour
    {
        public CombatUIController ui; // 指到 CombatUI
        public Slider slider;
        public Image fill;
        public TMP_Text label;
        public float updateRate = 0.1f;

        void OnEnable(){ StartCoroutine(Loop()); }

        System.Collections.IEnumerator Loop()
        {
            while (true)
            {
                var target = ui ? ui.currentTarget : null;
                if (target != null)
                {
                    float cur = target.TotalHP;
                    float max = Mathf.Max(1f, target.TotalMaxHP);
                    if (slider){ if (slider.maxValue!=max) slider.maxValue=max; slider.value=cur; }
                    if (fill) fill.fillAmount = cur/max;
                    if (label) label.text = $"{target.displayName} {Mathf.RoundToInt(cur)}/{Mathf.RoundToInt(max)}";
                }
                yield return new WaitForSeconds(updateRate);
            }
        }
    }
}
