#!/usr/bin/env bash
# Arauto — entrega ao remoto: contexto, add, commit, push, PR (aberto), watch, checkout main + fetch + pull.
# Só considera "PR já existe" se o PR estiver OPEN; se MERGED/CLOSED ou inexistente, cria novo PR para este push
# e valida as automações desse PR. Emite ARAUTO_RESULT/PR_URL/RUN_ID para o agente agir em caso de falha.

set -euo pipefail

COMMIT_MSG=""
PR_TITLE=""
PR_BODY=""
PR_BODY_FILE=""
SKIP_WATCH=false
DRY_RUN=false
PREVIEW=false
SKIP_CHECKOUT_MAIN=false

while [[ $# -gt 0 ]]; do
  case "$1" in
    --preview)
      PREVIEW=true
      shift
      ;;
    --no-checkout-main)
      SKIP_CHECKOUT_MAIN=true
      shift
      ;;
    --commit-msg)
      COMMIT_MSG="$2"
      shift 2
      ;;
    --pr-title)
      PR_TITLE="$2"
      shift 2
      ;;
    --pr-body)
      PR_BODY="$2"
      shift 2
      ;;
    --pr-body-file)
      PR_BODY_FILE="$2"
      shift 2
      ;;
    --no-watch)
      SKIP_WATCH=true
      shift
      ;;
    --dry-run)
      DRY_RUN=true
      shift
      ;;
    -h|--help)
      echo "Uso: arauto.sh [--preview] [--commit-msg \"msg\"] [--pr-title \"título\"] [--pr-body \"corpo\" | --pr-body-file path] [--no-watch] [--no-checkout-main] [--dry-run]"
      echo "  Só reutiliza PR existente se estiver OPEN; se MERGED/CLOSED, cria novo PR e valida as automações desse PR."
      exit 0
      ;;
    *)
      echo "Argumento desconhecido: $1" >&2
      exit 1
      ;;
  esac
done

run() {
  if [[ "$DRY_RUN" == true ]]; then
    echo "[dry-run] $*"
  else
    "$@"
  fi
}

emit_result() {
  echo "ARAUTO_RESULT=$1"
  [[ -n "${2:-}" ]] && echo "ARAUTO_RUN_ID=$2"
  [[ -n "${3:-}" ]] && echo "ARAUTO_PR_URL=$3"
}

BRANCH=$(git branch --show-current)
if [[ -z "$BRANCH" ]]; then
  echo "Erro: não foi possível obter a branch atual." >&2
  exit 1
fi

if [[ "$PREVIEW" == true ]]; then
  echo "=== Arauto: preview ==="
  echo "Branch: $BRANCH"
  echo "--- git status ---"
  git status
  echo "--- git diff --staged ---"
  git diff --staged || true
  echo "--- git diff (unstaged) ---"
  git diff || true
  echo "ARAUTO_PREVIEW=1"
  exit 0
fi

echo "=== Arauto: início ==="
echo "Branch: $BRANCH"
echo "--- git status ---"
git status
echo "--- git diff --staged ---"
git diff --staged || true
echo "--- git diff (unstaged) ---"
git diff || true
echo "=== git add -A ==="
run git add -A

if [[ -n "$COMMIT_MSG" ]]; then
  if ! git diff --staged --quiet 2>/dev/null; then
    echo "=== Commit ==="
    if [[ "$DRY_RUN" != true ]]; then
      TF=$(mktemp)
      printf '%s\n' "$COMMIT_MSG" > "$TF"
      git commit -F "$TF"
      rm -f "$TF"
    fi
  fi
fi

echo "=== Push ==="
if ! run git push -u origin "$BRANCH" 2>&1; then
  run git push 2>&1 || true
fi

if ! command -v gh &>/dev/null; then
  echo "gh não encontrado." >&2
  emit_result "no_gh"
  exit 0
fi
if ! gh auth status &>/dev/null; then
  echo "gh não autenticado." >&2
  emit_result "no_gh"
  exit 0
