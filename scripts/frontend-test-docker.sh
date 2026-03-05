#!/usr/bin/env bash
# Run frontend tests inside a Node 22 container (unit tests; optional E2E with Playwright).
set -e
cd "$(dirname "$0")/.."
docker run --rm \
  -v "$(pwd):/src" \
  -w /src/frontend \
  -e CI=true \
  node:22 \
  bash -c "npm ci && npm run test"
# Optional E2E (uncomment to run; requires Playwright browsers in container):
#  bash -c "npm ci && npx playwright install chromium && npm run e2e"
