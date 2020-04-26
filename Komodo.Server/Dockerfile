# Use SDK
FROM mcr.microsoft.com/dotnet/core/sdk:3.0 AS build

# Set the local working directory, copy in the project file, and perform a restore
WORKDIR /app
COPY *.csproj .
RUN dotnet restore

# Copy everything else and build
COPY . ./
RUN dotnet build   -f netcoreapp3.0 -c Release
RUN dotnet publish -f netcoreapp3.0 -c Release -o out

# Build runtime image
FROM mcr.microsoft.com/dotnet/core/runtime:3.0
WORKDIR /app
EXPOSE 8000/tcp
COPY --from=build /app/out .

# Copy in the System.json file containing the configuration for the external
# database and storage repositories.
#
# COPY ./System.json .

# Set the entrypoint for the container
ENTRYPOINT ["dotnet", "Komodo.Server.dll"]