#!/usr/bin/env bash
# Executa os testes unitários do frontend dentro de um container (Node 22 + Chromium).
# Use quando o host não tiver Node 20+ ou quando quiser garantir o mesmo ambiente do CI.
# Executar a partir da raiz do repositório.

set -e
REPO_ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
cd "$REPO_ROOT"

docker run --rm \
  -v "$(pwd)/frontend:/app" \
  -w /app \
  -e CHROME_BIN=/usr/bin/chromium \
  node:22-bookworm \
  sh -c "apt-get update -qq && apt-get install -y -qq chromium >/dev/null && npm ci && npm run test"