fi

# PR: só reutilizar se estiver OPEN; se MERGED/CLOSED ou não existir, criar novo para este push
echo "=== PR ==="
PR_STATE=$(gh pr view --json state -q '.state' 2>/dev/null || echo "")
if [[ "$PR_STATE" == "OPEN" ]]; then
  echo "PR aberto já existe."
  gh pr view
else
  if [[ -n "$PR_STATE" ]]; then
    echo "PR anterior está $PR_STATE; a criar novo PR para este push."
  else
    echo "Nenhum PR para esta branch; a criar."
  fi
  if [[ -z "$PR_TITLE" ]]; then
    PR_TITLE="Update $BRANCH"
  fi
  if [[ -n "$PR_BODY_FILE" ]] && [[ -f "$PR_BODY_FILE" ]]; then
    run gh pr create --title "$PR_TITLE" --body-file "$PR_BODY_FILE"
  elif [[ -n "$PR_BODY" ]]; then
    run gh pr create --title "$PR_TITLE" --body "$PR_BODY"
  else
    run gh pr create --title "$PR_TITLE"
  fi
fi
PR_URL=$(gh pr view --json url -q '.url' 2>/dev/null || true)

if [[ "$SKIP_WATCH" == true ]]; then
  emit_result "pr_only" "" "$PR_URL"
  exit 0
fi

echo "=== Workflows (deste PR) ==="
# Esperar pelo menos um run aparecer
for i in {1..12}; do
  RUN_ID=$(gh run list --branch "$BRANCH" --limit 1 --json databaseId -q '.[0].databaseId // empty' 2>/dev/null || true)
  [[ -n "$RUN_ID" ]] && break
  echo "À espera de run... ($i/12)"
  sleep 10
done

if [[ -z "$RUN_ID" ]]; then
  if [[ "$SKIP_CHECKOUT_MAIN" != true ]]; then
    run git checkout Master
    run git fetch
    run git pull
  fi
  emit_result "no_run" "" "$PR_URL"
  exit 0
fi

# Dar tempo para o run DESTE push ser disparado e aparecer na lista
echo "A aguardar mais 45s para o run deste push ser disparado..."
sleep 45

# Obter apenas o run MAIS RECENTE da branch (o disparado por este push) — obrigatório monitorar este
RUN_ID_TO_WATCH=$(gh run list --branch "$BRANCH" --limit 1 --json databaseId -q '.[0].databaseId // empty' 2>/dev/null)
if [[ -z "$RUN_ID_TO_WATCH" ]]; then
  if [[ "$SKIP_CHECKOUT_MAIN" != true ]]; then
    run git checkout Master
    run git fetch
    run git pull
  fi
  emit_result "no_run" "" "$PR_URL"
  exit 0
fi

# Monitorar o run deste push até concluir (obrigatório; não saltar)
FAILED_RUN_ID=""
echo "A monitorar run ID (deste push): $RUN_ID_TO_WATCH"
if ! run gh run watch "$RUN_ID_TO_WATCH"; then
  FAILED_RUN_ID=$RUN_ID_TO_WATCH
fi
if [[ -n "$FAILED_RUN_ID" ]]; then
  echo "=== Workflow falhou ==="
  emit_result "failure" "$FAILED_RUN_ID" "$PR_URL"
  echo "--- Logs (para o agente sugerir correções) ---"
  gh run view "$FAILED_RUN_ID" --log-failed 2>/dev/null || gh run view "$FAILED_RUN_ID"
  echo "--- Fim dos logs ---"
  exit 1
fi

if [[ "$SKIP_CHECKOUT_MAIN" != true ]]; then
  echo "=== Voltar à Master e atualizar ==="
  run git checkout Master
  run git fetch
  run git pull
else
  echo "=== (--no-checkout-main: mantendo-se na branch $BRANCH) ==="
fi
emit_result "success" "" "$PR_URL"
echo "=== Arauto: entrega concluída. ==="
