# Hospital.ClinicServices 前端專案

這是一個以 Vue 3 + TypeScript 建立的醫療掛號系統前端，負責呈現門診、排班、掛號與叫號流程，並與後端 API 串接。

## 專案目標

這個專案的目標是展示一個完整的全端醫療流程雛形，包含：
- 門診與科別頁面
- 排班查詢與掛號入口
- 叫號流程導覽
- 與後端 API 的資料串接

## 技術堆疊

- Vue 3
- TypeScript
- Vue Router
- Pinia
- Vite
- Vitest

## 本機開發

```bash
cd Hospital.ClinicServices.Web
npm install
npm run dev
```

## 建置與測試

```bash
npm run build
npm run test:unit
npm run lint
```

## 專案結構

- src/views：頁面元件
- src/components：共用 UI 元件
- src/router：路由設定
- src/stores：狀態管理

## 面試展示 checklist

- [x] 前端可啟動並成功建置
- [x] 主要頁面已建立
- [ ] 主流程從 UI 到 API 能完整跑通
- [ ] 錯誤提示與載入狀態補齊
- [ ] 基本單元測試補齊
- [ ] README 與 demo 內容整理完成
- [ ] UI 互動與資料流再優化
