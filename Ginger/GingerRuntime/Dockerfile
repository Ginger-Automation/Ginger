# Use an appropriate base image
FROM mcr.microsoft.com/dotnet/aspnet:8.0-alpine

# Copy the published application to the container
COPY ./bin/Release/net8.0/publish ./GingerRuntime

# Switch to root user for installation
USER root

# Update apk and install git
RUN apk update && \
    apk add ca-certificates && \
    apk update ca-certificates && \
    apk add --no-cache icu-libs

RUN apk add --no-cache git

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

# Switch back to non-root user
USER nobody

# Set the working directory
WORKDIR /GingerRuntime

# Specify the entry point
ENTRYPOINT ["dotnet", "GingerRuntime.dll"]
