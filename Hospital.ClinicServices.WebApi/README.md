# Hospital.ClinicServices - 智慧門診預約掛號與實時叫號系統後端 API

本專案是一個基於 **.NET 8 / 9 Web API** 開發的企業級醫療系統後端雛形，模擬醫院核心 HIS 系統中的「門診網路掛號」與「診間實時叫號」業務流程。專案採用嚴謹的 **三層式架構 (3-Tier Architecture)** 設計，特別針對醫療系統核心的**高併行安全性**、**資料一致性**與**低延遲即時推播**進行深度技術實作。

---

## 🚀 技術亮點與核心防禦

### 1. 高併發控制與防止超額掛號 (Concurrency Control)
*   **技術實作**：採用 MS SQL Server 原生的行級鎖機制 **`WITH (UPDLOCK, ROWLOCK)`**。
*   **解決痛點**：在早晨開放掛號等瞬間高流量（High Concurrency）情境下，多個執行緒同時爭搶最後一筆名額時，此機制能將該排班資料列鎖定，強制進行序列化處理。
*   **成果**：百分之百杜絕**競態條件 (Race Condition)**，確保實際掛號人數絕對不會超過設定限額 (`MaxQuota`)，守護醫療系統對資料精準度的極致要求。

### 2. 資料庫交易完整性 (Database Transaction)
*   **技術實作**：使用 EF Core 的 `BeginTransactionAsync()` 實作原子操作。
*   **解決痛點**：將「檢查剩餘名額」、「防重複掛號驗證」、「更新排班計數」與「寫入掛號紀錄表」四大步驟包裝在同一個資料庫交易中。
*   **成果**：確保操作的**原子性 (Atomicity)**，若任何一步因網路中斷或異常失敗，系統自動進行 `Rollback`（回滾），保證跨資料表（Schedules & Appointments）的資料永遠同步，絕不產生髒資料。

### 3. 低延遲精準即時推播 (SignalR WebSocket Grouping)
*   **技術實作**：整合 **ASP.NET Core SignalR** 實作雙向即時通訊，並導入 **Group (群組頻道)** 架構。
*   **解決痛點**：當醫生呼叫下一位號碼時，後端立即推播最新看診進度。若採用全局廣播 (`Clients.All`)，將會對全醫院無關的看板與民眾手機發送無效封包，造成巨大的伺服器頻寬浪費。
*   **成果**：客戶端（看板/手機）僅需訂閱特定門診的頻道（如 `Clinic_1`），即可在 **1 秒內** 接收到精準的叫號更新，兼顧極致的系統效能與低載荷傳輸。

### 4. 百萬級資料庫效能優化 (Database Performance)
*   **技術實作**：透過 EF Core Fluent API 在 `OnModelCreating` 中手動調校資料庫索引。
    *   為 `Patients.NationalId` 建立 **唯一索引 (Unique Index)**，加速病歷查詢並死守底層防線。
    *   為 `Schedules` 建立（`ServiceDate`, `Shift`）**複合索引 (Composite Index)**。
    *   為 `Appointments` 建立（`ScheduleId`, `SequenceNumber`）唯一複合索引。
*   **成果**：使醫院每天高達數萬次的「查詢明日門診」與「叫號進度追蹤」請求，能讓 SQL Server 進行精準的 **Index Seek（索引搜尋）**，而非低效的全表掃描 (Table Scan)，大幅降低資料庫 CPU 負載。

---

## 📐 系統分層架構 (3-Tier Architecture)

專案遵循物件導向設計原則（SOLID），將職責徹底分離，確保系統具備高可維護性與可測試性：

```text
[Client / Swagger / FrontEnd]
         │  (HTTP / WebSocket)
         ▼
 ┌────────────────────────────────────────────────────────┐
 │ 1. Controllers (表現層)                                │
 │    - 負責 API 入口路由與基礎 ModelState 欄位驗證         │
 │    - 依據執行結果回傳嚴謹的 HTTP 狀態碼 (201 Created/400)│
 └───────────────────────┬────────────────────────────────┘
                         │ (Dependency Injection)
                         ▼
 ┌────────────────────────────────────────────────────────┐
 │ 2. Services (商業邏輯層)                              │
 │    - 系統核心大腦，處理所有掛號演算法、安全鎖與排隊隊列    │
 │    - 串接 SignalR Hub 負責調度即時推播訊息            │
 └───────────────────────┬────────────────────────────────┘
                         │ (EF Core ORM)
                         ▼
 ┌────────────────────────────────────────────────────────┐
 │ 3. Data / Entities (資料存取層)                        │
 │    - ClinicDbContext 封裝資料庫連線與 Fluent API 優化   │
 │    - Entities 透過 Data Annotations 嚴格限制資料型態    │
 └────────────────────────────────────────────────────────┘
```

---

## 🛠️ 開發環境與技術棧

*   **後端架構**：.NET 8.0 / .NET 9.0 (ASP.NET Core Web API)
*   **開發工具**：VS Code / .NET CLI / Git
*   **資料庫 ORM**：Entity Framework Core 
*   **真實資料庫**：Microsoft SQL Server
*   **即時通訊**：ASP.NET Core SignalR (WebSocket)
*   **API 文件**：Swagger / OpenAPI (內建)

---

## 🏁 如何在本機執行

### 1. 複製專案
```bash
git clone <你的 GitHub 專案網址>
cd Hospital.ClinicServices
```

### 2. 設定資料庫連線字串
打開 `Hospital.ClinicServices.WebApi/appsettings.json`，修改 `ConnectionStrings` 以符合您的本機 MS SQL 環境：
```json
"DefaultConnection": "Server=localhost;Database=HospitalClinicDb;Trusted_Connection=True;TrustServerCertificate=True;"
```

### 3. 執行資料庫遷移 (Migrations)
確保您本機已安裝 MS SQL，並在 WebApi 專案目錄下執行以下指令自動建立資料庫與資料表：
```bash
cd Hospital.ClinicServices.WebApi
dotnet ef database update
```

### 4. 啟動 Web API 專案
```bash
dotnet run
```

### 5. 開啟 API 文件測試
專案啟動後，請於瀏覽器輸入以下網址進入 Swagger 測試介面：
```text
https://localhost:5001/swagger  (實際埠號請依終端機顯示為準)
```
