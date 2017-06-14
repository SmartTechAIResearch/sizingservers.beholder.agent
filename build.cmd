REM 2017 Sizing Servers Lab
REM University College of West-Flanders, Department GKG 
echo sizingservers.beholder.agent build script
echo ----------
rmdir /S /Q Build
cd sizingservers.beholder.agent
dotnet restore
dotnet publish -c Debug
cd ..\sizingservers.beholder.agent.linux
dotnet restore
dotnet publish -c Debug
cd ..\sizingservers.beholder.agent.windows
dotnet restore
dotnet build -c Debug
cd ..
copy /Y Build\netcoreapp1.1\publish\* Build\
rmdir /S /Q Build\netcoreapp1.1\
copy /Y Build\Linux\netcoreapp1.1\publish\* Build\Linux
rmdir /S /Q Build\Linux\netcoreapp1.1\