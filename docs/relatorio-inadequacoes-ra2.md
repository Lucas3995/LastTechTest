# Relatório de Inadequações — RA-2 (Consultar solicitações de antecipação)

## Escopo analisado

- `LastTechTest.Aplicacao/Anticipation/Queries/ListAnticipationRequests/*` (Query, Handler, Response, ListItem)
- `LastTechTest.Aplicacao/Anticipation/Queries/GetAnticipationRequestById/*` (Query, Handler, Response)
- `LastTechTest.Dominio/Interfaces/IAnticipationRequestRepository.cs` (contrato ListAsync)
- `LastTechTest.Persistencia/Repositories/AnticipationRequestRepository.cs` (ListAsync, GetByIdAsync)
- `LastTechTest.API/Program.cs` (endpoints GET /api/v1/anticipations e GET /api/v1/anticipations/{id})

---

## Resumo

- Total de achados: 2
- Por categoria: Bloaters (1), Dispensables (1)

---

## Achados

### 1 Long Parameter List — ListAnticipationRequestsQuery / IAnticipationRequestRepository.ListAsync

**Categoria:** Bloaters  
**Localização:** `LastTechTest.Aplicacao/Anticipation/Queries/ListAnticipationRequests/ListAnticipationRequestsQuery.cs:5-11` e `LastTechTest.Dominio/Interfaces/IAnticipationRequestRepository.cs:12-19`  
**Evidência:** A query e o método `ListAsync` expõem seis parâmetros (CreatorId, Status, FromUtc, ToUtc, Page, PageSize); o handler repassa todos ao repositório.  
**Princípio/Referência violada:** Long Parameter List (mais de 3 parâmetros); manutenção e evolução do contrato mais custosas.  
**Contexto adicional:** Agrupar filtros (creatorId, status, fromUtc, toUtc) num objeto de valor (ex.: `ListAnticipationRequestsFilter`) reduziria o número de parâmetros e alinharia com Data Clumps quando os mesmos filtros forem usados noutros pontos.  
**Nota (mestre-freire):** Não aplicado: alterar a assinatura de `ListAsync` obrigaria a alterar os mocks nos testes; a skill mestre-freire não permite editar ficheiros de teste.

---

### 2 Duplicate Code — Verificação de utilizador não autenticado nos handlers

**Categoria:** Dispensables  
**Localização:** `ListAnticipationRequestsQueryHandler.cs:24-26` e `GetAnticipationRequestByIdQueryHandler.cs:23-25`  
**Evidência:** Ambos os handlers repetem o mesmo bloco: `var userId = _currentUser.GetCurrentUserId(); if (userId is null) throw new UnauthorizedAccessException("User not authenticated.");`  
**Princípio/Referência violada:** DRY (Don't Repeat Yourself).  
**Contexto adicional:** Extrair para um método de extensão em `ICurrentUserService` (ex.: `EnsureAuthenticated()`) ou para um comportamento compartilhado (pipeline/behavior do MediatR) eliminaria a duplicação e centralizaria a mensagem.  
**Aplicado:** Criado `CurrentUserServiceExtensions.EnsureAuthenticated()` em `LastTechTest.Aplicacao/Common/Extensions/CurrentUserServiceExtensions.cs`; ambos os handlers passaram a usar `_currentUser.EnsureAuthenticated()`.

---
