#!/bin/sh
set -x
exec > /tmp/startup.log
exec 2>&1
export MONO_IOMAP=all

# из-за того, что в mono 6.x mono-service пытается загружать сборки из GAC
# возникала ошибка загрузки типа HttpConfigurationExtensions, т.к. использовалась
# сборка System.Web.Http.dll старой версии
# данные настройки позволили заставить mono-service загружать сборки из папки приложения
export MONO_OPTIONS="--assembly-loader=strict"
export MONO_PATH="/opt/flexberry-service-bus"

until telnet FlexberryServiceBusPostgres 5432 </dev/null | grep Connected
do 
	echo "Wait for start up postgres service" 
	sleep 5;
done

# удалим файл блокировки, если он остался с прошлого запуска
rm -f /tmp/flexberry-service-bus.lock 

cd /opt/flexberry-service-bus

/usr/bin/mono-service --no-daemon -l:/tmp/flexberry-service-bus.lock -d:/opt/flexberry-service-bus -m:flexberry-service-bus NewPlatform.Flexberry.ServiceBus.WinServiceHost.exe
