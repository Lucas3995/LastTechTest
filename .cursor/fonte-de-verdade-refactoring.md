# Fonte de verdade para refactoring

Documento único que orienta ciclos de refatoração do projeto LastTechTest (antecipação de recebíveis). Foco em **qualidade de código**: estrutura, arquitetura, DDD, clean code, padrões, gestão de recursos e manutenibilidade, **sem alterar regras de negócio** e mantendo testes e funcionalidades intactos.

---

## 1. Visão da arquitetura e estrutura atual

### 1.1 Backend (.NET)

- **Solução:** `backend/LastTechTest.sln`
- **Projetos e responsabilidades:**

| Projeto | Responsabilidade | Dependências |
|--------|-------------------|--------------|
| **LastTechTest.Dominio** | Entidades, value objects, enums, interfaces de repositórios e serviços, regras de domínio | Nenhuma |
| **LastTechTest.Aplicacao** | Casos de uso (CQRS: Commands/Queries com MediatR), validadores FluentValidation, comportamentos de pipeline | Dominio |
| **LastTechTest.Persistencia** | DbContext, repositórios (implementam interfaces do Dominio), migrações EF Core | Dominio |
| **LastTechTest.Infrastrutura** | Implementações de serviços (Token, PasswordHasher, Email, etc.), adaptadores | Aplicacao, Dominio, Persistencia |
| **LastTechTest.API** | Host ASP.NET Core, controllers, DTOs, configuração (JWT, CORS, DI), mapeamento de exceções | Aplicacao, Infrastrutura, Persistencia, Dominio |
| **LastTechTest.Testes** | Testes unitários, integração e E2E do backend | Demais projetos |

- **Padrões em uso:** CQRS (MediatR), injeção de dependência, repositórios no domínio, value objects (ex.: `ListAnticipationRequestsFilter`), validação com FluentValidation e pipeline behavior.
- **Entrada HTTP:** Controllers (`LastTechTest.API/Controllers/`) recebem requests, constroem Query/Command e enviam via `ISender` (MediatR); handlers na Aplicacao orquestram e usam interfaces do Dominio; repositórios e serviços concretos são resolvidos por DI no API/Infrastrutura/Persistencia.
- **Direção de dependência:** API e Infrastrutura dependem das camadas internas; Aplicacao depende apenas do Dominio; Persistencia depende apenas do Dominio. Não há dependência do Dominio ou da Aplicacao em direção a Persistencia ou API.

### 1.2 Frontend (Angular)

- **Aplicação:** Angular standalone, em `frontend/`; ponto de entrada e configuração em `src/app/app.config.ts` e `src/app/app.routes.ts`.
- **Camadas e estrutura:**

| Camada | Pasta | Responsabilidade | Pode depender de |
|--------|--------|-------------------|-------------------|
| **domain** | `app/domain/` | Interfaces, tipos, entidades (ex.: `AnticipationRequest`, `AnticipationRequestsPort`) | — |
| **application** | `app/application/` | Casos de uso, orquestração (facades; sem I/O direto) | domain |
| **infrastructure** | `app/infrastructure/` | HTTP, implementações de portas (ex.: `AnticipationRequestsHttpService` implementa `AnticipationRequestsPort`) | domain, application (apenas para tipos; implementações dependem de domain) |
| **core** | `app/core/` | Singletons: auth (service, guards, interceptor), API base URL | domain (evitar application/infrastructure) |
| **shared** | `app/shared/` | Componentes, directives, pipes e utils reutilizáveis (ex.: `backend-error.util.ts`, layout shell) | domain, core |
| **features** | `app/features/` | Páginas e componentes por funcionalidade (auth, anticipation, home) | domain, application, infrastructure, core, shared |

- **Padrões em uso:** Port/Adapter (porta `AnticipationRequestsPort` no domain, implementação HTTP na infrastructure); facade na application (`AnticipationMyRequestsFacade`) expondo estado via signals e métodos assíncronos; componentes de página delegam à facade; DI com `InjectionToken` para a porta; `templateUrl`/`styleUrl`; uso de Signals para estado e RxJS para fluxos assíncronos.
- **Registro de dependências:** Em `app.config.ts` a porta `ANTICIPATION_REQUESTS_PORT` é ligada a `AnticipationRequestsHttpService`; o facade injeta a porta (DIP).

