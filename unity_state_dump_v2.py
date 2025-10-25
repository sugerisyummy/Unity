#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""
Unity State Dump (v2) — Windows-safe (no backslashes inside f-string expressions)
Usage:
  python unity_state_dump_v2.py --root "." --out state --md5
"""
import os, sys, json, argparse, hashlib, pathlib, datetime

def md5_of(path, max_bytes=None, chunk=1024*1024):
    try:
        h = hashlib.md5()
        total = 0
        with open(path, 'rb') as f:
            while True:
                b = f.read(chunk)
                if not b: break
                h.update(b)
                total += len(b)
                if max_bytes is not None and total >= max_bytes:
                    break
        return h.hexdigest()
    except Exception as e:
        return f"ERROR:{e}"

def load_json(path):
    try:
        with open(path, 'r', encoding='utf-8') as f:
            return json.load(f)
    except Exception as e:
        return {"_error": str(e)}

def write_json(path, obj):
    os.makedirs(os.path.dirname(path), exist_ok=True)
    with open(path, 'w', encoding='utf-8') as f:
        json.dump(obj, f, ensure_ascii=False, indent=2)

def write_text(path, text):
    os.makedirs(os.path.dirname(path), exist_ok=True)
    with open(path, 'w', encoding='utf-8') as f:
        f.write(text)

def norm_rel(path, root):
    # Always posix-style for reports
    rel = os.path.relpath(path, root)
    return rel.replace("\\", "/")

def main():
    ap = argparse.ArgumentParser()
    ap.add_argument('--root', default='.', help='Unity project root (folder that contains Assets/)')
    ap.add_argument('--out', default='state', help='Output folder for reports')
    ap.add_argument('--md5', action='store_true', help='Compute md5 for files (can be slow)')
    ap.add_argument('--max-mb', type=int, default=20, help='Max MB to hash per file when --md5')
    args = ap.parse_args()

    project_root = os.path.abspath(args.root)
    assets_root = os.path.join(project_root, 'Assets')
    out_root = os.path.abspath(args.out)
    os.makedirs(out_root, exist_ok=True)

    summary = {
        "project_root": project_root,
        "time": datetime.datetime.now().isoformat(),
        "python": sys.version,
        "has_assets_folder": os.path.isdir(assets_root),
        "warnings": [],
    }

    if not os.path.isdir(assets_root):
        summary["warnings"].append("Assets folder not found under project root.")
        write_json(os.path.join(out_root, "Summary.json"), summary)
        print("No Assets folder; wrote Summary.json")
        return

    # Walk Assets
    file_entries = []
    meta_set = set()
    asset_set = set()
    for root, dirs, files in os.walk(assets_root):
        for name in files:
            full = os.path.join(root, name)
            rel = norm_rel(full, project_root)
            size = os.path.getsize(full)
            entry = {"path": rel, "size": size}
            if args.md5:
                entry["md5"] = md5_of(full, max_bytes=args.max_mb*1024*1024)
            file_entries.append(entry)
            if name.endswith(".meta"):
                meta_set.add(rel[:-5])
            else:
                asset_set.add(rel)

    def write_json_local(rel, obj):
        write_json(os.path.join(out_root, rel), obj)

    write_json_local("ProjectInventory_Lite.json", {"files": file_entries})

    meta_missing = sorted([a for a in asset_set if (a + ".meta") not in [m + ".meta" for m in meta_set]])
    orphan_meta = sorted([m for m in meta_set if m not in asset_set])
    write_json_local("MetaReport.json", {
        "assets_without_meta": meta_missing,
        "orphan_meta_without_asset": orphan_meta,
    })

    # Asmdef report
    asmdefs = []
    asm_by_name = {}
    for entry in file_entries:
        if entry["path"].lower().endswith(".asmdef"):
            p = os.path.join(project_root, entry["path"])
            data = load_json(p)
            name = data.get("name", "(unknown)")
            include = data.get("includePlatforms", [])
            refs = data.get("references", [])
            asm = {
                "name": name,
                "path": entry["path"],
                "includePlatforms": include,
                "references": refs,
                "_error": data.get("_error")
            }
            asmdefs.append(asm)
            asm_by_name.setdefault(name, []).append(asm)
    write_json_local("AsmdefReport.json", {"asmdefs": asmdefs})

    dups = {k:v for k,v in asm_by_name.items() if len(v) > 1}
    kg_editor = [a for a in asmdefs if a["name"] == "KG.Editor"]
    kg_board  = [a for a in asmdefs if a["name"] == "KG.Board"]
    bad_editor_paths = [a["path"] for a in kg_editor if not (a["path"].startswith("Assets/KG/Editor") or a["path"].startswith("Assets/Editor"))]
    bad_board_paths  = [a["path"] for a in kg_board if not a["path"].startswith("Assets/Scripts/Board")]
    write_json_local("KGPolicyReport.json", {
        "kg_editor_exists": len(kg_editor) > 0,
        "kg_board_exists": len(kg_board) > 0,
        "kg_editor_paths": [a["path"] for a in kg_editor],
        "kg_board_paths": [a["path"] for a in kg_board],
        "kg_editor_refs_board": any("KG.Board" in (a.get("references") or []) for a in kg_editor),
        "kg_editor_editor_only": all("Editor" in (a.get("includePlatforms") or []) and len(a.get("includePlatforms") or [])==1 for a in kg_editor) if kg_editor else False,
        "duplicate_asmdef_names": {k:[i["path"] for i in v] for k,v in dups.items()},
        "kg_editor_out_of_place": bad_editor_paths,
        "kg_board_out_of_place": bad_board_paths
    })

    must_have = [
        "Assets/Scripts/Combat/CombatPageController.cs",
        "Assets/Scripts/Combat/CombatManager.cs",
        "Assets/Scripts/Combat/CombatEventBridge.cs",
        "Assets/Scripts/Combat/CombatResultRouter.cs",
        "Assets/Scripts/Combat/CombatUIController.cs",
        "Assets/Scripts/Board/BoardController.cs",
        "Assets/Scripts/Board/PawnController.cs",
    ]
    present = [p for p in must_have if p in asset_set]
    missing = [p for p in must_have if p not in asset_set]
    write_json_local("BoardCombatPresence.json", {"present": present, "missing": missing})

    def list_tree(root_rel, max_depth=4):
        out = []
        root_abs = os.path.join(project_root, root_rel)
        if not os.path.isdir(root_abs):
            return [f"[missing] {root_rel}"]
        for r, d, f in os.walk(root_abs):
            depth = len(os.path.relpath(r, root_abs).split(os.sep))
            if depth > max_depth: 
                continue
            indent = "  " * (depth if os.path.relpath(r, root_abs)!="." else 0)
            rel_dir = norm_rel(r, project_root)
            out.append(f"{indent}{rel_dir}")
            for fn in sorted(f):
                out.append(f"{indent}  - {fn}")
        return out

    tree_txt = []
    for folder in ["Assets/Scripts", "Assets/KG/Editor", "Assets/Editor"]:
        tree_txt.append("== " + folder + " ==")
        tree_txt.extend(list_tree(folder))
        tree_txt.append("")
    write_text(os.path.join(out_root, "PathsReport.txt"), "\n".join(tree_txt))

    warnings = []
    if dups:
        warnings.append("Duplicate asmdef names: " + ", ".join(dups.keys()))
    if bad_editor_paths:
        warnings.append("KG.Editor.asmdef is not under Assets/KG/Editor or Assets/Editor")
    if bad_board_paths:
        warnings.append("KG.Board.asmdef is not under Assets/Scripts/Board")
    if missing:
        warnings.append("Missing key scripts: " + "; ".join(missing))
    write_json_local("Summary.json", {
        "project_root": project_root,
        "time": datetime.datetime.now().isoformat(),
        "warnings": warnings
    })

if __name__ == "__main__":
    main()
