using System.Collections.Generic;
using UnityEngine;

namespace CL.Combat
{
    public class Combatant
    {
        public string displayName;
        public int baseSpeed = 5;
        public int baseDefense = 2;
        public int baseAccuracy = 0;

        public WeaponDef mainHand;
        public ArmorDef head;
        public ArmorDef torso;
        public ArmorDef legs;
        public ItemDef utility;

        public List<BodyPartState> parts = new List<BodyPartState>();
        public List<StatusEffect> effects = new List<StatusEffect>();

        public int Speed
        {
            get
            {
                int eff = baseSpeed;
                foreach (var e in effects) if (e.def) eff += e.def.speedDelta;
                eff += mainHand ? mainHand.speed : 0;
                return Mathf.Max(0, eff);
            }
        }
        public int Defense
        {
            get
            {
                int eff = baseDefense;
                foreach (var e in effects) if (e.def) eff += e.def.defenseDelta;
                return Mathf.Max(0, eff);
            }
        }

        public bool IsDead
        {
            get
            {
                foreach (var p in parts)
                    if ((p.Vital && p.IsDestroyed) || (p.id == BodyPartId.Torso && p.IsDestroyed))
                        return true;
                return false;
            }
        }

        public void InitDefaultHuman()
        {
            parts.Clear();
            Add(BodyPartId.Head, 12);
            Add(BodyPartId.Brain, 5);
            Add(BodyPartId.LeftEye, 3);
            Add(BodyPartId.RightEye, 3);
            Add(BodyPartId.Jaw, 4);
            Add(BodyPartId.Neck, 8);

            Add(BodyPartId.Torso, 30);
            Add(BodyPartId.Heart, 5);
            Add(BodyPartId.LeftLung, 6);
            Add(BodyPartId.RightLung, 6);
            Add(BodyPartId.Liver, 6);
            Add(BodyPartId.Stomach, 6);
            Add(BodyPartId.LeftKidney, 4);
            Add(BodyPartId.RightKidney, 4);

            Add(BodyPartId.LeftArm, 12);
            Add(BodyPartId.RightArm, 12);
            Add(BodyPartId.LeftHand, 8);
            Add(BodyPartId.RightHand, 8);

            Add(BodyPartId.LeftLeg, 16);
            Add(BodyPartId.RightLeg, 16);
            Add(BodyPartId.LeftFoot, 8);
            Add(BodyPartId.RightFoot, 8);
        }
        void Add(BodyPartId id, int max) => parts.Add(new BodyPartState{ id=id, maxHP=max, hp=max });
    }
}
