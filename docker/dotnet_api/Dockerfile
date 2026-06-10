# =============================================================
# Stage 1 — BUILD
# Uses the full .NET SDK to restore dependencies and publish.
# This image (~800MB) is NOT shipped to production.
# =============================================================
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build

WORKDIR /src

# Copy only the .csproj first to leverage Docker layer caching.
# This layer only rebuilds when dependencies change.
COPY backend/src/MentraAI.API/MentraAI.API.csproj backend/src/MentraAI.API/

RUN dotnet restore backend/src/MentraAI.API/MentraAI.API.csproj

# Copy the full backend source code
COPY backend/ backend/

# Publish a self-contained, optimised Release build
RUN dotnet publish backend/src/MentraAI.API/MentraAI.API.csproj \
    -c Release \
    -o /app/publish \
    --no-restore

# =============================================================
# Stage 2 — RUNTIME
# Uses the lean ASP.NET Core alpine image (~120MB).
# Only the published output is copied from the build stage.
# =============================================================
FROM mcr.microsoft.com/dotnet/aspnet:9.0-alpine AS runtime

# Install postgresql-client so entrypoint.sh can use pg_isready
RUN apk add --no-cache postgresql-client

WORKDIR /app

# Copy published output from the build stage
COPY --from=build /app/publish .

# Copy and make the entrypoint executable
COPY docker/dotnet_api/entrypoint.sh /entrypoint.sh
RUN chmod +x /entrypoint.sh

ENTRYPOINT ["/entrypoint.sh"]

# Start the .NET API — port is controlled by PORT env var in .env.dotnet
CMD ["dotnet", "MentraAI.API.dll"]
