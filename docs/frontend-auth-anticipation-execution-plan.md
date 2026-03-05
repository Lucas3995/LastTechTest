## Plano de execução – frontend-auth-anticipation-guard

Este documento registra o plano de execução para implementar o plano `[frontend-auth-anticipation-guard_29d6c70f.plan.md]`, cobrindo todos os *todos* relacionados a autenticação e refinamento de erros da RF-1.

---

### 1. auth-contract – contrato mínimo de autenticação

- Consolidar requisitos de login/autorização a partir da RF-1 e dos cards de backend (RA-1, RA-3, RC-1), com foco em:
  - Login bem-sucedido com usuário Creator.
  - Credenciais inválidas.
  - Usuário sem role Creator.
  - Token expirado/sessão inválida.
- Criar `docs/auth-contract.md` documentando:
  - Endpoint `POST /api/v1/auth/login`.
  - Payload `{ email, password }` (ou `username` conforme ambiente).
  - Resposta `{ accessToken, refreshToken?, user: { id, role, creatorId? } }`.
  - Estrutura de erro `{ code, message, traceId }` + status HTTP.
- Usar este contrato como referência única para `AuthService`, guard e login.

### 2. core-auth-service – serviço de autenticação em core

- Criar `frontend/src/app/core/auth/auth.service.ts` com:
  - Tipos de domínio para sessão: `AuthUser`, `AuthSession`.
  - Estado reativo (`Signal` ou `Observable`) para `currentUser`, `isAuthenticated`, `currentCreatorId`.
  - Métodos:
    - `login(credentials)` chamando um `AuthHttpService` em `infrastructure` que usa `/api/v1/auth/login`.
    - `logout()`, `clearSession()`, `loadSessionFromStorage()`.
  - Persistência em `localStorage` (tokens + dados mínimos do usuário).
  - Tratamento de erros de login via `buildErrorPresentation` (contexto + motivo + supportId).
- Antes da implementação, derivar testes (unit/integração) para estes comportamentos, seguindo o fluxo tradutor → maestro → quadro-de-recompensas.

### 3. auth-guard-routes – guardas e proteção de rotas

- Criar `frontend/src/app/core/auth/auth.guard.ts` implementando `CanActivate` ou `CanMatch`:
  - Se não autenticado e rota exigir auth:
    - Cancelar navegação.
    - Redirecionar para `/login` com `returnUrl` apontando para a URL original.
  - Se autenticado mas sem `requiredRole` (ex.: `Creator`):
    - Bloquear acesso e redirecionar (para `/login` ou página de acesso negado mínima).
- Ajustar `frontend/src/app/app.routes.ts`:
  - Adicionar rota `/login` para `LoginPageComponent`.
  - Adicionar guard e `data: { requiredRole: 'Creator' }` na rota `anticipation/my-requests`.
- Garantir testes de guard cobrindo os três cenários: não autenticado, autenticado Creator, autenticado com role incorreta.

### 4. login-page-ui – página de login com UX/storytelling

- Criar `frontend/src/app/features/auth/pages/login/LoginPageComponent` com:
  - Formulário reativo tipado com campos de credenciais (`email`/`username`, `password`) e validações básicas.
  - Mensagens de erro de formulário claras (campos obrigatórios, formato).
  - Textos alinhados ao contexto de sistema interno LastLink (sem jargões técnicos de auth).
  - Uso de `AuthService.login()` no submit:
    - Em sucesso: redirecionar para `returnUrl` (se presente) ou `/anticipation/my-requests`.
    - Em erro: exibir mensagem rica baseada em `buildErrorPresentation` (contexto + motivo + código de suporte).
- Criar testes unitários/integração para:
  - Bloqueio de submit com form inválido.
  - Fluxo feliz de login.
  - Exibição de feedback de erro de credenciais.

### 5. shell-auth-integration – navegação condicionada ao estado de auth

- Atualizar `frontend/src/app/shared/components/layout/shell.component.ts/html` para:
  - Injetar `AuthService` (ou façade equivalente).
  - Expor `isAuthenticated()` para o template.
  - Mostrar link “Minhas antecipacoes” apenas quando autenticado.
  - Exibir ação de “Sair” quando autenticado, chamando `authService.logout()` e redirecionando para `/login`.
  - Quando não autenticado, exibir opção para “Entrar” apontando para `/login`.
