using UnityEngine;

namespace CL.Combat
{
    [CreateAssetMenu(menuName = "CL/Combat/Items/Armor")]
    public class ArmorDef : ItemDef
    {
        [Header("覆蓋部位")]
        public BodyPartId[] covers;

        [Header("減傷值（各型別）")]
        public int armorSlash = 0;
        public int armorPierce = 0;
        public int armorBlunt = 0;
        public int armorThermal = 0;
        public int armorChemical = 0;
        public int armorBallistic = 0;
    }
}
