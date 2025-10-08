// CombatUIController.cs — Label RectTransform 容錯（會自動抓同物件上的 TMP_Text/Text）
using UnityEngine;
using UnityEngine.UI;
using TMPro; // for TMP_Text
using System.Reflection;
using System;

namespace CyberLife.Combat
{
    public class CombatUIController : MonoBehaviour
    {
        [Header("Refs")] public CombatManager manager;
        public GameObject groupsPanel;

        [Header("Enemy HUD (optional)")] public Slider enemyHpSlider;
        public Component enemyHpLabel;   // 允許拖到 Text 物件（即便被 Unity 填成 RectTransform 也行）

        [Header("Player HUD (optional)")] public Slider playerHpSlider;
        public Component playerHpLabel;

        [Header("文字設定")]
        public string[] groupButtonLabels = new string[]{ "左手","右手","左腳","右腳","頭","軀幹" };
        public string playerHpPrefix = "我方";
        public string enemyHpPrefix  = "敵方";
        public bool useEnemyNameAsPrefix = false;

        [HideInInspector] public Combatant currentTarget;

        void Awake(){ TryAutowire(); ApplyGroupButtonLabels(); }
        void OnEnable(){ TryAutowire(); ApplyGroupButtonLabels(); if (groupsPanel) groupsPanel.SetActive(false); RefreshPlayerHUD(); HideEnemyHUD(); }
        void LateUpdate(){ RefreshPlayerHUD(); if (currentTarget){ if (!IsAliveLike(currentTarget)) ClearSelection(); else RefreshEnemyHUD(currentTarget);} }

        public void SelectTarget(Combatant target){ currentTarget=target; if (groupsPanel) groupsPanel.SetActive(target); if(target) RefreshEnemyHUD(target); else HideEnemyHUD(); }
        public void HitGroupButton(int index){ if(!manager||!currentTarget)return; var hg=(HitGroup)Mathf.Clamp(index,0,5); var t=currentTarget; manager.PlayerAttackTargetWithGroup(t,hg); RefreshEnemyHUD(t); currentTarget=null; if(groupsPanel) groupsPanel.SetActive(false); }
        public void AttackAuto(){ if(!manager)return; var t=currentTarget?currentTarget:manager.GetFirstAliveEnemy(); if(!t)return; manager.PlayerAttackTarget(t); RefreshEnemyHUD(t); currentTarget=null; if(groupsPanel) groupsPanel.SetActive(false); }
        public void ClearSelection(){ currentTarget=null; if(groupsPanel) groupsPanel.SetActive(false); HideEnemyHUD(); }

        // ==== UI text helpers ====
        Component ResolveTextComponent(Component any)
        {
            if (!any) return null;
            // 若傳進來是 RectTransform/GameObject，抓同物件上的 TMP_Text 或 UnityEngine.UI.Text
            var tr = any.transform;
            var tmp = tr.GetComponent<TMP_Text>(); if (tmp) return tmp;
            var utx = tr.GetComponent<UnityEngine.UI.Text>(); if (utx) return utx;
            // 萬一傳進來的是 Text 的父物件，往下找一層
            tmp = tr.GetComponentInChildren<TMP_Text>(true); if (tmp) return tmp;
            utx = tr.GetComponentInChildren<UnityEngine.UI.Text>(true); if (utx) return utx;
            // 直接用反射的 text 屬性（保底）
            var pi = any.GetType().GetProperty("text", BindingFlags.Public|BindingFlags.Instance);
            return (pi != null && pi.CanWrite && pi.PropertyType==typeof(string)) ? any : null;
        }
        void SetText(Component anyText, string value)
        {
            var real = ResolveTextComponent(anyText);
            if (!real) return;
            if (real is TMP_Text tmp) tmp.text = value;
            else if (real is UnityEngine.UI.Text ut) ut.text = value;
            else
            {
                var pi = real.GetType().GetProperty("text", BindingFlags.Public|BindingFlags.Instance);
                if (pi != null && pi.CanWrite) { try { pi.SetValue(real, value); } catch {} }
            }
        }

