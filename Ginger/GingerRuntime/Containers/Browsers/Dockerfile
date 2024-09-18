#Build and deploy one time with latest dependencies and another time use version specific dependencies

FROM ghcr.io/ginger-automation/gingerruntime:2024.2.0

USER root

#RUN apk add firefox

# Install Chromium, Chromedriver, and clean up
RUN apk add --no-cache chromium chromium-chromedriver \
    && rm -rf /var/cache/apk/*

# Set environment variables for Chromium
ENV CHROME_BIN=/usr/bin/chromium-browser \
    CHROME_PATH=/usr/lib/chromium/

# Update PATH to include Chromium binaries
ENV PATH="${PATH}:/usr/lib/chromium/"

USER nobody

WORKDIR /GingerRuntime
ENTRYPOINT ["dotnet", "GingerRuntime.dll"]