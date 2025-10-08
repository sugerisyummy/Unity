using UnityEngine;
namespace CyberLife.Combat {
  public class DeselectHotkey : MonoBehaviour {
    public CombatUIController ui;
    void Update() {
      if (Input.GetMouseButtonDown(1) || Input.GetKeyDown(KeyCode.Escape))
        ui?.SelectTarget(null);
    }
  }
}
