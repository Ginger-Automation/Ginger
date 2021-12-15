#Build and deploy one time with latest dependencies and another time use version specific dependenecies
#FROM ghcr.io/ginger-automation/gingerruntime:4.1
#FROM ghcr.io/ginger-automation/gingerruntime:latest

FROM ghcr.io/ginger-automation/gingerruntime:4.1

USER root

Run apk add firefox

RUN apk add chromium
RUN apk add chromium-chromedriver

ENV CHROME_BIN=/usr/bin/chromium-browser \
    CHROME_PATH=/usr/lib/chromium/

RUN export PATH=$PATH:/usr/lib/chromium/

WORKDIR /GingerRuntime
ENTRYPOINT ["dotnet", "GingerRuntime.dll"]

