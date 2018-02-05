FROM flexberry/servicebuseditor-base

LABEL repository="flexberry/servicebuseditor:1.0"

COPY Dockerfile /docker/

MAINTAINER mail@flexberry.net
RUN export http_proxy=http://10.120.252.2:8080;\
  rm -rf /var/www/vhosts/ServiceBusEditor/*; \
  bzip2 -d /var/lib/rpm/*.bz2; \
  apt-get update && \
  apt-get install -y mc htop && \
  rm -f /var/cache/apt/archives/*.rpm /var/cache/apt/*.bin /var/lib/apt/lists/*.*; \
  bzip2 -9 /var/lib/rpm/*  

COPY host/ /var/www/vhosts/ServiceBusEditor
