// Assets/Editor/SampleAssetsGenerator.cs — 動態對應你自訂的 CaseId；沒有就跳過，不報錯。
#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System;

public static class SampleAssetsGenerator
{
    private const string Root = "Assets/DOL_Example";

    [MenuItem("Tools/DOL/Generate Example Assets")]
    public static void Generate()
    {
        if (!AssetDatabase.IsValidFolder(Root))
            AssetDatabase.CreateFolder("Assets", "DOL_Example");

        // 1) 建立範例事件資產
        var eWelcomeGuard = Create_WelcomeGuard();    // 一次性
        var eStreetShow   = Create_StreetPerformer(); // 可重複 + 冷卻
        var eLostPuppy    = Create_LostPuppyQuest();  // 多頁支線 + 旗標
        var eMugger       = Create_AlleyMugger();     // 受傷事件（多地點可用）
        var eInnRest      = Create_InnRest();         // 回復事件（可重複，冷卻）

        // 2) 建立 CaseDatabase 並配置事件池（僅加入你 enum 內存在的地點）
        var db = ScriptableObject.CreateInstance<CaseDatabase>();
        db.cases = new List<CaseDatabase.CaseEntry>();

        TryAddCase(db, "TownSquare", new List<CaseDatabase.EventEntry>{
            EE(eWelcomeGuard, 5f), EE(eStreetShow, 2f), EE(eLostPuppy, 1.5f), EE(eMugger, 0.5f)
        });

        TryAddCase(db, "DarkAlley", new List<CaseDatabase.EventEntry>{
            EE(eMugger, 3f), EE(eStreetShow, 0.2f)
        });

        TryAddCase(db, "Inn", new List<CaseDatabase.EventEntry>{
            EE(eInnRest, 10f)
        });

        TryAddCase(db, "ForestEntrance", new List<CaseDatabase.EventEntry>{
            EE(eMugger, 0.3f), EE(eStreetShow, 0.5f)
        });

        var dbPath = $"{Root}/CaseDatabase.asset";
        AssetDatabase.CreateAsset(db, dbPath);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        EditorUtility.DisplayDialog("DOL Example",
            "範例資產已產生：\nAssets/DOL_Example/\n請在 GameManager 指向 CaseDatabase（若 enum 未新增地點，資料庫可能為空）。",
            "OK");
    }

    // =============== 事件建立（gotoCase 若目標地點不存在則回退 None） ===============
    private static DolEventAsset Create_WelcomeGuard()
    {
        var a = ScriptableObject.CreateInstance<DolEventAsset>();
        a.eventId = "EVT_WelcomeGuard";
        a.weight = 1f; a.oncePerSave = true; a.cooldownSeconds = 0f;

        var (hasInn, innId) = TryParseCase("Inn");
        a.stages = new List<DolEventAsset.EventStage>
        {
            new DolEventAsset.EventStage{
                text = "守衛：初來乍到？保持安分，小鎮就會對你友善。",
                choices = new List<DolEventAsset.EventChoice>{
                    new DolEventAsset.EventChoice{ text = "點頭致意（不回應）", nextStage = -1, endEvent = true },
                    new DolEventAsset.EventChoice{ text = "詢問治安狀況", nextStage = 1 }
                }
            },
            new DolEventAsset.EventStage{
                text = "守衛：白天還行，晚上別去小巷。旅店在廣場東邊。",
                choices = new List<DolEventAsset.EventChoice>{
                    new DolEventAsset.EventChoice{ text = "謝謝資訊（結束）", endEvent = true },
                    new DolEventAsset.EventChoice{
                        text = "前往旅店",
                        endEvent = true, gotoCaseAfterEnd = hasInn, gotoCase = hasInn ? innId : CaseId.None
                    }
                }
            }
        };
        SaveAsset(a, "EVT_WelcomeGuard.asset"); return a;
    }

    private static DolEventAsset Create_StreetPerformer()
    {
        var a = ScriptableObject.CreateInstance<DolEventAsset>();
        a.eventId = "EVT_StreetPerformer";
        a.weight = 1f; a.oncePerSave = false; a.cooldownSeconds = 60f;
        a.stages = new List<DolEventAsset.EventStage>
        {
            new DolEventAsset.EventStage{
                text = "你遇見一位街頭藝人，輕快的旋律讓你心情放鬆。",
                choices = new List<DolEventAsset.EventChoice>{
                    new DolEventAsset.EventChoice{ text = "給點打賞（HP +5）", hpChange = +5, endEvent = true },
                    new DolEventAsset.EventChoice{ text = "微笑離開（無事）", endEvent = true }
                }
            }
        };
        SaveAsset(a, "EVT_StreetPerformer.asset"); return a;
    }

