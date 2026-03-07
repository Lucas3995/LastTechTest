---
name: Fonte de verdade para refactoring — full sweep
overview: Análise completa do projecto (backend .NET + frontend Angular) para orientar ciclos de refactoring. O projecto acumulou múltiplas demandas sem passar por etapas de refactoring/mestre-freire; esta fonte de verdade passa a limpo o estado actual.
sourceArtifact: Melhoria contínua de qualidade técnica — projecto acumulou múltiplas demandas sem ciclos de refactoring
upstreamPlan: none
planType: sot-refatoracao
createdAt: 2026-03-07T00:00:00.000Z
updatedAt: 2026-03-07T01:00:00.000Z
---

# Fonte de verdade para refactoring

Documento único que orienta ciclos de refatoração do projecto LastTechTest (antecipação de recebíveis). Foco em **qualidade de código**: estrutura, arquitectura, DDD, clean code, padrões, gestão de recursos e manutenibilidade, **sem alterar regras de negócio** e mantendo testes e funcionalidades intactos.

---

## 1. Visão da arquitectura e estrutura actual

### 1.1 Backend (.NET 10)

**Solução:** `backend/LastTechTest.sln`

| Projecto | Responsabilidade | Dependências declaradas (.csproj) |
|----------|------------------|-----------------------------------|
| **LastTechTest.Dominio** | Entidades, value objects, enums, interfaces de repositórios e serviços, regras de domínio | Nenhuma |
| **LastTechTest.Aplicacao** | Casos de uso (CQRS: Commands/Queries com MediatR), FluentValidation, pipeline behaviors | Dominio |
| **LastTechTest.Persistencia** | DbContext (EF Core + SQLite), repositórios, migrações, `AnticipationAuditService` | Dominio |
| **LastTechTest.Infrastrutura** | TokenService, PasswordHasher, cache de simulação, adaptadores | Aplicacao, Dominio, **Persistencia** ⚠️ |
| **LastTechTest.API** | Host ASP.NET Core, controllers, DTOs, configuração (JWT, CORS, DI), ExceptionMapping | Aplicacao, Infrastrutura, Persistencia, Dominio |
| **LastTechTest.Testes** | Testes unitários e de integração do backend | Demais projectos |

**Padrões em uso:** CQRS (MediatR), DI, repositórios no domínio, value objects, FluentValidation + pipeline behavior, `ICurrentUserService` para contexto de utilizador.

**Direcção de dependência real:**

```
API → Infrastrutura → Persistencia → Dominio
         ↓                ↓
      Aplicacao ─────→ Dominio
```

A dependência **Infrastrutura → Persistencia** viola Clean Architecture (a camada de infra não deveria depender da camada de persistência; ambas deveriam depender apenas do Dominio/Aplicacao).

### 1.2 Frontend (Angular 20)

**Aplicação:** `frontend/`, standalone, Angular Material, Vitest, Playwright E2E.

| Camada | Pasta | Responsabilidade | Pode depender de |
|--------|-------|------------------|------------------|
| **domain** | `app/domain/` | Interfaces, tipos, entidades, InjectionTokens para ports | — |
| **application** | `app/application/` | Facades (casos de uso), orquestração com Signals | domain |
| **infrastructure** | `app/infrastructure/` | HTTP services, implementações de ports | domain, application |
| **core** | `app/core/` | Singletons: AuthService, guards, interceptor, API base URL | domain |
| **shared** | `app/shared/` | Componentes layout, pipes, utils reutilizáveis | domain, core |
| **features** | `app/features/` | Páginas e componentes por feature (auth, anticipation, home) | todas |

**Padrões em uso:** Port/Adapter (DIP com InjectionToken), Facades na application com Signals, templateUrl/styleUrl, standalone components, Vitest + Playwright.

**Conformidade de camadas:** ✅ Sem violações de importação entre camadas detectadas.

### 1.3 Infraestrutura (Docker / Observabilidade)

