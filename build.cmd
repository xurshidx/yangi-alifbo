@echo off
rem Yangi Alifbo - Windows yig'ish skripti
rem Foydalanish: build.cmd            - yig'ish
rem              build.cmd test       - yig'ish + yadro testlari
rem              build.cmd smoke      - yig'ish + Word/Excel/PowerPoint avtomatik testlari
rem              build.cmd release    - yig'ish + testlar + out\YangiAlifbo_v1.0.0.zip
setlocal
cd /d "%~dp0"
set VER=2.0.0
set CSC=%WINDIR%\Microsoft.NET\Framework\v4.0.30319\csc.exe
set FW=%WINDIR%\Microsoft.NET\Framework\v4.0.30319
if not exist "%CSC%" (
  echo .NET Framework 4 kompilyatori topilmadi: %CSC%
  exit /b 1
)
set OPTS=/nologo /codepage:65001 /langversion:5 /platform:anycpu /optimize+
set REFS=/r:"%FW%\System.Core.dll" /r:"%FW%\Microsoft.CSharp.dll" /r:"%FW%\System.Windows.Forms.dll" /r:"%FW%\System.Drawing.dll"
if not exist out mkdir out

"%CSC%" %OPTS% /target:library /out:out\YangiAlifbo.Core.dll src\Core\*.cs || exit /b 1
"%CSC%" %OPTS% /target:exe /r:out\YangiAlifbo.Core.dll /out:out\CoreTests.exe tests\CoreTests.cs || exit /b 1
rem Office DLL yadro kodini o'z ichiga oladi: COM CodeBase orqali yuklanganda yonidagi
rem bog'liqliklar (YangiAlifbo.Core.dll) topilmaydi (80070002), shuning uchun bitta DLL.
"%CSC%" %OPTS% /target:library %REFS% /resource:src\Office\Ribbon.xml,YangiAlifbo.Office.Ribbon.xml /resource:keys\alifbo_pub.xml,YangiAlifbo.Office.alifbo_pub.xml /out:out\YangiAlifbo.Office.dll src\Core\*.cs src\Office\*.cs || exit /b 1
"%CSC%" %OPTS% /target:exe /win32manifest:src\Setup\app.manifest /out:out\YangiAlifbo.Setup.exe src\Setup\Setup.cs || exit /b 1
"%CSC%" %OPTS% /target:exe %REFS% /r:out\YangiAlifbo.Office.dll /out:out\OfficeSmoke.exe tests\OfficeSmoke.cs || exit /b 1
"%CSC%" %OPTS% /target:exe %REFS% /r:out\YangiAlifbo.Office.dll /out:out\LicenseSmoke.exe tests\LicenseSmoke.cs || exit /b 1

if /i "%1"=="test" (
  out\CoreTests.exe || exit /b 1
)
if /i "%1"=="smoke" (
  out\CoreTests.exe || exit /b 1
  out\OfficeSmoke.exe word || exit /b 1
  out\OfficeSmoke.exe excel || exit /b 1
  out\OfficeSmoke.exe ppt || exit /b 1
)
if /i "%1"=="release" (
  out\CoreTests.exe || exit /b 1
  if exist out\YangiAlifbo_v%VER% rmdir /s /q out\YangiAlifbo_v%VER%
  mkdir out\YangiAlifbo_v%VER%
  copy /y out\YangiAlifbo.Office.dll out\YangiAlifbo_v%VER%\ >nul
  copy /y out\YangiAlifbo.Setup.exe out\YangiAlifbo_v%VER%\ >nul
  copy /y README.txt out\YangiAlifbo_v%VER%\ >nul
  powershell -NoProfile -Command "Compress-Archive -Force -Path 'out\YangiAlifbo_v%VER%\*' -DestinationPath 'out\YangiAlifbo_v%VER%.zip'" || exit /b 1
  echo Paket: out\YangiAlifbo_v%VER%.zip
)
echo Yig'ish tugadi: out\
endlocal
