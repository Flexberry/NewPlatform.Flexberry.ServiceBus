#!/bin/sh
cd /docker/host

/docker-cmd.sh& 

until psql -U postgres <create.sql; do echo "Wait...";sleep 2; done 
until psql -U flexberryservicebususer -d flexberryservicebus <CreateNewDB.sql; do echo "Wait...";sleep 5; done 

/etc/init.d/postgresql stop;  while su -c psql postgres </dev/null >/dev/null 2>&1; do sleep 1; done; echo "postgresql stopped"