- **docker-compose.yml:** API (porta 5114→8080), Frontend (porta 4200), Jaeger (tracing OTLP), Prometheus, Grafana.
- **Scripts:** `scripts/coverage.sh`, `scripts/frontend-test-docker.sh`, `scripts/frontend-e2e-docker.sh`, `scripts/run-tests-docker.sh` para testes containerizados.

---

## 2. Relatório de inadequações

### 2.1 Backend — Inadequações por categoria

#### 2.1.1 Arquitectura (Arch and Struct)

| ID | Achado | Localização | Princípio violado | Severidade |
|----|--------|-------------|-------------------|------------|
| B-A1 | **Infrastrutura depende de Persistencia** — O `.csproj` de Infrastrutura referencia `LastTechTest.Persistencia`, criando acoplamento entre camadas que deveriam ser peers (ambas dependendo do Dominio). | `backend/LastTechTest.Infrastrutura/LastTechTest.Infrastrutura.csproj` L11-13 | Clean Architecture — regra de dependência | 🔴 Crítica |
| B-A2 | **Handler duplicado no API layer** — `AdminCreateUserCommandHandler` existe **em dois locais**: `LastTechTest.API/AdminCreateUserCommandHandler.cs` e `LastTechTest.Aplicacao/Authentication/Commands/AdminCreateUser/AdminCreateUserCommandHandler.cs`. Contêm lógica divergente (a versão do API é mais simples, sem normalização nem deduplicação de roles). Apenas um será registado pelo MediatR (dependendo da ordem de scan), criando comportamento imprevisível. | `backend/LastTechTest.API/AdminCreateUserCommandHandler.cs` (inteiro) vs `backend/LastTechTest.Aplicacao/Authentication/Commands/AdminCreateUser/AdminCreateUserCommandHandler.cs` (inteiro) | SRP, Clean Architecture (handler de negócio no layer de apresentação) | 🔴 Crítica |
| B-A3 | **Placeholder `Class1.cs` na Infrastrutura** — Ficheiro contém o `TokenService` e `PasswordHasherAdapter` (160+ linhas) com nome genérico `Class1.cs`. Os serviços reais deveriam estar em ficheiros nomeados (`TokenService.cs`, `PasswordHasherAdapter.cs`). | `backend/LastTechTest.Infrastrutura/Class1.cs` | Organização de ficheiros, convenção de nomes | 🟠 Alta |
| B-A4 | **Stub `ReceivableRepository`** — Retorna dados fixos (hardcoded) em vez de consultar BD. Todos os handlers recebem os mesmos recebíveis mock independentemente do criador. | `backend/LastTechTest.Persistencia/Repositories/ReceivableRepository.cs` | Repository pattern, integridade de dados | 🟠 Alta |
| B-A5 | **`ProgramEntry.cs` vazio** — Classe selada sem membros; serve aparentemente como marker. | `backend/LastTechTest.API/ProgramEntry.cs` | Dispensables — dead code | 🟢 Baixa |
| B-A6 | **Pasta `backend/tests/` inteira é dead code** — Contém 3 projectos órfãos que não estão na solution, não compilam e não têm testes reais: (1) `EFCoreInspect` — console de diagnóstico descartável com paths hardcoded; (2) `LastTechTest.Antecipacao.Tests` — scaffolding vazio (`Test1()` vazio), targets `net9.0`, referencia projectos inexistentes (`src/LastTechTest.Antecipacao.Api/.Application/.Domain`); (3) `LastTechTest.Antecipacao.E2E` — scaffolding vazio idêntico. Os testes reais estão em `LastTechTest.Testes/`. Toda a pasta pode ser eliminada. | `backend/tests/` (3 projectos) | Dispensables — dead code, vestigial scaffolding | 🟠 Alta |

#### 2.1.2 SOLID

