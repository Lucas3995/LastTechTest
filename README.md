## LastTechTest Backend (.NET 10, Clean Architecture)

[![.NET](https://img.shields.io/badge/.NET-10.0-blue.svg)](https://dotnet.microsoft.com/download)
[![Architecture](https://img.shields.io/badge/Architecture-Clean%20Architecture-orange.svg)](https://blog.cleancoder.com/uncle-bob/2012/08/13/the-clean-architecture.html)
[![Tests](https://img.shields.io/badge/Tests-Unit%20%7C%20Integration%20%7C%20E2E-brightgreen.svg)](#-piramide-de-testes)
[![Docker](https://img.shields.io/badge/Docker-Compose-blue.svg)](#-como-rodar-com-docker-recomendado)
[![CI](https://img.shields.io/badge/GitHub-Actions-lightgrey.svg)](#-ci-e-fitness-functions)

Backend de referência inspirado na [OmniSuite API](https://github.com/DuoMasterGestaoTecnologia/nueva_api/blob/main/README.md), utilizando **.NET 10 / C# 14**, **Clean Architecture**, **CQRS + MediatR**, **EF Core 10 + SQLite**, autenticação **JWT** e pirâmide de testes completa (unitário, integração e E2E), totalmente containerizado.

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
| `LastTechTest.API`             | Endpoints HTTP (minimal API), autenticação JWT, Swagger/OpenAPI, wiring de DI            |
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

- `POST /auth/register` – registra usuário básico (email/senha)
- `POST /auth/login` – autentica e retorna access/refresh token
- `POST /auth/refresh` – renova access token a partir do refresh
- `DELETE /auth/logout` – invalida refresh token
- `GET /user/logged` – retorna dados do usuário autenticado

Swagger/OpenAPI disponível em `/swagger` quando a API estiver rodando.

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

A API ficará acessível em:

- `http://localhost:5114` (HTTP)
- `http://localhost:5114/swagger` (Swagger UI)

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

Dentro da pasta `backend`:

```bash
dotnet test LastTechTest.Testes/LastTechTest.Testes.csproj
```

**Cobertura por nível:**

- **Unitários**:
  - Entidades de domínio (`User`)
  - Serviços de infraestrutura (`PasswordHasher`, `TokenService`)
- **Integração**:
  - Repositórios EF Core (SQLite in-memory) – `UserRepository`
  - Fluxos de autenticação via MediatR – `AuthHandlersIntegrationTests`
- **E2E**:
  - Fluxo completo `register -> login -> /user/logged` via `WebApplicationFactory<Program>`

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
- **API** expõe endpoints HTTP mínimos e integra autenticação JWT + Swagger.

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

### 🔭 Próximos passos sugeridos

- Aumentar gradualmente a cobertura de testes acima do limite inicial de 30%.
- Evoluir MFA de stub para implementação real (TOTP, por exemplo).
- Integrar métricas e tracing (OpenTelemetry) usando a base já preparada de logs estruturados.

