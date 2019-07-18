::You must keep this file in the solution folder for it to work.
::Make sure to pass the solution configuration when calling it (either Debug or Release)

::Set the directories in the setdirectories.bat file if you want a different folder than Kerbal Space Program
::EXAMPLE:
:: SET KSPPATH=C:\Program Files (x86)\Steam\steamapps\common\Kerbal Space Program
:: SET KSPPATH2=C:\Users\Malte\Desktop\Kerbal Space Program
call "%~dp0\SetDirectories.bat"

IF DEFINED KSPPATH (ECHO KSPPATH is defined) ELSE (SET KSPPATH=C:\Kerbal Space Program)
SET SOLUTIONCONFIGURATION=Debug

RMDIR "%KSPPATH%\GameData\TiltEm\" /S /Q

mkdir "%KSPPATH%\GameData\TiltEm\"
mkdir "%KSPPATH%\GameData\TiltEm\Plugins"

"%~dp0..\External\pdb2mdb\pdb2mdb.exe" "%~dp0..\TiltEm\bin\%SOLUTIONCONFIGURATION%\TiltEm.dll"
"%~dp0..\External\pdb2mdb\pdb2mdb.exe" "%~dp0..\TiltEmKopernicus\bin\%SOLUTIONCONFIGURATION%\TiltEmKopernicus.dll"

xcopy /Y "%~dp0..\TiltEm\bin\%SOLUTIONCONFIGURATION%\TiltEm.*" "%KSPPATH%\GameData\TiltEm\Plugins"
xcopy /Y "%~dp0..\TiltEm\bin\%SOLUTIONCONFIGURATION%\0Harmony.dll" "%KSPPATH%\GameData\TiltEm\Plugins"
xcopy /Y "%~dp0..\TiltEmKopernicus\bin\%SOLUTIONCONFIGURATION%\TiltEmKopernicus.*" "%KSPPATH%\GameData\TiltEm\Plugins"

xcopy /Y /S "%~dp0..\Resources\*.*" "%KSPPATH%\GameData\TiltEm"