| ID | Achado | Localização | Princípio violado | Severidade |
|----|--------|-------------|-------------------|------------|
| B-S1 | **SRP — handlers god-class** — `CreateAnticipationRequestCommandHandler`, `SimulateAnticipationRequestCommandHandler` e `ConvertSimulationToRealRequestCommandHandler` acumulam 5-6 responsabilidades (autenticação, resolução de creator, verificação de eligibilidade, cálculo, persistência, cache). Cada handler tem 32-48 linhas no `Handle` com 5+ dependências injectadas. | Handlers em `backend/LastTechTest.Aplicacao/Anticipation/Commands/` | SRP | 🟠 Alta |
| B-S2 | **SRP — LoginCommandHandler** — Acumula 6 responsabilidades (lookup de user, verificação de password, obtenção de roles, geração de tokens, persistência de refresh token, update de timestamp de login). | `backend/LastTechTest.Aplicacao/Authentication/Commands/Login/LoginCommandHandler.cs` | SRP | 🟡 Média |
| B-S3 | **DRY — `ResolveCreatorId` duplicado** — Lógica idêntica de resolução de creator ID em `CreateAnticipationRequestCommandHandler` e `SimulateAnticipationRequestCommandHandler` como método privado estático. | `CreateAnticipationRequestCommandHandler` L66-71, `SimulateAnticipationRequestCommandHandler` L83-90 | DRY, SRP | 🟠 Alta |
| B-S4 | **OCP — role strings espalhadas** — Comparações `string.Equals(role, "Admin", ...)`, `string.Equals(role, "Creator", ...)` aparecem em múltiplos handlers e serviços de domínio. Existe `KnownRoles` mas não é usado consistentemente. | Múltiplos ficheiros (handlers de Anticipation, `AnticipationTransitionRules`, `AnticipationTransitionExecutor`) | OCP — alterar/adicionar role requer shotgun surgery | 🟠 Alta |
| B-S5 | **ISP — `ICurrentUserService`** — Handlers dependem da interface inteira (GetCurrentUserId + GetRole) quando frequentemente necessitam apenas de um dos métodos. | Handlers em `Anticipation/Commands/` e `Anticipation/Queries/` | ISP | 🟡 Média |
| B-S6 | **DIP — dependência de string literals para roles** — Handlers usam literais `"Admin"`, `"Creator"` directamente em vez de constantes ou enums. `KnownRoles` existe no Dominio mas não é referenciado em vários handlers. | Handlers de `Anticipation/Commands/` | DIP, OCP | 🟡 Média |

#### 2.1.3 Code Smells

| ID | Achado | Localização | Categoria | Severidade |
|----|--------|-------------|-----------|------------|
| B-CS1 | **Duplicate Code — lógica de criação de antecipação** — A sequência (auth → resolve creator → check eligibility → calculate → persist) é replicada em 3 handlers: Create, Simulate e ConvertSimulation. | Handlers de `Anticipation/Commands/` | Bloaters / Dispensables | 🟠 Alta |
| B-CS2 | **Long Method — Handle()** — Métodos `Handle` com 32-64 linhas e múltiplas responsabilidades. | Todos os command handlers de Anticipation e Auth | Bloaters | 🟠 Alta |
| B-CS3 | **Data Clumps** — O grupo `(creatorId, requestedAmount, grossAmount, feesAmount, netAmount)` e `(simulationCode, validUntilUtc, requestedAmount, ...)` aparece disperso em handlers e criação de entidades. | Handlers de Anticipation | Bloaters | 🟡 Média |
| B-CS4 | **Switch Statements em transições** — `AnticipationTransitionRules.GetNextState()` e `AnticipationTransitionExecutor.ApplyTransition()` usam switch/if em strings de acção. Não tipo-seguro. | `backend/LastTechTest.Dominio/Services/AnticipationTransitionRules.cs`, `backend/LastTechTest.Aplicacao/Anticipation/Services/AnticipationTransitionExecutor.cs` | OO Abusers | 🟡 Média |
| B-CS5 | **Dead code — `NotImplementedAnticipationSimulationCache`** — Placeholder que lança excepção; especulação de generalidade. | `backend/LastTechTest.Infrastrutura/Anticipation/NotImplementedAnticipationSimulationCache.cs` | Dispensables | 🟢 Baixa |
| B-CS6 | **Shotgun Surgery — adicionar novo role** — Requer alterações em 7+ ficheiros: ResolveCreatorId (2×), CanConvert, AnticipationTransitionRules, AnticipationTransitionExecutor, ListQueryHandler, GetByIdQueryHandler. | Múltiplos | Change Preventers | 🟠 Alta |
| B-CS7 | **Feature Envy — handlers com 8+ chamadas a colaboradores** — `CreateAnticipationRequestCommandHandler.Handle()` invoca 8 métodos remotos em sequência; a lógica deveria estar encapsulada num domain service. | `CreateAnticipationRequestCommandHandler` | Couplers | 🟡 Média |
| B-CS8 | **Inappropriate Intimacy — `AnticipationTransitionExecutor` ↔ `AnticipationRequest`** — O executor chama directamente `entity.Approve()`, `entity.Reject()`, `entity.Cancel()` sem que a entidade valide as suas próprias invariantes. | `backend/LastTechTest.Aplicacao/Anticipation/Services/AnticipationTransitionExecutor.cs` | Couplers | 🟡 Média |

