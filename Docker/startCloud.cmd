docker pull flexberry/servicebuseditor:latest
docker pull flexberry/hwsb-postgres:latest
docker pull flexberry/hwsb:latest
docker swarm init
docker stack  deploy -c cloud.yml FlexberryServiceBus
