FROM mono:latest as mono

RUN mono --version && \
    apt update && \
	apt install -y git && \
	git clone --depth=50 --branch=develop https://github.com/Flexberry/NewPlatform.Flexberry.ServiceBus.git /Flexberry/NewPlatform.Flexberry.ServiceBus && \
	msbuild /version && \
	cd /Flexberry/NewPlatform.Flexberry.ServiceBus && \
	nuget restore NewPlatform.Flexberry.ServiceBus.sln && \
	msbuild /p:Configuration=Release NewPlatform.Flexberry.ServiceBus.sln

FROM flexberry/hwsb

COPY --from=mono /Flexberry/NewPlatform.Flexberry.ServiceBus/NewPlatform.Flexberry.ServiceBus.WinServiceHost/bin/Release/ /opt/Release/
COPY host/prepareConf.sh /opt/

RUN mv /opt/flexberry-hwsb /opt/flexberry-hwsb.old && \
    mv /opt/Release /opt/flexberry-hwsb && \
	rm /opt/flexberry-hwsb/Mono.Security.dll
	cp /opt/flexberry-hwsb/App.Docker.config /opt/flexberry-hwsb/NewPlatform.Flexberry.ServiceBus.WinServiceHost.exe.config

MAINTAINER mail@flexberry.net

USER root

CMD echo "DOCKER_HOSTNAME=$DOCKER_HOSTNAME"; sh -x /opt/prepareConf.sh ; su -l -c /opt/startFlexberry-hwsb.sh highway