#### 2.1.4 DDD

| ID | Achado | Localização | Princípio violado | Severidade |
|----|--------|-------------|-------------------|------------|
| B-D1 | **Entidade anémica em transições** — `AnticipationRequest.Approve()`, `.Reject()`, `.Cancel()` apenas alteram status sem validar invariantes (ex.: se a transição é permitida). A validação fica em `AnticipationTransitionRules` (domain service estático), separada da entidade. | `backend/LastTechTest.Dominio/Entities/AnticipationRequest.cs` | DDD — aggregate root deve proteger invariantes | 🟠 Alta |
| B-D2 | **Domain logic leakage — resolução de creator** — A lógica de resolução de creator ID (Admin pode especificar outro; Creator usa o próprio) está nos handlers da Aplicacao em vez de estar encapsulada no domínio. | Handlers `CreateAnticipation`, `SimulateAnticipation`, `ConvertSimulation` | DDD — lógica de domínio fora do domínio | 🟡 Média |
| B-D3 | **LSP — `NoOpAnticipationAuditService`** — Implementação no-op que silenciosamente não grava auditorias; callers não podem confiar no contrato (auditoria pode ou não ser registada dependendo do binding). | `backend/LastTechTest.Aplicacao/Common/Services/NoOpAnticipationAuditService.cs` | LSP | 🟡 Média |

#### 2.1.5 Segurança e Tratamento de Erros

| ID | Achado | Localização | Severidade |
|----|--------|-------------|------------|
| B-SE1 | **Password default hardcoded** — `"Trocar@123"` aparece em ambos os `AdminCreateUserCommandHandler` (API e Aplicacao). Deveria vir de configuração. | Ambos `AdminCreateUserCommandHandler` | 🟠 Alta |
| B-SE2 | **CORS hardcoded para localhost** — `http://localhost:4200` fixo no `Program.cs`; sem configuração por ambiente. | `backend/LastTechTest.API/Program.cs` | 🟡 Média |
| B-SE3 | **Excepções genéricas** — Usa `InvalidOperationException` para cenários distintos (pending request, eligibilidade insuficiente, simulação expirada). Dificulta tratamento diferenciado por clientes. | Handlers de Anticipation e Auth | 🟡 Média |
| B-SE4 | **Silent failures possíveis** — `NoOpAnticipationAuditService` e `NotImplementedAnticipationSimulationCache` podem mascarar problemas em produção. | Applicacao e Infrastrutura | 🟡 Média |

#### 2.1.6 Performance/Memória

| ID | Achado | Localização | Severidade |
|----|--------|-------------|------------|
| B-P1 | **Cache de simulação sem limite** — `MemoryAnticipationSimulationCache` usa `ConcurrentDictionary` sem eviction; num cenário com muitos criadores, consumo de memória cresce indefinidamente. | `backend/LastTechTest.Infrastrutura/Anticipation/MemoryAnticipationSimulationCache.cs` | 🟡 Média |

---

### 2.2 Frontend — Inadequações por categoria

