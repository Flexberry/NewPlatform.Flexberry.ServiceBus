docker pull flexberry/flexberry-service-bus-editor:latest
docker pull flexberry/flexberry-service-bus-postgres-db:latest
docker pull flexberry/flexberry-service-bus:latest

IF %1 EQU rabbitmq (
	docker pull flexberry/rabbitmq:latest
)

docker stack ls >NUL  2>NUL
IF %ERRORLEVEL% NEQ 0 (
	docker swarm init
)

docker stack ls | findstr FlexberryServiceBus
IF %ERRORLEVEL% EQU 0 (
	echo Flexberry Service Bus is already started.
	echo To stop the Flexberry Service Bus, you need to run the command 'stop-flexberry-service-bus.cmd'
) ELSE (
	IF %1 EQU rabbitmq (
		docker stack deploy -c flexberry-service-bus-rabbitmq.yml FlexberryServiceBus
	) ELSE (
		docker stack deploy -c flexberry-service-bus-swarm-configuration.yml FlexberryServiceBus
	)
)
