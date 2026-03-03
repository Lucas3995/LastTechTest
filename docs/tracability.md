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
  - `maestro` → mapeia requisitos para comandos/handlers/endpoints (seção 1).
  - `quadro-de-recompensas` → deriva testes unit, integration e E2E por requisito.
  - `batedor-de-codigos` → verifica aderência à Clean Architecture e SOLID.
  - `mestre-freire` → refatora mantendo o mapeamento requisitos ↔ casos de uso ↔ testes.

Este arquivo funciona como ponto de apoio para o `maestro` e o `quadro-de-recompensas` manterem a rastreabilidade viva à medida que novos requisitos e testes forem adicionados.