#### 2.2.1 Arquitectura

| ID | Achado | Localização | Princípio violado | Severidade |
|----|--------|-------------|-------------------|------------|
| F-A1 | **Central de ações não implementada** — A regra `angular-frontend.instructions.md` exige um módulo central com Command pattern (fila, retry, persistência local) para requisições ao backend. Não existe. Os erros são tratados per-facade. | Inexistente no projecto | Resiliência, Command pattern | 🟠 Alta |

#### 2.2.2 SOLID e Code Smells

| ID | Achado | Localização | Princípio violado | Severidade |
|----|--------|-------------|-------------------|------------|
| F-S1 | **SRP — `AnticipationRequestsHttpService` (330+ linhas)** — Um único service implementa 9 métodos (list, get, cancel, approve, reject, create, simulate, convert) com mapeamentos individuais. Deveria ser separado em query service + command service (ou por sub-domínio). | `frontend/src/app/infrastructure/anticipation/anticipation-requests.http.service.ts` | SRP | 🟡 Média |
| F-S2 | **DRY — padrão de error handling replicado em facades** — Todas as facades repetem o mesmo padrão: `try { ... } catch { buildErrorPresentation(error, msg); setErrorFromPresentation(...) }`. | `anticipation-my-requests.facade.ts`, `anticipation-admin-list.facade.ts`, `admin.facade.ts`, `change-password.facade.ts` | DRY | 🟡 Média |
| F-S3 | **Dead code — enum `SimulationError`** — Definido em `domain/anticipation.ts` mas nunca importado nem utilizado. | `frontend/src/app/domain/anticipation.ts` | Dispensables | 🟢 Baixa |
| F-S4 | **Dead code — `ChangeDetectorRef` injectado sem necessidade aparente** — Injectado e usado uma vez em `ngOnInit` via `cdr.detectChanges()`. Com Signals, raramente necessário. | `frontend/src/app/features/anticipation/pages/admin-list/anticipation-admin-requests-page.component.ts` | Dispensables | 🟢 Baixa |
| F-S5 | **Magic strings — mensagens de erro hardcoded** — Mensagens como `'Não foi possível carregar...'` dispersas por facades e componentes sem centralização. | Múltiplas facades e componentes | Magic Numbers/Strings | 🟢 Baixa |
| F-S6 | **Long function — auth-token.interceptor (60+ linhas)** — Lógica complexa de retry/refresh num único interceptor funcional. | `frontend/src/app/core/auth/auth-token.interceptor.ts` | Bloaters | 🟡 Média |

#### 2.2.3 RxJS / Lifecycle

| ID | Achado | Localização | Princípio violado | Severidade |
|----|--------|-------------|-------------------|------------|
| F-R1 | **Subscription sem cleanup — LoginPageComponent** — `this.authService.login(...).subscribe({...})` sem `takeUntilDestroyed()` nem `ngOnDestroy`. Se o componente for destruído durante o login, há risco de memory leak. | `frontend/src/app/features/auth/pages/login/login-page.component.ts` L37-57 | Gestão de recursos, lifecycle | 🟡 Média |

#### 2.2.4 Acessibilidade (A11y)

| ID | Achado | Localização | Severidade |
|----|--------|-------------|------------|
| F-A11Y-1 | **`aria-sort` em falta em cabeçalho de tabela ordenável** — Coluna "Valor" tem handler `(click)="onToggleAmountSort()"` mas não tem binding `[attr.aria-sort]`. | `frontend/src/app/features/anticipation/components/anticipation-requests-table/anticipation-requests-table.component.html` | 🟡 Média |
| F-A11Y-2 | **Botões de paginação sem `aria-label`** — "← Anterior" e "Próxima →" sem `aria-label` descritivo. | `frontend/src/app/features/anticipation/components/anticipation-admin-requests-table/anticipation-admin-requests-table.component.html` | 🟡 Média |

#### 2.2.5 Testes

