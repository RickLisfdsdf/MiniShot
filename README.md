<div align="center">

**简体中文** | [English](README.en.md) | [繁體中文](README.zh-TW.md) | [日本語](README.ja.md) | [한국어](README.ko.md) | [Русский](README.ru.md)

<img src="docs/icon.png" width="96" alt="MiniShot">

# MiniShot

**能截到下拉菜单的极小截图工具，32 KB，不用安装**

软件的下拉菜单一按截图快捷键就收起？MiniShot 在系统底层把截图键拦下来，目标软件什么都收不到，菜单原样截下来。

![Windows](https://img.shields.io/badge/Windows-10%20%7C%2011-0078D6?logo=windows)
![Size](https://img.shields.io/badge/exe-32%20KB-brightgreen)
![.NET Framework](https://img.shields.io/badge/.NET%20Framework-4.x%20(系统自带)-512BD4)
![License](https://img.shields.io/badge/license-MIT-green)
![Stars](https://img.shields.io/github/stars/RickLisfdsdf/MiniShot?style=social)

### [⬇️ 下载最新版](https://github.com/RickLisfdsdf/MiniShot/releases/latest)

</div>

![框选并标注](docs/overlay.png)

## 为什么做这个

很多软件（尤其是 Delphi / VCL、工控、CAD 类软件）的下拉菜单**收到任何按键或者失去焦点就会收起**：

- 按 PrtSc：Win11 会弹出截图工具抢走焦点，菜单收起
- 按 Ctrl+Shift+S、Alt+A 之类的组合键：按下 Ctrl / Alt 的那一瞬间，按键已经传给了软件，菜单收起
- 用截图软件的延时截图：能用，但每次都要等几秒

**MiniShot 的做法**：用低级键盘钩子把截图键的按下和松开都“吞掉”，前台软件完全收不到；截图时不弹任何窗口，先把整个屏幕冻结成图片，再让你框选。菜单收不收起都不影响了。

## 功能

| | |
|---|---|
| 📌 **截得到下拉菜单** | 截图键被系统底层拦截，前台软件收不到按键，也不会失去焦点 |
| ✂️ **框选 + 红框标注** | 冻结全屏后拖动框选，在选区里拖动就画红框，可以画多个，支持撤销 |
| 📋 **只进剪贴板** | 结果复制到剪贴板，不保存文件；需要文件时在目标文件夹空白处右键「粘贴截图」 |
| ⌨️ **自定义截图键** | PrtSc 一直可用；可再绑一个任意单键，比如笔记本的 Fn+F1 |
| ⏱️ **延时截图** | 托盘菜单 3 秒 / 5 秒后截图，适合只能用鼠标打开的菜单 |
| 🖱️ **右键「粘贴截图」** | 在桌面或任意文件夹空白处右键，把剪贴板里的图片存成 PNG；Win11 可放进**一级菜单** |
| 🪶 **极小** | exe 32 KB，常驻内存约 4 MB，不用安装，不依赖任何运行库（Windows 自带 .NET Framework 4） |
| 🖥️ **高分屏** | 支持 150% / 200% 缩放和多显示器，按物理像素截图 |

| 截图结果（复制到剪贴板） |
|---|
| ![结果](docs/result.png) |

## 快速开始

1. 到 [Releases](https://github.com/RickLisfdsdf/MiniShot/releases/latest) 下载 zip，解压到一个**固定的文件夹**（例如 `D:\Tools\MiniShot`）
2. 双击 `MiniShot.exe`，托盘里出现图标
3. 按 **PrtSc**：拖动框选 → 在选区里拖动画红框 → **Enter** 或双击完成（进剪贴板）

| 操作 | 按键 |
|---|---|
| 截图 | PrtSc（或你设置的截图键） |
| 截全屏 | 不框选，直接按 Enter |
| 画红框 | 在选区里拖动 |
| 撤销红框 | Ctrl+Z / 「撤销」按钮 |
| 完成 | Enter / 双击选区 / 「完成」按钮 |
| 重新框选 | 在选区外按下鼠标 |
| 取消 | Esc / 右键 |

托盘图标右键可以设置：截图键、开机启动、旧式右键菜单「粘贴截图」。

### 可选：把「粘贴截图」放进 Win11 一级右键菜单

Win11 的新右键菜单只显示“已打包应用”的扩展。MiniShot 带了一个 6.5 KB 的菜单扩展 `MiniShotMenu.dll`，用开发者模式注册成轻量应用包即可，不需要签名证书：

1. 设置 → 系统 → 开发者选项 → 打开**开发人员模式**
2. 右键 `install-menu.ps1` → 使用 PowerShell 运行
3. 在桌面或文件夹空白处右键，第一层就有「粘贴截图」（没出现就重启一下资源管理器）

卸载：`powershell -File install-menu.ps1 -Uninstall`

## 工作原理

```
按下 PrtSc
   │
   ├─ WH_KEYBOARD_LL 低级键盘钩子：按下和松开都返回 1（吞掉）
   │     → 前台软件收不到任何按键消息，焦点也没变，下拉菜单保持展开
   │
   ├─ CopyFromScreen 截下整个虚拟屏幕（此时还没弹任何窗口）
   │
   └─ 弹出全屏置顶窗口，显示冻结的画面 → 框选 → 画红框 → 复制到剪贴板
         （这时菜单收起也无所谓了，画面已经冻结）
```

- **为什么只能用单键**：Ctrl / Alt 这类修饰键按下的那一刻就会传给前台软件，菜单马上收起；单键（或笔记本的 Fn+某键，Fn 由键盘固件处理，系统收不到）才能被完整拦截
- **Win11 一级菜单**：`menu/MiniShotMenu.cpp` 实现 `IExplorerCommand`，不链接 C 运行库；`AppxManifest.xml` 是稀疏包清单，通过 `Add-AppxPackage -Register -ExternalLocation` 注册

## 自己编译

```bat
build.bat
```

- `MiniShot.exe`：用 Windows 自带的 `csc.exe`（.NET Framework 4）编译，**什么都不用装**
- `MiniShotMenu.dll`（可选）：需要 Visual Studio 2022 Build Tools（C++ 工作负载）
- `tools/DocShots.cs`：生成 README 里的截图（假界面 + MiniShot 自己的绘制代码）

<details>
<summary><b>常见问题</b></summary>

**按截图键没反应？**
确认托盘里 MiniShot 在运行。如果前台软件是**以管理员身份运行**的，MiniShot 也要以管理员身份运行，否则钩子收不到按键。

**笔记本没有 PrtSc 键？**
托盘 → 「设置截图键」→ 按一下想用的键（比如 Fn+F1），会自动识别实际收到的是哪个键。

**被设成截图键的键在其他软件里失效了？**
是的，被拦截的键所有软件都收不到。换一个不常用的键，比如 F9、Pause、ScrollLock。

**杀毒软件报毒？**
MiniShot 使用了全局键盘钩子，未签名的小工具偶尔会被误报。代码全部公开，可以自己编译。

**Win11 右键菜单没出现「粘贴截图」？**
确认开发人员模式已打开、`install-menu.ps1` 执行成功，然后在任务管理器里重启“Windows 资源管理器”。

**界面语言？**
目前界面只有简体中文。

</details>

## 路线图

- [ ] 箭头、文字、马赛克标注
- [ ] 贴图（截图钉在屏幕上）
- [ ] 截图后可调整选区大小
- [ ] 英文界面

## 觉得有用？

点个 ⭐ Star 支持一下，也欢迎提 Issue 说说你在哪个软件里遇到了“下拉菜单截不到”的问题。

[![Star History Chart](https://api.star-history.com/svg?repos=RickLisfdsdf/MiniShot&type=Date)](https://star-history.com/#RickLisfdsdf/MiniShot&Date)

## License

[MIT](LICENSE)
