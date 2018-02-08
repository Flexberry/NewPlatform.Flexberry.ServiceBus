FROM flexberry/alt.p8-postgresql9.6-ru


COPY . /docker/
ADD ../../Flexberry Service Bus/SQL/PostgreSQL/CreateNewDB.sql /docker/host/

RUN /bin/sh /docker/host/createdb.sh

MAINTAINER mail@flexberry.net


