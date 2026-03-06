## Rastreamento: requisitos → casos de uso → testes

### 1. Requisitos de autenticação

- **R1 – Registro de usuário básico (email/senha)**
  - Casos de uso:
    - `RegisterUserCommand` + `RegisterUserCommandHandler`
  - Endpoints:
    - `POST /auth/register`
  - Testes:
    - Integração: `AuthHandlersIntegrationTests.Register_Then_Login_Should_Return_Tokens`
    - E2E: `AuthE2ETests.Full_Auth_Flow_Should_Succeed`

- **R2 – Login com geração de access/refresh token**
  - Casos de uso:
    - `LoginCommand` + `LoginCommandHandler`
    - Serviços: `ITokenService`, `IUserPasswordHasher`
  - Endpoints:
    - `POST /auth/login`
  - Testes:
    - Unit: `PasswordHasherTests`, `TokenServiceTests`
    - Integração: `AuthHandlersIntegrationTests.Register_Then_Login_Should_Return_Tokens`
    - E2E: `AuthE2ETests.Full_Auth_Flow_Should_Succeed`

- **R3 – Renovação de access token via refresh token**
  - Casos de uso:
    - `RefreshTokenCommand` + `RefreshTokenCommandHandler`
    - Repositório: `IUserTokenRepository`
  - Endpoints:
    - `POST /auth/refresh`
  - Testes:
    - (ponto de extensão para futuros testes unitários/integrados específicos de refresh)

- **R4 – Logout com invalidação lógica de refresh tokens**
  - Casos de uso:
    - `LogoutCommand` + `LogoutCommandHandler`
  - Endpoints:
    - `DELETE /auth/logout`
  - Testes:
    - (ponto de extensão para futuros testes focados em revogação de tokens)

- **R5 – Consulta de usuário logado**
  - Casos de uso:
    - `GetLoggedUserQuery` + `GetLoggedUserQueryHandler`
    - Serviço: `ICurrentUserService`
  - Endpoints:
    - `GET /user/logged` (requer autenticação)
  - Testes:
    - E2E: `AuthE2ETests.Full_Auth_Flow_Should_Succeed` (inclui chamada autenticada a `/user/logged`)

### 2. Apoio à rotina-completa

- **Skills e fluxo**:
  - `tradutor` → requisitos como os listados acima.
  - `maestro` → mapeia requisitos para comandos/handlers/endpoints (seção 1 e 3).
  - `quadro-de-recompensas` → deriva testes unit, integration e E2E por requisito.
  - `batedor-de-codigos` → verifica aderência à Clean Architecture e SOLID.
  - `mestre-freire` → refatora mantendo o mapeamento requisitos ↔ casos de uso ↔ testes.

Este arquivo funciona como ponto de apoio para o `maestro` e o `quadro-de-recompensas` manterem a rastreabilidade viva à medida que novos requisitos e testes forem adicionados.

### 3. Requisitos de antecipação de recebíveis

- **RA-1 – Criar solicitação de antecipação** (implementado)
  - Casos de uso:
    - `CreateAnticipationRequestCommand` + `CreateAnticipationRequestCommandHandler`
  - Endpoints:
    - `POST /api/v1/anticipations`
  - Persistência: migração `InitialCreate` (LastTechTest.Persistencia/Migrations) inclui tabela `AnticipationRequests`.
  - Testes (árvore RA-1):
    - Unit (U1–U8):
      - U1–U2: `AnticipationCalculationServiceTests.cs` — cálculo bruto/taxas/líquido, limite creator
      - U3–U4: `EligibilityServiceTests.cs` — elegibilidade de recebíveis
      - U5–U6: `CreateAnticipationRequestCommandValidatorTests.cs` — validação do command
      - U7–U8: `AnticipationRequestEntityTests.cs` — estado inicial, regras sem persistência (CA5)
    - Integração (I1–I5):
      - `CreateAnticipationRequestHandlerIntegrationTests.cs` — Creator/Admin, persistência, rejeição por limite e recebível ineligível
    - E2E (E1–E5):
      - `AnticipationE2ETests.cs` — POST como Creator/Admin, 401 sem token, 403/400 Creator com outro creator_id, 400 validação negócio
    - Integração (containerização):
      - `DatabaseStartupLegacySchemaIntegrationTests.cs` — arranque da API com DB em esquema legado (sem migration history) não deve falhar; garante que o fallback cria a tabela `AnticipationRequests` e evita regressão em Docker/volumes antigos

