# Build
FROM mcr.microsoft.com/dotnet/sdk:10.0 as build
WORKDIR /src
COPY . .
RUN dotnet restore "./SparkTrack.WebAPI/SparkTrack.WebAPI.csproj"
RUN dotnet publish "./SparkTrack.WebAPI/SparkTrack.WebAPI.csproj" -c Production -o /app --no-restore

# Serve
FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /app ./

# Install dependencies for PostgreSQL Npgsql
RUN apt-get update && apt-get install -y \
    libgssapi-krb5-2

ENTRYPOINT ["dotnet", "SparkTrack.WebAPI.dll"]