using System.Collections.Generic;
using UnityEngine;

namespace CyberLife.Combat
{
    [CreateAssetMenu(menuName = "CL/Combat/Body Preset")]
    public class BodyPreset : ScriptableObject
    {
        [System.Serializable]
        public class Entry
        {
            public string id = "Head";
            public BodyTag tag = BodyTag.Head;
            [Min(1)] public float maxHP = 10f;
        }

        [Header("Preset Parts (edit per enemy type)")]
        public List<Entry> parts = new List<Entry>();

        [ContextMenu("Generate Default 22 Parts")]
        void GenerateDefault()
        {
            parts = new List<Entry>
            {
                new Entry{ id="Head",         tag=BodyTag.Head,  maxHP=12 },
                new Entry{ id="Neck",         tag=BodyTag.Vital, maxHP=10 },
                new Entry{ id="Chest",        tag=BodyTag.Torso, maxHP=24 },
                new Entry{ id="Abdomen",      tag=BodyTag.Torso, maxHP=20 },

                new Entry{ id="LeftArm",      tag=BodyTag.Arm,   maxHP=14 },
                new Entry{ id="LeftForearm",  tag=BodyTag.Arm,   maxHP=12 },
                new Entry{ id="LeftHand",     tag=BodyTag.Arm,   maxHP=10 },

                new Entry{ id="RightArm",     tag=BodyTag.Arm,   maxHP=14 },
                new Entry{ id="RightForearm", tag=BodyTag.Arm,   maxHP=12 },
                new Entry{ id="RightHand",    tag=BodyTag.Arm,   maxHP=10 },

                new Entry{ id="LeftThigh",    tag=BodyTag.Leg,   maxHP=18 },
                new Entry{ id="LeftCalf",     tag=BodyTag.Leg,   maxHP=16 },
                new Entry{ id="LeftFoot",     tag=BodyTag.Leg,   maxHP=12 },

                new Entry{ id="RightThigh",   tag=BodyTag.Leg,   maxHP=18 },
                new Entry{ id="RightCalf",    tag=BodyTag.Leg,   maxHP=16 },
                new Entry{ id="RightFoot",    tag=BodyTag.Leg,   maxHP=12 },

                new Entry{ id="Heart",        tag=BodyTag.Vital, maxHP=10 },
                new Entry{ id="LungL",        tag=BodyTag.Vital, maxHP=10 },
                new Entry{ id="LungR",        tag=BodyTag.Vital, maxHP=10 },
                new Entry{ id="Liver",        tag=BodyTag.Vital, maxHP=10 },
                new Entry{ id="Stomach",      tag=BodyTag.Vital, maxHP=10 },
                new Entry{ id="Spleen",       tag=BodyTag.Vital, maxHP=10 },
            };
        }
    }
}
