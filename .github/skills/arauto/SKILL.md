---
name: arauto
description: Executa o fluxo completo de entrega ao repositório remoto: commit com mensagem descritiva, push, abertura de pull request (GitHub CLI), validação obrigatória dos workflows do PR com gh run watch e, em caso de sucesso, checkout para main, fetch e pull. Considera a entrega concluída somente quando todos os workflows críticos estiverem verdes e o repositório local estiver em main atualizada; em falha, reporta o motivo e sugere correções. Usar quando o utilizador pedir para entregar no remoto, fazer commit e push, abrir PR, criar pull request, validar CI, ou verificar se as automações do PR passaram.
---

# Arauto — Entrega ao repositório remoto

Fluxo em 5 etapas. Executar em sequência; não pular etapas. Entrega só está concluída quando os workflows do PR estiverem verdes e, em seguida, o repositório local estiver em `Master` atualizada (checkout, fetch, pull). Se `gh` não estiver disponível, depende de confirmação manual do utilizador.

---

## Execução via script (recomendada)

Para reduzir uso de créditos do Cursor, executar o fluxo através do script: o agente faz **uma** chamada ao script e lê o retorno; só volta a agir se as automações do PR falharem (para reportar e sugerir correções). O script faz tudo: imprime contexto (status e diffs) no início, `git add -A` (respeitando .gitignore), commit, push, PR, **monitoração obrigatória dos workflows do PR** (watch) e, em sucesso, checkout para `Master`, fetch e pull.

**Monitorar o PR é obrigatório:** o agente **nunca** deve dar a entrega como concluída sem ter **efectivamente monitorado** os workflows do PR (via script com watch ou, se o script não fizer watch, manualmente com `gh run list --branch BRANCH` e `gh run watch <run-id>` no run mais recente). Só informar «Entrega concluída» / «Workflows verdes» depois de ter assistido à conclusão dos runs. Não pular esta etapa.

1. **Preparar** mensagem de commit (secção 1) e título/corpo do PR (secção 3). Para mensagens mais claras, concisas e eficientes, o agente deve obter o diff antes: invocar o script com `--preview` e usar a saída (status e diffs) para redigir commit e PR; em seguida invocar o script com os argumentos. Caso contrário, basear-se no que o utilizador pediu ou no contexto da conversa. Garantir que `.gitignore` está correto (o script adiciona todas as alterações).
2. **Garantir que não está na branch de produção** (ex.: `main`, `master`): se `git branch --show-current` indicar uma dessas branches, o agente deve criar e trocar para uma **nova branch de trabalho** antes de rodar o arauto, por exemplo:

```bash
BRANCH_ATUAL=$(git branch --show-current)
if [[ "$BRANCH_ATUAL" == "main" || "$BRANCH_ATUAL" == "Master" ]]; then
  NOVA_BRANCH="feat/arauto-$(date +%Y%m%d-%H%M%S)"
  git switch -c "$NOVA_BRANCH"
fi
```

3. **Invocar o script** a partir da raiz do repositório (uma vez se já tiver mensagem/PR; ou preview/execução se tiver usado --preview para redigir):

```bash
.github/skills/arauto/scripts/arauto.sh \
  --commit-msg "tipo(escopo): título

Corpo do commit com impacto funcional e técnico." \
  --pr-title "tipo(escopo): título descritivo" \
  --pr-body "## O que foi feito
...
## Impacto funcional
...
## Impacto técnico
...
## Como testar
..."
```

Alternativa para o corpo do PR: `--pr-body-file caminho/para/ficheiro.md`.

Opções: `--preview` (opcional; só imprime status e diffs e sai, para preparar mensagem/PR sem executar o fluxo); `--no-watch` (não esperar pelos workflows); `--dry-run` (apenas mostrar o que seria feito). Ver `arauto.sh --help`.

