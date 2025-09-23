using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace CyberLife.Combat
{
    public class HPBarBinder : MonoBehaviour
    {
        public Combatant target;              // 可留空，會自動找 Tag=Player
        public Slider slider;                 // 用 Slider 就填它
        public Image fill;                    // 或用 Image fillAmount 就填它
        public TMP_Text label;                // 可選：顯示 HP 數字
        public float updateRate = 0.1f;

        void OnEnable() { StartCoroutine(Loop()); }

        System.Collections.IEnumerator Loop()
        {
            while (true)
            {
                if (target == null)
                {
                    var go = GameObject.FindGameObjectWithTag("Player");
                    if (go) target = go.GetComponent<Combatant>();
                }

                if (target != null)
                {
                    float cur = target.TotalHP;
                    float max = Mathf.Max(1f, target.TotalMaxHP);
                    if (slider)
                    {
                        if (slider.maxValue != max) slider.maxValue = max;
                        slider.value = cur;
                    }
                    if (fill) fill.fillAmount = cur / max;
                    if (label) label.text = $"HP {Mathf.RoundToInt(cur)}";
                }
                yield return new WaitForSeconds(updateRate);
            }
        }
    }
}