### 1.3 Rastreabilidade e documentação

- **Requisitos e testes:** `docs/tracability.md` descreve requisitos (R1–R5 auth, RA-1 a RA-4 antecipação, RC-x), casos de uso, endpoints e árvores de testes (unit, integração, E2E).
- **Regras do projeto:** Critério técnico do frontend em `.cursor/rules/angular-frontend.mdc`; skills batedor-de-codigos, mestre-freire, clean-architecture-analysis e mestre-freire-angular disponíveis para análise e refatoração guiada.

---

## 2. Relatório de inadequações

### 2.1 Relatórios existentes

- **docs/relatorio-inadequacoes-ra2.md** — Escopo: RA-2 (consultar solicitações de antecipação). Achados:
  - **1 – Long Parameter List:** Query `ListAnticipationRequestsQuery` e contrato de listagem expõem seis parâmetros (CreatorId, Status, FromUtc, ToUtc, Page, PageSize); o handler repassa ao repositório. **Nota:** O repositório já utiliza o value object `ListAnticipationRequestsFilter` (Dominio); a Query e o Controller continuam com muitos parâmetros. Agrupar filtros num objeto de valor na camada de aplicação/API reduziria o smell. O relatório indica que a skill mestre-freire não alterou isto por implicar alteração de mocks nos testes.
  - **2 – Duplicate Code (verificação de utilizador não autenticado):** **Aplicado.** Criado `CurrentUserServiceExtensions.EnsureAuthenticated()`; handlers passaram a usar `_currentUser.EnsureAuthenticated()`.

### 2.2 Inadequações adicionais identificadas (visão atual)

Estes pontos podem ser usados para alimentar relatórios futuros do batedor-de-codigos ou clean-architecture-analysis; refatorações devem seguir o fluxo relatório → mestre-freire (sem alterar comportamento nem testes).

- **Backend**
  - **Long Parameter List (persistente):** `ListAnticipationRequestsQuery` (Aplicacao) e o método List do `AnticipationController` (API) ainda recebem vários parâmetros soltos; alinhar com o uso de `ListAnticipationRequestsFilter` no repositório (ex.: Query receber um filter ou objeto de parâmetros) reduziria acoplamento e evolução do contrato. Qualquer alteração que exija mudar assinaturas em testes deve ser planeada (possível atualização de mocks em acordo com a metodologia do projeto).
  - **Código morto / template:** **Parcialmente aplicado.** Removidos os template `Class1.cs` em `LastTechTest.Aplicacao` e `LastTechTest.Persistencia`. Os de `LastTechTest.Dominio` e `LastTechTest.Infrastrutura` continham código de produção (entidade `User`, serviços TokenService/PasswordHasher/etc.); restaurados como `Dominio/Entities/User.cs` e `Infrastrutura/Class1.cs`.

- **Frontend**
  - **Dependência application → shared:** **Aplicado.** O comentário em `app/application/index.ts` afirma “Depends only on domain”. O `AnticipationMyRequestsFacade` importa `buildErrorPresentation` de `../../shared/utils/backend-error.util`. Para aderência estrita à regra de camadas (application depende apenas de domain), considerar: (1) mover a construção de apresentação de erro para um tipo/interface no domain e implementação em infrastructure ou shared, ou (2) documentar exceção para utils cross-cutting. Refatoração não deve alterar comportamento nem quebrar testes.
  - **Central de ações (Command):** A regra angular-frontend exige que requisições ao backend passem por uma “central de ações” (persistir comando, retry, não quebrar a página). O comentário em `infrastructure/index.ts` menciona “Future: central de ações (Command pattern)”. Refatorações futuras podem introduzir esse módulo sem alterar regras de negócio.

