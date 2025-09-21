using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace CL.Combat.UI
{
    public class TargetingPanel : MonoBehaviour
    {
        public Button headBtn, torsoBtn, leftArmBtn, rightArmBtn, leftLegBtn, rightLegBtn;
        public Button attackBtn, escapeBtn;
        public TMP_Text stateText;

        void Start()
        {
            var cm = CL.Combat.CombatManager.Instance;
            if (cm == null) { gameObject.SetActive(false); return; }

            headBtn.onClick.AddListener(()=> cm.SetTarget(CL.Combat.BodyPartId.Head));
            torsoBtn.onClick.AddListener(()=> cm.SetTarget(CL.Combat.BodyPartId.Torso));
            leftArmBtn.onClick.AddListener(()=> cm.SetTarget(CL.Combat.BodyPartId.LeftArm));
            rightArmBtn.onClick.AddListener(()=> cm.SetTarget(CL.Combat.BodyPartId.RightArm));
            leftLegBtn.onClick.AddListener(()=> cm.SetTarget(CL.Combat.BodyPartId.LeftLeg));
            rightLegBtn.onClick.AddListener(()=> cm.SetTarget(CL.Combat.BodyPartId.RightLeg));
            attackBtn.onClick.AddListener(()=> cm.PlayerAttackSelected());
            escapeBtn.onClick.AddListener(()=> cm.TryEscape());

            UpdateState();
            cm.OnStateChanged += UpdateState;
        }
        void OnDestroy()
        {
            var cm = CL.Combat.CombatManager.Instance;
            if (cm != null) cm.OnStateChanged -= UpdateState;
        }
        void UpdateState()
        {
            var cm = CL.Combat.CombatManager.Instance;
            if (!cm || stateText==null) return;
            stateText.text = $"Your Torso HP: {cm.player?.parts.Find(p=>p.id==CL.Combat.BodyPartId.Torso)?.hp}";
        }
    }
}
