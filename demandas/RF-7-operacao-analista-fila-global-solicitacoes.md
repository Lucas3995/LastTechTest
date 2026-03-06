## [RF-7] Operação de Analista na fila global de solicitações de antecipação

### Contexto e objetivo de negócio

O frontend já permite login com role `Analista` e exibe a rota/menu de lista global de solicitações (`/anticipation/list`), inclusive com ações de aprovar/recusar no detalhe (RF-2 e RF-4). Porém, o backend ainda restringe a leitura (`GET /api/v1/anticipations` e `GET /api/v1/anticipations/{id}`) para `Creator` e `Admin`, bloqueando o fluxo operacional do Analista.

O objetivo desta demanda é fechar esse gap de ponta a ponta para a persona `Analista`, garantindo que:

- A jornada de análise funcione do login até a decisão (visualizar lista, abrir detalhe, aprovar/recusar).
- O backend trate `Analista` como papel interno com visão global de leitura, alinhado à operação.
- O frontend apresente cópia/identidade visual de operação interna (Admin/Analista), sem ambiguidades de permissão.

### User stories de frontend e backend

- **US-AN1 – Acessar fila global como Analista**
  - Como **Analista**, quero **abrir a lista global de solicitações**, para **operar minha fila de análise sem bloqueios de autorização**.
- **US-AN2 – Abrir detalhe de qualquer solicitação**
  - Como **Analista**, quero **visualizar o detalhe de qualquer solicitação da fila**, para **avaliar contexto financeiro e decidir a ação correta**.
- **US-AN3 – Decidir solicitações em análise**
  - Como **Analista**, quero **aprovar ou recusar solicitações em estado Pending**, para **concluir o processo de análise com rastreabilidade**.
- **US-AN4 – Receber mensagens coerentes de permissão**
  - Como **Analista**, quero **não receber erros indevidos de acesso negado ao consultar lista/detalhe**, para **ter confiança na operação e reduzir escalonamentos para suporte técnico**.

### Personas / papéis

- **Analista (persona principal)**
  - Papel operacional interno responsável por triagem e decisão de solicitações.
- **Admin (persona secundária)**
  - Compartilha a mesma visão global e mantém acesso total aos fluxos internos.
- **Creator (impacto indireto)**
  - Não acessa a fila global; se beneficia de decisões mais rápidas e previsíveis.

### Telas, módulos, relatórios e navegação

- **Módulo lógico de frontend**
  - Área/módulo de **Antecipação** no contexto de operação interna.
- **Telas impactadas**
  - `Tela Lista global de solicitações` (`/anticipation/list`): fluxo existente passa a funcionar plenamente para `Analista`.
  - `Componente de detalhe da solicitação`: leitura e ações já existentes em RF-4 permanecem, com comportamento validado para `Analista`.
- **Navegação**
  - `Home` (role `Analista`) → `Lista global de solicitações` → seleção de item → detalhe → aprovar/recusar quando Pending.
- **Ajuste de UX/cópia**
  - Textos da página devem refletir contexto de **operação interna (Admin/Analista)**, evitando rótulos que sugiram exclusividade de Admin.

### Permissões e segurança

- **Role `Analista`**
  - Pode acessar a rota de lista global no frontend.
  - Pode consultar lista e detalhe no backend (`GET /api/v1/anticipations` e `GET /api/v1/anticipations/{id}`).
  - Pode aprovar/recusar solicitações Pending conforme RF-4/RA-3.
  - Não pode acessar operações exclusivas de administração de identidade (`/auth/admin/*`) nem criar simulaçoes ou novas solicitações.
- **Role `Admin`**
  - Mantém visão global de leitura e ações administrativas.
- **Role `Creator`**
  - Continua sem acesso à lista global e sem acesso a dados globais.

### Fluxos de uso e regras de negócio (UI + API)

#### Fluxo 1 – Acesso e leitura da fila global como Analista

1. Analista autentica na aplicação.
2. Acessa `/anticipation/list` via menu.
3. Frontend chama `GET /api/v1/anticipations` com filtros/paginação.
4. Backend retorna lista global sem erro de autorização indevido para `Analista`.
5. Analista clica em um item e frontend chama `GET /api/v1/anticipations/{id}`.
6. Backend retorna detalhe completo para operação.

#### Fluxo 2 – Decisão da solicitação (aprovar/recusar)

1. Com solicitação em `Pending`, o detalhe exibe botões de decisão (RF-4).
2. Analista aprova (`POST .../approve`) ou recusa (`POST .../reject`).
3. UI atualiza status e mantém feedback de sucesso/erro conforme contrato existente.

#### Regras de negócio da autorização (backend)

- Em consultas de lista:
  - `Admin` e `Analista` podem consultar visão global.
  - `Creator` continua restrito às próprias solicitações.
- Em consulta por id:
  - `Admin` e `Analista` podem consultar qualquer solicitação.
  - `Creator` só pode consultar solicitação própria.

### Critérios de aceitação (testáveis)

- **CA-RF7-1 – Analista acessa lista global sem bloqueio indevido**
  - Dado que estou autenticado como `Analista`,
  - Quando acesso `/anticipation/list`,
  - Então devo visualizar a lista global e o backend deve responder com sucesso para a consulta de lista.
- **CA-RF7-2 – Analista abre detalhe de qualquer solicitação**
  - Dado que estou autenticado como `Analista` e existe ao menos uma solicitação na lista,
  - Quando seleciono uma solicitação,
  - Então devo visualizar o detalhe sem erro de autorização.
