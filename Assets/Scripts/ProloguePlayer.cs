using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class ProloguePlayer : MonoBehaviour
{
    [Header("Prologue Text")]
    [TextArea(3, 10)]
    public string prologueText =
@"未來沒有黎明；城市只剩鐵、玻璃與鏡頭。
高牆裡的巨塔記錄每一筆呼吸，帳本替人決定存亡；
外圍的軍團拉直邊界，以火線劃出新的地圖；
宣稱自由的秩序，用審查與指標將語言格式化——
說錯一個字，就被從版面上刪除。
科技不再屬於人。
晶片縫進血肉，義體讓體溫降到金屬的冷；
演算法把夢標上標籤，把記憶改成「推薦」。
你發出的每一次心跳，都是資料；每一次沈默，都是罪。
飢餓、口渴、疲勞只會磨損身體；
真正致命的是希望變薄、服從變厚。
名聲成為枷鎖，信任被量化成分數；
監控像霧，輻射像雨，從不停止。
停火像玻璃，一碰就碎；糧食被當成籌碼，能源變成封鎖線。
世界分裂成彼此不相容的真理，各說各話，誰也聽不見誰。
而你——流亡者、棄民、無名子——
沒有神，沒有靠山。
若要活下去，就在陰影裡書寫命運；";

    [Header("UI")]
    public TextMeshProUGUI textUI;
    public GameObject continueHint;     // 顯示“按任意鍵/繼續”的提示
    public Button continueButton;       // 可選，完成後啟用
    public Button skipButton;           // 可選，進度中=跳字，完成後=繼續

    [Header("Typewriter")]
    [Range(1f, 120f)] public float charsPerSecond = 40f;
    [Tooltip("句號/驚嘆/問號等延遲倍率")]
    public float endPauseMult = 8f;
    [Tooltip("逗號/頓號/分號等延遲倍率")]
    public float midPauseMult = 4f;
    [Tooltip("每幾個字觸發一次 SFX(0=不播）")]
    public int sfxEveryNChars = 2;

    [Header("SFX (Optional)")]
    public AudioSource sfxSource;
    public AudioClip typeClip;

    Coroutine typingCo;
    bool isTyping;
    int typedCount;

    void OnEnable()
    {
        StartTyping();
    }

    public void StartTyping()
    {
        if (!textUI) return;
        if (typingCo != null) StopCoroutine(typingCo);
        textUI.text = "";
        typedCount = 0;
        isTyping = true;
        SetContinueUI(false);
        typingCo = StartCoroutine(TypeRoutine(prologueText));
    }

    IEnumerator TypeRoutine(string src)
    {
        int i = 0;
        while (i < src.Length)
        {
            // 即時處理 RichText 標籤（<...>）與 TMP Sprite 標籤
            if (src[i] == '<')
            {
                int close = src.IndexOf('>', i);
                if (close != -1)
                {
                    string tag = src.Substring(i, close - i + 1);
                    textUI.text += tag; // 標籤不延遲，立即插入
                    i = close + 1;
                    continue;
                }
            }

            char c = src[i];
            textUI.text += c;
            i++;
            typedCount++;

            // 播放打字 SFX（可選）
            if (sfxEveryNChars > 0 && typeClip && sfxSource && (typedCount % sfxEveryNChars == 0))
            {
                sfxSource.PlayOneShot(typeClip);
            }

            // 計算延遲
            float delay = 1f / Mathf.Max(1f, charsPerSecond);
            if (IsEndPause(c)) delay *= endPauseMult;
            else if (IsMidPause(c)) delay *= midPauseMult;

            // 支援玩家點擊跳過逐字（SkipTyping）
            float t = 0f;
            while (t < delay)
            {
                if (!isTyping) break;
                t += Time.unscaledDeltaTime;
                yield return null;
            }
            if (!isTyping) break;
        }

        // 若被 SkipTyping 中斷，直接填滿剩餘文字
        if (!isTyping && textUI.text != src)
            textUI.text = src;

        isTyping = false;
        typingCo = null;
        SetContinueUI(true);
    }

    bool IsEndPause(char c)
    {
        // 句末：。．. ! ！ ? ？ … ～ ─
        return c == '.' || c == '。' || c == '．' ||
               c == '!' || c == '！' ||
               c == '?' || c == '？' ||
               c == '…' || c == '～' || c == '─';
    }

    bool IsMidPause(char c)
    {
        // 中停：, ， 、 ; ； : ： ) 】 」 ’ ” 
        return c == ',' || c == '，' || c == '、' ||
               c == ';' || c == '；' ||
               c == ':' || c == '：' ||
               c == ')' || c == '】' || c == '」' ||
               c == '’' || c == '”';
    }

    void SetContinueUI(bool on)
    {
        if (continueHint) continueHint.SetActive(on);
        if (continueButton) continueButton.interactable = on;
        if (skipButton)     skipButton.GetComponentInChildren<TMP_Text>()?.SetText(on ? "繼續" : "跳過");
    }

    // UI 綁定：點擊或按鍵 → 進度中=跳字；完成後=前往難度
    public void OnSkipOrContinue()
    {
        if (isTyping) SkipTyping();
        else OnContinue();
    }

    public void SkipTyping()
    {
        if (!isTyping) return;
        isTyping = false; // 讓協程快速收尾並填滿文字
    }

    public void OnContinue()
    {
        // 進入選難度（沿用你的流程）
        if (MenuManager.Instance) MenuManager.Instance.GoToDifficulty();
    }

    // 可綁定 ESC 或返回鍵
    public void OnSkip()
    {
        OnSkipOrContinue();
    }

    // 可選：鍵盤支援
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return))
            OnSkipOrContinue();
        if (Input.GetKeyDown(KeyCode.Escape))
            OnSkipOrContinue();
    }
}
