# Use the official .NET 9.0 SDK image for building
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build-env
WORKDIR /app

# Copy csproj and restore dependencies
COPY *.csproj ./
RUN dotnet restore

# Copy everything else and build
COPY . ./
RUN dotnet publish -c Release -o out

# Use the official .NET 9.0 runtime image
FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app

# Copy the build output from the build stage
COPY --from=build-env /app/out .

# Expose the port that the app runs on
EXPOSE 8080

# Set the entry point
ENTRYPOINT ["dotnet", "rfid-receiver-api.dll"]