4. **Ler a saída** do script (no início vêm status e diffs; no fim, linhas parseáveis `ARAUTO_*`) e **interpretar o resultado** para agir em conformidade. **Não considerar entrega concluída sem ter monitorado o PR:** se o script terminou com `success` mas o agente não tiver a certeza de que o run **deste** push foi assistido (ex.: script fez watch de run antigo), o agente deve **monitorar manualmente**: `gh run list --branch BRANCH --limit 5` e `gh run watch <run-id>` no run mais recente; só então informar conclusão.
   - O script espera o run mais recente da branch (run deste push), aguarda 45s e faz `gh run watch` nesse run. Só emite sucesso quando o run estiver verde.
   - **`ARAUTO_RESULT=success`** — Informar «Entrega concluída. Todos os workflows do PR estão verdes. Repositório local em Master atualizada.» e, se existir, o `ARAUTO_PR_URL` para o utilizador abrir o PR.
   - **`ARAUTO_RESULT=failure`** — **Não** marcar a entrega como concluída. Reportar que o workflow falhou; usar `ARAUTO_RUN_ID` e os logs impressos entre «--- Logs do(s) step(s)...» e «--- Fim dos logs ---» para **entender o problema**.
     - **Verificação obrigatória:** antes de qualquer alteração de código de produção, avaliar se a falha é por **testes do CI desatualizados** (ex.: teste espera status 403 ou resposta antiga, mas a regra de negócio ou o contrato da API mudou e agora devolve 400 ou outro formato). Se for o caso: **corrigir apenas os testes** (E2E, unitários, integração) para refletir o comportamento atual; **não alterar regras de negócio nem código de produção** para satisfazer expectativas antigas dos testes. Depois fazer commit/push e executar o arauto novamente.
     - Se a falha **não** for por testes desatualizados (ex.: bug real, handler em falta, lógica quebrada), tratar como **nova demanda**: usar o `tradutor` para interpretar o erro em linguagem de negócio/fluxo/uso; usar o `maestro` para relatório de alterações; seguir a rotina-completa (planejamento → implementação → entrega) e só então executar o arauto novamente.
   - **`ARAUTO_RESULT=no_gh`** — Não considerar entrega concluída. Informar que `gh` não está disponível ou autenticado e que o utilizador deve criar/ver o PR e verificar a aba **Actions** manualmente.
   - **`ARAUTO_RESULT=pr_only`** — PR foi criado/aberto com `--no-watch`. Informar o `ARAUTO_PR_URL` e que a validação de workflows não foi executada; o utilizador deve verificar a aba Actions.
   - **`ARAUTO_RESULT=no_run`** — Master foi atualizada mas nenhum run foi detectado para a branch. Informar e pedir ao utilizador que verifique a aba Actions manualmente.

Só voltar a fazer algo (ex.: sugerir correções e pedir nova execução do script) se **`ARAUTO_RESULT=failure`**. Em sucesso ou no_run/pr_only/no_gh, informar o utilizador e encerrar.

Se o script não for usado, seguir as 5 etapas abaixo manualmente.

---

## 1. Commit

**Antes de gerar a mensagem:**

1. `git status` e `git diff --staged` (e `git diff` se necessário) para saber o que será commitado.
2. Incluir apenas ficheiros relevantes; nunca segredos, `.env`, nem o que deva estar no `.gitignore`.
3. `git add` apenas do que deve ir no commit.

**Mensagem:**

- Convenção: `tipo(escopo): descrição` (feat, fix, refactor, docs, chore, test, ci).
- Descritiva e informativa: um leitor deve entender **o que foi feito**, **impacto funcional** (o que o utilizador/negócio passa a ter) e **impacto técnico** (camadas, ficheiros, endpoints).
- Usar corpo do commit (linha em branco após o título + parágrafos) quando o título não bastar.

**Formato:**

```
tipo(escopo): título curto e claro (≤ 72 caracteres)

Parágrafo com o que foi feito, impacto funcional e técnico.

Refs: #<issue> (se houver)
```

Exemplos detalhados em [reference.md](reference.md).

---

## 2. Push

```bash
git push -u origin <branch>   # primeira vez
git push                       # branch já rastreada
```

- Nunca usar `--force` em `Master` ou `develop`. Se a branch estiver protegida, reportar e usar PR.
- Se o push for rejeitado por restrição de branch principal, informar o utilizador e seguir para o passo 3.

---

## 3. Abrir PR

É obrigatório ter um **PR aberto** para este push e validar as automações **desse** PR. Se a branch já tiver um PR mas estiver **MERGED** ou **CLOSED**, criar um **novo** PR para o push atual (não reutilizar o antigo).

**Verificar estado do PR da branch:**

```bash
gh pr view --json state -q '.state'
```

- Se `state == OPEN`: o PR aberto já existe; usar e ir para o passo 4.
- Se não existir PR ou `state` for `MERGED` ou `CLOSED`: criar novo com `gh pr create` (título e corpo descritivos) e depois validar as automações desse novo PR.

