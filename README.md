## LastTechTest Backend (.NET 10, Clean Architecture)

[![.NET](https://img.shields.io/badge/.NET-10.0-blue.svg)](https://dotnet.microsoft.com/download)
[![Architecture](https://img.shields.io/badge/Architecture-Clean%20Architecture-orange.svg)](https://blog.cleancoder.com/uncle-bob/2012/08/13/the-clean-architecture.html)
[![Tests](https://img.shields.io/badge/Tests-Unit%20%7C%20Integration%20%7C%20E2E-brightgreen.svg)](#-piramide-de-testes)
[![Docker](https://img.shields.io/badge/Docker-Compose-blue.svg)](#-como-rodar-com-docker-recomendado)
[![CI](https://img.shields.io/badge/GitHub-Actions-lightgrey.svg)](#-ci-e-fitness-functions)

Backend em **.NET 10 / C# 14**, com **Clean Architecture**, **CQRS + MediatR**, **EF Core 10 + SQLite**, autenticação **JWT** e pirâmide de testes completa (unitário, integração e E2E), totalmente containerizado.

> **Objetivo**: ser um _blueprint_ de backend monolítico moderno, fácil de entender, extender e rodar com um único comando Docker.

---

### 🔧 Stack principal

- **Framework**: .NET 10 (`net10.0`), ASP.NET Core 10
- **Arquitetura**: Clean Architecture (Domínio isolado de frameworks), CQRS, DI nativa
- **Persistência**: EF Core 10 + SQLite (placeholder para futuros bancos)
- **Autenticação**: JWT (access + refresh token), estrutura para MFA
- **Testes**: xUnit, FluentAssertions, Coverlet (cobertura), WebApplicationFactory para E2E
- **DevOps**: Docker + docker compose, GitHub Actions (build + testes + cobertura mínima)

---

### 🧱 Visão geral da solução

| Camada / Projeto                | Responsabilidade principal                                                                 |
|---------------------------------|-------------------------------------------------------------------------------------------|
| `LastTechTest.Dominio`         | Entidades, enums e interfaces de domínio, sem dependência de frameworks                   |
| `LastTechTest.Aplicacao`       | Casos de uso (Commands/Queries/Handlers), validações, DTOs                               |
| `LastTechTest.Persistencia`    | `ApplicationDbContext`, configurações EF Core 10, repositórios SQLite                    |
| `LastTechTest.Infrastrutura`   | Serviços técnicos (TokenService, PasswordHasher, MFA, Email, KeyGenerator)               |
| `LastTechTest.API`             | Endpoints HTTP (minimal API), autenticação JWT, Scalar/OpenAPI, wiring de DI            |
| `LastTechTest.Testes`          | Testes unitários, integração e E2E                                                       |

Estrutura de pastas (resumida):

```text
backend/
  LastTechTest.API/
  LastTechTest.Aplicacao/
  LastTechTest.Dominio/
  LastTechTest.Infrastrutura/
  LastTechTest.Persistencia/
  LastTechTest.Testes/
docker/
  docker-compose.yml
docs/
  architecture/ADR-001-clean-architecture.md
  architecture/ADR-002-sqlite-placeholder.md
  architecture/ADR-003-observability-and-security.md
  tracability.md
```

---

### 🔐 Endpoints principais de autenticação

- `POST /auth/register` – registra usuário básico (email/senha) usando ASP.NET Core Identity
- `POST /auth/login` – autentica e retorna access/refresh token (incluindo claims de role)
- `POST /auth/refresh` – renova access token a partir do refresh
- `DELETE /auth/logout` – invalida refresh token
- `GET /user/logged` – retorna dados do usuário autenticado
- `POST /auth/admin/users` – criação de usuários por admin, com senha inicial fixa
- `POST /auth/change-password` – troca de senha do usuário autenticado

Documentação OpenAPI (Scalar) disponível em `/scalar` quando a API estiver rodando.

---

### ▶️ Como rodar com Docker (recomendado)

**Pré-requisitos**

- Docker e Docker Compose instalados

**Passos**

```bash
git clone <url-do-repositorio>
cd LastTechTest

docker compose -f docker/docker-compose.yml up --build
```

Se aparecer aviso de _orphan containers_ (ex.: `Found orphan containers (docker-frontend-1 docker-backend-1)...`), use `--remove-orphans` para limpar containers de outro compose que não fazem mais parte deste projeto:

```bash
docker compose -f docker/docker-compose.yml up --build --remove-orphans
```

Com o mesmo comando sobem **API e frontend**:

- **API:** `http://localhost:5114` (HTTP), `http://localhost:5114/scalar` (Scalar API Reference)
- **Frontend:** `http://localhost:4200`

O banco SQLite é persistido em um volume Docker (`lasttechtest-data`), configurado em `docker/docker-compose.yml`.

---

### ▶️ Como rodar localmente (sem Docker)

**Pré-requisitos**

- .NET SDK 10 instalado

**Comandos**

```bash
cd backend
dotnet restore
dotnet run --project LastTechTest.API/LastTechTest.API.csproj
```

A API ficará disponível nas URLs padrão definidas pelo ASP.NET Core (ou `http://localhost:5114` se configurado).

---

### 🧪 Pirâmide de testes

Com .NET SDK 10 instalado, na raiz do repositório ou em `backend`:

```bash
dotnet test backend/LastTechTest.Testes/LastTechTest.Testes.csproj -c Release
```

Sem .NET 10 no host (execução via Docker):

```bash
./scripts/run-tests-docker.sh
```

---

### 👤 Usuário admin e gerenciamento de usuários

- **Usuário admin padrão** (seed via `IdentitySeeder`):
  - Login/E-mail: `usu_acesso_total@example.com`
  - Senha inicial: `Acess0@t0ta1`
  - Roles: `Admin`

- **Criação de usuários por admin**
  - Endpoint: `POST /auth/admin/users`
  - Autorização: requer role `Admin`
  - Payload:

    ```json
    {
      "email": "novo-usuario@example.com",
      "roles": ["Creator", "Analista"]
    }
    ```

  - Comportamento:
    - Cria usuário Identity com senha inicial fixa `Trocar@123`.
    - Atribui as roles informadas, validadas contra `Admin`, `Creator`, `Analista`.
    - Cria o usuário de domínio espelhado (entidade `User`) com o mesmo email e hash da senha padrão.

- **Registro público de usuário**
  - Endpoint: `POST /auth/register`
  - Payload:

    ```json
    {
      "email": "user@example.com",
      "password": "SenhaF0rte!"
    }
    ```

  - Comportamento:
    - Cria usuário em ASP.NET Core Identity usando a política de senha configurada.
    - Cria usuário de domínio `User` equivalente.
    - Retorna tokens de autenticação.

- **Troca de senha**
  - Endpoint: `POST /auth/change-password`
  - Autorização: qualquer usuário autenticado
  - Payload:

    ```json
    {
      "currentPassword": "SenhaAntiga1!",
      "newPassword": "SenhaNova2!"
    }
    ```

  - Comportamento:
    - Usa `UserManager.ChangePasswordAsync` (Identity) para validar senha atual e aplicar a política de senha:
      - Mínimo 8 caracteres
      - Pelo menos 1 dígito, 1 minúscula, 1 maiúscula, 1 símbolo
    - Atualiza a senha no usuário de domínio (`User`) para manter consistência.

#### Roles de acesso (`Admin`, `Creator`, `Analista`)

- **Admin**
  - Pode criar novos usuários via `POST /auth/admin/users`.
  - Define as roles atribuídas a cada usuário.
  - Tem acesso administrativo completo aos fluxos do sistema.
- **Creator**
  - Responsável por criar e operar solicitações de antecipação, conforme demandas RA-1 a RA-4.
  - É a role padrão atribuída aos usuários legados que não possuíam roles antes da migração para Identity.
- **Analista**
  - Focado em análise, aprovação e acompanhamento das solicitações de antecipação.
  - Permissões alinhadas às regras de negócio descritas nos cards em `demandas/RA-*`.

As roles são gerenciadas pelo ASP.NET Core Identity (`KnownRoles`) e propagadas como claims de role nos tokens JWT, sendo usadas pela API para proteger endpoints conforme as regras de autorização descritas nas demandas.

**Cobertura por nível:**

- **Unitários**:
  - Entidades de domínio (`User`)
  - Serviços de infraestrutura (`PasswordHasher`, `TokenService`)
- **Integração**:
  - Repositórios EF Core (SQLite in-memory) – `UserRepository`
  - Fluxos de autenticação via MediatR – `AuthHandlersIntegrationTests`
- **E2E**:
  - Fluxo completo `register -> login -> /user/logged` via `WebApplicationFactory<ProgramEntry>` (CustomWebApplicationFactory)
  - Fluxos de refresh token, logout, e casos de erro (401 sem token, 400 email duplicado, refresh inválido)

Diagrama conceitual:

```text
      E2E     (fluxos completos HTTP)
   Integração (handlers + EF Core + SQLite)
Unitário      (domínio + serviços puros)
```

---

### 🏗️ Arquitetura em alto nível

- **Domínio** não conhece EF Core, ASP.NET ou bibliotecas externas.
- **Aplicação** orquestra casos de uso via MediatR (Commands/Queries) e FluentValidation.
- **Infraestrutura/Persistência** implementam interfaces de domínio e são plugadas via DI na API.
- **API** expõe endpoints HTTP mínimos e integra autenticação JWT + Scalar (documentação OpenAPI).

Para detalhes aprofundados da arquitetura, veja:

- `docs/architecture/ADR-001-clean-architecture.md`
- `docs/architecture/ADR-002-sqlite-placeholder.md`
- `docs/architecture/ADR-003-observability-and-security.md`

---

### 📊 CI e fitness functions

Pipeline em `.github/workflows/ci.yml`:

- Roda em `ubuntu-latest` com `.NET 10.0.x`.
- Etapas:
  - `dotnet restore` da solução.
  - `dotnet build` em modo Release.
  - `dotnet test` com coleta de cobertura (`XPlat Code Coverage`).
  - Verificação automática de **cobertura mínima (30%)** a partir do arquivo `coverage.cobertura.xml`.
- Se a cobertura cair abaixo do limiar, o pipeline falha, atuando como _fitness function_ de qualidade.

---

### 🔍 Rastreamento requisitos → casos de uso → testes

O arquivo `docs/tracability.md` descreve como requisitos de autenticação (registro, login, refresh, logout, usuário logado) se ligam a:

- Commands/Queries/Handlers (por exemplo, `RegisterUserCommand`, `LoginCommand`, `GetLoggedUserQuery`).
- Endpoints HTTP (`/auth/*`, `/user/logged`).
- Testes correspondentes (unit, integração e E2E).

Esse mapeamento é pensado para dialogar diretamente com o fluxo das skills `tradutor`, `maestro` e `quadro-de-recompensas`.

---

### ⚠️ Troubleshooting

**Build: "Access to the path '.../obj/Release/net10.0/...' is denied"**

Se o repositório estiver num disco externo ou montagem só-leitura, o `dotnet build` pode falhar ao escrever em `obj/` e `bin/`. Soluções:

- Garantir que a pasta do projeto tem permissão de escrita (ex.: montagem com `rw`).
- Rodar build e testes dentro de Docker: `./scripts/run-tests-docker.sh` (não depende das permissões do host).
- Ou clonar/copiar o repo para um diretório com permissão de escrita (ex.: `~/projects/`) e buildar a partir daí.

**E2E: "DirectoryNotFoundException: /src/backend/LastTechTest.API/"**

Os testes E2E usam `CustomWebApplicationFactory`, que define o content root do host para a pasta do projeto API. Se o erro aparecer ao rodar testes no host (e não em Docker), execute os testes a partir da raiz do repositório (`dotnet test backend/LastTechTest.Testes/...`) ou use `./scripts/run-tests-docker.sh`.

---

### 🔭 Próximos passos sugeridos

- Aumentar gradualmente a cobertura de testes acima do limite inicial de 30%.
- Evoluir MFA de stub para implementação real (TOTP, por exemplo).
- Integrar métricas e tracing (OpenTelemetry) usando a base já preparada de logs estruturados.

