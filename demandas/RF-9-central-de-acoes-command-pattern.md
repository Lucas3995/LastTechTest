## [RF-9] Central de ações — Command pattern para todas as requisições ao backend

### Contexto e objetivo de negócio

O frontend Angular atualmente faz todas as chamadas HTTP aos endpoints da API diretamente a partir das facades (via ports/adapters). As instruções de arquitetura do projeto (`.github/instructions/angular-frontend.instructions.md`) exigem que **todas** as requisições ao backend sejam geridas por uma **central de ações** baseada no padrão Command: intenção serializável, fila, retry, persistência local e feedback ao utilizador. Se o backend falhar ou estiver indisponível, o sistema deve avisar o utilizador, persistir o comando e permitir retry posterior, sem quebrar a página.

Atualmente essa camada não existe. Cada facade faz chamadas diretas e trata erros inline. Este card implementa a central de ações como módulo transversal, cobrindo **todas** as requisições (leituras e escritas), incluindo a UI da central (badge no menu, painel de ações).

### Objetivo

Implementar no frontend Angular um **serviço central de ações** (Command pattern) que intermedeie todas as requisições HTTP ao backend — leituras e escritas — com fila de execução, persistência em IndexedDB, retry com backoff, notificação de resultado e resiliência a falhas de rede. Inclui a UI da central: badge no menu superior com contagem de pendentes/falhadas e painel onde o utilizador vê, reenvia ou descarta ações.

### User stories

- **US-CA1 – Submeter ação via central**
  Como **utilizador autenticado** (qualquer role), quero que **todas as minhas interações que chamem o backend** (criar solicitação, listar, aprovar, simular, etc.) **passem por uma central de ações**, para que **falhas de rede não quebrem a página e eu possa tentar novamente**.

- **US-CA2 – Persistência de ações falhadas**
  Como **utilizador**, quero que **ações que falharam por indisponibilidade do backend** sejam **persistidas localmente (IndexedDB)**, para que **ao reabrir a aplicação eu possa reenviá-las sem perder contexto**.

- **US-CA3 – Retry automático com backoff**
  Como **utilizador**, quero que **ações falhadas por erro de rede** (timeout, 0, 502, 503, 504) **sejam reenviadas automaticamente com backoff exponencial**, para **reduzir a necessidade de intervenção manual**.

- **US-CA4 – Notificação de resultado**
  Como **utilizador**, quero **ser notificado quando uma ação pendente/retry for concluída** (sucesso ou falha definitiva), para **saber o resultado sem ficar à espera**.

- **US-CA5 – Ações de leitura resilientes**
  Como **utilizador**, quero que **listagens e detalhes que falhem** mostrem **estado vazio ou último estado conhecido** e ofereçam **opção de tentar novamente**, sem quebrar a navegação.

- **US-CA6 – Badge no menu**
  Como **utilizador**, quero ver um **indicador visual (badge) no menu superior** com a contagem de ações pendentes ou falhadas, para **saber imediatamente se há algo que precisa da minha atenção**.

- **US-CA7 – Painel da central de ações**
  Como **utilizador**, quero **acessar um painel** (dropdown ou drawer a partir do badge) onde vejo **todas as ações** (em progresso, concluídas, falhadas) e posso **reenviá-las ou descartá-las manualmente**.

### Personas / papéis

- **Creator, Analista, Admin** — todas as roles beneficiam igualmente; a central é transversal.

### Telas, módulos e navegação

- **Componente "Badge de ações"** — sempre visível no menu/header da aplicação; mostra contagem de ações pendentes + falhadas. Clicável para abrir o painel.
- **Painel "Central de ações"** — dropdown ou drawer lateral acessível a partir do badge. Lista de ações agrupadas por status (em progresso, falhadas, concluídas recentes). Para falhadas: botão "Tentar novamente" e "Descartar". Para concluídas: exibição e remoção automática após 24h.
- **Todas as telas existentes** (RF-1 a RF-8) passam a submeter operações via central. O feedback inline (mensagens de sucesso/erro) é mantido nas telas existentes, alimentado pela notificação de resultado da central.

### Arquitetura técnica

**Camadas impactadas:**

| Camada | Artefato | Responsabilidade |
|--------|----------|------------------|
| `domain` | `ServerAction<TPayload, TResult>` interface | Intenção serializável: tipo, payload, metadata (timestamp, tentativas, status) |
| `domain` | `ActionStatus` enum | `Queued`, `InProgress`, `Completed`, `Failed`, `FailedPermanent` |
| `domain` | `ActionQueuePort` interface | Contrato: `dispatch()`, `retry()`, `cancel()`, `getActions()`, `onResult()` |
| `infrastructure` | `IndexedDbActionStore` | Persistência de ações em IndexedDB (criar, atualizar, remover, listar) |
| `infrastructure` | `ActionQueueService` | Implementação da fila: dequeue, execução, retry com backoff, notificação |
| `application` | Facades (todas) | Delegam operações ao `ActionQueuePort` em vez de chamar ports HTTP diretamente |
| `shared` | `ActionBadgeComponent` | Badge no header com contagem de ações pendentes/falhadas |
| `shared` | `ActionPanelComponent` | Painel/drawer com lista de ações, retry e descartar |
| `core` | Integração no bootstrap | Providenciar `ActionQueuePort` → `ActionQueueService` em `app.config.ts` |

