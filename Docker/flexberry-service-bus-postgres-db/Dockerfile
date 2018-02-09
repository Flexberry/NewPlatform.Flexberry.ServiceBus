FROM flexberry/alt.p8-postgresql9.6-ru


COPY . /docker/
ADD http://example.com/big.tar.xz /docker/host/

RUN cd /docker/host/ && wget https://github.com/Flexberry/NewPlatform.Flexberry.ServiceBus/blob/develop/Flexberry%20Service%20Bus/SQL/PostgreSQL/CreateNewDB.sql && \
  /bin/sh /docker/host/createdb.sh

MAINTAINER mail@flexberry.net