- **RA-2 – Consultar solicitações de antecipação** (implementado)
  - Casos de uso:
    - `ListAnticipationRequestsQuery` + `ListAnticipationRequestsQueryHandler` (lista paginada com filtros; Creator: forçar creator_id do token; Admin: aceitar filtros)
    - `GetAnticipationRequestByIdQuery` + `GetAnticipationRequestByIdQueryHandler` (detalhe por id; Creator: 403 se não for do creator; Admin: qualquer id)
  - Persistência: `IAnticipationRequestRepository.ListAsync` (filtros creator, status, período; paginação em banco).
  - Endpoints:
    - `GET /api/v1/anticipations` (query params: creatorId, status, fromUtc, toUtc, page, pageSize; autorização Creator ou Admin)
    - `GET /api/v1/anticipations/{id}` (detalhe; autorização Creator ou Admin)
  - Testes (árvore RA-2):
    - Unit:
      - `ListAnticipationRequestsQueryHandlerTests.cs` — CA1 (Creator ignora filtro e usa token), CA3 (Admin aplica filtros), CA5 (paginação), não autenticado
      - `GetAnticipationRequestByIdQueryHandlerTests.cs` — CA2 (Creator outro → Unauthorized), CA4 (Admin qualquer id), id inexistente → null, não autenticado
    - Integração:
      - `ListAnticipationRequestsHandlerIntegrationTests.cs` — CA1 (Creator só próprias), CA3 (Admin todas / filtro por creator), CA5 (paginação), não autenticado
      - `GetAnticipationRequestByIdHandlerIntegrationTests.cs` — CA2 (Creator próprio/outro), CA4 (Admin qualquer), 404, não autenticado
    - E2E:
      - `AnticipationE2ETests.cs` (E6–E12) — GET list Creator só próprias (CA1), GET list Admin (CA3), GET by id Creator outro → 403 (CA2), GET by id Admin (CA4), GET list sem token 401, GET list paginação (CA5), GET by id 404

- **RA-3 – Estados e transições da solicitação de antecipação** (testes criados; handlers/endpoints stub até implementação)
  - Casos de uso:
    - `ApproveAnticipationRequestCommand` + `ApproveAnticipationRequestCommandHandler` (LastTechTest.Aplicacao/Anticipation/Commands/ApproveAnticipationRequest/)
    - `RejectAnticipationRequestCommand` + `RejectAnticipationRequestCommandHandler` (LastTechTest.Aplicacao/Anticipation/Commands/RejectAnticipationRequest/)
    - `CancelAnticipationRequestCommand` + `CancelAnticipationRequestCommandHandler` (LastTechTest.Aplicacao/Anticipation/Commands/CancelAnticipationRequest/)
  - Domínio:
    - `AnticipationRequestStatus`: valores `Approved`, `Rejected`, `CanceledByCreator` (enum em LastTechTest.Dominio/Enums/AnticipationRequestStatus.cs)
    - Especificação executável de regras de transição no projeto de testes: `AnticipationTransitionSpec` + `AnticipationTransitionRulesTests.cs`
  - Endpoints:
    - `POST /api/v1/anticipations/{id}/approve` (body: Observation opcional; roles Analista, Admin; retorna 501 até implementação)
    - `POST /api/v1/anticipations/{id}/reject` (body: Reason obrigatório; roles Analista, Admin; retorna 501 até implementação)
    - `POST /api/v1/anticipations/{id}/cancel` (body: Reason opcional; roles Creator, Admin; retorna 501 até implementação)
  - Relatório de alterações: `docs/relatorio-alteracoes-RA-3.md`
  - Testes (árvore RA-3):
    - Unit:
      - `AnticipationTransitionSpec.cs` — especificação de estados e regras (AnalysisPending, Approved, Rejected, CanceledByCreator; CanApprove/CanReject/CanCancel por role; IsSameStateTransition)
      - `AnticipationTransitionRulesTests.cs` — transições válidas (Analista/Admin approve/reject, Creator dono e Admin cancel), inválidas (Creator approve/reject, Creator não dono cancel, a partir de estados finais), A→A (idempotência)
    - Integração:
      - `AnticipationTransitionHandlersIntegrationTests.cs` — Approve/Reject/Cancel handlers (invocação; cenários Analista/Admin approve, Creator/Admin cancel, Creator não pode approve/reject, cancel idempotente, transições a partir de Approved/Rejected/CanceledByCreator; atualmente esperam NotImplementedException até implementação)
    - E2E:
      - `AnticipationE2ETests.cs` (E13–E20) — CA1 approve por Analista (E13), CA2 cancel válido (E14) e idempotente (E15), CA3 reject por Analista (E16), CA4 bloqueio transição inválida (E17), CA5 Creator approve → 403 (E18) e Admin approve (E19), POST approve sem token 401 (E20)

