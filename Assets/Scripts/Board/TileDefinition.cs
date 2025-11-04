using UnityEngine;
using UnityEngine.Scripting.APIUpdating;

namespace Game.Board
{
    public enum TileType
    {
        Empty,
        Start,
        Property,
        Chance,
        Chest,
        Tax,
        Jail,
        GoToJail,
        FreeParking,
        Combat,
        Event,
    }

    [CreateAssetMenu(menuName = "CyberLife/Board/TileDefinition")]
    [MovedFrom(true, sourceNamespace: "CyberLife.Board")]
    public class TileDefinition : ScriptableObject
    {
        public TileType type;
        public string displayName;
        public int cost;
        public int rent;
        public string eventKey;
        public int combatLevel;
    }
}
