#!/bin/sh
# =============================================================
# MentraAI .NET API — Docker Entrypoint
# Mirrors the pattern from docker/mentra_ai/entrypoint.sh
#
# Responsibilities:
#   1. Wait for PostgreSQL to be ready before starting the app.
#      (EF Core migrations run automatically inside Program.cs)
#   2. Hand off to the CMD passed by docker-compose (dotnet ...)
# =============================================================
set -e

echo "================================================"
echo " MentraAI .NET API — Starting up"
echo "================================================"

# --- Wait for PostgreSQL ---------------------------------
# DB_HOST and DB_USER are injected via .env.dotnet
echo "Waiting for PostgreSQL at ${DB_HOST}:5432 ..."

until pg_isready -h "${DB_HOST}" -U "${DB_USER}" -q; do
  echo "  PostgreSQL not ready yet — retrying in 2s..."
  sleep 2
done

echo "PostgreSQL is ready."
echo ""

# --- Start the application ------------------------------
# EF Core migrations run automatically on startup in Program.cs
echo "Starting .NET API (port ${PORT:-8080})..."
exec "$@"