**Fluxo de uma ação:**

1. Facade chama `actionQueue.dispatch({ type: 'createAnticipation', payload: {...} })`
2. Central serializa, persiste em IndexedDB com status `Queued`
3. Central dequeue → executa chamada HTTP via port existente → status `InProgress`
4. Sucesso → status `Completed` → notifica facade (callback/observable) → facade atualiza Signals
5. Falha de rede (0, 502-504) → retry com backoff (max 3 tentativas) → se esgotado: `Failed` → notifica
6. Falha de negócio (400, 403, 422) → `FailedPermanent` → notifica sem retry (erro do domínio)

**Retry policy:**
- Erros retryable: status 0 (offline), 408 (timeout), 429 (rate limit), 502, 503, 504
- Erros permanentes: 400, 401, 403, 404, 422, 500
- Backoff: exponencial com jitter (1s, 2s, 4s); máximo 3 tentativas
- 401: delega ao `TokenRefreshService` existente; se refresh falhar → `FailedPermanent`

**Persistência (IndexedDB):**
- Store `server_actions` com índice por `status` e `createdAt`
- Cleanup automático: ações `Completed` removidas após 24h; `FailedPermanent` mantidas até o utilizador descartar
- Ao iniciar a aplicação: reprocessar ações `Queued` ou `InProgress` (crash recovery)

### Permissões e segurança

- A central não altera permissões. Cada ação executa via port existente com o token JWT do utilizador autenticado.
- Ações persistidas em IndexedDB não contêm tokens — apenas tipo + payload. O token é adicionado no momento da execução pelo interceptor.

### Fluxos de uso

#### Fluxo 1 – Escrita com sucesso (happy path)

1. Creator clica em "Criar solicitação" (RF-5).
2. Facade despacha ação `createAnticipation` via central.
3. Central persiste em IndexedDB, executa imediatamente, backend retorna 201.
4. Central notifica facade → facade mostra "Solicitação criada com sucesso" e atualiza lista.
5. Badge mostra brevemente a ação concluída (ou mantém count 0 se nenhuma pendente).

#### Fluxo 2 – Escrita com falha de rede + retry

1. Creator submete criação, mas o backend está offline.
2. Central tenta executar → status 0 → marca como `Failed`, agenda retry (1s).
3. Badge mostra "1" (ação pendente). Tela mostra mensagem "Falha ao criar. Tentando novamente...".
4. Retry #2 após 2s → backend volta → sucesso → notifica facade → lista atualizada.

#### Fluxo 3 – Falha de negócio (sem retry)

1. Creator tenta criar segunda solicitação com uma já em análise.
2. Central executa → backend retorna 400 "Já existe solicitação em análise".
3. Central marca como `FailedPermanent` → notifica facade com mensagem do backend.
4. Tela mostra erro contextual. Badge mostra "1" (ação falhada). No painel: "Descartar".

#### Fluxo 4 – Leitura resiliente

1. Creator navega para "Minhas solicitações" (RF-1) com backend offline.
2. Central tenta GET → falha → retorna estado vazio. Tela mostra "Não foi possível carregar. Tentar novamente?"
3. Creator clica "Tentar novamente" → central reexecuta → backend volta → lista carregada.

#### Fluxo 5 – Painel da central

1. Utilizador vê badge "2" no menu → clica.
2. Painel abre mostrando: 1 ação `Failed` ("Criar solicitação – falha de rede") e 1 `Completed` ("Listar solicitações – sucesso").
3. Utilizador clica "Tentar novamente" na ação falhada → central reexecuta → sucesso → badge atualiza.

### Critérios de aceitação

- **CA-RF9-1 – Toda escrita passa pela central**
  Dado que submeto uma operação de escrita (criar, aprovar, rejeitar, cancelar, simular, converter), quando a facade é chamada, então a operação é despachada via `ActionQueuePort`, não diretamente via port HTTP.

- **CA-RF9-2 – Toda leitura passa pela central**
  Dado que navego para uma tela que lista dados (Minhas solicitações, Lista global, Detalhe), quando a facade carrega dados, então a requisição GET é despachada via central.

- **CA-RF9-3 – Persistência em IndexedDB**
  Dado que despacho uma ação e a aplicação é fechada antes da execução, quando reabro a aplicação, então a ação persiste em IndexedDB e é reprocessada automaticamente.

- **CA-RF9-4 – Retry com backoff**
  Dado que uma ação falha com erro de rede (status 0 ou 502-504), quando a central processa o retry, então tenta novamente até 3 vezes com backoff exponencial.

