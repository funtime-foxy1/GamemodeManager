@echo off
setlocal
set localDll=.\bin\Debug\GamemodeManager.dll
set destFile="D:\SteamLibrary\steamapps\common\Lethal Company\BepInEx\plugins\GamemodeManager\GamemodeManager.dll"
set dest="D:\SteamLibrary\steamapps\common\Lethal Company\BepInEx\plugins\GamemodeManager\"
set unityAssetBundleLocation="D:\Projects\UNITY_wtfamidoing\lethal_mods\AssetBundles\StandaloneWindows\funfoxrr_gamemodes"
set dep1=.\GamemodeManagerAPI.dll
set dep2=.\LethalNetworkAPI.dll
set dep3=.\OdinSerializer.dll
set dep4=.\OdinSerializer.pdb
set dep5=.\TerminalApi.dll

copy /y %localDll% %dest% >NUL

set EXE=Lethal Company.exe
FOR /F %%x IN ("%EXE%") do set EXE_=%%x
FOR /F %%x IN ('tasklist /NH /FI "IMAGENAME eq %EXE%"') DO IF %%x == %EXE_% (
	echo Please close Lethal Company before executing this file.
	exit /b
)

if "%1"=="/q" (
	echo Successfully updated Gamemode Manager!
	exit /b
)

if "%1"=="/f" (
	copy /y %dep1% %dest% >NUL
	copy /y %dep2% %dest% >NUL
	copy /y %dep3% %dest% >NUL
	copy /y %dep4% %dest% >NUL
	copy /y %dep5% %dest% >NUL
	copy /y %unityAssetBundleLocation% %dest% >NUL
	echo Successfully updated everything!

	exit /b
)

if "%1"=="info" (
	echo /q: Only update gamemode Manager
	echo /f: Update dependencies, asset bundle, and GamemodeManager.

	exit /b
)

set /p updateDep="Update dependencies? y/n "
set /p updateUnityFile="Update asset bundle? y/n "

if "%updateDep%" == "y" (
	copy /y %dep1% %dest% >NUL
	copy /y %dep2% %dest% >NUL
	copy /y %dep3% %dest% >NUL
	copy /y %dep4% %dest% >NUL
	copy /y %dep5% %dest% >NUL
	echo.Successfully updated dependencies!
)

if "%updateUnityFile%" == "y" (
	copy /y %unityAssetBundleLocation% %dest% >NUL
	echo.Successfully updated unity asset bundle!
)

echo Successfully updated Gamemode Manager!