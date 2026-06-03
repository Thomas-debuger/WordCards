# WordCards (單字卡程式)

這是「視窗程式設計 (II)」課程的單字卡學習應用程式專案 。本程式旨在提供使用者一個視覺化與聽覺兼具的單字背誦環境，支援手動瀏覽、自動播放音效，以及即時編輯並儲存單字卡內容的功能。

## 專案簡介 (Project Introduction)

WordCards 是一個使用 C# Windows Forms 開發的桌面應用程式。系統啟動時會自動載入 TSV 格式的單字文字檔 (`WordCards.txt`) ，並透過介面呈現單字、音標與詳細解釋 。專案整合了 Windows Media Player 元件來播放每個單字的標準發音 ，並具備自動定時切換單字的功能，方便使用者進行反覆的聽讀訓練 。

## 主要功能 (Features)

*  **單字資料載入**：啟動時自動讀取 `WordCards.txt`，並顯示於左側的清單中 。

* **多媒體音效支援**：點擊單字清單時，會自動顯示詳細內容並透過 Windows Media Player 播放對應的 `.mp3` 音效檔 。

* **自動播放模式**：內建 Auto Play 功能（間隔 2000 毫秒），啟用後系統會自動依序切換單字並播放發音 。


* **快捷鍵支援**：
  * `Enter`：切換並播放「下一個」單字 。
  * `Space` (空白鍵)：重新播放「目前」單字的發音 。

* **單字編輯與儲存**：雙擊左側單字清單 (DoubleClick)，可開啟編輯表單 (`frmEditWord`) 修改音標、音檔路徑與解釋 。儲存後會即時更新集合資料，並將變更回寫至 `WordCards.txt` 檔案中 。



## 開發環境與相依性 (Prerequisites)

* **語言與框架**：C# / .NET Framework 4.8 (Windows Forms) 

* **外部參考元件**：Windows Media Player (COM 元件) 

## 執行說明 (How to Run)

1. **準備資料夾與檔案**：
   * 確保專案根目錄下有 `WordCards.txt` 單字庫檔案，並在 Visual Studio 屬性面板中將 **「複製到輸出目錄」** 設定為 **「有更新時才複製」** 。

   * 在專案中建立 `Sound\A` 資料夾，並將所有單字的 `.mp3` 音效檔放入。同樣全選音效檔，將其屬性設定為 **「有更新時才複製」** 。

2. **加入 COM 參考**：
   * 在方案總管的「參考」點擊右鍵 ->「加入參考」 。

   * 選擇「COM」-> 勾選 `Windows Media Player`，點擊確定 。

3. **建置與執行**：
   * 按下 `F5` 啟動專案，程式會自動載入 302 個單字並顯示於介面上 。

4. **操作方式**：
   * 點選右側的 **「Play」** 按鈕可開始自動播放，按鈕會變更為 **「Stop」** 。

   * 在主畫面上可直接使用 `Enter` 或 `Space` 鍵控制播放進度 。

   * 對左側 `lstWordList` 內的任一單字「連按兩下左鍵」，即可開啟編輯視窗 。修改完畢按下「儲存」，程式會自動覆寫原始文字檔 。

## 程式截圖 (Screenshots)

### 1. 主畫面與單字自動播放 (Main Interface)

<img width="734" height="497" alt="image" src="https://github.com/user-attachments/assets/c303b340-27c7-4ed0-ae06-847cef52ae4f" />


### 2. 編輯單字表單 (Edit Word Form)

<img width="301" height="647" alt="image" src="https://github.com/user-attachments/assets/c093e45a-34dd-4afa-80b9-59c82819ed2d" />
