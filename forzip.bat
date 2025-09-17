@echo off
REM === 壓縮 Unity 專案：只打包程式碼與設定（不含美術、音樂等大檔） ===
set ZIPNAME=UnityScriptsOnly.zip

if exist %ZIPNAME% del %ZIPNAME%

REM 打包 Assets/Scripts + 必要的設定檔
powershell -command ^
"Compress-Archive -Path 'Assets\Scripts','ProjectSettings\ProjectVersion.txt','Packages\manifest.json' -DestinationPath %ZIPNAME% -Force"

echo.
echo 打包完成：%ZIPNAME%
pause
