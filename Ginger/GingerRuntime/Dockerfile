FROM  mcr.microsoft.com/dotnet/aspnet:3.1-alpine

COPY ./bin/Release/netcoreapp3.1/publish ./GingerRuntime

USER root

RUN apk update
RUN apk add git

USER guest

WORKDIR /GingerRuntime
ENTRYPOINT ["dotnet", "GingerRuntime.dll"]

