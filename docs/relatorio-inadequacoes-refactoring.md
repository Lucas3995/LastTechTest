# Relatório de Inadequações — Refactoring (Backend + Frontend)

## Escopo analisado

- **Backend:** LastTechTest.API (Controllers), LastTechTest.Aplicacao (Anticipation Queries/Commands/Handlers), LastTechTest.Dominio (interfaces, value objects), LastTechTest.Persistencia (repositories), LastTechTest.Infrastrutura.
- **Frontend:** frontend/src/app/application, frontend/src/app/domain, frontend/src/app/infrastructure, frontend/src/app/shared/utils, frontend/src/app/features/anticipation.

---

## Resumo

- Total de achados: 4
- Por categoria: Bloaters (1), Dispensables (1), Arch and Struct (1), Arch and Struct documentado (1)

---

## Achados

### 1 Long Parameter List — ListAnticipationRequestsQuery e AnticipationController.List

**Categoria:** Bloaters  
**Localização:** `backend/LastTechTest.Aplicacao/Anticipation/Queries/ListAnticipationRequests/ListAnticipationRequestsQuery.cs:5-11` e `backend/LastTechTest.API/Controllers/AnticipationController.cs:31-38`  
**Evidência:** A query expõe seis parâmetros (CreatorId, Status, FromUtc, ToUtc, Page, PageSize); o método List do controller recebe seis parâmetros [FromQuery] e repassa à query. O repositório e o handler já usam o value object `ListAnticipationRequestsFilter` (Dominio); a Query e o Controller continuam com parâmetros soltos.  
**Princípio/Referência violada:** Long Parameter List (mais de 3 parâmetros); manutenção e evolução do contrato mais custosas.  
**Contexto adicional:** Agrupar filtros num objeto (ex.: a Query receber um Filter ou objeto de parâmetros) alinharia a camada de aplicação/API ao uso já existente de `ListAnticipationRequestsFilter` no repositório. Alterar assinaturas que impactem testes pode exigir atualização de mocks (decisão explícita do projeto).

---

### 2 Dead Code — Ficheiros template Class1.cs

**Categoria:** Dispensables  
**Localização:** `backend/LastTechTest.Aplicacao/Class1.cs`, `backend/LastTechTest.Persistencia/Class1.cs`  
**Evidência:** Dois ficheiros `Class1.cs` (template vazio ou apenas comentário); removidos. Os ficheiros `LastTechTest.Dominio/Class1.cs` e `LastTechTest.Infrastrutura/Class1.cs` tinham nome de template mas continham código de produção (entidade `User` e `TokenService`/etc.); foram restaurados como `Dominio/Entities/User.cs` e `Infrastrutura/Class1.cs`.  
**Princípio/Referência violada:** Dead Code; limpeza de código.  
**Contexto adicional:** **Aplicado** apenas para Aplicacao e Persistencia. Dominio e Infrastrutura corrigidos com restauro/renomeação.

---

### 3 Violação de camadas (application → shared) — AnticipationMyRequestsFacade

**Categoria:** Arch and Struct  
**Localização:** `frontend/src/app/application/anticipation/anticipation-my-requests.facade.ts:4` e uso em linhas 60-64, 83-87, 109-113  
**Evidência:** O facade importa `buildErrorPresentation` de `../../shared/utils/backend-error.util`. O comentário em `frontend/src/app/application/index.ts` declara "Depends only on domain". A camada application não deve depender de shared para aderência estrita à regra de camadas (angular-frontend).  
**Princípio/Referência violada:** Clean Architecture (direção de dependência); camada application deve depender apenas de domain.  
**Contexto adicional:** **Aplicado.** Criado `domain/error-presentation.ts` (interface `ErrorPresentation` e token `ERROR_PRESENTATION_BUILDER`); facade injeta o token; implementação em `shared/utils/backend-error.util` e registo em `app.config.ts`. Fallback opcional no facade para testes (sem alterar ficheiros de teste).

---

### 4 Central de ações (Command) — documentado para evolução futura

**Categoria:** Arch and Struct  
**Localização:** `frontend/src/app/infrastructure/index.ts:3-4`  
**Evidência:** Comentário "Future: central de ações (Command pattern) for all backend requests." A regra angular-frontend exige que requisições ao backend passem por uma central de ações (persistir comando, retry, não quebrar a página).  
**Princípio/Referência violada:** Regra angular-frontend (central de ações); não aplicado no código atual.  
**Contexto adicional:** Prioridade baixa (fonte-de-verdade §3.1). Achado documentado para evolução futura; não obrigatório neste ciclo de refatoração.

---