- **RA-4 – Simulação de solicitação de antecipação (fake, sem persistência)** (implementado)
  - Casos de uso:
    - `SimulateAnticipationRequestCommand` + `SimulateAnticipationRequestCommandHandler` + `SimulateAnticipationRequestCommandValidator`
    - `ConvertSimulationToRealRequestCommand` + `ConvertSimulationToRealRequestCommandHandler`
  - Abstrações: `IAnticipationSimulationCache`, `SimulationData`, `CachedSimulationEntry` (LastTechTest.Aplicacao.Anticipation.Simulation); implementação `MemoryAnticipationSimulationCache` (Infrastrutura) registada em DI.
  - Endpoints:
    - `POST /api/v1/anticipations/simulations` (simular, retornar código e validade exposta 20 min antes do TTL real de 2h)
    - `POST /api/v1/anticipations/simulations/{simulationCode}/confirm` (converter simulação em solicitação real; 422 para "already used" / "open request")
  - Testes:
    - Unit:
      - `SimulateAnticipationRequestCommandHandlerTests.cs` — CA1 (Creator válido, SimulationCode + ValidUntilUtc + valores), CA2 (validação falha), CA3 (não chama repo), CA4 (Analista/Admin em nome de), CA6 (duas simulações substituem), limites 100, sem elegíveis
      - `SimulateAnticipationRequestCommandValidatorTests.cs` — valor &lt; 100 inválido, ≥ 100 válido (limite)
      - `ConvertSimulationToRealRequestCommandHandlerTests.cs` — CA7 (conversão com valores idênticos, MarkAsUsed), CA8 (código inexistente, já utilizado, pendente em aberto, revalidação falha), não autenticado
    - Integração:
      - `AnticipationSimulationHandlerIntegrationTests.cs` — I1 (simulação não persiste), I2 (segunda substitui primeira), I3 (valor inválido validação), I4–I6 (conversão válida / código usado / pendente em aberto); usa `InMemoryAnticipationSimulationCache`
    - E2E:
      - `AnticipationSimulationE2ETests.cs` — RA4_CA1 (POST simulations 200 + contrato), RA4_CA2 (amount &lt; 100 → 400), RA4_CA3 (múltiplas simulações sem criar antecipações), RA4_CA4 (Analista/Admin com creatorId), RA4_CA6/CA7/CA8 (confirm primeiro/segundo código, confirm inexistente/usado/pendente), POST sem token 401
  - Mapeamento CA → testes: CA1 (unit Simulate handler, E2E RA4_CA1), CA2 (unit Simulate + Validator, E2E RA4_CA2), CA3 (unit + integration I1, E2E RA4_CA3), CA4 (unit Simulate Analista/Admin, E2E RA4_CA4), CA5 (opcional desempenho), CA6 (unit duas simulações, integration I2, E2E RA4_CA6), CA7 (unit Convert, integration I4, E2E RA4_CA7), CA8 (unit Convert cenários, integration I5–I6, E2E RA4_CA8).

