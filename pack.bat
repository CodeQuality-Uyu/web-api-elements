@echo off
echo Batch to pack projects
del /q packs\*
set /p input= Type the new version: 
dotnet build --configuration Release /p:Version=%input%
dotnet pack --configuration Release /p:Version=%input% --no-build --output ./packs/.
pause