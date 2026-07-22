# ===========================
# Base Runtime Image
# ===========================
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app

EXPOSE 8080
EXPOSE 8081

# ===========================
# Build Image
# ===========================
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build

ARG BUILD_CONFIGURATION=Release

WORKDIR /src

# Copy the project file
COPY ProductService/ProductService.csproj ProductService/

# Restore packages
RUN dotnet restore ProductService/ProductService.csproj

# Copy the remaining files
COPY . .

WORKDIR /src/ProductService

RUN dotnet build ProductService.csproj \
    -c $BUILD_CONFIGURATION \
    -o /app/build

# ===========================
# Publish Image
# ===========================
FROM build AS publish

ARG BUILD_CONFIGURATION=Release

RUN dotnet publish ProductService.csproj \
    -c $BUILD_CONFIGURATION \
    -o /app/publish \
    /p:UseAppHost=false

# ===========================
# Final Runtime Image
# ===========================
FROM base AS final

WORKDIR /app

COPY --from=publish /app/publish .

ENTRYPOINT ["dotnet", "ProductService.dll"]