        void ApplyGroupButtonLabels()
        {
            if (!groupsPanel) return;
            var root = groupsPanel.transform;
            int count = Mathf.Min(root.childCount, 6);
            for (int i = 0; i < count; i++)
            {
                var tr = root.GetChild(i);
                var text = ResolveTextComponent(tr);
                if (text) SetText(text, (i < groupButtonLabels.Length && !string.IsNullOrEmpty(groupButtonLabels[i])) ? groupButtonLabels[i] : $"部位 {i}");
            }
        }

        void TryAutowire()
        {
            if (!manager) manager = FindObjectOfType<CombatManager>();
            var canvas = GetComponentInParent<Canvas>()?.transform;
            if (canvas)
            {
                if (!enemyHpSlider) enemyHpSlider = FindByPath<Slider>(canvas, "CombatPanel/UI/Hp_Enemys/Bar");
                if (!playerHpSlider) playerHpSlider = FindByPath<Slider>(canvas, "CombatPanel/UI/Hp_player/Bar");
                if (!enemyHpLabel)  enemyHpLabel  = FindTr(canvas, "CombatPanel/UI/Hp_Enemys/Bar/Text (TMP)")?.GetComponent<RectTransform>();
                if (!playerHpLabel) playerHpLabel = FindTr(canvas, "CombatPanel/UI/Hp_player/Bar/Text (TMP)")?.GetComponent<RectTransform>();
            }
        }
        Transform FindTr(Transform root, string path){ if (!root) return null; var cur=root; foreach (var p in path.Split('/')){ if(string.IsNullOrEmpty(p))continue; cur=cur?.Find(p); if(!cur) return null;} return cur; }
        T FindByPath<T>(Transform root, string path) where T: Component { var tr=FindTr(root,path); return tr?tr.GetComponent<T>():null; }

        private void RefreshEnemyHUD(Combatant c){ if(!c){ HideEnemyHUD(); return; } float hp=ReadHP(c); float max=ReadMaxHP(c,hp);
            if(enemyHpSlider){ enemyHpSlider.maxValue=Mathf.Max(1f,max); enemyHpSlider.value=Mathf.Clamp(hp,0f,enemyHpSlider.maxValue); }
            var prefix = useEnemyNameAsPrefix && c ? c.name : enemyHpPrefix;
            SetText(enemyHpLabel, $"{prefix} {Mathf.RoundToInt(hp)}/{Mathf.RoundToInt(max)}");
        }
        private void HideEnemyHUD(){ if(enemyHpSlider) enemyHpSlider.value=0f; SetText(enemyHpLabel, ""); }
        private void RefreshPlayerHUD(){ var p=manager?manager.GetFirstAliveAlly():null; if(p==null) return; float hp=ReadHP(p); float max=ReadMaxHP(p,hp);
            if(playerHpSlider){ playerHpSlider.maxValue=Mathf.Max(1f,max); playerHpSlider.value=Mathf.Clamp(hp,0f,playerHpSlider.maxValue); }
            SetText(playerHpLabel, $"{playerHpPrefix} {Mathf.RoundToInt(hp)}/{Mathf.RoundToInt(max)}");
        }

        private float ReadHP(Combatant c){ if(!c) return 0f; var bh=c.GetComponent<BasicHealth>(); if(bh) return bh.HP;
            var pi=c.GetType().GetProperty("HP", BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Instance); if(pi!=null && pi.CanRead){ try{ return Convert.ToSingle(pi.GetValue(c)); }catch{} } return 0f; }
        private float ReadMaxHP(Combatant c, float fallback){ if(!c) return Mathf.Max(1,fallback); var bh=c.GetComponent<BasicHealth>(); if(bh) return Mathf.Max(bh.MaxHP,fallback);
            var pi=c.GetType().GetProperty("MaxHP", BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Instance); if(pi!=null && pi.CanRead){ try{ return Convert.ToSingle(pi.GetValue(c)); }catch{} } return Mathf.Max(1,fallback); }
        private bool IsAliveLike(Combatant c){ if(!c) return false; var bh=c.GetComponent<BasicHealth>(); if(bh) return bh.IsAlive;
            var pi=c.GetType().GetProperty("IsAlive", BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Instance); if(pi!=null && pi.CanRead){ try{ return (bool)pi.GetValue(c); }catch{} }
            var mi=c.GetType().GetMethod("IsAlive", BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Instance, null, System.Type.EmptyTypes, null); if(mi!=null){ try{ return (bool)mi.Invoke(c,null);}catch{} } return ReadHP(c)>0f; }
    }
}