### 3.1. Requisitos de frontend – antecipação (RF-x)

- **RF-4 – Aprovar e recusar solicitações no frontend (Analistas e Admins)** (implementado)
  - Demanda: `demandas/RF-4-aprovar-recusar-solicitacoes-frontend.md`
  - Backend utilizado: RA-3 — `POST /api/v1/anticipations/{id}/approve` (body: Observation opcional), `POST /api/v1/anticipations/{id}/reject` (body: Reason obrigatório); roles Analista, Admin.
  - Frontend (implementado):
    - Domain: `AnticipationRequestsPort` em `domain/anticipation.ts` estendido com `approveRequest(id, observation?)` e `rejectRequest(id, reason)`.
    - Infrastructure: `AnticipationRequestsHttpService` — POST `.../id/approve` e POST `.../id/reject`; mapeamento resposta `{ id, protocol, status }` para domínio.
    - Application: `AnticipationAdminListFacade` com `approveRequest` e `rejectRequest`; atualização de `selectedRequest` e lista; mensagens de sucesso/erro.
    - Features: `AnticipationRequestDetailComponent` com input `canShowApproveReject`, botões Aprovar/Recusar quando status Pending, outputs `requestApprove`/`requestReject`; recusa com motivo obrigatório (diálogo e validação). Página admin-list passa `canShowApproveReject` e liga eventos à facade; exibe `infoMessage`.
  - Dependências: RF-2 (lista global e detalhe).
  - Testes: unit (facade approve/reject sucesso e erro; componente detalhe visibilidade e validação motivo); integração (HTTP approve/reject); E2E em `anticipation-admin-requests.e2e.spec.ts`.

- **RF-5 – Criar nova solicitação de antecipação no frontend (Creators)** (card criado; implementação pendente)
  - Demanda: `demandas/RF-5-criar-solicitacao-frontend.md`
  - Backend utilizado: RA-1, RC-1 — `POST /api/v1/anticipations` (body: requestedAmount, creatorId?); validação valor >= 100 e uma pendente por creator.
  - Frontend (a implementar):
    - Domain: `AnticipationRequestsPort` estendido com `createRequest(payload)` (requestedAmount, creatorId?).
    - Infrastructure: `AnticipationRequestsHttpService` — POST `/api/v1/anticipations`; mapeamento resposta para domínio.
    - Application: facade Minhas solicitações (ou dedicado) com `createRequest(...)`; em sucesso recarrega lista e feedback; em erro mensagem apresentável.
    - Features: formulário/página "Nova solicitação" (campo valor mínimo 100, opcional creator para Admin); rota ex.: `anticipation/my-requests/new` ou modal; botão "Nova solicitação" em RF-1 ligado a navegação/abertura.
  - Dependências: RF-1 (Minhas solicitações e botão "Nova solicitação").
  - Testes (a preencher na implementação): unit (port, facade, formulário — validação valor, cancelar); integração (HTTP create, facade + lista); E2E opcional (Creator cria e vê na lista).

### 4. Correções (cards RC-x)

