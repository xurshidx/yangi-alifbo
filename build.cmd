@echo off
rem Yangi Alifbo - Windows yig'ish skripti
rem Foydalanish: build.cmd [test]
setlocal
cd /d "%~dp0"
set CSC=%WINDIR%\Microsoft.NET\Framework\v4.0.30319\csc.exe
if not exist "%CSC%" (
  echo .NET Framework 4 kompilyatori topilmadi: %CSC%
  exit /b 1
)
set OPTS=/nologo /codepage:65001 /langversion:5 /platform:anycpu /optimize+
if not exist out mkdir out

"%CSC%" %OPTS% /target:library /out:out\YangiAlifbo.Core.dll src\Core\*.cs || exit /b 1
"%CSC%" %OPTS% /target:exe /r:out\YangiAlifbo.Core.dll /out:out\CoreTests.exe tests\CoreTests.cs || exit /b 1

if /i "%1"=="test" (
  out\CoreTests.exe || exit /b 1
)
echo Yig'ish tugadi: out\
endlocal
