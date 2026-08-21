scene_path = 'Assets/Scenes/SampleScene.unity'
with open(scene_path, 'r', encoding='utf-8') as f:
    content = f.read()

# 1. Background_Panel Sprite
content = content.replace(
    '  m_Sprite: {fileID: 21300000, guid: 66795ed7512d4a26b5baa485e905bd07, type: 3}',
    '  m_Sprite: {fileID: 21300000, guid: 6acb921de9fe43d2a0e42001874a2541, type: 3}'
)

# 2. PlayAgain_Button & MainMenu_Button Sprites
idx1 = content.find('--- !u!114 &778437114\n')
if idx1 != -1:
    end1 = content.find('--- !u!', idx1 + 10)
    chunk1 = content[idx1:end1]
    chunk1_new = chunk1.replace(
        '  m_Sprite: {fileID: 10905, guid: 0000000000000000f000000000000000, type: 0}',
        '  m_Sprite: {fileID: 21300000, guid: 5da06ea1fa244297b183bc8de1c20a0a, type: 3}'
    )
    content = content[:idx1] + chunk1_new + content[end1:]

idx2 = content.find('--- !u!114 &1523810605\n')
if idx2 != -1:
    end2 = content.find('--- !u!', idx2 + 10)
    chunk2 = content[idx2:end2]
    chunk2_new = chunk2.replace(
        '  m_Sprite: {fileID: 10905, guid: 0000000000000000f000000000000000, type: 0}',
        '  m_Sprite: {fileID: 21300000, guid: 4ee6fc76390f4ce485cfbac8831da914, type: 3}'
    )
    content = content[:idx2] + chunk2_new + content[end2:]

# 3. Texts
content = content.replace('  m_text: PlayAgain', '  m_text: TÁI CHIẾN')
content = content.replace('  m_text: MainMenu', '  m_text: HỒI QUY')

with open(scene_path, 'w', encoding='utf-8') as f:
    f.write(content)

print('Updated SampleScene.unity successfully!')
