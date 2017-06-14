# 2017 Sizing Servers Lab
# University College of West-Flanders, Department GKG
#
#
#           IF RUNNING THIS SCRIPT DOES NOT WORK REPLACE ALL \r\n by \n !
#
# Stops a daemon started using ./start-as-daemon.sh. 
# Please make sure that you have read and write rights on the directory containing this script.

# Display the help.
if [ "$1" == --help -o "$1" == -h ]; then
    echo "Synopsis: ./stop-daemon.sh [--help (-h)]"
    exit 0
fi

# Should be only one...
PIDS=$(ps aux | grep -w "sizingservers.beholder.agent.linux.dll" | grep -v grep | awk '{print $2}')

if [ "$PIDS" == "" ]; then
    echo "sizingservers.beholder.agent.linux.dll was not running..."
else
    for PID in $PIDS; do
        kill -9 $PID 2> /dev/null
        echo "sizingservers.beholder.agent.linux stopped! (PID $PID)"
    done
fi

# Cleanup files
if [ -f out ]; then
    rm -f out
fi
if [ -f errors ]; then
    ERRORS=$(cat errors)
    if [ "$ERRORS" == "" ]; then
        rm -f errors
    fi
fi
exit 0
