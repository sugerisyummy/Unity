@echo off
setlocal enabledelayedexpansion

rem 檢查是否為 Git repo
git rev-parse --is-inside-work-tree >NUL 2>&1 || (echo Not a git repo.& exit /b 1)

rem 取得目前分支
for /f "delims=" %%i in ('git rev-parse --abbrev-ref HEAD') do set BRANCH=%%i

rem 訊息：可自行傳入，否則用時間戳
if "%~1"=="" (
  for /f "tokens=1-4 delims=/ " %%a in ("%date%") do set TODAY=%%a-%%b-%%c
  set MSG=chore:auto-save %TODAY% %time%
) else (
  set MSG=%*
)

echo === git add ===
git add -A

echo === git commit ===
git diff --cached --quiet && (
  echo No staged changes, skip commit.
) || (
  git commit -m "%MSG%" || echo Commit skipped/failed.
)

echo === git pull --rebase origin %BRANCH% ===
git pull --rebase origin %BRANCH% || (
  echo Pull failed. Please resolve conflicts then run again.
  exit /b 1
)

echo === git push origin %BRANCH% ===
git push origin %BRANCH%

echo Done: pushed %BRANCH%.
endlocal
