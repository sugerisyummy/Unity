namespace CyberLife.Combat
{
    // 0..5 的順序：左手、右手、左腳、右腳、頭、軀幹
    public enum HitGroup {
        LeftArm = 0, RightArm = 1, LeftLeg = 2, RightLeg = 3, Head = 4, Torso = 5
    }

    // 部位標籤（要能對應到上面 HitGroup）
    public enum BodyTag {
        LeftArm, RightArm, LeftLeg, RightLeg, Head, Torso, Misc,
        // 相容舊資料
        Arm = LeftArm, Leg = LeftLeg, Vital = Torso
    }

    public enum DamageType { Blunt, Slash, Pierce, Ballistic, Thermal, Chemical, Electric, Poison }
    public enum EffectTag { None, Bleed, Burn, Poison, Stun, Slow, Cure, Regen, Shield }
    public enum CombatOutcome { None, Win, Lose, Escape, Special }
}