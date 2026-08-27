# -*- coding: utf-8 -*-
import os

prefabs_info = {
    'E_MAGIAP': {'id': 5431972333897014678, 'guid': 'e78d9a5a4396d0a43b442de8fa48afc7', 'name': 'Ma Giap'},
    'E_MADA': {'id': 1193935233806825299, 'guid': 'cc60aa0652631604ea95def9ce66ecce', 'name': 'Ma Da'},
    'E_MATROI': {'id': 4720196618505412990, 'guid': '40336bbe99d02974eb09b2c9201ef4c9', 'name': 'Ma Troi'},
    'E_HOALYTINH': {'id': 509878819556777675, 'guid': 'cfbcec5a64e8e8e45b9fdf7888549503', 'name': 'Hoa Ly Tinh'},
    'E_MADOINO': {'id': 4338019940683393192, 'guid': '88a398719a324b91950e1fa41890e101', 'name': 'Ma Doi No'},
    'E_QUYNHAPTRANG': {'id': 4942488841039399808, 'guid': '944cc003ebcffde4a8add0899773079a', 'name': 'Quy Nhap Trang'},
    'Boss_NguuDauMaDien': {'id': 6556722217386542342, 'guid': '193a344b7a316cd42b5e32fc47df8ba7', 'name': 'Boss Nguu Dau Ma Dien'},
    'Boss_DiemVuong': {'id': 1348244042159176007, 'guid': 'bb55896b976f62a4cb0393430a4d8e0f', 'name': 'Boss Diem Vuong'}
}

timeline_events = [
    {
        'eventName': 'Phut 00:00 - Khoi dau: Ma Giap quy binh xuat hien nen',
        'timestampSeconds': 0,
        'eventType': 0,
        'enemy': 'E_MAGIAP',
        'spawnCount': 3,
        'spawnInterval': 4.0
    },
    {
        'eventName': 'Phut 01:00 - Ma Da tron truot tang toc ap sat',
        'timestampSeconds': 60,
        'eventType': 0,
        'enemy': 'E_MADA',
        'spawnCount': 4,
        'spawnInterval': 3.5
    },
    {
        'eventName': 'Phut 02:00 - Ma Troi bay lo lung phong ma hoa',
        'timestampSeconds': 120,
        'eventType': 0,
        'enemy': 'E_MATROI',
        'spawnCount': 3,
        'spawnInterval': 4.0
    },
    {
        'eventName': 'Phut 03:00 - Bay Ma Da tran len bao vay (Burst Wave)',
        'timestampSeconds': 180,
        'eventType': 1,
        'enemy': 'E_MADA',
        'spawnCount': 12,
        'spawnInterval': 0.2
    },
    {
        'eventName': 'Phut 04:00 - Bay Ho Ly Tinh tinh quai lao vao tu no',
        'timestampSeconds': 240,
        'eventType': 0,
        'enemy': 'E_HOALYTINH',
        'spawnCount': 4,
        'spawnInterval': 4.0
    },
    {
        'eventName': 'Phut 05:00 - ELITE QUY NHAP TRANG XUAT HIEN',
        'timestampSeconds': 300,
        'eventType': 1,
        'enemy': 'E_QUYNHAPTRANG',
        'spawnCount': 1,
        'spawnInterval': 0.0
    },
    {
        'eventName': 'Phut 06:00 - Ma Doi No len lut tho tien chay tron',
        'timestampSeconds': 360,
        'eventType': 0,
        'enemy': 'E_MADOINO',
        'spawnCount': 2,
        'spawnInterval': 12.0
    },
    {
        'eventName': 'Phut 08:00 - Bao Ma Hoa & Ho Ly Tinh bao vay (Burst Wave)',
        'timestampSeconds': 480,
        'eventType': 1,
        'enemy': 'E_HOALYTINH',
        'spawnCount': 16,
        'spawnInterval': 0.2
    },
    {
        'eventName': 'Phut 10:00 - MID-BOSS NGUU DAU MA DIEN XUAT HIEN',
        'timestampSeconds': 600,
        'eventType': 3,
        'enemy': 'Boss_NguuDauMaDien',
        'spawnCount': 1,
        'spawnInterval': 0.0
    },
    {
        'eventName': 'Phut 12:00 - Doi hinh Quy Binh & Cuong Thi tong luc',
        'timestampSeconds': 720,
        'eventType': 0,
        'enemy': 'E_QUYNHAPTRANG',
        'spawnCount': 2,
        'spawnInterval': 6.0
    },
    {
        'eventName': 'Phut 15:00 - Dai Bao Yeu Ma tong luc (Multi-Burst Wave)',
        'timestampSeconds': 900,
        'eventType': 1,
        'enemy': 'E_MAGIAP',
        'spawnCount': 25,
        'spawnInterval': 0.1
    },
    {
        'eventName': 'Phut 20:00 - FINAL BOSS DIEM VUONG GIANG LAM',
        'timestampSeconds': 1200,
        'eventType': 3,
        'enemy': 'Boss_DiemVuong',
        'spawnCount': 1,
        'spawnInterval': 0.0
    }
]

lines = [
    '%YAML 1.1',
    '%TAG !u! tag:unity3d.com,2011:',
    '--- !u!114 &11400000',
    'MonoBehaviour:',
    '  m_ObjectHideFlags: 0',
    '  m_CorrespondingSourceObject: {fileID: 0}',
    '  m_PrefabInstance: {fileID: 0}',
    '  m_PrefabAsset: {fileID: 0}',
    '  m_GameObject: {fileID: 0}',
    '  m_Enabled: 1',
    '  m_EditorHideFlags: 0',
    '  m_Script: {fileID: 11500000, guid: 41d564f8503fb9f46894a138ca0791b3, type: 3}',
    '  m_Name: Level1_Timeline',
    '  m_EditorClassIdentifier: ',
    '  levelName: "Man 1: U Minh Gioi"',
    '  maxLevelDuration: 1200',
    '  events:'
]

for ev in timeline_events:
    p_info = prefabs_info[ev['enemy']]
    p_id = p_info['id']
    p_guid = p_info['guid']
    
    lines.append(f'  - eventName: "{ev["eventName"]}"')
    lines.append(f'    timestampSeconds: {ev["timestampSeconds"]}')
    lines.append(f'    eventType: {ev["eventType"]}')
    lines.append(f'    spawnPrefab: {{fileID: {p_id}, guid: {p_guid}, type: 3}}')
    lines.append('    enemyAddress: ')
    lines.append('    spawnPrefabRef:')
    lines.append(f'      m_AssetGUID: {p_guid}')
    lines.append('      m_SubObjectName: ')
    lines.append('      m_SubObjectType: ')
    lines.append('      m_EditorAssetChanged: 0')
    lines.append(f'    spawnCount: {ev["spawnCount"]}')
    lines.append(f'    spawnInterval: {ev["spawnInterval"]}')

timeline_path = r'C:\Users\thuon\Unity\Projectzombie\Assets\_Data\Levels\Level1_Timeline.asset'
with open(timeline_path, 'w', encoding='utf-8') as f:
    f.write('\n'.join(lines) + '\n')

print(f'Successfully updated Level1_Timeline.asset with full {len(timeline_events)} rich enemy events!')