| ID | Achado | Localização | Severidade |
|----|--------|-------------|------------|
| F-T1 | **Cobertura baixa em componentes de feature (~20%)** — Componentes de tabela, filtros e alguns formulários não têm testes unitários ou têm apenas assertions triviais (`toBeTruthy`). | `frontend/src/app/features/anticipation/components/` | 🟡 Média |

---

## 3. Prioridades e critérios para refatorações

### 3.1 Prioridades (por severidade e impacto)

#### 🔴 Prioridade 1 — Crítica (corrigir imediatamente)

| # | Achado | IDs | Acção esperada |
|---|--------|-----|----------------|
| 1 | Handler duplicado `AdminCreateUserCommandHandler` na camada API | B-A2 | Eliminar o ficheiro `LastTechTest.API/AdminCreateUserCommandHandler.cs`; garantir que o MediatR resolve apenas o da Aplicacao. |
| 2 | Infrastrutura depende de Persistencia | B-A1 | Mover `AnticipationAuditService` para Infrastrutura (ou registar via DI sem referência de projecto) e remover a referência ao `.csproj` da Persistencia. |

#### 🟠 Prioridade 2 — Alta (próximo ciclo)

| # | Achado | IDs | Acção esperada |
|---|--------|-----|----------------|
| 3 | Pasta `backend/tests/` com 3 projectos órfãos (dead code) | B-A6 | Eliminar toda a pasta `backend/tests/`. Sem impacto — não estão na solution nem são referenciados. |
| 4 | Ficheiro `Class1.cs` com serviços reais na Infrastrutura | B-A3 | Renomear/separar em `TokenService.cs` e `PasswordHasherAdapter.cs`. |
| 5 | Stub `ReceivableRepository` com dados hardcoded | B-A4 | Implementar persistência real ou documentar explicitamente como stub de dev com flag de configuração. |
| 6 | `ResolveCreatorId` duplicado | B-S3 | Extrair para domain service (`ICreatorResolutionService` ou método estático no Dominio). |
| 7 | Role strings espalhadas (shotgun surgery) | B-S4, B-S6, B-CS6 | Usar `KnownRoles` consistentemente; considerar enum no Dominio; remover literais. |
| 8 | Handlers god-class com 5+ responsabilidades | B-S1, B-CS1, B-CS2 | Extrair domain service `IAnticipationRequestCreationService` que encapsule a sequência (eligibilidade → cálculo → criação). Handlers delegam. |
| 9 | Entidade anémica em transições | B-D1, B-CS8 | Mover validação de transições para dentro do aggregate (`AnticipationRequest`); métodos `Approve()`, `Reject()`, `Cancel()` devem verificar invariantes. |
| 10 | Password default hardcoded | B-SE1 | Mover para configuração (`appsettings.json` ou variável de ambiente). |
| 11 | Central de ações (Command pattern) no frontend | F-A1 | Implementar `CommandDispatcher` ou `ActionQueue` na infrastructure com retry e notificação de erro. |

#### 🟡 Prioridade 3 — Média

| # | Achado | IDs | Acção esperada |
|---|--------|-----|----------------|
| 12 | `LoginCommandHandler` com 6 responsabilidades | B-S2 | Extrair autenticação para domain service dedicado. |
| 13 | `ICurrentUserService` — interface fat | B-S5 | Avaliar segregação em interfaces menores. |
| 14 | Switch Statements em transições (string-based) | B-CS4 | Considerar uso de polymorphism ou enum-based dispatch. |
| 15 | `NoOpAnticipationAuditService` — LSP concern | B-D3, B-SE4 | Substituir por implementação real ou documentar explicitamente como stub de teste. |
| 16 | Excepções genéricas | B-SE3 | Criar excepções de domínio específicas (`PendingRequestExistsException`, `SimulationExpiredException`, etc.). |
| 17 | CORS hardcoded | B-SE2 | Externalizar origens para configuração por ambiente. |
| 18 | Cache sem eviction | B-P1 | Implementar TTL ou limite de entradas no `MemoryAnticipationSimulationCache`. |
| 19 | `AnticipationRequestsHttpService` 330+ linhas | F-S1 | Separar em query service + command service. |
| 20 | Error handling duplicado em facades | F-S2 | Extrair para `BaseApplicationFacade` ou método utilitário no application layer. |
| 21 | Subscription sem cleanup em LoginPage | F-R1 | Adicionar `takeUntilDestroyed(this.destroyRef)`. |
| 22 | Auth-token interceptor 60+ linhas | F-S6 | Extrair lógica de refresh para serviço dedicado. |
| 23 | A11y — `aria-sort` e `aria-label` em falta | F-A11Y-1, F-A11Y-2 | Adicionar bindings ARIA nos templates afectados. |

