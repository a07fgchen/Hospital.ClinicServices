# Hospital.ClinicServices 後端 API

這是一個基於 ASP.NET Core Web API 與 Entity Framework Core 建立的醫療掛號系統後端，提供門診、病患、排班、掛號與叫號相關 API。

## 專案目標

這個專案用來展示：
- 後端 API 架構設計
- 資料庫模型與資料初始化
- 基本的業務邏輯與資料流程
- 與前端的串接能力

## 技術堆疊

- ASP.NET Core Web API
- Entity Framework Core
- SQL Server
- Swagger / OpenAPI
- SignalR

## 本機開發

```bash
cd Hospital.ClinicServices.WebApi
dotnet restore
dotnet build
dotnet run
```

## API 文件

啟動後可透過 Swagger 查看 API：

```text
http://localhost:5000/swagger
```

## 目前功能

- 病患相關 API
- 科別與門診資料 API
- 排班與掛號相關 API
- 叫號流程相關 API

## 面試展示 checklist

- [x] 後端可成功建置
- [x] 基本 API 與資料模型已建立
- [ ] 主要業務流程可完整串接
- [ ] 錯誤處理與資料驗證補齊
- [ ] 測試覆蓋與重構優化
- [ ] README 與 demo 說明整理完成
