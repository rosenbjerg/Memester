FROM alpine AS gitCloner
RUN apk --update --no-cache --quiet add git openssh
WORKDIR /project
RUN git -clone --depth 1 --single-branch --branch <branchname> https://github.com/rosenbjerg/Memester.git

FROM mcr.microsoft.com/dotnet/core/sdk:3.1-alpine AS backendBuilder
WORKDIR /app
COPY --from=gitCloner /project/Memester/ ./
RUN dotnet publish Backend/Memester.ApplicationHost/Memester.ApplicationHost.Api.csproj -c Release -r linux-musl-x64 -o build

FROM mcr.microsoft.com/dotnet/core/aspnet:3.1-alpine AS runtime
LABEL maintainer="jbaagaard, rosenbjerg"
LABEL description="Memester"
LABEL repository="https://github.com/rosenbjerg/Memester"
WORKDIR /app
COPY --from=backendBuilder /app/build ./
EXPOSE 5000
ENV ASPNETCORE_URLS="http://*:5000"
ENTRYPOINT ["dotnet", "Memester.ApplicationHost.Api.dll"]