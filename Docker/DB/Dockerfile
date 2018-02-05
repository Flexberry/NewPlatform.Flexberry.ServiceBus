FROM flexberry/alt.p8-postgresql9.6-ru

LABEL repository="flexberry/hwsb-postgres:1.1"

COPY . /docker/

RUN /bin/sh /docker/host/createdb.sh

MAINTAINER mail@flexberry.net


