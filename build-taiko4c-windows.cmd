@echo off
setlocal

cd /d "%~dp0"

set "OUT=%CD%\artifacts\taiko4c-win-x64"

echo [1/3] Restoring desktop dependencies...
dotnet restore osu.Desktop.slnf
if errorlevel 1 goto :error

echo [2/3] Cleaning previous published build...
if exist "%OUT%" rmdir /s /q "%OUT%"
mkdir "%OUT%"
if errorlevel 1 goto :error

echo [3/3] Publishing Windows x64 self-contained Release build...
dotnet publish osu.Desktop\osu.Desktop.csproj -f net10.0 -r win-x64 -c Release --self-contained -o "%OUT%" -p:Version=2026.804.2-taiko4c
if errorlevel 1 goto :error

echo.
echo Build completed successfully.
echo Output: %OUT%
echo Run: "%OUT%\osu!.exe"
exit /b 0

:error
echo.
echo Build failed with exit code %errorlevel%.
exit /b %errorlevel%
