FROM  mcr.microsoft.com/dotnet/aspnet:7.0-alpine

COPY ./bin/Release/net7.0/linux-x64/publish ./GingerRuntime

USER root

RUN apk update
RUN apk add git

WORKDIR /GingerRuntime
ENTRYPOINT ["dotnet", "GingerRuntime.dll"]