    private static DolEventAsset Create_LostPuppyQuest()
    {
        var (hasSquare, squareId) = TryParseCase("TownSquare");

        var a = ScriptableObject.CreateInstance<DolEventAsset>();
        a.eventId = "EVT_LostPuppy";
        a.weight = 1.2f; a.oncePerSave = false; a.cooldownSeconds = 0f;
        a.stages = new List<DolEventAsset.EventStage>
        {
            new DolEventAsset.EventStage{
                text = "小女孩哭泣：你有看到我的小狗嗎？",
                choices = new List<DolEventAsset.EventChoice>{
                    new DolEventAsset.EventChoice{ text = "答應幫忙找（設旗標：PuppyQuest）",
                        setFlagsTrue = new List<string>{"PuppyQuest"}, nextStage = 1 },
                    new DolEventAsset.EventChoice{ text = "拒絕（-）", endEvent = true }
                }
            },
            new DolEventAsset.EventStage{
                text = "你在小巷找到受驚的小狗。",
                choices = new List<DolEventAsset.EventChoice>{
                    new DolEventAsset.EventChoice{ text = "溫柔靠近（HP -5）", hpChange = -5, nextStage = 2 },
                    new DolEventAsset.EventChoice{ text = "呼喚它的名字（若知道才有效）", nextStage = 2 }
                }
            },
            new DolEventAsset.EventStage{
                text = "小狗跟你回到廣場，小女孩破涕為笑。",
                choices = new List<DolEventAsset.EventChoice>{
                    new DolEventAsset.EventChoice{
                        text = "歸還小狗（設旗標：PuppySaved，結束並回廣場）",
                        setFlagsTrue = new List<string>{"PuppySaved"},
                        endEvent = true, gotoCaseAfterEnd = hasSquare, gotoCase = hasSquare ? squareId : CaseId.None
                    }
                }
            }
        };
        SaveAsset(a, "EVT_LostPuppy.asset"); return a;
    }

    private static DolEventAsset Create_AlleyMugger()
    {
        var (hasSquare, squareId) = TryParseCase("TownSquare");

        var a = ScriptableObject.CreateInstance<DolEventAsset>();
        a.eventId = "EVT_AlleyMugger";
        a.weight = 1.0f; a.oncePerSave = false; a.cooldownSeconds = 30f;
        a.minHP = 10;
        a.stages = new List<DolEventAsset.EventStage>
        {
            new DolEventAsset.EventStage{
                text = "陰影中跳出劫匪！你被搶了一拳。",
                choices = new List<DolEventAsset.EventChoice>{
                    new DolEventAsset.EventChoice{ text = "硬扛（HP -15，結束）", hpChange = -15, endEvent = true },
                    new DolEventAsset.EventChoice{
                        text = "衝向人多處（結束並回廣場）",
                        endEvent = true, gotoCaseAfterEnd = hasSquare, gotoCase = hasSquare ? squareId : CaseId.None
                    }
                }
            }
        };
        SaveAsset(a, "EVT_AlleyMugger.asset"); return a;
    }

    private static DolEventAsset Create_InnRest()
    {
        var a = ScriptableObject.CreateInstance<DolEventAsset>();
        a.eventId = "EVT_InnRest";
        a.weight = 1.0f; a.oncePerSave = false; a.cooldownSeconds = 120f;
        a.stages = new List<DolEventAsset.EventStage>
        {
            new DolEventAsset.EventStage{
                text = "你在旅店稍作休息，精神恢復。",
                choices = new List<DolEventAsset.EventChoice>{
                    new DolEventAsset.EventChoice{ text = "小睡片刻（HP +20）", hpChange = +20, endEvent = true },
                    new DolEventAsset.EventChoice{ text = "喝杯溫茶（HP +10）",  hpChange = +10, endEvent = true }
                }
            }
        };
        SaveAsset(a, "EVT_InnRest.asset"); return a;
    }

    // =============== 工具 ===============
    private static (bool ok, CaseId id) TryParseCase(string name)
    {
        try
        {
            if (Enum.TryParse(name, out CaseId id)) return (true, id);
        }
        catch { }
        return (false, CaseId.None);
    }

    private static void TryAddCase(CaseDatabase db, string caseName, List<CaseDatabase.EventEntry> entries)
    {
        var (ok, id) = TryParseCase(caseName);
        if (!ok) return; // 你的 enum 沒有這個地點 → 跳過
        db.cases.Add(new CaseDatabase.CaseEntry { caseId = id, events = entries });
    }

    private static CaseDatabase.EventEntry EE(DolEventAsset a, float w)
    {
        return new CaseDatabase.EventEntry { evt = a, weightOverride = w };
    }

    private static void SaveAsset(ScriptableObject obj, string fileName)
    {
        var path = $"{Root}/{fileName}";
        AssetDatabase.CreateAsset(obj, path);
        EditorUtility.SetDirty(obj);
    }
}
#endif
