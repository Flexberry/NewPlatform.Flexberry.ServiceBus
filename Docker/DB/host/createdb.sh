#!/bin/sh
cd /docker/host

/docker-cmd.sh& 

until psql -U postgres <sqls/create.sql; do echo "Wait...";sleep 2; done 
until psql -U flexberryhwsbuser -d flexberryhwsb <sqls/highwaysb.sql; do echo "Wait...";sleep 5; done 

/etc/init.d/postgresql stop;  while su -c psql postgres </dev/null >/dev/null 2>&1; do sleep 1; done; echo "postgresql stoped"

