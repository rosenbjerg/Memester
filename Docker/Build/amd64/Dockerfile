FROM mcr.microsoft.com/dotnet/sdk:5.0-alpine AS backendBuilder
WORKDIR /app
COPY ./Backend/ ./
RUN dotnet publish Memester.ApplicationHost/Memester.ApplicationHost.csproj -c Release -r linux-musl-x64 -o build


FROM node:12-alpine AS frontendBuilder
WORKDIR /app
COPY ./Frontend/ ./
RUN yarn install --frozen-lockfile --no-progress --silent
RUN yarn build


FROM alpine AS runtime
ARG build
LABEL maintainer="rosenbjerg, jbaagaard"
LABEL description="Memester"
LABEL repository="github.com/rosenbjerg/Memester"
RUN apk add --update --no-cache ffmpeg imagemagick
WORKDIR /app
COPY --from=backendBuilder /app/build ./
COPY --from=frontendBuilder /app/build ./public/
ENV MEMESTER_BUILD__VERSION=$build
ENV ASPNETCORE_URLS="http://*:5000"
EXPOSE 5000
ENTRYPOINT ["./Memester.ApplicationHost"]
