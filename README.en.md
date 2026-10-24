<div align="center">

[简体中文](README.md) | **English** | [繁體中文](README.zh-TW.md) | [日本語](README.ja.md) | [한국어](README.ko.md) | [Русский](README.ru.md)

<img src="docs/icon.png" width="96" alt="MiniShot">

# MiniShot

**A tiny screenshot tool that can capture open dropdown menus — 32 KB, no install**

Does a dropdown menu close the moment you press your screenshot hotkey? MiniShot swallows the key at the system level, so the target app never sees it and the menu stays open.

![Windows](https://img.shields.io/badge/Windows-10%20%7C%2011-0078D6?logo=windows)
![Size](https://img.shields.io/badge/exe-32%20KB-brightgreen)
![.NET Framework](https://img.shields.io/badge/.NET%20Framework-4.x%20(built--in)-512BD4)
![License](https://img.shields.io/badge/license-MIT-green)
![Stars](https://img.shields.io/github/stars/RickLisfdsdf/MiniShot?style=social)

### [⬇️ Download latest release](https://github.com/RickLisfdsdf/MiniShot/releases/latest)

</div>

> **Note:** the user interface is currently **Simplified Chinese only**. The key bindings below work the same regardless of language.

![Select and annotate](docs/overlay.png)

## Why

Many apps (especially Delphi/VCL, industrial-control and CAD software) close their dropdown menus **on any key press or focus change**:

- PrtSc: Windows 11 launches Snipping Tool, which steals focus — the menu closes
- Ctrl+Shift+S, Alt+A and similar combos: the instant Ctrl/Alt goes down the app receives it — the menu closes
- Delayed capture works, but you wait a few seconds every time

**MiniShot** installs a low-level keyboard hook that swallows both key-down and key-up of the capture key, so the foreground app receives nothing. No window is shown while capturing: the whole screen is frozen into an image first, then you select a region.

## Features

| | |
|---|---|
| 📌 **Captures dropdown menus** | The capture key is intercepted at the system level; the foreground app never sees it and keeps focus |
| ✂️ **Select + red boxes** | Drag to select on the frozen screen, then drag inside the selection to draw red rectangles (multiple, with undo) |
| 📋 **Clipboard only** | Copied to the clipboard, no file is written; right-click "Paste screenshot" in a folder when you need a file |
| ⌨️ **Custom capture key** | PrtSc always works; bind any additional single key, e.g. a laptop's Fn+F1 |
| ⏱️ **Delayed capture** | 3 s / 5 s from the tray menu |
| 🖱️ **"Paste screenshot" in Explorer** | Right-click an empty area of any folder or the desktop to save the clipboard image as PNG; can go into the **Windows 11 top-level menu** |
| 🪶 **Tiny** | 32 KB exe, ~4 MB RAM when idle, no install, no runtime to download (uses the built-in .NET Framework 4) |
| 🖥️ **High DPI** | 150% / 200% scaling and multiple monitors, captured at physical resolution |

| Result (copied to the clipboard) |
|---|
| ![Result](docs/result.png) |

## Quick start

1. Download the zip from [Releases](https://github.com/RickLisfdsdf/MiniShot/releases/latest) and extract it to a **permanent folder** (e.g. `D:\Tools\MiniShot`)
2. Run `MiniShot.exe` — an icon appears in the tray
3. Press **PrtSc**: drag to select → drag inside to draw red boxes → **Enter** or double-click to finish

| Action | Key |
|---|---|
| Capture | PrtSc (or your custom key) |
| Full screen | Press Enter without selecting |
| Draw red box | Drag inside the selection |
| Undo box | Ctrl+Z / Undo button |
| Finish | Enter / double-click / Done button |
| Reselect | Press the mouse outside the selection |
| Cancel | Esc / right-click |

Right-click the tray icon to set the capture key, autostart and the classic "Paste screenshot" context-menu entry.

### Optional: "Paste screenshot" in the Windows 11 top-level context menu

The new Windows 11 menu only shows extensions from packaged apps. MiniShot ships a 6.5 KB extension `MiniShotMenu.dll` that is registered as a sparse package in Developer Mode — no signing certificate needed:

1. Settings → System → For developers → turn on **Developer Mode**
2. Right-click `install-menu.ps1` → Run with PowerShell
3. Right-click an empty area of a folder or the desktop — "粘贴截图" (Paste screenshot) is on the first level (restart Explorer if it does not appear)

Uninstall: `powershell -File install-menu.ps1 -Uninstall`

## How it works

```
PrtSc pressed
   │
   ├─ WH_KEYBOARD_LL hook returns 1 for key-down and key-up (swallowed)
   │     → the foreground app gets no key message and keeps focus; the menu stays open
   │
   ├─ CopyFromScreen grabs the whole virtual screen (no window shown yet)
   │
   └─ Full-screen topmost window shows the frozen image → select → red boxes → copy to clipboard
```

- **Why single keys only:** modifier keys like Ctrl/Alt reach the foreground app the moment they are pressed. A single key (or Fn+key on laptops — Fn is handled by keyboard firmware) can be fully intercepted.
- **Windows 11 menu:** `menu/MiniShotMenu.cpp` implements `IExplorerCommand` without the C runtime; `AppxManifest.xml` is a sparse package manifest registered with `Add-AppxPackage -Register -ExternalLocation`.

## Build

```bat
build.bat
```

- `MiniShot.exe`: compiled with the `csc.exe` that ships with Windows (.NET Framework 4) — **nothing to install**
- `MiniShotMenu.dll` (optional): needs Visual Studio 2022 Build Tools (C++ workload)
- `tools/DocShots.cs`: renders the README screenshots

<details>
<summary><b>FAQ</b></summary>

**Nothing happens when I press the key?**
Make sure MiniShot is running in the tray. If the foreground app runs **as administrator**, MiniShot must too.

**My laptop has no PrtSc key.**
Tray → "设置截图键" (Set capture key) → press the key you want (e.g. Fn+F1).

**The capture key stopped working in other apps.**
Yes — an intercepted key is hidden from every app. Pick a rarely used key such as F9, Pause or ScrollLock.

**Antivirus warning?**
MiniShot uses a global keyboard hook, and small unsigned tools sometimes get false positives. All code is open; build it yourself if you prefer.

</details>

## Roadmap

- [ ] Arrows, text and blur annotations
- [ ] Pin screenshot on screen
- [ ] Resize selection after capture
- [ ] English UI

## Like it?

A ⭐ Star helps a lot. Issues about apps whose menus you couldn't capture are welcome too.

[![Star History Chart](https://api.star-history.com/svg?repos=RickLisfdsdf/MiniShot&type=Date)](https://star-history.com/#RickLisfdsdf/MiniShot&Date)

## License

[MIT](LICENSE)