- **CA-RF7-3 – Creator continua bloqueado da lista global**
  - Dado que estou autenticado como `Creator`,
  - Quando tento acessar `/anticipation/list`,
  - Então devo ser redirecionado/bloqueado no frontend e não devo visualizar dados globais.
- **CA-RF7-4 – Analista decide solicitações Pending**
  - Dado que estou autenticado como `Analista` e a solicitação está `Pending`,
  - Quando aprovo ou recuso a solicitação,
  - Então a ação deve ser processada com sucesso e o status exibido deve refletir a transição.
- **CA-RF7-5 – Coerência de mensagens de permissão**
  - Dado que estou autenticado como `Analista`,
  - Quando uso a tela de lista global e detalhe em fluxos permitidos,
  - Então não devo receber mensagens de “acesso negado” para operações autorizadas.

### Componentes de UI e comportamento

- **Página de lista global**
  - Reaproveita filtros, tabela e detalhe existentes em RF-2/RF-4.
  - Exibe textos de contexto interno compatíveis com `Admin` e `Analista`.
- **Componente de detalhe**
  - Mantém botões de aprovar/recusar condicionados a status Pending e role permitida.
  - Mantém feedback de erro com contexto + motivo + código de suporte.

### Requisitos visuais, UX e acessibilidade (UX/UI)

- Linguagem e microcopy devem refletir “Operações internas” em vez de contexto exclusivo de Admin.
- Fluxo de teclado e foco (filtros → tabela → detalhe → ações) deve permanecer íntegro para Analista.
- Mensagens de erro devem seguir padrão do app e evitar jargão técnico.

### Requisitos técnicos/metodológicos aplicáveis (frontend + backend)

- **Frontend Angular**
  - Manter arquitetura por camadas (`domain`, `application`, `infrastructure`, `features`, etc.) sem acoplamento indevido.
  - Guardas/rotas e renderização por role devem continuar centralizados em `AuthGuard` e shell.
- **Backend .NET / Clean Architecture**
  - Ajustar autorizações dos endpoints de leitura de antecipações para incluir role `Analista`.
  - Ajustar handlers de leitura para tratar `Analista` como papel de visão global, mantendo restrição por owner apenas para `Creator`.
  - Preservar regras de domínio e transições existentes em RA-3.
- **Metodologia**
  - Spec-driven development com rastreabilidade explícita CA-RF7-x.
  - TDD obrigatório com pirâmide completa (unitário, integração, E2E).

### Mapeamento de impacto (maestro)

- **Frontend**
  - Rotas/guards: manter `requiredRoles: ['Admin', 'Analista']` para `/anticipation/list`.
  - Feature de lista global/detalhe: ajustar cópias para contexto interno compartilhado.
  - E2E: garantir cenário de `Analista` completo (listar, detalhar, decidir).
- **Backend**
  - `AnticipationController`:
    - Incluir `Analista` nos atributos `[Authorize(Roles = ...)]` de `GET /api/v1/anticipations` e `GET /api/v1/anticipations/{id}`.
  - `ListAnticipationRequestsQueryHandler`:
    - Tratar `Analista` como visão global (não forçar `creatorFilter = userId`).
  - `GetAnticipationRequestByIdQueryHandler`:
    - Permitir acesso global para `Analista` no detalhe.

### Diretrizes de testes

- **Unitários (backend)**
  - Handler de lista: `Analista` consulta global com filtros.
  - Handler de detalhe: `Analista` acessa solicitação de qualquer creator.
- **Integração (backend)**
  - `GET /api/v1/anticipations` com token `Analista` retorna 200 + lista.
  - `GET /api/v1/anticipations/{id}` com token `Analista` retorna 200 para item de outro creator.
- **Unit/integração (frontend)**
  - Guard/rota/menu para `Analista` permanecem acessíveis.
  - Página de lista global carrega e abre detalhe sem erro para role `Analista`.
- **E2E**
  - Login como `Analista` → abrir lista global → abrir detalhe → aprovar/recusar solicitação `Pending`.
  - Login como `Creator` → tentativa de acesso à lista global continua bloqueada.

### Spec para agentes de IA

- **Seções-chave para implementação**
  - Critérios CA-RF7-x.
  - Mapeamento de impacto (maestro).
  - Diretrizes de testes.
- **Nomenclatura sugerida**
  - Testes backend: sufixos específicos para role `Analista` em testes de listagem/detalhe.
  - Testes E2E frontend: cenário dedicado de operação completa do `Analista` na fila global.

### Dependências e riscos

- **Dependências**
  - RF-2 (lista global), RF-4 (aprovar/recusar), RA-3 (transições), autenticação JWT com role `Analista`.
- **Riscos**
  - Divergência entre permissão de rota frontend e autorização backend.
  - Mitigação: testes de integração backend e E2E frontend cobrindo role `Analista` explicitamente.

### Rastreabilidade

- **Backend**
  - Endpoints: `GET /api/v1/anticipations`, `GET /api/v1/anticipations/{id}`, `POST /api/v1/anticipations/{id}/approve`, `POST /api/v1/anticipations/{id}/reject`.
  - Handlers: `ListAnticipationRequestsQueryHandler`, `GetAnticipationRequestByIdQueryHandler`.
- **Frontend**
  - Rota: `/anticipation/list`.
  - Componentes/páginas: lista global, filtros, tabela e detalhe de solicitação.
  - Testes: unitários/integrados de rota e E2E de jornada completa do `Analista`.

