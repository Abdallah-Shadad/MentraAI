#!/bin/bash
set -e

cd /app

echo "Starting FastAPI server..."
exec uv run uvicorn main:app --host 0.0.0.0 --port 8000