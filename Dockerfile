FROM ubuntu:latest

ARG DEBIAN_FRONTEND=noninteractive

COPY . streamfox
RUN /streamfox/container-build.sh

ENTRYPOINT /bin/sh /streamfox/container-startup.sh && /bin/sh