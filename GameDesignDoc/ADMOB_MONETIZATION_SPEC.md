# Tài Liệu Kỹ Thuật: Hệ Thống Kiếm Tiền & Quảng Cáo (AdMob Monetization Spec)
**Dự án:** Project Zombie (2D Top-down Roguelike Action Cổ Phong)  
**Tác giả:** AI Assistant  
**Trạng thái:** Bản thiết kế đề xuất  

---

## 1. Mục Tiêu & Nguyên Tắc Thiết Kế

1. **Tối ưu Doanh Thu (eCPM):** Ưu tiên dạng **Rewarded Ads (Xem video nhận thưởng)** mang lại trải nghiệm tích cực và tỷ lệ tương tác cao.
2. **Không Gây Ức Chế (Fair Play):** Tuyệt đối không hiển thị quảng cáo ép buộc (Banner/Interstitial) khi người chơi đang trong trận chiến né quái / dùng kỹ năng.
3. **Mediation Realtime Bidding:** Sử dụng **Google AdMob kết hợp Unity Ads / AppLovin Bidding** để đạt tỷ lệ lấp đầy (Fill Rate) > 98% và eCPM cao nhất tại Việt Nam & Quốc tế.

---

## 2. Danh Sách Vị Trí Đặt Quảng Cáo (Ad Placements)

| Loại Quảng Cáo | Vị Trí Trong Game | Tác Dụng Cho Người Chơi | Tần Suất / Ràng Buộc |
| :--- | :--- | :--- | :--- |
| **Rewarded Video** 🌟 | **Màn hình Kết quả Game Over** ([`GameOverScreenPresenter.cs`](file:///c:/Users/thuon/Unity/Projectzombie/Assets/Features/UI/GameOverScreenPresenter.cs)) | **Nhân đôi Cổ Tiền (x2 Currency)** kiếm được trong Run vừa qua | Không giới hạn số lần, kích hoạt tự nguyện |
| **Rewarded Video** | **Hồi Sinh Ngay Lập Tức (Revive)** khi nhân vật chết ([`HealthSystem.cs`](file:///c:/Users/thuon/Unity/Projectzombie/Assets/Features/Player/HealthSystem.cs)) | Giữ trọn mạch chơi cho người chơi khi đang có Run tốt | Tối đa 1 lần / mỗi Run |
| **Rewarded Video** | **Cửa Hàng Nâng Cấp Meta** ([`MetaUpgradeShopPresenter.cs`](file:///c:/Users/thuon/Unity/Projectzombie/Assets/Features/UI/MetaUpgradeShopPresenter.cs)) | Nhận ngay **100–200 Cổ Tiền miễn phí** hàng ngày | Giới hạn 3–5 lần / ngày (Cooldown 5 phút) |
| **Rewarded Video** | **Bảng Chọn Nâng Cấp Khi Lên Cấp** ([`UpgradeUIView.cs`](file:///c:/Users/thuon/Unity/Projectzombie/Assets/Features/UI/UpgradeUIView.cs)) | **Đổi thẻ kỹ năng (Reroll)** khi không ra thẻ ưng ý | Tối đa 2 lần / mỗi Run |
| **Interstitial (Xen kẽ)** | Khi bấm **`Chơi Tiếp`** hoặc **`Về Sảnh Chính`** | Giữ nhịp chuyển tiếp game | Chỉ hiển thị sau mỗi **2–3 Run** (Cooldown tối thiểu 180s) |
| **Collapsible Banner** | Đáy màn hình **Main Hub / Meta Shop** | Tăng doanh thu tĩnh | Chỉ bật ở Menu chính, tắt hoàn toàn khi vào In-game |

---

## 3. Kiến Trúc C# Hệ Thống Quảng Cáo (`Core/Ads`)

```
Assets/Core/Ads/
├── IAdService.cs          (Interface định nghĩa: ShowRewarded, ShowInterstitial, ShowBanner)
├── AdMobManager.cs        (Quản lý nạp Ads, Cache Ads ngầm, Callbacks Main Thread)
└── AdUnitConfig.cs        (ScriptableObject lưu ID AdUnit Android/iOS/TestMode)
```

### 3.1. Interface C# Chuẩn (`IAdService.cs`)
```csharp
public interface IAdService
{
    void Initialize();
    bool IsRewardedAdReady();
    void ShowRewardedAd(System.Action onSuccess, System.Action onFailed = null);
    void ShowInterstitialAd(System.Action onClosed = null);
    void ShowBanner();
    void HideBanner();
}
```

---

## 4. Kế Hoạch Triển Khai

1. **Import SDK:** Cài đặt `GoogleMobileAds.unitypackage` (bản 9.x+) và chạy *Android Resolver*.
2. **Setup Test Mode:** Cấu hình Test Ad Unit IDs của Google để test nội bộ an toàn trên Unity Editor.
3. **Tích hợp UI:**
   - Thêm nút **`x2 Thưởng (Ads)`** vào UI Kết Quả ([`GameOverScreenView.cs`](file:///c:/Users/thuon/Unity/Projectzombie/Assets/Features/UI/GameOverScreenView.cs)).
   - Thêm nút **`Nhận Miễn Phí`** vào UI Cửa Hàng.
