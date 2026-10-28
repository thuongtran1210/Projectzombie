"""Kiểm kê tĩnh phục vụ migration; không sửa hoặc xóa asset Unity."""

import csv
import json
import re
import subprocess
from collections import Counter, defaultdict, deque
from datetime import datetime, timezone
from pathlib import Path


ROOT = Path(__file__).resolve().parents[2]
OUT = ROOT / "Docs/Migration/Generated"
TEXT_EXTENSIONS = {
    ".cs", ".shader", ".hlsl", ".cginc", ".compute", ".shadergraph",
    ".shadersubgraph", ".unity", ".prefab", ".asset", ".mat", ".anim",
    ".controller", ".overridecontroller", ".spriteatlas", ".spriteatlasv2",
    ".json", ".asmdef", ".asmref", ".uxml", ".uss", ".inputactions",
}
GUID = re.compile(r"\b(?:guid|m_AssetGUID|m_GUID):\s*([a-fA-F0-9]{32})\b")
LEGACY_SYMBOLS = re.compile(
    r"\b(RunLoadoutState|GameplayBootstrapper|SpawnManager|WeaponManager|"
    r"PlayerController|PlayerStats|GameStateManager|UpgradeManager|"
    r"RunStatsTracker|CharacterEntry|WeaponData|HealthSystem|Rigidbody2D|Physics2D)\b"
)
DYNAMIC = re.compile(
    r"Shader\.Find|Resources\.Load|LoadAssetAsync|LoadAllAsync|"
    r"InstantiateAsync|LoadScene|AssetDatabase\.FindAssets|RuntimeInitializeOnLoadMethod"
)


def read_text(path):
    # Không giải mã asset nhị phân thành văn bản để rồi kết luận nhầm là không có dependency.
    data = path.read_bytes()
    if b"\0" in data:
        return None
    try:
        return data.decode("utf-8-sig")
    except UnicodeDecodeError:
        return None


def policy(path):
    if path.startswith(("Assets/Photon/", "Assets/Plugins/", "Assets/TextMesh Pro/")):
        return "VENDOR_REVIEW", "Không xóa vendor trong batch gameplay"
    if path.startswith(("Assets/Features/Defense3D/", "Assets/Resources/Defense3D/", "Assets/VFX/Defense3D/")):
        return "KEEP_REVIEW", "Nội dung/implementation Defense3D; kiểm tra đường legacy bên trong"
    if path.startswith("Assets/Shaders/VFX/Defense"):
        return "KEEP_REVIEW", "Shader Defense3D"
    if path.startswith(("Assets/Core/", "Assets/Features/UI/", "Assets/Features/MetaProgression/", "Assets/Features/Startup/", "Assets/Features/MatchFlow/", "Assets/Features/Multiplayer/")):
        return "ADAPT_REVIEW", "Có phần dùng chung và dependency cần thay thế"
    if path.startswith(("Assets/Features/Player/", "Assets/Features/Weapons/", "Assets/Features/Enemies/", "Assets/Features/Spawners/", "Assets/Features/Projectiles/", "Assets/Features/Collectibles/", "Assets/Features/Boss/")):
        return "LEGACY_REVIEW", "Ứng viên runtime cũ; chưa chứng minh xóa an toàn"
    return "UNCLASSIFIED_REVIEW", "Cần đối chiếu consumer và mục đích"


def write_csv(name, fields, rows):
    with (OUT / name).open("w", encoding="utf-8-sig", newline="") as stream:
        writer = csv.DictWriter(stream, fieldnames=fields)
        writer.writeheader()
        writer.writerows(rows)


def scene_components(path, text, guid_paths):
    blocks = {}
    for match in re.finditer(r"^--- !u!(\d+) &(-?\d+)[^\n]*\n(.*?)(?=^--- !u!|\Z)", text, re.M | re.S):
        blocks[match[2]] = (match[1], match[3])
    names, parents, owners = {}, {}, {}
    for identity, (kind, body) in blocks.items():
        if kind == "1":
            name = re.search(r"^  m_Name: (.*)$", body, re.M)
            names[identity] = name[1].strip() if name else identity
        if kind in {"4", "224"}:
            owner = re.search(r"m_GameObject: \{fileID: (-?\d+)", body)
            parent = re.search(r"m_Father: \{fileID: (-?\d+)", body)
            if owner:
                owners[identity] = owner[1]
                parents[owner[1]] = parent[1] if parent else "0"

    def hierarchy(owner):
        result, visited = [], set()
        while owner != "0" and owner not in visited:
            visited.add(owner)
            result.append(names.get(owner, "UNKNOWN_" + owner))
            owner = owners.get(parents.get(owner, "0"), "0")
        return "/".join(reversed(result))

    for identity, (kind, body) in blocks.items():
        if kind != "114":
            continue
        owner = re.search(r"m_GameObject: \{fileID: (-?\d+)", body)
        script = re.search(r"m_Script: \{fileID: [^,]+, guid: ([a-fA-F0-9]{32})", body)
        enabled = re.search(r"m_Enabled: (\d+)", body)
        targets = guid_paths.get(script[1].lower(), []) if script else []
        yield dict(scene=path, component_file_id=identity, gameobject_file_id=owner[1] if owner else "", hierarchy=hierarchy(owner[1]) if owner else "", enabled=enabled[1] if enabled else "", script="|".join(targets), script_guid=script[1] if script else "", evidence="TEXT_ONLY_PREFAB_INSTANCES_NOT_EXPANDED")