- **RC-1 – Corrigir validações no endpoint de criar solicitação de antecipação** (implementado)
  - Demanda: `demandas/RC-1-bugs-criar-solicitacao-antecipacao.md`
  - Regras aplicadas: valor solicitado **>= 100** (antecipação de 100 ou mais; rejeitar &lt; 100); no máximo uma solicitação em análise (Created/Pending) por creator.
  - Casos de uso impactados:
    - `CreateAnticipationRequestCommand` + `CreateAnticipationRequestCommandHandler`
    - `CreateAnticipationRequestCommandValidator`
  - Pipeline de validação: `LastTechTest.Aplicacao.Common.Behaviors.ValidationBehavior<TRequest, TResponse>`; validadores registados via `AddValidatorsFromAssemblyContaining`; `ValidationException` mapeada para 400 no `POST /api/v1/anticipations`.
  - Endpoints:
    - `POST /api/v1/anticipations` (validação FluentValidation + regra "uma pendente por creator" no handler)
  - Domínio: `AnticipationTransitionRules.IsAnalysisPendingStatus(AnticipationRequestStatus)` exposto para uso em memória; no repositório usa-se expressão traduzível para SQL (EF Core não traduz chamada a método estático).
  - Infraestrutura/domínio:
    - `IAnticipationRequestRepository.HasPendingByCreatorAsync(creatorId)`; implementação em `AnticipationRequestRepository` com condição inline `Status == Created || Status == Pending` (traduzível para SQL).
  - Testes criados/alterados:
    - Unit: `CreateAnticipationRequestCommandValidatorTests` — valor 0, negativo, 99, 99.99 (rejeitado); 100, 100.00, 100.01, 101 (aceite). `CreateAnticipationRequestCommandHandlerTests` — `Handle_WhenCreatorHasPendingRequest_ThrowsInvalidOperation`, `Handle_WhenCreatorHasNoPendingRequest_ContinuesToCreate`; mocks de `HasPendingByCreatorAsync` nos testes existentes.
    - Integração: `CreateAnticipationRequestHandlerIntegrationTests` — I5b (HasPendingByCreatorAsync contra SQLite, cobre LINQ-to-SQL), I6 (creator com pendente → segunda criação falha), I7 (admin criando para creator com pendente → falha).
    - E2E: `AnticipationE2ETests` — E5b_RC1 (POST 50, 99, 99.99 → 400), E5c_RC1 (POST 100, 100.00, 100.01, 101 sem pendente → 201), E5d_RC1 (POST 100 com pendente → 400); E7 e E8 com valores >= 100 (101/201 e 150).

- **RC-2 – Registrar auditoria na criação de solicitação de antecipação (RA-1)** (card criado; implementação pendente)
  - Demanda: `demandas/RC-2-auditoria-criacao-solicitacao.md`
  - Regra: auditoria mínima na criação (RA-1): quem chamou, role, timestamp, valores envolvidos.
  - Casos de uso impactados:
    - `CreateAnticipationRequestCommandHandler` (invocar serviço de auditoria após criar entidade)
  - Endpoints:
    - `POST /api/v1/anticipations` (sem mudança de contrato; apenas efeito lateral de auditoria)
  - Infraestrutura/domínio:
    - Extensão de `IAnticipationAuditService` (ex.: `RecordCreationAsync`) ou novo contrato; entidade/tabela de auditoria de criação ou extensão de `AnticipationRequestAudit` com ação "Create".
  - Testes (a preencher na implementação):
    - Unit: componente que monta evento de auditoria de criação.
    - Integração: após criar solicitação, verificar registo de auditoria com userId, role, timestamp e valores.
    - E2E: opcional.

- **RC-3 – Ativar auditoria persistente nas transições de solicitação (RA-3)** (card criado; implementação pendente)
  - Demanda: `demandas/RC-3-auditoria-persistente-transicoes.md`
  - Regra: transições (approve/reject/cancel) devem persistir registo em `AnticipationRequestAudits`; hoje a API usa `NoOpAnticipationAuditService`.
  - Casos de uso impactados:
    - Mesmos handlers e `AnticipationTransitionExecutor` de RA-3; alteração no registro de `IAnticipationAuditService`.
  - Endpoints:
    - `POST /api/v1/anticipations/{id}/approve`, `.../reject`, `.../cancel` (sem mudança de contrato; apenas ativação de persistência).
  - Infraestrutura:
    - `LastTechTest.Persistencia.Services.AnticipationAuditService`; registro em `Program.cs` por ambiente/config (usar implementação persistente em vez de NoOp).
  - Testes (a preencher na implementação):
    - Integração: approve/reject/cancel com `AnticipationAuditService` real; verificar registos em `AnticipationRequestAudits`.
    - E2E: opcional — verificar registo de auditoria após chamada HTTP de transição.
