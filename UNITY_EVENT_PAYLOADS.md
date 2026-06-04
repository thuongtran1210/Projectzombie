# Tài Liệu Đặc Tả Payload Server Gửi Cho Unity Client

Tài liệu này mô tả chi tiết các trường hợp và cấu trúc dữ liệu (payload) mà Node.js Server sẽ gửi (qua WebSocket) cho Unity Client khi nhận được các tương tác từ người xem trên TikTok LIVE. Unity Developer sử dụng tài liệu này để parse JSON và xử lý logic UI/Gameplay tương ứng.

---

## 1. Cấu Trúc Payload Cơ Bản (Base Event Data)

Tất cả các JSON payload gửi từ Server về Unity đều được kế thừa từ một cấu trúc cơ bản chung. Điều này giúp Unity dễ dàng parse và phân loại sự kiện.

### Cấu trúc JSON Base:
```json
{
  "type": "TÊN_SỰ_KIỆN",             // Loại sự kiện (VD: SHOW_GIFT, SPAWN_ENEMY)
  "user": "tên_người_dùng_tiktok",   // Tên người thực hiện tương tác
  "targetUser": "người_bị_tác_động", // Thường giống 'user' hoặc 'Community'
  "gameMode": "default",             // Chế độ chơi hiện tại (để phân rẽ logic nếu cần)
  "source": "chat",                  // Nguồn sự kiện: 'chat', 'gift', 'like', 'follow'
  "timestamp": 1717081510000,        // Unix timestamp (milliseconds)
  "avatar": "https://..."            // URL ảnh đại diện của người dùng (có thể null)
}
```

### C# Class Gợi Ý (Dùng với `Newtonsoft.Json` hoặc `JsonUtility`):
```csharp
[System.Serializable]
public class BaseTikTokEvent
{
    public string type;
    public string user;
    public string targetUser;
    public string gameMode;
    public string source;
    public long timestamp;
    public string avatar;
}
```

> **LƯU Ý QUAN TRỌNG:** Server sử dụng **Dynamic Schema**. Tức là ngoài các trường cơ bản trên, mỗi gói tin có thể chứa thêm các trường tự định nghĩa (từ file config trên Node.js) như `damage`, `speed`, `color`, v.v. Bạn nên sử dụng các thư viện như `Newtonsoft.Json` (`JObject`) hoặc thiết kế lớp C# linh hoạt (có các trường Optional) để không bị lỗi parse khi server gửi thêm dữ liệu mới.

---

## 2. Phân Loại Sự Kiện (Event Types)

Các sự kiện được chia làm hai nhóm chính:
1. **Sự Kiện UI (UI Events):** Dùng để hiển thị thông báo lên màn hình (Cảm ơn tặng quà, like, follow...).
2. **Sự Kiện Gameplay (Gameplay Events):** Dùng để can thiệp trực tiếp vào logic game (Sinh quái, rơi đồ, buff...).

---

## 3. Chi Tiết Các Sự Kiện UI (UI Events)

Nhóm sự kiện này thường được Server gửi ngay lập tức khi nhận được tín hiệu từ TikTok.

### 3.1. Hiển Thị Tặng Quà (`SHOW_GIFT`)
- **Mô tả:** Gửi khi một người dùng tặng quà xong (đã kết thúc chuỗi combo quà).
- **Rate Limit:** Áp dụng chống spam ở phía Server.
- **Payload mở rộng:** `giftName`, `amount`
```json
{
  "type": "SHOW_GIFT",
  "user": "tên_người_dùng",
  "giftName": "Rose",
  "amount": 1,
  "timestamp": 1717081510000,
  // ... (bao gồm cả các trường Base)
}
```

### 3.2. Hiển Thị Lượt Thích (`SHOW_LIKE`)
- **Mô tả:** Gửi để cập nhật số lượng Like của một người dùng. Số lượng này được cộng dồn theo từng đợt gửi của TikTok.
- **Payload mở rộng:** `likeCount`
```json
{
  "type": "SHOW_LIKE",
  "user": "tên_người_dùng",
  "likeCount": 15,
  "timestamp": 1717081510000,
  // ...
}
```

### 3.3. Hiển Thị Người Theo Dõi Mới (`SHOW_FOLLOW`)
- **Mô tả:** Gửi ngay lập tức khi có người ấn Follow kênh (không có cooldown).
- **Payload mở rộng:** Không có thêm trường đặc biệt.
```json
{
  "type": "SHOW_FOLLOW",
  "user": "tên_người_dùng",
  "timestamp": 1717081510000,
  // ...
}
```

---

## 4. Chi Tiết Các Sự Kiện Gameplay (Gameplay Events)

Nhóm sự kiện này kích hoạt các hành động thực tế trong Game thế giới.

### 4.1. Sinh Quái Vật (`SPAWN_ENEMY`)
- **Mô tả:** Kích hoạt từ nhiều nguồn khác nhau (Quà tặng, Chat đúng từ khóa, Đạt mốc Like, hoặc Follow).
- **Nguồn:**
  - `gift`: Nếu tên quà khớp Rule -> Sinh Boss. Nếu không -> Sinh quái Default.
  - `chat`: Nếu bình luận khớp keyword xác định (Cooldown: 15s/người).
  - `like`: Khi tổng like của user đạt mốc (VD: 100 like).
  - `follow`: Khi có người theo dõi.
- **Payload mở rộng:** `enemy`, `amount` (cộng thêm các trường Dynamic từ Config nếu có)
```json
{
  "type": "SPAWN_ENEMY",
  "enemy": "slime",      // Tên hoặc ID loại quái vật (vd: "slime", "zombie_runner", "boss_tiktok")
  "amount": 1,           // Số lượng (VD: bằng số combo quà, hoặc chỉ định theo mốc like)
  "source": "gift",      // 'gift', 'chat', 'like', hoặc 'follow'
  "user": "tên_người_dùng",
  "targetUser": "tên_người_dùng", // Hoặc "Community" nếu kích hoạt bởi cống hiến chung
  "timestamp": 1717081510000,
  // ...
}
```

### 4.2. Các Loại Sự Kiện Mở Rộng Khác (Custom Types)
Dựa vào cơ chế Config động của Node.js Server, có thể có thêm các type khác mà Unity cần chuẩn bị đón đầu:
- `DROP_ITEM`: Rơi vật phẩm.
- `HEAL_BASE`: Hồi máu cứ điểm.
*(Cấu trúc các custom event này sẽ kế thừa phần Base và mang theo các trường cấu hình động).*

---

## 5. Sự Kiện Bị Bỏ Qua (Không Gửi Về Unity)

**Tham Gia Phòng (Member Join):**
- **Trạng thái:** Node.js Server có ghi Log nhưng **KHÔNG GỬI** sang Unity.
- **Lý do:** Tránh nghẽn mạng do lượng người ra vào có thể rất lớn. Unity Client không cần quan tâm sự kiện này.