**Corpo do PR:** descritivo — o que foi feito, impacto funcional, impacto técnico, como testar. Template em [reference.md](reference.md).

```bash
gh pr create --title "tipo(escopo): título descritivo" --body "$(cat <<'EOF'
## O que foi feito
[Descrição clara.]

## Impacto funcional
[O que o utilizador/negócio passa a ter.]

## Impacto técnico
- Camadas/ficheiros, endpoints, migrations, testes, dependências

## Como testar
[Passos para validar.]

Refs: #<issue> (se houver)
EOF
)"
```

---

## 4. Validação dos workflows (obrigatória)

Validar as automações do **PR aberto** que corresponde a este push (o que foi criado ou o que já estava OPEN). A entrega só está concluída quando todos os **workflows críticos** desse PR estiverem com conclusão **success**. Jobs com `continue-on-error: true` são informativos; falha neles não bloqueia.

### 4.1 Listar runs da branch

```bash
BRANCH=$(git branch --show-current)
gh run list --branch "$BRANCH" --limit 20
```

Se não aparecer nenhum run, aguardar ~10s e repetir (GitHub Actions pode demorar a enfileirar).

### 4.2 Monitorar jobs críticos primeiro

1. Identificar na listagem quais jobs são críticos (sem `continue-on-error`) e quais são opcionais (com `continue-on-error: true`). Exemplo neste projeto: `lint-build-test` = crítico; `e2e` = opcional.
2. Assistir apenas os runs/jobs críticos:

```bash
gh run watch <run-id-critico>
```

3. Se um job crítico **falhar**: obter logs com `gh run view <run-id> --log-failed`, diagnosticar e **não** marcar a entrega como concluída. **Primeiro** verificar se a falha é por **testes desatualizados** (teste espera comportamento antigo; regra de negócio ou API já mudaram). Se sim, corrigir **apenas os testes** para refletir o comportamento atual; **não alterar regras de negócio nem código de produção**. Se não for o caso, reportar ao utilizador com sugestões concretas e, após correção (commit/push), voltar ao passo 4.1.
4. Se todos os críticos estiverem **success**: opcionalmente aguardar jobs opcionais para relato; não bloqueiam.

### 4.3 Resultado final

```bash
gh run list --branch "$BRANCH" --limit 20
```

- **Todos os runs críticos com `conclusion = success`:** informar «Workflows do PR estão verdes.» e executar o **passo 5**. Só depois informar «Entrega concluída.» e sugerir `gh pr view --web` para abrir o PR no browser.
- **Algum run com `conclusion = failure` (ou `cancelled`):** usar `gh run view <run-id>` e `gh run view <run-id> --log-failed`. Avaliar se a falha é por **testes desatualizados**; se sim, corrigir apenas os testes (sem alterar regras de negócio ou produção). Caso contrário, reportar workflow/job/step que falhou, resumo do erro e sugestões de correção; não marcar como concluído e não executar o passo 5.

---

## 5. Voltar à Master e atualizar (após sucesso dos workflows)

Só executar quando todos os workflows críticos do PR estiverem com conclusão **success**. Último passo da entrega.

```bash
git checkout Master
git fetch
git pull
```

- Deixar o repositório local em `Master` atualizada com o remoto.
- Em seguida, informar ao utilizador: «Entrega concluída. Todos os workflows do PR estão verdes. Repositório local em `Master` atualizada.»

---

## Escape: `gh` não disponível ou não autenticado

1. Informar o utilizador.
2. Omitir a validação automatizada (passo 4).
3. Indicar a URL do PR para verificação manual na aba **Actions** do GitHub.
4. Não considerar a entrega concluída sem confirmação manual do utilizador.

---

## Script (opcional)

O script `.github/skills/arauto/scripts/arauto.sh` implementa este fluxo: só reutiliza PR existente se `state == OPEN`; se MERGED/CLOSED ou inexistente, cria novo PR. **Monitoração obrigatória:** após o push, obtém o **run mais recente** da branch (o disparado por este push), faz `gh run watch` nesse run e só emite success quando estiver verde. Em caso de falha, emite `ARAUTO_RESULT=failure` e logs para o agente sugerir correções. O agente não deve dar a entrega por concluída sem esta monitoração (via script ou manual).

## Recursos adicionais

- Exemplos de commit, template de PR, classificação de workflows e alinhamento com regras do projeto: [reference.md](reference.md).
