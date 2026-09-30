<div align="center">

[简体中文](README.md) | [English](README.en.md) | **繁體中文** | [日本語](README.ja.md) | [한국어](README.ko.md) | [Русский](README.ru.md)

<img src="docs/icon.png" width="96" alt="MiniShot">

# MiniShot

**截得到下拉選單的極小截圖工具，32 KB，免安裝**

軟體的下拉選單一按截圖快捷鍵就收起？MiniShot 在系統底層把截圖鍵攔下來，目標軟體什麼都收不到，選單原樣截下來。

![Windows](https://img.shields.io/badge/Windows-10%20%7C%2011-0078D6?logo=windows)
![Size](https://img.shields.io/badge/exe-32%20KB-brightgreen)
![License](https://img.shields.io/badge/license-MIT-green)
![Stars](https://img.shields.io/github/stars/RickLisfdsdf/MiniShot?style=social)

### [⬇️ 下載最新版](https://github.com/RickLisfdsdf/MiniShot/releases/latest)

</div>

> **注意：** 目前介面只有**簡體中文**。

![框選並標註](docs/overlay.png)

## 為什麼做這個

很多軟體（尤其是 Delphi / VCL、工控、CAD 類軟體）的下拉選單**收到任何按鍵或失去焦點就會收起**：

- 按 PrtSc：Win11 會跳出剪取工具搶走焦點，選單收起
- 按 Ctrl+Shift+S、Alt+A 等組合鍵：按下 Ctrl / Alt 的瞬間按鍵已傳給軟體，選單收起
- 延時截圖：可以用，但每次都要等幾秒

**MiniShot 的做法**：用低階鍵盤鉤子把截圖鍵的按下和放開都「吞掉」，前景軟體完全收不到；截圖時不彈任何視窗，先把整個螢幕凍結成圖片，再讓你框選。

## 功能

| | |
|---|---|
| 📌 **截得到下拉選單** | 截圖鍵在系統底層被攔截，前景軟體收不到按鍵、也不會失去焦點 |
| ✂️ **框選 + 紅框標註** | 凍結全螢幕後拖曳框選，在選區內拖曳畫紅框，可畫多個、可復原 |
| 📋 **自動儲存 + 複製** | 同時複製到剪貼簿、存成 PNG（預設桌面，可更改） |
| ⌨️ **自訂截圖鍵** | PrtSc 一直可用；可再綁一個任意單鍵，例如筆電的 Fn+F1 |
| ⏱️ **延時截圖** | 系統匣選單 3 秒 / 5 秒後截圖 |
| 🖱️ **右鍵「貼上截圖」** | 在桌面或任意資料夾空白處按右鍵，把剪貼簿圖片存成 PNG；Win11 可放進**第一層選單** |
| 🪶 **極小** | exe 32 KB，常駐記憶體約 4 MB，免安裝，使用 Windows 內建 .NET Framework 4 |
| 🖥️ **高 DPI** | 支援 150% / 200% 縮放與多螢幕，按實體像素截圖 |

| 截圖結果 |
|---|
| ![結果](docs/result.png) |

## 快速開始

1. 到 [Releases](https://github.com/RickLisfdsdf/MiniShot/releases/latest) 下載 zip，解壓到**固定的資料夾**
2. 執行 `MiniShot.exe`，系統匣出現圖示
3. 按 **PrtSc**：拖曳框選 → 在選區內拖曳畫紅框 → **Enter** 或雙擊儲存

| 操作 | 按鍵 |
|---|---|
| 截圖 | PrtSc（或自訂的截圖鍵） |
| 全螢幕 | 不框選直接按 Enter |
| 畫紅框 | 在選區內拖曳 |
| 復原紅框 | Ctrl+Z |
| 儲存 | Enter / 雙擊選區 |
| 取消 | Esc / 右鍵 |

### 選用：把「貼上截圖」放進 Win11 第一層右鍵選單

1. 設定 → 系統 → 開發人員專用 → 開啟**開發人員模式**
2. 右鍵 `install-menu.ps1` → 用 PowerShell 執行
3. 在資料夾空白處按右鍵，第一層就有「粘贴截图」（沒出現就重新啟動檔案總管）

## 運作原理

```
按下 PrtSc
   ├─ WH_KEYBOARD_LL 鉤子：按下與放開都回傳 1（吞掉）→ 前景軟體收不到、選單不收起
   ├─ CopyFromScreen 擷取整個虛擬螢幕（此時尚未彈出任何視窗）
   └─ 全螢幕置頂視窗顯示凍結畫面 → 框選 → 紅框 → 儲存
```

只能用單鍵：Ctrl / Alt 等修飾鍵按下瞬間就會傳給前景軟體。

## 自行編譯

執行 `build.bat`。`MiniShot.exe` 用 Windows 內建的 `csc.exe` 編譯，什麼都不用裝；`MiniShotMenu.dll` 需要 VS 2022 Build Tools。

<details>
<summary><b>常見問題</b></summary>

**按鍵沒反應？** 若前景軟體以系統管理員身分執行，MiniShot 也要以系統管理員身分執行。

**筆電沒有 PrtSc？** 系統匣 → 「设置截图键」→ 按一下想用的鍵（例如 Fn+F1）。

**防毒軟體誤報？** 使用了全域鍵盤鉤子的未簽章小工具偶爾會被誤報，程式碼全部公開，可自行編譯。

</details>

## 喜歡的話

點個 ⭐ Star 支持一下！

## License

[MIT](LICENSE)
