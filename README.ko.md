<div align="center">

[简体中文](README.md) | [English](README.en.md) | [繁體中文](README.zh-TW.md) | [日本語](README.ja.md) | **한국어** | [Русский](README.ru.md)

<img src="docs/icon.png" width="96" alt="MiniShot">

# MiniShot

**열린 드롭다운 메뉴까지 캡처하는 초소형 스크린샷 도구 — 32 KB, 설치 불필요**

캡처 단축키를 누르는 순간 메뉴가 닫혀 버리나요? MiniShot은 키를 시스템 수준에서 가로채기 때문에 대상 프로그램은 아무것도 받지 못하고, 메뉴는 열린 채로 캡처됩니다.

![Windows](https://img.shields.io/badge/Windows-10%20%7C%2011-0078D6?logo=windows)
![Size](https://img.shields.io/badge/exe-32%20KB-brightgreen)
![License](https://img.shields.io/badge/license-MIT-green)
![Stars](https://img.shields.io/github/stars/RickLisfdsdf/MiniShot?style=social)

### [⬇️ 최신 버전 다운로드](https://github.com/RickLisfdsdf/MiniShot/releases/latest)

</div>

> **참고:** 현재 UI는 **중국어(간체)만** 지원합니다. 키 조작은 언어와 관계없이 동일합니다.

![영역 선택과 주석](docs/overlay.png)

## 왜 만들었나

많은 프로그램(특히 Delphi/VCL, 산업 제어, CAD 소프트웨어)은 **키 입력이나 포커스 변화가 생기면 드롭다운을 닫습니다**:

- PrtSc: Windows 11이 캡처 도구를 띄워 포커스를 빼앗음 → 메뉴가 닫힘
- Ctrl+Shift+S 같은 조합키: Ctrl/Alt를 누르는 순간 프로그램에 전달됨 → 메뉴가 닫힘
- 지연 캡처는 되지만 매번 몇 초씩 기다려야 함

**MiniShot**은 저수준 키보드 후크로 캡처 키의 누름과 뗌을 모두 삼킵니다. 캡처 중에는 창을 띄우지 않고 먼저 전체 화면을 정지 이미지로 고정한 뒤 영역을 선택합니다.

## 기능

| | |
|---|---|
| 📌 **드롭다운 캡처** | 키를 시스템 수준에서 가로채 전경 프로그램이 받지 못하고 포커스도 유지 |
| ✂️ **영역 선택 + 빨간 상자** | 고정된 화면에서 영역을 선택하고 그 안을 드래그해 빨간 상자 그리기(여러 개, 실행 취소 지원) |
| 📋 **클립보드만** | 클립보드에 복사만 하고 파일 저장 없음. 파일이 필요하면 폴더 빈 곳 우클릭 "스크린샷 붙여넣기" 사용 |
| ⌨️ **캡처 키 지정** | PrtSc는 항상 사용 가능, 노트북의 Fn+F1 같은 단일 키를 추가로 지정 가능 |
| ⏱️ **지연 캡처** | 트레이 메뉴에서 3초 / 5초 후 |
| 🖱️ **우클릭 "스크린샷 붙여넣기"** | 폴더나 바탕 화면 빈 곳을 우클릭해 클립보드 이미지를 PNG로 저장, Windows 11 **최상위 메뉴**에도 추가 가능 |
| 🪶 **초소형** | exe 32 KB, 상주 메모리 약 4 MB, 설치 불필요(Windows 내장 .NET Framework 4) |
| 🖥️ **고 DPI** | 150% / 200% 배율, 다중 모니터 지원 |

| 결과 |
|---|
| ![결과](docs/result.png) |

## 빠른 시작

1. [Releases](https://github.com/RickLisfdsdf/MiniShot/releases/latest)에서 zip을 받아 **고정된 폴더**에 압축 해제
2. `MiniShot.exe` 실행(트레이에 아이콘 표시)
3. **PrtSc** → 드래그로 영역 선택 → 안쪽을 드래그해 빨간 상자 → **Enter** 또는 더블클릭으로 완료

| 동작 | 키 |
|---|---|
| 캡처 | PrtSc(또는 지정한 키) |
| 전체 화면 | 선택 없이 Enter |
| 빨간 상자 | 선택 영역 안에서 드래그 |
| 실행 취소 | Ctrl+Z |
| 완료 | Enter / 더블클릭 |
| 취소 | Esc / 우클릭 |

### 선택: Windows 11 최상위 우클릭 메뉴에 추가

1. 설정 → 시스템 → 개발자용 → **개발자 모드** 켜기
2. `install-menu.ps1` 우클릭 → PowerShell에서 실행
3. 폴더 빈 곳을 우클릭하면 최상위에 "粘贴截图"가 표시됩니다(안 보이면 탐색기 재시작)

## 동작 원리

```
PrtSc 누름
   ├─ WH_KEYBOARD_LL 후크가 누름/뗌 모두 1을 반환(삼킴) → 전경 프로그램에 전달되지 않아 메뉴 유지
   ├─ CopyFromScreen으로 가상 화면 전체 캡처(아직 창 없음)
   └─ 전체 화면 최상위 창에 정지 이미지 표시 → 영역 선택 → 빨간 상자 → 클립보드로 복사
```

단일 키만 지원: Ctrl/Alt 같은 수정 키는 누르는 순간 전경 프로그램에 전달되기 때문입니다.

## 빌드

`build.bat` 실행. `MiniShot.exe`는 Windows 내장 `csc.exe`로 빌드되어 추가 설치가 필요 없습니다. `MiniShotMenu.dll`은 VS 2022 Build Tools가 필요합니다.

<details>
<summary><b>FAQ</b></summary>

**키를 눌러도 반응이 없음:** 전경 프로그램이 관리자 권한으로 실행 중이면 MiniShot도 관리자 권한으로 실행하세요.

**PrtSc 키가 없음:** 트레이 → "设置截图键" → 사용할 키(Fn+F1 등)를 누르세요.

**백신 오탐:** 전역 키보드 후크를 쓰는 서명 없는 도구는 오탐될 수 있습니다. 코드는 모두 공개되어 있습니다.

</details>

## 마음에 드셨다면

⭐ Star로 응원해 주세요!

## License

[MIT](LICENSE)
