using UnityEngine;
public class ButtonController : MonoBehaviour
{
    public void GoToPanel(GameObject p){ if (MenuManager.Instance) MenuManager.Instance.ShowPanel(p); }
    public void Back(){ if (MenuManager.Instance) MenuManager.Instance.Back(); }
    public void BackToMain(){ if (MenuManager.Instance) MenuManager.Instance.BackToStartMenu(); }

}
