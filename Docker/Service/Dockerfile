FROM flexberry/hwsb-base

COPY . /docker/
COPY host/prepareConf.sh /opt/
COPY host/NewPlatform.Flexberry.HighwaySB.WinServiceHost.exe.config /opt/flexberry-hwsb/

MAINTAINER mail@flexberry.net

USER root

RUN export http_proxy=http://10.120.252.2:8080;\
  bzip2 -d /var/lib/rpm/*.bz2; \
  apt-get update && \
  apt-get install -y mc htop && \
  rm -f /var/cache/apt/archives/*.rpm /var/cache/apt/*.bin /var/lib/apt/lists/*.*; \
  bzip2 -9 /var/lib/rpm/* 

CMD echo "DOCKER_HOSTNAME=$DOCKER_HOSTNAME"; sh -x /opt/prepareConf.sh ; su -l -c /opt/startFlexberry-hwsb.sh highway
