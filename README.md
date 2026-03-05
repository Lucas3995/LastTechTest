## LastTechTest Backend (.NET 10, Clean Architecture)

[![.NET](https://img.shields.io/badge/.NET-10.0-blue.svg)](https://dotnet.microsoft.com/download)
[![Architecture](https://img.shields.io/badge/Architecture-Clean%20Architecture-orange.svg)](https://blog.cleancoder.com/uncle-bob/2012/08/13/the-clean-architecture.html)
[![Tests](https://img.shields.io/badge/Tests-Unit%20%7C%20Integration%20%7C%20E2E-brightgreen.svg)](#-pirâmide-de-testes)
[![Docker](https://img.shields.io/badge/Docker-Compose-blue.svg)](#quick-start)
[![CI](https://img.shields.io/badge/GitHub-Actions-lightgrey.svg)](#-ci-e-fitness-functions)

**Sumário**

- [Parte 1 – Clonar e rodar](#parte-1--clonar-e-rodar) (Quick start, rodar localmente, testes, troubleshooting)
- [Parte 2 – Projeto e regras de negócio](#parte-2--projeto-e-regras-de-negócio) (usuários e roles, antecipação, permissões, além do escopo)
- [Parte 3 – Descrição técnica](#parte-3--descrição-técnica) (stack, arquitetura, metodologia, workflow assistido por IA)

---

## Parte 1 – Clonar e rodar

Aqui você coloca o projeto no ar em poucos minutos.

### Quick start

**Pré-requisito:** Docker e Docker Compose instalados.

```bash
git clone <url-do-repositorio>
cd LastTechTest

docker compose -f docker/docker-compose.yml up --build
```

- **API:** `http://localhost:5114`
- **Documentação (Scalar):** `http://localhost:5114/scalar`

O banco SQLite fica em volume Docker `lasttechtest-data` (ver `docker/docker-compose.yml`). Se aparecer aviso de _orphan containers_, use `--remove-orphans` no comando acima.

Com o mesmo comando sobem **API e frontend**:

- **API:** `http://localhost:5114` (HTTP), `http://localhost:5114/scalar` (Scalar API Reference)
- **Frontend:** `http://localhost:4200`

**Para acessar a API com usuário admin e entender perfis, credenciais e o que cada role faz, veja a secção [Usuários, roles e credenciais](#usuários-roles-e-credenciais) na Parte 2.**

### Rodar localmente

**Pré-requisito:** .NET SDK 10.

```bash
cd backend
dotnet restore
dotnet run --project LastTechTest.API/LastTechTest.API.csproj
```

A API fica disponível nas URLs padrão do ASP.NET Core (ex.: `http://localhost:5114`).

### Testes (pirâmide de testes) {#-piramide-de-testes}

Com .NET SDK 10 na raiz do repositório:

```bash
dotnet test backend/LastTechTest.Testes/LastTechTest.Testes.csproj -c Release
```

Sem .NET 10 no host:

```bash
./scripts/run-tests-docker.sh
```

### Troubleshooting

**Build: "Access to the path '.../obj/Release/net10.0/...' is denied"**

Em disco externo ou montagem só-leitura, o build pode falhar. Use `./scripts/run-tests-docker.sh` para testes em Docker ou clone o repositório para um diretório com permissão de escrita.

**E2E: "DirectoryNotFoundException: /src/backend/LastTechTest.API/"**

Execute os testes a partir da raiz do repositório (`dotnet test backend/LastTechTest.Testes/...`) ou use `./scripts/run-tests-docker.sh`.

---

## Parte 2 – Projeto e regras de negócio

Esta parte descreve o que o sistema faz, quem usa e quais regras se aplicam.

### Usuários, roles e credenciais {#usuários-roles-e-credenciais}

O sistema usa autenticação JWT (access + refresh token) e três roles: **Admin**, **Creator** e **Analista**. Quem acabou de subir a API pode usar o usuário admin abaixo para explorar os endpoints.

**Usuário admin padrão** (criado pelo seed ao subir a API):

| Campo   | Valor                          |
|--------|---------------------------------|
| E-mail | `usu_acesso_total@example.com` |
| Senha  | `Acess0@t0ta1`                 |
| Role   | `Admin`                        |

**Senha padrão de usuários criados pelo admin:** ao chamar `POST /auth/admin/users`, o novo usuário recebe senha inicial fixa **`Trocar@123`** (o admin não envia senha no payload). Roles válidas no payload: `Admin`, `Creator`, `Analista`.

**Registro público:** `POST /auth/register` — o usuário informa e-mail e senha (mínimo 8 caracteres, 1 dígito, 1 minúscula, 1 maiúscula, 1 símbolo).

**Troca de senha:** `POST /auth/change-password` (usuário autenticado informa senha atual e nova).

**O que cada role faz:**

- **Admin:** Cria usuários (`POST /auth/admin/users`) e define roles; acessa todos os fluxos (antecipação: criar, listar, ver, aprovar, recusar, cancelar e simular para qualquer creator).
- **Creator:** Cria e opera solicitações de antecipação **apenas para si** (criar, listar/ver só as próprias, simular, cancelar as próprias em análise); não pode aprovar nem recusar.
- **Analista:** Aprova ou recusa solicitações em análise; pode simular em nome de qualquer creator (suporte); não cria usuários nem cancela; listagem conforme regras de leitura do módulo de antecipação.

Endpoints de auth: `POST /auth/login`, `POST /auth/refresh`, `DELETE /auth/logout`, `GET /user/logged`. Contratos detalhados em `/scalar` com a API rodando.

### Descrição breve do projeto

A LastLink permite que criadores recebam receitas pela plataforma. Para ajudar no fluxo de caixa, existe **antecipação de valores**: o criador pode solicitar que parte dos recebíveis futuros seja liberada antes do prazo, mediante taxa. Este backend expõe uma **API REST** para gerenciar essas solicitações, consumida por sistema interno (sem front-end no escopo atual).

### Regras de negócio e fluxos

**Criação de solicitação:** Apenas recebíveis elegíveis; valor solicitado **acima de R$ 100**; respeito ao limite por criador (configurável). **Uma solicitação “em análise” por criador** — não é permitido criar nova solicitação enquanto existir uma com status `Created` ou `Pending`.

**Consulta:** Creator lista e vê detalhe apenas das próprias solicitações; Admin lista e vê de qualquer criador, com filtros (creatorId, status, período) e paginação (page, pageSize).

**Estados e transições:** Estados: `Created`, `Pending` (ambos “em análise”), `Approved`, `Rejected`, `CanceledByCreator`. **Aprovar/Recusar:** apenas Analista ou Admin; apenas a partir de “em análise”. **Cancelar:** Creator (apenas próprias) ou Admin; apenas a partir de “em análise”. Cancelar já cancelada é idempotente (resposta clara, estado inalterado).

**Simulação:** Usa as mesmas regras de cálculo e validação da criação real; sem persistência na chamada. Cache in-memory: apenas a última simulação por criador; validade real 2 h; validade exposta ao usuário 20 min antes. Endpoint de conversão transforma simulação em solicitação real (valores idênticos; bloqueada se já existir solicitação em aberto ou simulação expirada/já utilizada).

### Permissões (resumo)

| Papel    | Criar solicitação | Listar/Ver     | Simular             | Aprovar/Recusar  | Cancelar                 |
|----------|-------------------|----------------|---------------------|------------------|--------------------------|
| Creator  | Própria           | Só suas        | Própria             | Não              | Só próprias (em análise) |
| Analista | Não               | Conforme API   | Em nome de qualquer | Sim (em análise) | Não                      |
| Admin    | Qualquer criador  | Todas          | Qualquer            | Sim (em análise) | Qualquer (em análise)    |

### O que foi criado além do escopo inicial

O escopo inicial do desafio (referência: `.cursor/escopo_inicial.txt`) previa: API para criar solicitação (creator_id, valor, data; taxa 5%); listar por creator_id; aprovar ou recusar; opcional simulação GET; regras valor > R$ 100, uma pendente por creator, taxa 5% fixa. Stack sugerida: C#/.NET Core, SQLite, README com como rodar.

**Entregue além desse escopo:** Autenticação e autorização (JWT, roles Admin, Creator, Analista); cancelamento pelo creator (e por admin) em solicitações em análise; simulação como fluxo completo (POST, cache, conversão em real), não GET opcional; estados explícitos e regras de transição centralizadas; auditoria de transições; Clean Architecture, CQRS/MediatR, DDD; API versionada (`/api/v1/anticipations`); pirâmide de testes e CI com cobertura mínima; Docker e script para testes sem SDK local.

---

## Parte 3 – Descrição técnica

Esta parte resume a stack, as escolhas técnicas e a metodologia de desenvolvimento.

### Stack e ganhos

- **Stack:** .NET 10 / C# 14, ASP.NET Core 10, EF Core 10, SQLite — ambiente moderno; SQLite como placeholder para troca futura de persistência.
- **Arquitetura:** Clean Architecture (Domínio → Aplicação → Persistência/Infraestrutura → API) — domínio e regras isolados de frameworks; testes e evolução previsíveis. Ver `docs/architecture/ADR-001-clean-architecture.md`.
- **Padrões:** CQRS + MediatR (Commands/Queries/Handlers) — casos de uso explícitos, testáveis e rastreáveis a requisitos.
- **Domínio:** DDD (entidades, value objects, serviços de domínio), regras de transição centralizadas — linguagem ubíqua e uma única fonte de verdade para “em análise” e transições.
- **Validação:** FluentValidation nos commands — validação declarativa e mensagens claras.
- **Segurança:** JWT (access + refresh), roles — API pronta para consumo por sistema interno com controle por papel.
- **Persistência:** EF Core 10, repositórios, migrações — modelo consistente e esquema versionado.
- **Testes:** xUnit, FluentAssertions, Coverlet, WebApplicationFactory (E2E), pirâmide unit → integração → E2E — confiança em refatoração; cobertura mínima (30%) no CI como fitness function.
- **API e documentação:** Minimal API, Scalar/OpenAPI em `/scalar` — contrato visível e consumo facilitado.
- **DevOps:** Docker Compose, `scripts/run-tests-docker.sh`, GitHub Actions (build, test, cobertura) — um comando para rodar e validar.
- **Antecipação:** Cálculo e validação em serviços de domínio reutilizáveis (simulação vs. criação real); interface de cache de simulação preparada para evolução (ex.: Redis) — sem duplicação de regras.

### Visão geral da solução

| Camada / Projeto              | Responsabilidade principal                                                       |
|------------------------------|-----------------------------------------------------------------------------------|
| `LastTechTest.Dominio`       | Entidades, enums e interfaces de domínio, sem dependência de frameworks          |
| `LastTechTest.Aplicacao`     | Casos de uso (Commands/Queries/Handlers), validações, DTOs                        |
| `LastTechTest.Persistencia`  | ApplicationDbContext, configurações EF Core 10, repositórios SQLite               |
| `LastTechTest.Infrastrutura` | Serviços técnicos (TokenService, PasswordHasher, MFA, Email, KeyGenerator)        |
| `LastTechTest.API`           | Endpoints HTTP (minimal API), autenticação JWT, Scalar/OpenAPI, wiring de DI       |
| `LastTechTest.Testes`        | Testes unitários, integração e E2E                                                |

Estrutura de pastas: `backend/` (API, Aplicacao, Dominio, Infrastrutura, Persistencia, Testes), `docker/docker-compose.yml`, `docs/architecture/`, `docs/tracability.md`.

### Workflow rotina-completa e desenvolvimento assistido por IA

O projeto segue uma metodologia de **desenvolvimento assistido por IA** com visão **spec-driven design**, apoiada por skills e rules em `.cursor/`.

**Workflow rotina-completa** (fonte: `.cursor/rules/metodologia-para-devs.mdc`):

1. **Planejamento:** tradutor (demanda em linguagem de negócio/UX) → maestro (relatório de alterações no código) → quadro-de-recompensas (testes a partir do relatório, TDD explícito); validação com o operador antes de seguir.
2. **Implementação** (ciclo até qualidade satisfatória): mercenario (código para testes passarem) → batedor-de-codigos (relatório de inadequações) → mestre-freire (refatoração conforme relatório); reexecutar testes após refatoração; validar com o operador após cada ciclo.
3. **Entrega:** arauto (commit, push, PR, validação do CI); se CI falhar, nova demanda e nova rotina-completa.

**Skills globais de apoio** (clean code e engenharia de software): **clean-architecture-analysis** (análise de conformidade com Clean Architecture, SOLID, coesão/acoplamento; relatório para agentes), **clean-architecture-remediation** (aplicar correções a partir do relatório com TDD), **software-engineering-practice** (Pressman & Maxim; requisitos, projeto, testes, qualidade, spec-driven development).

**Regras do projeto:** As rules em `.cursor/rules/` (metodologia-para-devs, poupar-creditos-com-skills, planos-todos, cards-demandas) orientam o agente em todas as fases — comunicação com o operador, envolvimento contínuo, uso de scripts para poupar créditos, preenchimento dos todos nos planos, convenções de cards em `demandas/`. São parte integrante do desenvolvimento assistido por IA do projeto.

**Spec-driven design:** A especificação (requisitos, contrato de API, comportamento esperado) é tratada como **fonte única de verdade**: primeiro define-se o que o sistema deve fazer (spec, contratos, critérios de aceitação); em seguida implementação e testes são alinhados à spec, com rastreabilidade spec–código–testes. Isso reduz derivação entre o que foi pedido e o que foi construído; testes e documentação refletem o comportamento especificado e facilitam evolução. No projeto, os cards em `demandas/`, o relatório do maestro e os testes criados pelo quadro-de-recompensas funcionam como spec; o mercenario implementa em cima disso, e o batedor/mestre-freire garantem qualidade técnica sem alterar o comportamento.

### CI e fitness functions {#-ci-e-fitness-functions}

Pipeline em `.github/workflows/ci.yml`: roda em `ubuntu-latest` com .NET 10.0.x; `dotnet restore`, `dotnet build` (Release), `dotnet test` com cobertura; verificação de **cobertura mínima (30%)** a partir de `coverage.cobertura.xml`. Se a cobertura cair abaixo do limiar, o pipeline falha (fitness function de qualidade).

### Rastreamento requisitos → casos de uso → testes

O arquivo `docs/tracability.md` descreve como requisitos de autenticação e de antecipação se ligam a Commands/Queries/Handlers, endpoints HTTP e testes (unit, integração, E2E), em diálogo com o fluxo das skills tradutor, maestro e quadro-de-recompensas.

### Próximos passos sugeridos

- Aumentar gradualmente a cobertura de testes acima de 30%.
- Evoluir MFA de stub para implementação real (ex.: TOTP).
- Integrar métricas e tracing (OpenTelemetry) com a base de logs estruturados.
