# Use an appropriate base image
FROM mcr.microsoft.com/dotnet/aspnet:8.0-jammy

# Copy the published application to the container
COPY ./bin/Release/net8.0/publish ./GingerRuntime

# Switch to root user for installation
USER root

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

# Install necessary packages and libraries  
RUN apt-get update && \
    apt-get install -y --no-install-recommends \
    wget \
    gnupg2 \
    software-properties-common \
    libglib2.0-0 \
    libnss3 \
    libxss1 \
    libasound2 \
    fonts-liberation \
    libappindicator3-1 \
    libgtk-3-0 \
    xvfb \
    libgbm1 \
    && rm -rf /var/lib/apt/lists/*  

# Switch back to non-root user
#USER nobody

# Set the working directory
WORKDIR /GingerRuntime

RUN ./selenium-manager/linux/selenium-manager --browser chrome
RUN ./selenium-manager/linux/selenium-manager --browser firefox
RUN ./selenium-manager/linux/selenium-manager --browser edge

# Specify the entry point
ENTRYPOINT ["dotnet", "GingerRuntime.dll"]
