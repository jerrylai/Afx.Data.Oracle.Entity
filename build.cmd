@echo off
set Build="%SYSTEMDRIVE%\Program Files (x86)\Microsoft Visual Studio\2019\Enterprise\MSBuild\Current\Bin\MsBuild.exe"
if exist publish rd /s /q publish
%Build% "NET45/Afx.Data.Oracle.Entity/Afx.Data.Oracle.Entity.csproj" /t:Rebuild /p:Configuration=Release
cd publish
del /q/s *.pdb
del /q/s EntityFramework*
del /q/s Oracle.ManagedDataAccess*
del /q/s Afx.Data.dll Afx.Data.Entity.dll Afx.Data.Entity.xml Afx.Data.xml Afx.Data.Oracle.Entity.dll.config
pause