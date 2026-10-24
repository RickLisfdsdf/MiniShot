<div align="center">

[简体中文](README.md) | [English](README.en.md) | [繁體中文](README.zh-TW.md) | **日本語** | [한국어](README.ko.md) | [Русский](README.ru.md)

<img src="docs/icon.png" width="96" alt="MiniShot">

# MiniShot

**開いたドロップダウンメニューも撮れる超軽量スクリーンショットツール — 32 KB・インストール不要**

スクショのキーを押した瞬間にメニューが閉じてしまう？ MiniShot はキーをシステムレベルで横取りするので、対象アプリには何も届かず、メニューは開いたまま撮影できます。

![Windows](https://img.shields.io/badge/Windows-10%20%7C%2011-0078D6?logo=windows)
![Size](https://img.shields.io/badge/exe-32%20KB-brightgreen)
![License](https://img.shields.io/badge/license-MIT-green)
![Stars](https://img.shields.io/github/stars/RickLisfdsdf/MiniShot?style=social)

### [⬇️ 最新版をダウンロード](https://github.com/RickLisfdsdf/MiniShot/releases/latest)

</div>

> **注意：** 現在 UI は**簡体字中国語のみ**です。キー操作は言語に関係なく同じです。

![範囲選択と注釈](docs/overlay.png)

## なぜ作ったか

多くのアプリ（特に Delphi/VCL、産業用制御、CAD ソフト）は、**キー入力やフォーカスの変化があるとドロップダウンを閉じます**：

- PrtSc：Windows 11 が Snipping Tool を起動してフォーカスを奪う → メニューが閉じる
- Ctrl+Shift+S などの組み合わせ：Ctrl/Alt を押した瞬間にアプリに届く → メニューが閉じる
- 遅延キャプチャは使えるが、毎回数秒待つ必要がある

**MiniShot** は低レベルキーボードフックでキャプチャキーの押下・解放を両方とも握りつぶします。キャプチャ中はウィンドウを出さず、まず画面全体を静止画として固定してから範囲を選びます。

## 機能

| | |
|---|---|
| 📌 **ドロップダウンが撮れる** | キーはシステムレベルで横取り。前面アプリには届かず、フォーカスも移らない |
| ✂️ **範囲選択 + 赤枠** | 固定された画面で範囲選択し、その中をドラッグして赤枠を描画（複数可・元に戻す対応） |
| 📋 **クリップボードのみ** | クリップボードにコピーするだけでファイル保存なし。ファイルが必要な場合はフォルダーの空白を右クリックして「スクショを貼り付け」 |
| ⌨️ **キャプチャキー変更** | PrtSc は常に有効。任意の単一キー（ノートPCの Fn+F1 など）を追加可能 |
| ⏱️ **遅延キャプチャ** | トレイメニューから 3 秒 / 5 秒後 |
| 🖱️ **右クリック「スクショを貼り付け」** | フォルダーやデスクトップの空白を右クリックして、クリップボード画像を PNG で保存。Windows 11 の**最上位メニュー**にも追加可能 |
| 🪶 **超軽量** | exe 32 KB、常駐メモリ約 4 MB、インストール不要（Windows 標準の .NET Framework 4） |
| 🖥️ **高 DPI** | 150% / 200% スケーリング、マルチモニター対応 |

| 結果 |
|---|
| ![結果](docs/result.png) |

## クイックスタート

1. [Releases](https://github.com/RickLisfdsdf/MiniShot/releases/latest) から zip をダウンロードし、**固定のフォルダー**に展開
2. `MiniShot.exe` を実行（トレイにアイコンが出ます）
3. **PrtSc** → ドラッグで範囲選択 → 中をドラッグして赤枠 → **Enter** かダブルクリックで完了

| 操作 | キー |
|---|---|
| キャプチャ | PrtSc（または設定したキー） |
| 全画面 | 選択せずに Enter |
| 赤枠 | 選択範囲内でドラッグ |
| 元に戻す | Ctrl+Z |
| 完了 | Enter / ダブルクリック |
| キャンセル | Esc / 右クリック |

### オプション：Windows 11 の最上位右クリックメニューに追加

1. 設定 → システム → 開発者向け → **開発者モード**をオン
2. `install-menu.ps1` を右クリック → PowerShell で実行
3. フォルダーの空白を右クリックすると最上位に「粘贴截图」が表示されます（出ない場合はエクスプローラーを再起動）

## 仕組み

```
PrtSc を押す
   ├─ WH_KEYBOARD_LL フックが押下/解放とも 1 を返す（握りつぶす）→ 前面アプリに届かずメニューは開いたまま
   ├─ CopyFromScreen で仮想画面全体を取得（まだウィンドウは出さない）
   └─ 全画面の最前面ウィンドウで静止画を表示 → 範囲選択 → 赤枠 → クリップボードへコピー
```

単一キーのみ対応：Ctrl/Alt などの修飾キーは押した瞬間に前面アプリへ届くためです。

## ビルド

`build.bat` を実行。`MiniShot.exe` は Windows 標準の `csc.exe` でビルドでき、追加インストール不要です。`MiniShotMenu.dll` には VS 2022 Build Tools が必要です。

<details>
<summary><b>FAQ</b></summary>

**キーを押しても反応しない：** 前面アプリが管理者として実行されている場合、MiniShot も管理者として実行してください。

**PrtSc キーがない：** トレイ →「设置截图键」→ 使いたいキー（Fn+F1 など）を押します。

**ウイルス対策ソフトの誤検知：** グローバルキーボードフックを使う未署名ツールは誤検知されることがあります。コードはすべて公開しています。

</details>

## 気に入ったら

⭐ Star をいただけると励みになります！

## License

[MIT](LICENSE)
