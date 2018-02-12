FROM flexberry/alt.p8-postgresql9.6-ru


COPY . /docker/

RUN cd /docker/host/ && wget https://raw.githubusercontent.com/Flexberry/NewPlatform.Flexberry.ServiceBus/develop/Flexberry%20Service%20Bus/SQL/PostgreSQL/CreateNewDB.sql && \
  /bin/sh /docker/host/createdb.sh

MAINTAINER mail@flexberry.net


