using UnityEngine;

namespace Game.UI
{
    /// <summary>
    /// 簡單的頁面切換器：切換可見面板，並呼叫 UICameraPageBinder 綁定相機。
    /// 用法：把想切換的面板拖進欄位，按需要的公開方法（可給 UI Button 用）。
    /// </summary>
    public class PageSwitcher : MonoBehaviour
    {
        public RectTransform mainMenu;
        public RectTransform loadMenu;
        public RectTransform boardPanel;
        public RectTransform combatPanel;
        public RectTransform storyPanel;
        public UICameraPageBinder binder;

        void Reset()
        {
            if (!binder && Camera.main != null) binder = Camera.main.GetComponent<Game.UI.UICameraPageBinder>();
        }

        public void ShowMain()  => Show(mainMenu);
        public void ShowLoad()  => Show(loadMenu);
        public void ShowBoard() => Show(boardPanel);
        public void ShowCombat()=> Show(combatPanel);
        public void ShowStory() => Show(storyPanel);

        public void Show(RectTransform panel)
        {
            if (!panel) return;
            SetActive(mainMenu,  panel == mainMenu);
            SetActive(loadMenu,  panel == loadMenu);
            SetActive(boardPanel,panel == boardPanel);
            SetActive(combatPanel,panel == combatPanel);
            SetActive(storyPanel, panel == storyPanel);

            if (binder) binder.BindTo(panel);
        }

        void SetActive(RectTransform rt, bool on)
        {
            if (!rt) return;
            rt.gameObject.SetActive(on);
        }
    }
}