- **Geral**
  - **Consistência de relatórios:** Novos escopos (outras áreas além de RA-2) devem ser analisados com batedor-de-codigos ou clean-architecture-analysis; os achados devem ser incorporados a este documento ou referenciados na secção 2.

---

## 3. Prioridades e critérios para refatorações

### 3.1 Prioridades

1. **Alta:** Corrigir violações de camadas e dependências (ex.: application a depender apenas de domain; remoção de dependências circulares).
2. **Alta:** Aplicar achados de relatórios de inadequações já produzidos (ex.: RA-2), respeitando a restrição de não alterar testes salvo acordo explícito (e, quando necessário, atualizando mocks de forma controlada).
3. **Média:** Reduzir Long Parameter List e Data Clumps (objetos de valor / parâmetros agrupados) na API e na Aplicacao do backend.
4. **Média:** Eliminar código morto (Class1, ficheiros não referenciados).
5. **Baixa:** Evoluir para central de ações no frontend (Command pattern) conforme regra angular-frontend; manter compatibilidade com testes e comportamento atual.

### 3.2 Critérios técnicos

- **Clean Architecture / SOLID:** Respeitar direção de dependência (camadas internas não dependem das externas); DIP (depender de abstrações); SRP por classe/módulo; evitar que casos de uso dependam de detalhes de I/O.
- **DDD:** Manter entidades, value objects e interfaces no domínio; evitar lógica de aplicação ou infra no domínio.
- **CQRS:** Manter separação Command/Query; handlers como orquestradores; repositórios e serviços via interfaces.
- **Frontend (angular-frontend.mdc):** Camadas domain / application / infrastructure / core / shared / features com dependências permitidas; componentes com templateUrl/styleUrl; páginas sem HttpClient direto; uso de portas (DIP); Signals para estado síncrono, RxJS para assíncrono; typed forms; a11y.
- **Qualidade de código:** Preferir nomes claros, funções pequenas, ausência de duplicação desnecessária (DRY); gestão de recursos (subscrições, memória) conforme boas práticas Angular e .NET.

### 3.3 Processo de refatoração

- **Entrada:** Relatório de inadequações (batedor-de-codigos) ou de análise de arquitetura (clean-architecture-analysis).
- **Execução:** Skill **mestre-freire** (backend) ou **mestre-freire-angular** (frontend): refatorar conforme relatório, sem alterar comportamentos nem regras de negócio; não alterar testes exceto quando a metodologia do projeto permitir (ex.: atualização de mocks documentada).
- **Validação:** Executar suíte completa de testes (backend e frontend) após cada ciclo; usar containerização (ex.: scripts do projeto) se o ambiente do host não permitir executar os testes.
- **Build:** Garantir que `docker compose -f docker/docker-compose.yml build` (e, quando aplicável, execução) continue a funcionar após refatorações.

---

## 4. Restrições

- **Regras de negócio:** Não alterar. Refatoração é mudança de estrutura e qualidade técnica, não de funcionalidade ou regras de domínio.
- **Testes e funcionalidades:** Testes existentes devem continuar a passar; funcionalidades devem permanecer intactas. Não alterar ficheiros de teste para “acomodar” refatoração, salvo decisão explícita do projeto (ex.: atualização de mocks com justificação).
- **Contratos públicos:** APIs REST e contratos de frontend (portas, DTOs expostos) não devem ser alterados em comportamento observável; mudanças de assinaturas internas (ex.: Query com objeto de filter) são permitidas desde que os testes sejam atualizados de forma controlada e o contrato HTTP permaneça compatível.
- **Escopo deste documento:** Fonte de verdade para refatoração; não substitui especificações de novas funcionalidades nem relatórios de alterações (maestro) para novas demandas.

---

## Referências rápidas

- **Rastreabilidade:** `docs/tracability.md`
- **Relatório de inadequações RA-2:** `docs/relatorio-inadequacoes-ra2.md`
- **Regra frontend Angular:** `.cursor/rules/angular-frontend.mdc`
- **Skills:** batedor-de-codigos, mestre-freire, mestre-freire-angular, clean-architecture-analysis (ver descrições nas skills do agente).
