// Auto-generated replacement by ChatGPT (Combat core enums)
using UnityEngine;

namespace CyberLife.Combat
{
    public enum DamageType
    {
        Ballistic,
        Slash,
        Blunt,
        Thermal,
        Chemical,
        Energy
    }

    /// <summary>High-level hit groups for Fallout-style targeting (6 groups).</summary>
    public enum HitGroup
    {
        Head,
        Torso,
        Arms,
        Legs,
        Vital,
        Misc
    }

    /// <summary>Fine-grained tag on body parts; maps to one of the HitGroup buckets.</summary>
    public enum BodyTag
    {
        Head,
        Torso,
        Arm,
        Leg,
        Vital,
        Misc
    }

    /// <summary>Combat final outcome for the player side.</summary>
    public enum CombatOutcome
    {
        Win,
        Lose,
        Escape
    }

    /// <summary>Simple status tags that a consumable can cure or apply.</summary>
    public enum EffectTag
    {
        None,
        Poison,
        Burn,
        Bleed,
        Stun
    }
}