#### 🟢 Prioridade 4 — Baixa (code health)

| # | Achado | IDs |
|---|--------|-----|
| 24 | `ProgramEntry.cs` vazio | B-A5 |
| 25 | `NotImplementedAnticipationSimulationCache` placeholder | B-CS5 |
| 26 | Enum `SimulationError` não utilizado | F-S3 |
| 27 | `ChangeDetectorRef` desnecessário | F-S4 |
| 28 | Magic strings em facades | F-S5 |
| 29 | Cobertura de testes unitários baixa em features | F-T1 |

### 3.2 Critérios técnicos para refatorações

- **Clean Architecture / SOLID:** Respeitar direcção de dependência; DIP; SRP por classe/módulo; camadas internas não dependem de externas.
- **DDD:** Entidades protegem invariantes; value objects para conceitos; domain logic no domínio; aggregate roots como ponto de entrada.
- **CQRS:** Separação Command/Query; handlers como orquestradores leves; lógica delegada a domain services.
- **Frontend (angular-frontend.instructions.md):** Camadas domain/application/infrastructure/core/shared/features; DIP com ports e InjectionToken; templateUrl/styleUrl; Signals para estado síncrono; RxJS para async com `takeUntilDestroyed`; typed forms; a11y; central de ações (Command).
- **Qualidade geral:** Nomes claros, funções pequenas, DRY, gestão de recursos, sem dead code.

### 3.3 Processo de refatoração

1. **Entrada:** Este documento (ou secções relevantes).
2. **Execução:** Skill **mestre-freire** (backend) e/ou **mestre-freire-angular** (frontend) — uma inadequação (ou grupo coerente) por vez.
3. **Validação:** Executar suíte completa de testes (backend `dotnet test` + frontend `npm run test`) após cada passo; usar containerização (`scripts/`) se necessário.
4. **Build:** Garantir que `docker compose -f docker/docker-compose.yml build` continua funcional.
5. **Regra:** Testes existentes não são alterados. Se um teste falhar, a causa é a refatoração — corrigir o código de produção.

---

## 4. Restrições

- **Regras de negócio:** Não alterar. Refatoração é mudança de estrutura e qualidade técnica, não de funcionalidade.
- **Testes e funcionalidades:** Testes existentes devem continuar a passar; funcionalidades permanecem intactas. Não alterar ficheiros de teste para "acomodar" refatoração, salvo decisão explícita do operador (ex.: actualização de mocks documentada).
- **Contratos públicos:** APIs REST e contratos de frontend (ports, DTOs) não devem ser alterados em comportamento observável. Mudanças de assinaturas internas (ex.: handler receber value object em vez de parâmetros soltos) são permitidas desde que o contrato HTTP seja compatível.
- **Escopo:** Fonte de verdade para refatoração; não substitui especificações de novas features.

---

## Referências rápidas

| Recurso | Localização |
|---------|-------------|
| Rastreabilidade de requisitos e testes | `docs/tracability.md` |
| Regra frontend Angular | `.github/instructions/angular-frontend.instructions.md` |
| Skill batedor-de-codigos | `.github/skills/batedor-de-codigos/SKILL.md` |
| Skill mestre-freire | `.github/skills/mestre-freire/SKILL.md` |
| Skill mestre-freire-angular | `.github/skills/mestre-freire-angular/SKILL.md` |
| Docker Compose | `docker/docker-compose.yml` |
| Scripts de teste containerizado | `scripts/` |
