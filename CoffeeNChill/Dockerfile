# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY CoffeeNChill.csproj ./
RUN dotnet restore

COPY . ./
RUN dotnet publish -c Release -o /app/publish

# Stage 2: Runtime
FROM mcr.microsoft.com/azure-functions/dotnet-isolated:4-dotnet-isolated10.0
WORKDIR /home/site/wwwroot

COPY --from=build /app/publish .

ENV AzureWebJobsStorage="UseDevelopmentStorage=true"
ENV FUNCTIONS_WORKER_RUNTIME="dotnet-isolated"
ENV BlobContainerName="staffdocuments"

EXPOSE 80

# Correct entry point for Azure Functions container
ENTRYPOINT ["/opt/startup/startup.sh"]