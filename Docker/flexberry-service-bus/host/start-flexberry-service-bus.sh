#!/bin/sh
set -x
exec > /tmp/startup.log
exec 2>&1
export MONO_IOMAP=all
until telnet FlexberryServiceBusPostgres 5432 </dev/null | grep Connected
do 
	echo "Wait for start up postgres service" 
	sleep 5;
done
/usr/bin/mono-service2 --no-daemon -l:/tmp/flexberry-service-bus.lock -d:/opt/flexberry-service-bus -m:flexberry-service-bus NewPlatform.Flexberry.ServiceBus.WinServiceHost.exe
