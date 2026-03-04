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

- **RA-3 – Estados e transições da solicitação de antecipação**
  - Casos de uso (previstos):
    - `ApproveAnticipationRequestCommand` + `ApproveAnticipationRequestCommandHandler`
    - `RejectAnticipationRequestCommand` + `RejectAnticipationRequestCommandHandler`
    - `CancelAnticipationRequestCommand` + `CancelAnticipationRequestCommandHandler`
  - Endpoints (previstos):
    - `POST /api/v1/anticipations/{id}/approve`
    - `POST /api/v1/anticipations/{id}/reject`
    - `POST /api/v1/anticipations/{id}/cancel`
  - Testes (a serem criados):
    - Unit: máquina de estados, transições válidas/ inválidas (incluindo bloqueio A → A), regras por role (`Creator`, `Analista`, `Admin`)
    - Integração: handlers + persistência + auditoria de transições
    - E2E: cenários CA1–CA5 do RA-3 via API

- **RA-4 – Simulação de solicitação de antecipação (fake, sem persistência)**
  - Casos de uso (previstos):
    - `SimulateAnticipationRequestQuery`/`SimulateAnticipationRequestCommand` + handler correspondente
    - `ConvertSimulationToRealRequestCommand` + `ConvertSimulationToRealRequestCommandHandler`
  - Endpoints (previstos):
    - `POST /api/v1/anticipations/simulations` (simular, retornar código e validade exposta)
    - `POST /api/v1/anticipations/simulations/{simulationCode}/confirm` (converter simulação em solicitação real)
  - Testes (a serem criados):
    - Unit: reuso de regras de cálculo/validação da criação real, comportamento do cache (TTL de 2h, apenas última simulação por creator, substituição, marcação como utilizada)
    - Integração: casos de uso de simulação e conversão com cache in-memory + persistência, garantindo que simulação não grava solicitações e que conversão cria solicitação real com valores idênticos aos da simulação
    - E2E: cenários CA1–CA8 do RA-4 via API (simulação, cache, conversão, rejeições e bloqueio por solicitação em aberto)