- Adicionar testes que validem a visibilidade dos links conforme o estado de autenticação.

### 6. anticipation-facade-auth – façade usando creatorId

- Ajustar `frontend/src/app/application/anticipation/anticipation-my-requests.facade.ts` para:
  - Injetar `AuthService` (ou interface de contexto de usuário).
  - Obter `currentCreatorId` antes de:
    - `loadWithFilter` / `listMyRequests`.
    - `cancelRequest`.
    - Outras operações que dependam do Creator.
  - Se não houver `creatorId` válido:
    - Produzir mensagem de erro de auth coerente com o storytelling (sessão inválida/expirada, peça para entrar novamente).
  - Manter uso de `buildErrorPresentation` para erros de backend, incluindo os de auth.
- Testar que todas as chamadas de infraestrutura recebem o `creatorId` correto e que a lista RF-1 está sempre restrita ao Creator logado.

### 7. auth-tests – testes de AuthService, AuthGuard, LoginPage e E2E básicos

- Criar/ajustar specs para:
  - `AuthService`:
    - Login sucesso (estado e persistência).
    - Logout/clearSession.
    - Bootstrap de sessão via `loadSessionFromStorage()`.
  - `AuthGuard`:
    - Redirecionamento de não autenticado para `/login?returnUrl=...`.
    - Permissão para autenticado Creator.
    - Bloqueio para role incorreta.
  - `LoginPageComponent`:
    - Validação de formulário.
    - Fluxo feliz e erros de credenciais.
- Adicionar um novo spec E2E (ex.: `frontend/e2e/auth-login-flow.spec.ts`) cobrindo:
  - Acesso não autenticado a `/anticipation/my-requests` redirecionando para `/login`.
  - Login e navegação subsequente até RF-1.
  - Logout e novo bloqueio de acesso à RF-1.

### 8. api-proxy-config – garantir que /api/v1 atinja o backend real

- Criar ou ajustar `frontend/proxy.conf.json` para mapear `/api/v1` para o backend real de antecipação (host/porta usados em desenvolvimento).
- Configurar `angular.json` ou o script `npm run start` para usar esse proxy (`ng serve --proxy-config proxy.conf.json`).
- Alternativamente (ou em complemento), garantir que serviços HTTP usem um `baseUrl` correto para backend em dev.
- Validar via devtools:
  - Que chamadas `/api/v1/...` não retornam mais HTML do dev-server (status 200 com página).
  - Que falhas de autenticação retornam 401/403 do backend, permitindo que guards/interceptores reajam corretamente.

### 9. rf1-error-pattern-refine – refinar padrão de erros de RF-1

- Em `frontend/src/app/application/anticipation/backend-error.util.ts`:
  - Tratar explicitamente:
    - `status === 0` como problema de rede/CORS:
      - Mensagem de contexto “Nao foi possivel contactar o servico de antecipacao agora.”.
      - `supportId = NETWORK_UNAVAILABLE` (ou equivalente).
    - `status === 200` com payload inválido/HTML:
      - Mensagem de contexto indicando possível problema de ambiente/proxy (“Resposta inesperada do servico de antecipacao.”).
      - `supportId` distinto de `HTTP_200`, por exemplo `UNEXPECTED_RESPONSE_200`.
  - Manter extração de `code`, `message`, `traceId` quando presentes, para enriquecer motivo e suporte.
- Em `anticipation-my-requests.facade.ts`:
  - Continuar usando `buildErrorPresentation`, mas:
    - Manter `errorMessage` apenas com contexto + motivo.
    - Expor `errorSupportId` separadamente para a UI.
- Em `anticipation-my-requests-page.component.html`:
  - Banner de erro exibindo `errorMessage()`.
  - Linha separada, condicional, para “Codigo para suporte: {{ errorSupportId() }}”.
- Adicionar testes unitários para `buildErrorPresentation` cobrindo:
  - Payload estruturado (`code`, `message`, `traceId`).
  - `status === 0`.
  - `status === 200` com corpo HTML/string inesperada.

