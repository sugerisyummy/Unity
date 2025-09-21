using System;

namespace CL.Combat
{
    // 傷害型別（含：斬/刺/鈍/熱能/化學/彈道）
    public enum DamageType { Slash, Pierce, Blunt, Thermal, Chemical, Ballistic }

    // 武器大類（方便 UI/規則）
    public enum WeaponCategory { Blade, Firearm, Blunt, Thermal, Chemical }

    // 身體部位（含器官，RimWorld 取向）
    public enum BodyPartId
    {
        Head, Brain, LeftEye, RightEye, Jaw, Neck,
        Torso, Heart, LeftLung, RightLung, Liver, Stomach, LeftKidney, RightKidney,
        LeftArm, RightArm, LeftHand, RightHand,
        LeftLeg, RightLeg, LeftFoot, RightFoot
    }
}
