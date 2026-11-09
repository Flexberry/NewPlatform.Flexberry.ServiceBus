FROM mono:latest AS mono

ARG TAG=2.0.0-service
RUN mono --version && \
    apt update && \
	apt install -y git && \
	git clone --depth=50 --branch=$TAG https://github.com/Flexberry/NewPlatform.Flexberry.ServiceBus.git /Flexberry/NewPlatform.Flexberry.ServiceBus && \
	msbuild /version && \
	cd /Flexberry/NewPlatform.Flexberry.ServiceBus && \
	nuget restore NewPlatform.Flexberry.ServiceBus.sln && \
	msbuild /p:Configuration=Release NewPlatform.Flexberry.ServiceBus.sln

FROM flexberry/alt.p11-mono:6.12 AS runtime

COPY --from=mono /Flexberry/NewPlatform.Flexberry.ServiceBus/NewPlatform.Flexberry.ServiceBus.WinServiceHost/bin/Release /opt/flexberry-service-bus
COPY host/prepareConf.sh /opt/
COPY host/start-flexberry-service-bus.sh /opt/

RUN mkdir /opt/flexberry-service-bus ; adduser flexberry ; \
	rm /opt/flexberry-service-bus/Mono.Security.dll ; \
	rm /opt/flexberry-service-bus/NewPlatform.Flexberry.ServiceBus.WinServiceHost.exe.config

LABEL maintainer="mail@flexberry.ru"

USER root

CMD echo "DOCKER_HOSTNAME=$DOCKER_HOSTNAME"; sh -x /opt/prepareConf.sh ; sh -x /bin/configTransform.sh && su -l -c "sh /opt/start-flexberry-service-bus.sh" flexberry
