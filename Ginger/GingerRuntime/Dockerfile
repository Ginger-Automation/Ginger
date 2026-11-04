# .NET 10 runtime images are published for Ubuntu 24.04 (noble), not 22.04 (jammy).
# The image defaults to a non-root user and WORKDIR /app, so switch to root and
# copy into the absolute path used by the rest of this file.
FROM mcr.microsoft.com/dotnet/aspnet:10.0-noble

USER root

# Copy the published application to the container
COPY ./bin/Release/net10.0/linux-x64 /GingerRuntime

# Update package list and install dependencies  
RUN apt-get update && \
    apt-get install -y --no-install-recommends ca-certificates libicu-dev git && \
    rm -rf /var/lib/apt/lists/*  
    
RUN update-ca-certificates


# Create the /usr/share/amdocs/Ginger folder
RUN mkdir -p /usr/share/amdocs/Ginger \
    && chmod -R 777 /usr/share/amdocs/Ginger

# Set write permission for all users on the /GingerRuntime folder
RUN chmod -R 777 /GingerRuntime

# Add neccessary environment variables for the container
ENV NO_PROXY="localhost"
ENV GIT_SSL_NO_VERIFY=true
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false
ENV GINGER_EXE_ENV=docker

# Debian and Ubuntu rename these libraries across releases (Ubuntu 24.04 uses the
# t64 packages). Pick the name this image ships. The SONAMEs stay
# libglib-2.0.so.0, libgtk-3.so.0, and libasound.so.2, so browsers behave the same.
RUN apt-get update && \
    pkgs="wget gnupg2 software-properties-common libnss3 libxss1 fonts-liberation xvfb libgbm1" && \
    for group in \
        "libglib2.0-0t64 libglib2.0-0" \
        "libasound2t64 libasound2" \
        "libgtk-3-0t64 libgtk-3-0" \
        "libappindicator3-1 libayatana-appindicator3-1"; do \
        picked="" && \
        for pkg in $group; do \
            if apt-cache show "$pkg" >/dev/null 2>&1; then \
                picked="$pkg" && \
                break; \
            fi; \
        done && \
        if [ -z "$picked" ]; then \
            echo "No installable package in: $group" >&2 && \
            exit 1; \
        fi && \
        pkgs="$pkgs $picked"; \
    done && \
    apt-get install -y --no-install-recommends $pkgs && \
    rm -rf /var/lib/apt/lists/*

# Keep running as root. The .NET 10 base image creates a non-root user named app,
# and switching to it would change file ownership and browser startup.
USER root

# Set the working directory
WORKDIR /GingerRuntime

# Selenium 4.41 publishes the binary as a native asset. Older packages used
# selenium-manager/linux/selenium-manager. Use whichever path this build contains.
RUN set -eu; \
    manager=""; \
    for candidate in \
        /GingerRuntime/selenium-manager \
        /GingerRuntime/runtimes/linux/native/selenium-manager \
        /GingerRuntime/publish/selenium-manager \
        /GingerRuntime/publish/runtimes/linux/native/selenium-manager \
        /GingerRuntime/selenium-manager/linux/selenium-manager \
        /GingerRuntime/publish/selenium-manager/linux/selenium-manager; do \
        if [ -f "$candidate" ]; then \
            manager="$candidate"; \
            break; \
        fi; \
    done; \
    if [ -z "$manager" ]; then \
        echo "selenium-manager was not found under /GingerRuntime" >&2; \
        find /GingerRuntime -name 'selenium-manager*' -print >&2; \
        exit 1; \
    fi; \
    chmod +x "$manager"; \
    "$manager" --browser chrome; \
    "$manager" --browser firefox --browser-version 134; \
    "$manager" --browser edge

# Specify the entry point
ENTRYPOINT ["dotnet", "GingerRuntime.dll"]
