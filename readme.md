# sizingservers.beholder.agent
    2017 Sizing Servers Lab  
    University College of West-Flanders, Department GKG


![flow](readme_img/flow.png)

This project is part of a computer hardware inventorization solution, together with sizingservers.beholder.api and sizingservers.beholder.frontend.

Agents are installed on the computers you want to inventorize. These communicate with the REST API which stores hardware info. The front-end app visualizes that info.

## Languages, libraries, tools, technologies used and overview
The code is encapsulated in a **Visual Studio 2017** solution. The available agents are console applications.

### Agent selector
The sizingservers.beholder.agent console app does nothing more than checking the OS and launching the correct agent.

Runs using **dotnet core 1.1**, target framework netcoreapp1.1.

### Linux agent
Reads the standard output of inxi (<https://github.com/smxi/inxi>). Check the SystemInformationRetreiver class to see how this works.

Runs using **dotnet core 1.1**, target framework netcoreapp1.1.

### Windows agent
Uses WMI to gather system info.

Runs as a Windows executable, target framework net462.

### Shared functionality
Contains the agent configuration functionality, the SystemInformations struct and a reporter class to periodically send JSON serialized info (**NewtonSoft.Json**) to the API over http.

Multi-targets netcoreapp1.1 and net462 to be usable in the Linux- and Windows agent both. Right-click the project and click *Edit...* to check this.

## Build
You need the dotnet SDK (<https://www.microsoft.com/net/download/core#/sdk>) to build the source and the .Net framework: your build machine needs to be Windows (I think).

You need to be connected to the Internet for restoring NuGet packages.

Execute *build.cmd* (debug config):

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
    
## Configure

### sizingservers.beholder.agent.conf
    # URL to the REST API where info will be pushed to.
    # endpoint http://x.x.x.x:0000
    endpoint http://localhost:5000
    
    # API Key
    apiKey <insert a SHA-512 of a piece of text here> 

    # Uncomment / edit the one you want. Defaults to reportEvery day.
    # reportEvery minute
    # reportEvery 20 minutes
    # reportEvery hour
    # reportEvery 5 hours
    reportEvery day
    
Needs to be configured in the Linux and Windows Build both.

## Run
You need the .NET core runtime (<https://www.microsoft.com/net/download/core#/runtime>) to run the build: 1.1.2 at the time of writing.

You need the .NET framework on Windows, but you have that by default.

Execute run.cmd or run.sh.

BETTER is to run the Linux- or Windows agent as a service.  
For that you need to use either the start script for Linux or NSSM for Windows in the Linux or the Windows folder.