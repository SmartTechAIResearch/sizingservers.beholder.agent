# 2017 Sizing Servers Lab
# University College of West-Flanders, Department GKG
#
#
#           IF RUNNING THIS SCRIPT DOES NOT WORK REPLACE ALL \r\n by \n !
#
# This is a start script for running a beholder agent as a daemon (service).
# It runs that agent using nohup.
#
# To stop the service use ./stop-daemon.sh.
#
# Please make sure that you have read and write rights on the directory containing this script. 

# Display the help.
if [ "$1" == --help -o "$1" == -h ]; then
    echo "Synopsis: ./start-as-daemon.sh [--help (-h)]"
    exit 0
fi

# Start the service if it is not running already.
PID=$(ps aux | grep -w "sizingservers.beholder.agent.linux.dll" | grep -v grep | awk '{print $2}')
if [ "$PID" != "" ]; then
    echo "sizingservers.beholder.agent.linux is already running! (PID $PID)"
    exit 0
fi


# If stuff goes wrong, you should catch it and log it yourself.
nohup dotnet sizingservers.beholder.agent.linux.dll >out 2>errors &

# The errors, if any, won't be written to file immediately. Therefor this sleep.
sleep 2
ERRORS=""
if [ -f errors ]; then
    ERRORS=$(cat errors)
fi
if [ "$ERRORS" == "" ]; then
    echo "sizingservers.beholder.agent.linux is initializing... (PID $!)"
    echo "To see the output: tail -f out"
else
    echo "Failed to start sizingservers.beholder.agent.linux correctly now or previously! Do you have wite and read rights on this location? Delete the errors file when appropriate."
    echo $ERRORS
fi
exit 0
