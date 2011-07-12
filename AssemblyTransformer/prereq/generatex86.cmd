@ECHO OFF
xcopy "%~1\Debug" "%~1\x86" /I /Y 
"%~1\..\..\..\prereq\CorFlags.exe" "%~1\x86\%~2" /Force /32BIT+ /nologo