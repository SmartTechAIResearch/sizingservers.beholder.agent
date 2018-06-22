REM 2018 Sizing Servers Lab
REM University College of West-Flanders, Department GKG 
echo sizingservers.beholder.agent for Windows start as service script
echo Please note that this script needs administrator privileges. If not 'ran as administrator', please do so.
echo ----------
nssm remove sizingservers.beholder.agent confirm
nssm install sizingservers.beholder.agent sizingservers.beholder.agent.windows.exe
nssm start sizingservers.beholder.agent
echo If you get system info on the frontend , everything is working. If not, check if you have set the endpoint in the config correctly or the Windows Event Viewer for errors, if any.