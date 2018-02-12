#!/bin/sh
set -x
exec > /tmp/startup.log
exec 2>&1
export MONO_IOMAP=all
until telnet HWSBPostgres 5432 </dev/null | grep Connected
do 
	echo "Wait for start up postgres service" 
	sleep 5;
done
/usr/bin/mono-service2 --no-daemon -l:/tmp/highwaysb.lock -d:/opt/flexberry-hwsb -m:highwaysb NewPlatform.Flexberry.ServiceBus.WinServiceHost.exe
