# 2017 Sizing Servers Lab
# University College of West-Flanders, Department GKG 
echo sizingservers.beholder.agent build script
echo ----------
rm -rf Build
cd sizingservers.beholder.agent
dotnet restore
dotnet publish -c Debug
cd ../sizingservers.beholder.agent.linux
dotnet restore
dotnet publish -c Debug
cd ../sizingservers.beholder.agent.windows
dotnet restore
dotnet build -c Debug
cd ..
mv Build/netcoreapp1.1/publish/* Build
rm -r Build/netcoreapp1.1
mv Build/Linux/netcoreapp1.1/publish/* Build/Linux
rm -r Build/Linux/netcoreapp1.1