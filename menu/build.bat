@echo off
call "C:\Program Files (x86)\Microsoft Visual Studio\2022\BuildTools\VC\Auxiliary\Build\vcvars64.bat" >nul
cd /d %~dp0
cl /nologo /O1 /GS- /GR- /Zl /utf-8 /LD MiniShotMenu.cpp /link /NODEFAULTLIB /ENTRY:DllMain /DEF:MiniShotMenu.def kernel32.lib /OUT:..\MiniShotMenu.dll shlwapi.lib shell32.lib ole32.lib user32.lib uuid.lib
del *.obj ..\MiniShotMenu.exp ..\MiniShotMenu.lib 2>nul