- **CA-RF9-5 – Erros de negócio não retried**
  Dado que uma ação falha com 400/422 (ex.: "já existe solicitação em análise"), quando a central recebe o erro, então marca como `FailedPermanent` e notifica a facade com a mensagem de erro.

- **CA-RF9-6 – Feedback mantido nas telas existentes**
  Dado que uma ação completa (sucesso ou falha), quando a facade recebe o resultado via callback, então atualiza os Signals de mensagem/erro de forma idêntica ao comportamento atual.

- **CA-RF9-7 – Páginas não quebram por falha de rede**
  Dado que o backend está indisponível, quando navego entre páginas, então nenhuma página apresenta erro não tratado ou fica em branco; as ações ficam como `Queued`/`Failed` e a UI mostra estado vazio/último conhecido.

- **CA-RF9-8 – Badge visível com contagem**
  Dado que existem ações pendentes ou falhadas, quando olho para o menu superior, então vejo um badge com o número de ações que requerem atenção.

- **CA-RF9-9 – Painel mostra ações por status**
  Dado que clico no badge, quando o painel abre, então vejo ações agrupadas por status (em progresso, falhadas, concluídas recentes) com opções de retry e descartar.

- **CA-RF9-10 – Retry manual no painel**
  Dado que tenho uma ação falhada no painel, quando clico "Tentar novamente", então a central reenvia a ação e atualizo o status no painel.

- **CA-RF9-11 – Descartar ação falhada**
  Dado que tenho uma ação `FailedPermanent` no painel, quando clico "Descartar", então a ação é removida de IndexedDB e do painel.

### Componentes de UI e comportamento

- **ActionBadgeComponent** (shared)
  - Posição: menu superior/header, visível em todas as páginas quando autenticado.
  - Mostra contagem de ações com status `Queued` + `InProgress` + `Failed`.
  - Contagem 0: badge oculto ou sem destaque.
  - Contagem > 0: badge visível com número e cor de alerta.
  - Clique: abre o painel.

- **ActionPanelComponent** (shared)
  - Posição: dropdown ou drawer lateral a partir do badge.
  - Lista de ações com: tipo (ícone/label), timestamp, status (chip colorido), mensagem de erro (se falhada).
  - Para `Failed`: botão "Tentar novamente".
  - Para `FailedPermanent`: botões "Tentar novamente" e "Descartar".
  - Para `Completed`: exibição breve; cleanup automático após 24h.
  - Estado vazio: "Nenhuma ação recente."
  - A11y: navegável por teclado; `aria-label` em botões; `role="status"` no badge; `aria-live="polite"` para atualizações.

### Requisitos técnicos/metodológicos aplicáveis

- Aplicar `.github/instructions/angular-frontend.instructions.md` e skill `mestre-freire-angular`.
- TypeScript, design patterns (Command, Queue, Observer).
- SOLID e os 3 princípios de coesão de componentes.
- TDD e DDD obrigatórios.
- Pirâmide completa: testes unitários, de integração e E2E.
- Port/Adapter com DIP (InjectionToken): `ActionQueuePort` no domain, `ActionQueueService` + `IndexedDbActionStore` na infrastructure.
- Signals para estado síncrono (lista de ações, contagem do badge); RxJS para async (resultados, retry).
- A11y: semântica, ARIA, teclado, feedback para leitores de tela no badge e painel.

### Diretrizes de testes

- **Unitários**
  - `ActionQueueService`: dispatch enfileira, dequeue executa, retry conta tentativas, backoff calcula delays, erros permanentes não retried, cleanup remove concluídas após TTL.
  - `IndexedDbActionStore`: CRUD em fake/mock de IndexedDB (idb-keyval ou wrapper).
  - `ActionBadgeComponent`: mostra contagem correta; oculta quando 0; abre painel ao clicar.
  - `ActionPanelComponent`: lista ações por status; retry chama `actionQueue.retry()`; descartar chama `actionQueue.cancel()`.
- **Integração**
  - Facade → `ActionQueuePort` → port HTTP mock: ação completa atualiza Signals.
  - Crash recovery: ações persistidas são reprocessadas ao instanciar o serviço.
- **E2E**
  - Creator cria solicitação → ação aparece como Queued → backend responde → lista atualizada.
  - Badge e painel visíveis quando existem ações pendentes.

### Dependências e riscos

- **Dependência:** Todas as facades existentes (RF-1 a RF-8) precisam ser adaptadas para usar a central — alteração incremental por facade.
- **Risco mitigado (performance):** IndexedDB em operações frequentes (listagens) — mitigado com cleanup automático, índices e execução síncrona sem persistir leituras bem-sucedidas.

### Rastreabilidade

- **Origem:** `.github/instructions/angular-frontend.instructions.md` §"Frontend independente do backend — central de ações (Command)" + `.github/reference/fonte-de-verdade-refactoring.md` item F-A1.
- **Impacta:** Todas as facades em `frontend/src/app/application/`, todos os ports em `frontend/src/app/domain/`, `app.config.ts`, layout/header component.
