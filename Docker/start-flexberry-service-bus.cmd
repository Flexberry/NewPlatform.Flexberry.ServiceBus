docker pull flexberry/flexberry-service-bus-editor:latest
docker pull flexberry/flexberry-service-bus-postgres-db:latest
docker pull flexberry/flexberry-service-bus:latest
docker swarm init
docker stack  deploy -c flexberry-service-bus-swarm-configuration.yml FlexberryServiceBus
