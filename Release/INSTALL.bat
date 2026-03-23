@echo off
setlocal enabledelayedexpansion
chcp 65001 > nul

echo.
echo  +==========================================+
echo  ^|       DORADCA WIEZY - Installer          ^|
echo  ^|   Tower Advisor mod for Slay the Spire 2 ^|
echo  +==========================================+
echo.

:: Sprawdz czy DLL jest obok instalatora
if not exist "%~dp0DoradcaWiezy.dll" (
    echo  [!] Nie znaleziono DoradcaWiezy.dll obok tego pliku!
    echo  [!] DoradcaWiezy.dll not found next to this file!
    pause
    exit /b 1
)

:: Sprawdz czy gra jest uruchomiona
tasklist /FI "IMAGENAME eq SlayTheSpire2.exe" 2>NUL | find /I "SlayTheSpire2.exe" > NUL
if not errorlevel 1 (
    echo  [!] Zamknij Slay the Spire 2 przed instalacja!
    echo  [!] Close Slay the Spire 2 before installing!
    pause
    exit /b 1
)

:: Szukaj gry - sprawdzaj krok po kroku
set GAME_DIR=

if exist "C:\SteamLibrary\steamapps\common\Slay the Spire 2\SlayTheSpire2.exe" set "GAME_DIR=C:\SteamLibrary\steamapps\common\Slay the Spire 2"
if exist "D:\SteamLibrary\steamapps\common\Slay the Spire 2\SlayTheSpire2.exe" set "GAME_DIR=D:\SteamLibrary\steamapps\common\Slay the Spire 2"
if exist "E:\SteamLibrary\steamapps\common\Slay the Spire 2\SlayTheSpire2.exe" set "GAME_DIR=E:\SteamLibrary\steamapps\common\Slay the Spire 2"
if exist "F:\SteamLibrary\steamapps\common\Slay the Spire 2\SlayTheSpire2.exe" set "GAME_DIR=F:\SteamLibrary\steamapps\common\Slay the Spire 2"
if exist "G:\SteamLibrary\steamapps\common\Slay the Spire 2\SlayTheSpire2.exe" set "GAME_DIR=G:\SteamLibrary\steamapps\common\Slay the Spire 2"
if exist "H:\SteamLibrary\steamapps\common\Slay the Spire 2\SlayTheSpire2.exe" set "GAME_DIR=H:\SteamLibrary\steamapps\common\Slay the Spire 2"

if exist "C:\Steam\steamapps\common\Slay the Spire 2\SlayTheSpire2.exe" set "GAME_DIR=C:\Steam\steamapps\common\Slay the Spire 2"
if exist "D:\Steam\steamapps\common\Slay the Spire 2\SlayTheSpire2.exe" set "GAME_DIR=D:\Steam\steamapps\common\Slay the Spire 2"
if exist "E:\Steam\steamapps\common\Slay the Spire 2\SlayTheSpire2.exe" set "GAME_DIR=E:\Steam\steamapps\common\Slay the Spire 2"
if exist "F:\Steam\steamapps\common\Slay the Spire 2\SlayTheSpire2.exe" set "GAME_DIR=F:\Steam\steamapps\common\Slay the Spire 2"
if exist "G:\Steam\steamapps\common\Slay the Spire 2\SlayTheSpire2.exe" set "GAME_DIR=G:\Steam\steamapps\common\Slay the Spire 2"

if exist "%ProgramFiles(x86)%\Steam\steamapps\common\Slay the Spire 2\SlayTheSpire2.exe" set "GAME_DIR=%ProgramFiles(x86)%\Steam\steamapps\common\Slay the Spire 2"
if exist "%ProgramFiles%\Steam\steamapps\common\Slay the Spire 2\SlayTheSpire2.exe" set "GAME_DIR=%ProgramFiles%\Steam\steamapps\common\Slay the Spire 2"

:: Jesli nie znaleziono - zapytaj
if "!GAME_DIR!"=="" (
    echo  [?] Nie znaleziono automatycznie. Podaj sciezke recznie.
    echo  [?] Not found automatically. Enter path manually.
    echo.
    echo  Sciezka do folderu z SlayTheSpire2.exe:
    echo  Path to folder containing SlayTheSpire2.exe:
    echo  Przyklad: G:\SteamLibrary\steamapps\common\Slay the Spire 2
    echo.
    set /p GAME_DIR="> "
    if "!GAME_DIR!"=="" (
        echo  Anulowano. / Cancelled.
        pause & exit /b 1
    )
    if not exist "!GAME_DIR!\SlayTheSpire2.exe" (
        echo  [!] Nie znaleziono SlayTheSpire2.exe w: !GAME_DIR!
        pause & exit /b 1
    )
)

echo  Gra znaleziona / Game found:
echo  !GAME_DIR!
echo.

:: Stworz foldery
if not exist "!GAME_DIR!\mods" mkdir "!GAME_DIR!\mods"
if not exist "!GAME_DIR!\mods\DoradcaWiezy" mkdir "!GAME_DIR!\mods\DoradcaWiezy"

:: Kopiuj DLL
copy /Y "%~dp0DoradcaWiezy.dll" "!GAME_DIR!\mods\DoradcaWiezy\DoradcaWiezy.dll" > nul

if errorlevel 1 (
    echo  [!] Blad! Sprobuj: prawy przycisk na INSTALL.bat - Uruchom jako administrator
    echo  [!] Error! Try: right-click INSTALL.bat - Run as administrator
    pause & exit /b 1
)

echo  +==========================================+
echo  ^|        INSTALACJA UDANA!  OK  :D         ^|
echo  +==========================================+
echo.
echo  Zainstalowano do:
echo  !GAME_DIR!\mods\DoradcaWiezy\
echo.
echo  1. Uruchom gre / Launch game
echo  2. Settings ^> Mods ^> wlacz DoradcaWiezy / enable DoradcaWiezy
echo.
echo  Skroty / Hotkeys:
echo    F1 - HUD walki wl/wyl / Combat HUD toggle
echo    F2 - Ustawienia / Settings  (wybor jezyka PL/EN tutaj!)
echo.
pause
