using UnityEngine;
using TMPro;

public class SaveSlotButton : MonoBehaviour
{
    public TMP_Text summary;

    public void Bind(SaveData s)
    {
        if (!summary) return;
        if (s == null)
        {
            summary.text = "(空白存檔)";
            return;
        }

        var a = s.abilities ?? new AbilityStats();

        summary.text =
$@"HP {s.hp}  $ {s.money}  SAN {s.sanity}  CRD {s.credits}
HUN {s.hunger}  THI {s.thirst}  FAT {s.fatigue}  HOP {s.hope}
OBE {s.obedience}  REP {s.reputation}  T-PART {s.techParts}  INF {s.information}
AUG {s.augmentationLoad}  RAD {s.radiation}  INFEC {s.infection}  TRU {s.trust}  CTRL {s.control}
STR {a.strength}  AGI {a.agility}  INT {a.intellect}  CHA {a.charisma}  STL {a.stealth}  TEC {a.tech}";
    }
}
