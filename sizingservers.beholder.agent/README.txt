sizingservers.beholder.agent

    2017 Sizing Servers Lab  
    University College of West-Flanders, Department GKG


This project is part of a computer hardware inventorization solution, together with sizingservers.beholder.api and sizingservers.beholder.frontend.

Agents are installed on the computers you want to inventorize. These communicate with the REST API which stores hardware info. The front-end app visualizes that info.

You need the .NET core runtime (<https://www.microsoft.com/net/download/core#/runtime>) to run the build: 1.1.2 at the time of writing.

You need the .NET framework on Windows, but you have that by default.

Execute run.cmd or run.sh.

BETTER is to run the Linux- or Windows agent as a service.
For that you need to use either the start script for Linux or NSSM for Windows in the Linux or the Windows folder.