def ui_registry_entries(text, guid_paths):
    for match in re.finditer(r"^  - screenType: (\d+)\s*\n(.*?)(?=^  - screenType:|\Z)", text, re.M | re.S):
        body = match[2]
        prefab = re.search(r"screenPrefab: \{[^\n]*guid: ([a-fA-F0-9]{32})", body)
        reference = re.search(r"m_AssetGUID:[ \t]*([a-fA-F0-9]{32})", body)
        name = re.search(r"screenHierarchyName: ([^\r\n]+)", body)
        guid = prefab[1].lower() if prefab else ""
        yield dict(screen_type=match[1], hierarchy_name=name[1].strip() if name else "", direct_guid=guid, direct_prefab="|".join(guid_paths.get(guid, [])), asset_reference_guid=reference[1].lower() if reference else "", decision="RETAIN_AND_ADAPT_REVIEW")


def main():
    OUT.mkdir(parents=True, exist_ok=True)
    baseline = subprocess.run(
        ["git", "status", "--porcelain=v1", "--untracked-files=normal"],
        cwd=ROOT, capture_output=True, text=True, check=True,
    ).stdout
    (OUT / "working_tree_at_scan.txt").write_text(baseline, encoding="utf-8")
    paths = sorted(p for p in (ROOT / "Assets").rglob("*") if p.is_file())
    assets = {p.relative_to(ROOT).as_posix(): p for p in paths if p.suffix != ".meta"}
    guid_paths = defaultdict(list)
    own_guids = {}
    for path in paths:
        if path.suffix != ".meta":
            continue
        text = read_text(path)
        match = re.search(r"^guid:\s*([a-fA-F0-9]{32})", text or "", re.M)
        if match:
            asset = path.relative_to(ROOT).as_posix()[:-5]
            guid = match[1].lower()
            guid_paths[guid].append(asset)
            own_guids[asset] = guid

    edges, dynamic, legacy, entries, scene_rows, registry_rows = [], [], [], [], [], []
    adjacency, incoming = defaultdict(set), defaultdict(set)
    scan_states = {}
    extra = sorted(p for p in (ROOT / "ProjectSettings").glob("*") if p.is_file())
    for path in paths + extra:
        relative = path.relative_to(ROOT).as_posix()
        if path.suffix not in TEXT_EXTENSIONS and path.suffix != ".meta":
            continue
        text = read_text(path)
        if path.suffix != ".meta":
            scan_states[relative] = "TEXT_SCANNED" if text is not None else "BINARY_OR_ENCODING_REVIEW"
        if text is None:
            continue
        if path.suffix == ".unity":
            scene_rows.extend(scene_components(relative, text, guid_paths))
        if relative == "Assets/Resources/UI/UIRegistry.asset":
            registry_rows.extend(ui_registry_entries(text, guid_paths))
        source = relative[:-5] if path.suffix == ".meta" else relative
        for guid in sorted({m.lower() for m in GUID.findall(text)}):
            if guid == own_guids.get(source):
                continue
            targets = guid_paths.get(guid, [])
            resolution = "RESOLVED" if len(targets) == 1 else "DUPLICATE_GUID" if targets else "BUILTIN_OR_EXTERNAL_OR_MISSING_REVIEW"
            target = targets[0] if len(targets) == 1 else ""
            edges.append(dict(source=source, guid=guid, target=target, resolution=resolution))
            if target:
                adjacency[source].add(target)
                incoming[target].add(source)
        if path.suffix == ".cs":
            for line_number, line in enumerate(text.splitlines(), 1):
                if DYNAMIC.search(line):
                    dynamic.append(dict(source=relative, line=line_number, evidence=line.strip()))
                for symbol in sorted(set(LEGACY_SYMBOLS.findall(line))):
                    legacy.append(dict(source=relative, line=line_number, symbol=symbol, evidence=line.strip()))
        if relative.startswith("Assets/AddressableAssetsData/AssetGroups/") and "/Schemas/" not in relative:
            for match in re.finditer(r"m_GUID:\s*([a-fA-F0-9]{32})(.*?)(?=\n\s*- m_GUID:|\n  m_ReadOnly:|\Z)", text, re.S):
                guid, body = match[1].lower(), match[2]
                address = re.search(r"m_Address:\s*([^\r\n]*)", body)
                target = guid_paths.get(guid, [])
                labels = re.search(r"m_SerializedLabels:(.*?)(?=\n\s*m_\w+:|\Z)", body, re.S)
                entries.append(dict(group=path.stem, guid=guid, address=address[1].strip() if address else "", target="|".join(target), labels="|".join(re.findall(r"- ([^\r\n]+)", labels[1])) if labels else "", folder=(ROOT / target[0]).is_dir() if len(target) == 1 else ""))

    # Root bảo thủ để bảo toàn nội dung; reachability không được dùng làm lệnh xóa.
    roots = []
    for path in assets:
        if path == "Assets/Resources/UI/UIRegistry.asset":
            roots.append(dict(path=path, reason="CONFIRMED_DYNAMIC_META_UI_ROOT"))
        elif path in {"Assets/Scenes/SampleScene.unity", "Assets/Scenes/Defense3D/MapSongQue3D_Prototype.unity"}:
            roots.append(dict(path=path, reason="SCENE_ROOT_MIXED_REVIEW"))
        elif path.startswith(("Assets/Features/Defense3D/", "Assets/Resources/Defense3D/")):
            roots.append(dict(path=path, reason="DEFENSE_AUTHORING_ROOT_CONSERVATIVE"))
    for path in extra:
        roots.append(dict(path=path.relative_to(ROOT).as_posix(), reason="PROJECT_SETTINGS_ROOT"))
    reachable = set(r["path"] for r in roots)
    queue = deque(sorted(reachable))
    while queue:
        for target in adjacency.get(queue.popleft(), ()):
            if target not in reachable:
                reachable.add(target)
                queue.append(target)

    inventory = []
    for path, file in assets.items():
        decision, reason = policy(path)
        inventory.append(dict(path=path, guid=own_guids.get(path, ""), extension=file.suffix.lower(), bytes=file.stat().st_size, decision=decision, reason=reason, scan=scan_states.get(path, "NOT_TEXT_SCANNED"), incoming_sources=len(incoming[path]), conservative_root_reachable=path in reachable, deletion_approved=False))
    write_csv("assets.csv", list(inventory[0]), inventory)
    write_csv("guid_edges.csv", ["source", "guid", "target", "resolution"], edges)
    write_csv("dynamic_load_sites.csv", ["source", "line", "evidence"], dynamic)
    write_csv("legacy_symbol_sites.csv", ["source", "line", "symbol", "evidence"], legacy)
    write_csv("addressables.csv", ["group", "guid", "address", "target", "labels", "folder"], entries)
    write_csv("roots.csv", ["path", "reason"], roots)
    write_csv("scene_components.csv", ["scene", "component_file_id", "gameobject_file_id", "hierarchy", "enabled", "script", "script_guid", "evidence"], scene_rows)
    write_csv("ui_registry_roots.csv", ["screen_type", "hierarchy_name", "direct_guid", "direct_prefab", "asset_reference_guid", "decision"], registry_rows)
    duplicates = {guid: targets for guid, targets in guid_paths.items() if len(targets) > 1}
    summary = dict(scanned_at_utc=datetime.now(timezone.utc).isoformat(), assets=len(inventory), source_bytes=sum(r["bytes"] for r in inventory), decisions=dict(Counter(r["decision"] for r in inventory)), guid_edges=len(edges), unresolved_review_edges=sum(r["resolution"] != "RESOLVED" for r in edges), dynamic_sites=len(dynamic), legacy_symbol_sites=len(legacy), addressable_entries=len(entries), scene_components=len(scene_rows), roots=len(roots), duplicate_guids=duplicates, binary_or_encoding_review=sum(v == "BINARY_OR_ENCODING_REVIEW" for v in scan_states.values()), limitations=["Text GUID references only; package/builtin GUID unresolved is not proof of missing asset", "Conservative roots include mixed SampleScene; unreachable is not unused", "C# symbol sites include comments/declarations, not semantic dependencies", "Folder Addressables entries are listed, not expanded into effective bundle contents", "Scene components exclude inherited prefab contents and runtime-created objects", "Dynamic strings, binary serialization, shader stripping and runtime require further validation", "No asset deletion approved; no Unity executed"])
    (OUT / "summary.json").write_text(json.dumps(summary, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")
    print(json.dumps({k: v for k, v in summary.items() if k not in {"duplicate_guids", "limitations"}}, ensure_ascii=True))


if __name__ == "__main__":
    main()
