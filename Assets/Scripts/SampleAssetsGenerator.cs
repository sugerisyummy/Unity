// Assets/Editor/SampleAssetsGenerator.cs  — 一鍵產出範例資產
#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public static class SampleAssetsGenerator
{
    private const string Root = "Assets/DOL_Example";

    [MenuItem("Tools/DOL/Generate Example Assets")]
    public static void Generate()
    {
        if (!AssetDatabase.IsValidFolder(Root))
            AssetDatabase.CreateFolder("Assets", "DOL_Example");

        // 1) 建立範例事件資產
        var eWelcomeGuard   = Create_WelcomeGuard();    // 一次性
        var eStreetShow     = Create_StreetPerformer(); // 可重複 + 冷卻
        var eLostPuppy      = Create_LostPuppyQuest();  // 多頁支線 + 旗標
        var eMugger         = Create_AlleyMugger();     // 受傷事件（多地點可用）
        var eInnRest        = Create_InnRest();         // 回復事件（可重複，冷卻）

        // 2) 建立 CaseDatabase 並配置事件池（可重複指向同一事件）
        var db = ScriptableObject.CreateInstance<CaseDatabase>();
        db.cases = new List<CaseDatabase.CaseEntry>
        {
            new CaseDatabase.CaseEntry{
                caseId = CaseId.TownSquare,
                events = new List<CaseDatabase.EventEntry>{
                    new CaseDatabase.EventEntry{ evt = eWelcomeGuard,   weightOverride = 5f  }, // 初來乍到，權重高但 once
                    new CaseDatabase.EventEntry{ evt = eStreetShow,     weightOverride = 2f  },
                    new CaseDatabase.EventEntry{ evt = eLostPuppy,      weightOverride = 1.5f},
                    new CaseDatabase.EventEntry{ evt = eMugger,         weightOverride = 0.5f}, // 也可能在廣場被扒
                }
            },
            new CaseDatabase.CaseEntry{
                caseId = CaseId.DarkAlley,
                events = new List<CaseDatabase.EventEntry>{
                    new CaseDatabase.EventEntry{ evt = eMugger,     weightOverride = 3f },
                    new CaseDatabase.EventEntry{ evt = eStreetShow, weightOverride = 0.2f }, // 罕見遇見街頭藝人
                }
            },
            new CaseDatabase.CaseEntry{
                caseId = CaseId.Inn,
                events = new List<CaseDatabase.EventEntry>{
                    new CaseDatabase.EventEntry{ evt = eInnRest, weightOverride = 10f }
                }
            },
            // 舊森林例子：可繼續沿用
            new CaseDatabase.CaseEntry{
                caseId = CaseId.ForestEntrance,
                events = new List<CaseDatabase.EventEntry>{
                    new CaseDatabase.EventEntry{ evt = eMugger, weightOverride = 0.3f },
                    new CaseDatabase.EventEntry{ evt = eStreetShow, weightOverride = 0.5f },
                }
            },
        };

        var dbPath = $"{Root}/CaseDatabase.asset";
        AssetDatabase.CreateAsset(db, dbPath);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        EditorUtility.DisplayDialog("DOL Example", "範例資產已產生：\nAssets/DOL_Example/\n請在 GameManager 指向 CaseDatabase", "OK");
    }

    // =============== 事件建立函式們 ===============

    // 小鎮守衛迎新（一次性）
    private static DolEventAsset Create_WelcomeGuard()
    {
        var a = ScriptableObject.CreateInstance<DolEventAsset>();
        a.eventId = "EVT_WelcomeGuard";
        a.weight = 1f;
        a.oncePerSave = true; // 只觸發一次
        a.cooldownSeconds = 0f;
        a.stages = new List<DolEventAsset.EventStage>
        {
            new DolEventAsset.EventStage{
                text = "守衛：初來乍到？保持安分，小鎮就會對你友善。",
                choices = new List<DolEventAsset.EventChoice>{
                    new DolEventAsset.EventChoice{
                        text = "點頭致意（不回應）",
                        nextStage = -1, endEvent = true
                    },
                    new DolEventAsset.EventChoice{
                        text = "詢問治安狀況",
                        nextStage = 1
                    }
                }
            },
            new DolEventAsset.EventStage{
                text = "守衛：白天還行，晚上別去小巷。旅店在廣場東邊。",
                choices = new List<DolEventAsset.EventChoice>{
                    new DolEventAsset.EventChoice{
                        text = "謝謝資訊（結束）",
                        endEvent = true
                    },
                    new DolEventAsset.EventChoice{
                        text = "前往旅店",
                        endEvent = true, gotoCaseAfterEnd = true, gotoCase = CaseId.Inn
                    }
                }
            }
        };
        SaveAsset(a, "EVT_WelcomeGuard.asset"); return a;
    }

    // 街頭藝人表演（可重複，但有冷卻）
    private static DolEventAsset Create_StreetPerformer()
    {
        var a = ScriptableObject.CreateInstance<DolEventAsset>();
        a.eventId = "EVT_StreetPerformer";
        a.weight = 1f;
        a.oncePerSave = false;
        a.cooldownSeconds = 60f; // 60 秒冷卻
        a.stages = new List<DolEventAsset.EventStage>
        {
            new DolEventAsset.EventStage{
                text = "你遇見一位街頭藝人，輕快的旋律讓你心情放鬆。",
                choices = new List<DolEventAsset.EventChoice>{
                    new DolEventAsset.EventChoice{
                        text = "給點打賞（HP +5）",
                        hpChange = +5, endEvent = true
                    },
                    new DolEventAsset.EventChoice{
                        text = "微笑離開（無事）",
                        endEvent = true
                    }
                }
            }
        };
        SaveAsset(a, "EVT_StreetPerformer.asset"); return a;
    }

    // 走失的小狗（多頁支線 + 旗標）
    private static DolEventAsset Create_LostPuppyQuest()
    {
        var a = ScriptableObject.CreateInstance<DolEventAsset>();
        a.eventId = "EVT_LostPuppy";
        a.weight = 1.2f;
        a.oncePerSave = false;
        a.cooldownSeconds = 0f;
        a.stages = new List<DolEventAsset.EventStage>
        {
            new DolEventAsset.EventStage{
                text = "小女孩哭泣：你有看到我的小狗嗎？",
                choices = new List<DolEventAsset.EventChoice>{
                    new DolEventAsset.EventChoice{
                        text = "答應幫忙找（設旗標：PuppyQuest）",
                        setFlagsTrue = new List<string>{"PuppyQuest"},
                        nextStage = 1
                    },
                    new DolEventAsset.EventChoice{
                        text = "拒絕（-）",
                        endEvent = true
                    }
                }
            },
            new DolEventAsset.EventStage{
                text = "你在小巷找到受驚的小狗。",
                choices = new List<DolEventAsset.EventChoice>{
                    new DolEventAsset.EventChoice{
                        text = "溫柔靠近（HP -5）",
                        hpChange = -5, nextStage = 2
                    },
                    new DolEventAsset.EventChoice{
                        text = "呼喚它的名字（若知道才有效）",
                        nextStage = 2
                    }
                }
            },
            new DolEventAsset.EventStage{
                text = "小狗跟你回到廣場，小女孩破涕為笑。",
                choices = new List<DolEventAsset.EventChoice>{
                    new DolEventAsset.EventChoice{
                        text = "歸還小狗（設旗標：PuppySaved，結束並回廣場）",
                        setFlagsTrue = new List<string>{"PuppySaved"},
                        endEvent = true, gotoCaseAfterEnd = true, gotoCase = CaseId.TownSquare
                    }
                }
            }
        };
        SaveAsset(a, "EVT_LostPuppy.asset"); return a;
    }

    // 小巷劫匪（可重複；HP 扣血；多地點共用）
    private static DolEventAsset Create_AlleyMugger()
    {
        var a = ScriptableObject.CreateInstance<DolEventAsset>();
        a.eventId = "EVT_AlleyMugger";
        a.weight = 1.0f;
        a.oncePerSave = false;
        a.cooldownSeconds = 30f;
        a.minHP = 10; // 太低 HP 不再出現以免虐殺
        a.stages = new List<DolEventAsset.EventStage>
        {
            new DolEventAsset.EventStage{
                text = "陰影中跳出劫匪！你被搶了一拳。",
                choices = new List<DolEventAsset.EventChoice>{
                    new DolEventAsset.EventChoice{
                        text = "硬扛（HP -15，結束）",
                        hpChange = -15, endEvent = true
                    },
                    new DolEventAsset.EventChoice{
                        text = "衝向人多處（結束並回廣場）",
                        endEvent = true, gotoCaseAfterEnd = true, gotoCase = CaseId.TownSquare
                    }
                }
            }
        };
        SaveAsset(a, "EVT_AlleyMugger.asset"); return a;
    }

    // 旅店休息（可重複；冷卻）
    private static DolEventAsset Create_InnRest()
    {
        var a = ScriptableObject.CreateInstance<DolEventAsset>();
        a.eventId = "EVT_InnRest";
        a.weight = 1.0f;
        a.oncePerSave = false;
        a.cooldownSeconds = 120f; // 兩分鐘內不可連續刷
        a.stages = new List<DolEventAsset.EventStage>
        {
            new DolEventAsset.EventStage{
                text = "你在旅店稍作休息，精神恢復。",
                choices = new List<DolEventAsset.EventChoice>{
                    new DolEventAsset.EventChoice{
                        text = "小睡片刻（HP +20）",
                        hpChange = +20, endEvent = true
                    },
                    new DolEventAsset.EventChoice{
                        text = "喝杯溫茶（HP +10）",
                        hpChange = +10, endEvent = true
                    }
                }
            }
        };
        SaveAsset(a, "EVT_InnRest.asset"); return a;
    }

    // 工具：寫檔
    private static void SaveAsset(ScriptableObject obj, string fileName)
    {
        var path = $"{Root}/{fileName}";
        AssetDatabase.CreateAsset(obj, path);
        EditorUtility.SetDirty(obj);
    }
}
#endif
