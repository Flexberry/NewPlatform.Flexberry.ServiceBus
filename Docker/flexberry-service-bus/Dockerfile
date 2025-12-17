# 1. Clone
FROM alpine/git AS gitclone
ARG TAG=2.0.0-beta01-service
RUN git clone --depth=50 --branch="${TAG}" \
    https://github.com/Flexberry/NewPlatform.Flexberry.ServiceBus.git /repo

# 2. Build
FROM mono:latest AS mono
COPY --from=gitclone /repo /Flexberry/NewPlatform.Flexberry.ServiceBus
RUN mono --version && \
	msbuild /version && \
	cd /Flexberry/NewPlatform.Flexberry.ServiceBus && \
	nuget restore NewPlatform.Flexberry.ServiceBus.sln && \
	msbuild /p:Configuration=Release NewPlatform.Flexberry.ServiceBus.sln

# 3. Prepare runtime
FROM flexberry/alt.p8-apache2-mono:5.20.1-1.0.0 AS runtime
COPY --from=mono /Flexberry/NewPlatform.Flexberry.ServiceBus/NewPlatform.Flexberry.ServiceBus.WinServiceHost/bin/Release /opt/flexberry-service-bus
COPY host/prepareConf.sh /opt/
COPY host/start-flexberry-service-bus.sh /opt/
RUN mkdir /opt/flexberry-service-bus ; adduser flexberry ; \
	rm /opt/flexberry-service-bus/Mono.Security.dll ; \
	rm /opt/flexberry-service-bus/NewPlatform.Flexberry.ServiceBus.WinServiceHost.exe.config

LABEL org.opencontainers.image.authors="Flexberry Foundation <mail@flexberry.net>"

USER root

CMD echo "DOCKER_HOSTNAME=$DOCKER_HOSTNAME"; sh -x /opt/prepareConf.sh ; sh -x /bin/change_XMLconfig_from_env.sh && su -l -c "sh /opt/start-flexberry-service-bus.sh" flexberry
