@echo off
chcp 65001 >nul
rem 编译 MiniShot.exe（系统自带 .NET Framework 编译器）和 MiniShotMenu.dll（需要 VS Build Tools，可选）
cd /d %~dp0
C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe -nologo -codepage:65001 -target:winexe -optimize+ -win32icon:app.ico -out:MiniShot.exe MiniShot.cs || exit /b 1
echo MiniShot.exe OK
if exist "C:\Program Files (x86)\Microsoft Visual Studio\2022\BuildTools\VC\Auxiliary\Build\vcvars64.bat" (
    call menu\build.bat && echo MiniShotMenu.dll OK
) else (
    echo 未找到 VS 2022 Build Tools，跳过 MiniShotMenu.dll（Win11 一级右键菜单）
)
