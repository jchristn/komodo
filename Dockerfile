FROM mcr.microsoft.com/dotnet/core/sdk:3.1 AS build
WORKDIR /app
COPY . ./
RUN dotnet restore ./Komodo.Server/Komodo.Server.csproj

WORKDIR /app/Komodo.Server
RUN dotnet build   -f netcoreapp3.1 -c Release
RUN dotnet publish -f netcoreapp3.1 -c Release -o out

# Build runtime image
FROM mcr.microsoft.com/dotnet/core/runtime:3.1
WORKDIR /app
EXPOSE 9090/tcp
COPY --from=build /app/Komodo.Server/out .

#
# Copy in the System.json file containing the appropriate external
# database and storage settings.  Refer to Docker.md for details.
#
# The Komodo.db file is only present to demonstrate operation and
# should not be used in a production containerized environment.  
# External databases should be used instead.
#
RUN mkdir Data
RUN mkdir Data/ParsedDocuments
RUN mkdir Data/SourceDocuments
RUN mkdir Data/Postings
RUN mkdir Data/Temp
RUN mkdir Logs
COPY ./Komodo.db ./Data
COPY ./System.json .

# Set the entrypoint for the container
ENTRYPOINT ["dotnet", "Komodo.Server.dll"]