@ECHO OFF
xcopy "%1\Debug" "%1\x86" /I /Y
"C:\Program Files (x86)\Microsoft SDKs\Windows\v7.0A\Bin\corflags" %1\x86\%2 /32BIT+