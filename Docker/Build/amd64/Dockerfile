FROM alpine AS gitCloner
RUN apk --update --no-cache --quiet add git openssh
WORKDIR /project
RUN git clone https://github.com/rosenbjerg/Memester.git


FROM mcr.microsoft.com/dotnet/core/sdk:3.1-alpine AS backendBuilder
WORKDIR /app
COPY --from=gitCloner /project/Memester/Backend/ ./
RUN dotnet publish Memester.ApplicationHost/Memester.ApplicationHost.csproj -c Release -r linux-musl-x64 -o build


FROM node:12-alpine AS frontendBuilder
WORKDIR /app
COPY --from=gitCloner /project/Memester/Frontend/ ./
RUN yarn install --frozen-lockfile --no-progress --silent
RUN yarn build


FROM mcr.microsoft.com/dotnet/core/aspnet:3.1-alpine AS runtime
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
ENTRYPOINT ["dotnet", "Memester.ApplicationHost.dll"]
