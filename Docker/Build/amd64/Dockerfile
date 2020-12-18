FROM mcr.microsoft.com/dotnet/sdk:5.0-buster-slim AS backendBuilder
WORKDIR /app
COPY ./Backend/ ./
RUN dotnet publish Memester.ApplicationHost/Memester.ApplicationHost.csproj -c Release -r linux-musl-x64 -o build


FROM node:12-buster-slim AS frontendBuilder
WORKDIR /app
COPY ./Frontend/ ./
RUN yarn install --frozen-lockfile --no-progress --silent
RUN yarn build


FROM debian:buster-slim AS runtime
ARG build
LABEL maintainer="rosenbjerg, jbaagaard"
LABEL description="Memester"
LABEL repository="github.com/rosenbjerg/Memester"
RUN apt-get update \
&& apt-get install -y --no-install-recommends \
        ca-certificates \
        curl \
        jq \
        git \
        iputils-ping \
        libcurl4 \
        libicu63 \
        libunwind8 \
        netcat \
        libssl1.0 \
        ffmpeg \
        imagemagick
WORKDIR /app
COPY --from=backendBuilder /app/build ./
COPY --from=frontendBuilder /app/build ./public/
ENV MEMESTER_BUILD__VERSION=$build
ENV ASPNETCORE_URLS="http://*:5000"
EXPOSE 5000
ENTRYPOINT ["./Memester.ApplicationHost"]
