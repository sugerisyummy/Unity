// Enums.cs — unified enums for CyberLife.Combat (with BodyTag aliases)
// Keep all combat-related enums in one place to avoid "type not found" (CS0246).

namespace CyberLife.Combat
{
    /// <summary>Standardized 6-slot targeting groups (index-aligned).</summary>
    public enum HitGroup
    {
        Head     = 0,
        Torso    = 1,
        LeftArm  = 2,
        RightArm = 3,
        LeftLeg  = 4,
        RightLeg = 5,

        // Aliases for legacy code
        Body = Torso,
        Chest = Torso,
        LArm = LeftArm,
        RArm = RightArm,
        LLeg = LeftLeg,
        RLeg = RightLeg
    }

    /// <summary>Bucket tags for body parts; map to HitGroup.</summary>
    public enum BodyTag
    {
        Head,
        Torso,
        LeftArm,
        RightArm,
        LeftLeg,
        RightLeg,
        Misc,

        // Backward-compat aliases (kept for old code/data)
        Arm = LeftArm,
        Leg = LeftLeg,
        Vital = Torso
    }

    /// <summary>Damage channels referenced by weapons/armor/effects.</summary>
    public enum DamageType
    {
        Blunt = 0,
        Slash = 1,
        Pierce = 2,
        Ballistic = 3,
        Thermal = 4,
        Chemical = 5,
        Electric = 6,
        Poison = 7
    }

    /// <summary>Status/effect identifiers used in Effects/Consumables.</summary>
    public enum EffectTag
    {
        None = 0,
        Bleed,
        Burn,
        Poison,
        Stun,
        Slow,
        Cure,     // for antidotes/medkits to remove negative effects
        Regen,
        Shield
    }

    /// <summary>Final outcome returned by combat controller.</summary>
    public enum CombatOutcome
    {
        None = 0,
        Win,
        Lose,
        Escape,
        Special
    }
}
