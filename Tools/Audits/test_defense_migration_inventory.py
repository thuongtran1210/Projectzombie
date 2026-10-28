"""Kiểm tra parser kiểm kê bằng dữ liệu giả, không khởi chạy Unity."""

import unittest
from pathlib import Path
from unittest.mock import patch

from defense_migration_inventory import GUID, read_text, scene_components, ui_registry_entries


class SceneInventoryTests(unittest.TestCase):
    def test_guid_scan_includes_asset_reference_and_group_entry(self):
        text = "guid: " + "a" * 32 + "\nm_AssetGUID: " + "b" * 32 + "\nm_GUID: " + "c" * 32
        self.assertEqual(GUID.findall(text), ["a" * 32, "b" * 32, "c" * 32])

    def test_registry_preserves_direct_and_async_references_separately(self):
        text = """  - screenType: 0
    screenPrefab: {fileID: 1, guid: aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa, type: 3}
    screenPrefabRef:
      m_AssetGUID: bbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbb
    screenHierarchyName: Hub
  - screenType: 1
    screenPrefab: {fileID: 0}
    screenPrefabRef:
      m_AssetGUID:
    screenHierarchyName: Hero
"""
        rows = list(ui_registry_entries(text, {"a" * 32: ["Assets/Hub.prefab"]}))
        self.assertEqual(len(rows), 2)
        self.assertEqual(rows[0]["direct_prefab"], "Assets/Hub.prefab")
        self.assertEqual(rows[0]["asset_reference_guid"], "b" * 32)
        self.assertEqual(rows[1]["asset_reference_guid"], "")

    def test_component_resolves_owner_hierarchy_and_script(self):
        # Không phụ thuộc thứ tự block hoặc thứ tự khai báo cha/con trong scene.
        text = """--- !u!114 &30
MonoBehaviour:
  m_GameObject: {fileID: 2}
  m_Enabled: 0
  m_Script: {fileID: 11500000, guid: aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa, type: 3}
--- !u!4 &20
Transform:
  m_GameObject: {fileID: 2}
  m_Father: {fileID: 10}
--- !u!1 &2
GameObject:
  m_Name: Child
--- !u!1 &1
GameObject:
  m_Name: Root
--- !u!4 &10
Transform:
  m_GameObject: {fileID: 1}
  m_Father: {fileID: 0}
"""
        for newline in ("\n", "\r\n"):
            rows = list(scene_components("test.unity", text.replace("\n", newline), {"a" * 32: ["Assets/Test.cs"]}))
            self.assertEqual(len(rows), 1)
            self.assertEqual(rows[0]["hierarchy"], "Root/Child")
            self.assertEqual(rows[0]["enabled"], "0")
            self.assertEqual(rows[0]["script"], "Assets/Test.cs")

    def test_missing_script_is_reported_without_guessing(self):
        text = """--- !u!114 &-5
MonoBehaviour:
  m_GameObject: {fileID: 0}
  m_Script: {fileID: 11500000, guid: bbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbb, type: 3}
"""
        row = list(scene_components("test.unity", text, {}))[0]
        self.assertEqual(row["script"], "")
        self.assertEqual(row["script_guid"], "b" * 32)
        self.assertEqual(row["hierarchy"], "")

    def test_binary_is_not_treated_as_reference_free_text(self):
        path = Path("scene.unity")
        for data in (b"binary\0data", b"\xff\xff"):
            with patch.object(Path, "read_bytes", return_value=data):
                self.assertIsNone(read_text(path))


if __name__ == "__main__":
    unittest.main()
