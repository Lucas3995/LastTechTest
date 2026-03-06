#!/usr/bin/env bash
# Executa os testes E2E do frontend dentro de um container (Node 22 + Playwright Chromium).
# O próprio Playwright sobe o servidor (npm run start) antes dos testes.
# Executar a partir da raiz do repositório.

set -e
REPO_ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
cd "$REPO_ROOT"

# Node 22 bookworm (mesma base dos testes unitários; satisfaz Angular ^22.12)
docker run --rm \
  -v "$(pwd)/frontend:/app" \
  -w /app \
  --ipc=host \
  node:22-bookworm \
  sh -c "apt-get update -qq && apt-get install -y -qq libnss3 libatk1.0-0 libatk-bridge2.0-0 libcups2 libdrm2 libxkbcommon0 libxcomposite1 libxdamage1 libxfixes3 libxrandr2 libgbm1 libasound2 libpango-1.0-0 libcairo2 >/dev/null && npm ci && npx playwright install chromium --with-deps && npm run e2e"
