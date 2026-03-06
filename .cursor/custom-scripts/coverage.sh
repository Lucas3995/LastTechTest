#!/usr/bin/env bash
# Gera cobertura de testes (backend e frontend) e exibe a porcentagem.
# Executar a partir da raiz do repositório.

set -e
REPO_ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
cd "$REPO_ROOT"

BACKEND_PCT="N/A"
FRONTEND_PCT="N/A"

# Pré-requisitos
command -v dotnet >/dev/null 2>&1 || { echo "Erro: dotnet não encontrado no PATH."; exit 1; }
command -v node >/dev/null 2>&1 || { echo "Erro: node não encontrado no PATH."; exit 1; }
command -v npm >/dev/null 2>&1 || { echo "Erro: npm não encontrado no PATH."; exit 1; }

echo "=== Backend: build e testes com cobertura ==="
export DOTNET_ENVIRONMENT=Testing
export Jwt__Key="${JWT_KEY_TESTS:-chave-de-ci-com-tamanho-minimo-32x!}"
export Jwt__Issuer="${Jwt__Issuer:-diariodebordo-ci}"
export Jwt__Audience="${Jwt__Audience:-diariodebordo-clients-ci}"

if (cd backend && dotnet build -c Release --nologo -v q && dotnet test --no-build -c Release --nologo -v q --collect:"XPlat Code Coverage" --results-directory ./TestResults); then
  BACKEND_XML=$(find backend/TestResults -name 'coverage.cobertura.xml' 2>/dev/null | head -1)
  if [ -n "$BACKEND_XML" ] && [ -r "$BACKEND_XML" ]; then
    LINE_RATE=$(grep -o 'line-rate="[^"]*"' "$BACKEND_XML" | head -1 | sed 's/line-rate="\(.*\)"/\1/')
    if [ -n "$LINE_RATE" ]; then
      BACKEND_PCT=$(awk "BEGIN { printf \"%.1f%%\", $LINE_RATE * 100 }")
    fi
  else
    echo "Aviso: coverage.cobertura.xml não encontrado em backend/TestResults."
  fi
else
  echo "Aviso: build ou testes do backend falharam; cobertura backend = N/A."
fi

echo ""
echo "=== Frontend: testes com cobertura ==="
if (cd frontend && npm run test:coverage); then
  FRONTEND_JSON="$REPO_ROOT/frontend/coverage/frontend/coverage-summary.json"
  if [ -r "$FRONTEND_JSON" ]; then
    FRONTEND_PCT=$(node -e "
      try {
        const j = require(process.argv[1]);
        const pct = j.total && j.total.lines && typeof j.total.lines.pct === 'number' ? j.total.lines.pct : null;
        console.log(pct != null ? pct.toFixed(1) + '%' : 'N/A');
      } catch (e) {
        console.log('N/A');
      }
    " "$FRONTEND_JSON")
  else
    echo "Aviso: $FRONTEND_JSON não encontrado."
  fi
else
  echo "Aviso: testes do frontend falharam; cobertura frontend = N/A."
fi

echo ""
echo "--- Cobertura de testes ---"
echo "Backend:  ${BACKEND_PCT}"
echo "Frontend: ${FRONTEND_PCT}"
if [ "$BACKEND_PCT" != "N/A" ] && [ "$FRONTEND_PCT" != "N/A" ]; then
  BACKEND_NUM=$(echo "$BACKEND_PCT" | sed 's/%//')
  FRONTEND_NUM=$(echo "$FRONTEND_PCT" | sed 's/%//')
  AVG=$(awk "BEGIN { printf \"%.1f\", ($BACKEND_NUM + $FRONTEND_NUM) / 2 }")
  echo "Resumo:   média ${AVG}% (backend e frontend)"
fi
echo "Relatórios detalhados: backend/TestResults e frontend/coverage/frontend/"
