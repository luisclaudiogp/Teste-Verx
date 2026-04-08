@echo off
echo Running Consolidado Unit Tests...
dotnet test src/Consolidado/Consolidado.UnitTests/Consolidado.UnitTests.csproj
if %errorlevel% neq 0 exit /b %errorlevel%
echo Tests completed successfully!
