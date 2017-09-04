REM 2017 Sizing Servers Lab
REM University College of West-Flanders, Department GKG 
echo sizingservers.beholder.agent for Windows build script
echo ----------
rmdir /S /Q Build
cd sizingservers.beholder.agent
dotnet restore
dotnet publish -c Debug
cd ..\sizingservers.beholder.agent.windows
dotnet restore
dotnet build -c Debug
cd ..
copy /Y Build\netcoreapp2.0\publish\* Build\
rmdir /S /Q Build\netcoreapp2.0\