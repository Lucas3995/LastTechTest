# Relatório de Alterações para Demanda — RA-3 Estados e transições da solicitação de antecipação

## Resumo da demanda

Definir, modelar e expor via API o ciclo de vida da solicitação de antecipação com estados explícitos (ANALISE_PENDENTE, APROVADA, RECUSADA, CANCELADA_POR_CREATOR) e regras claras de transição. Apenas transições válidas devem ser permitidas, com permissões por papel (Creator, Analista, Admin) e trilhas de auditoria. Escopo: apenas backend/API (sem telas).

## Âmbito da análise

- **Áreas consideradas:** LastTechTest.Dominio (Entities, Enums, Interfaces, Authorization), LastTechTest.Aplicacao (Anticipation Commands/Queries), LastTechTest.Persistencia (ApplicationDbContext, AnticipationRequestRepository), LastTechTest.API (Program.cs, rotas de antecipação).
- **Premissas:** Estado inicial da solicitação criada (RA-1) será mapeado para ANALISE_PENDENTE (enum atual tem Created/Pending; será estendido ou renomeado). Auditoria será persistida em tabela/entidade dedicada (quem, quando, motivo/observação). Resposta idempotente para cancelamento já cancelado: 200 com mensagem ou 409/422 conforme padrão do projeto.

## Alterações necessárias

### 1. Enum de estados da solicitação

**Onde:** LastTechTest.Dominio/Enums/AnticipationRequestStatus.cs  
**Tipo:** Alterar  
**Descrição:** Estender o enum com os valores da demanda: ANALISE_PENDENTE (estado inicial, pode mapear de Created/Pending existentes), APROVADA, RECUSADA, CANCELADA_POR_CREATOR. Manter ou deprecar Created/Pending conforme decisão de migração; garantir que novas solicitações fiquem em ANALISE_PENDENTE.  
**Requisito atendido:** Estados da solicitação de antecipação (fluxos de uso e regras de negócio).

### 2. Componente de regras de transição no domínio

**Onde:** LastTechTest.Dominio (novo serviço de domínio ou métodos no agregado AnticipationRequest)  
**Tipo:** Criar  
**Descrição:** Implementar lógica centralizada que determina se uma transição é permitida: (estado atual, ação solicitada, papel do usuário, ownership quando Creator). Regras: ANALISE_PENDENTE → APROVADA (Analista/Admin); ANALISE_PENDENTE → RECUSADA (Analista/Admin, com motivo); ANALISE_PENDENTE → CANCELADA_POR_CREATOR (Creator dono ou Admin). Bloquear transições a partir de APROVADA, RECUSADA, CANCELADA_POR_CREATOR; bloquear A→A (ex.: aprovar já aprovada); Creator não pode aprovar/recusar. Pode ser um serviço de domínio (ex.: IAnticipationTransitionService) ou métodos no agregado (Approve, Reject, Cancel) que aplicam as regras.  
**Requisito atendido:** Modelagem explícita de estados; regra geral de transição; transições permitidas e não permitidas.

### 3. Métodos de transição no agregado AnticipationRequest

**Onde:** LastTechTest.Dominio/Entities/AnticipationRequest.cs  
**Tipo:** Alterar  
**Descrição:** Expor métodos que alteram o estado do agregado quando a transição for válida (ex.: Approve(string? observation), Reject(string reason), Cancel(Guid requestedByUserId, string? role)), lançando exceção de domínio ou retornando resultado quando a transição for inválida. O componente do item 2 pode ser usado internamente ou encapsulado aqui.  
**Requisito atendido:** Transições permitidas; efeitos (status alterado).

### 4. Persistência de auditoria

**Onde:** LastTechTest.Dominio (entidade ou value object de registro de auditoria), LastTechTest.Persistencia (tabela, repositório se necessário)  
**Tipo:** Criar  
**Descrição:** Modelar e persistir registro de auditoria por transição: identificador da solicitação, usuário que executou, data/hora UTC, tipo de ação (approve/reject/cancel), motivo/observação quando aplicável. Incluir migração EF Core para nova tabela se for entidade separada.  
**Requisito atendido:** Efeitos das transições (registro de auditoria); CA1, CA2, CA3.

### 5. Repositório de solicitações – suporte a atualização

**Onde:** LastTechTest.Dominio/Interfaces/IAnticipationRequestRepository.cs, LastTechTest.Persistencia/Repositories/AnticipationRequestRepository.cs  
**Tipo:** Alterar  
**Descrição:** Garantir que a solicitação possa ser atualizada após mudança de estado (ex.: método UpdateAsync ou GetByIdForUpdate que retorna entidade rastreada para alteração e SaveChanges). Hoje GetByIdAsync usa AsNoTracking; para transições é necessário carregar para atualizar.  
**Requisito atendido:** Persistência do novo estado após transição.

### 6. ApproveAnticipationRequestCommand e Handler

**Onde:** LastTechTest.Aplicacao/Anticipation/Commands/ApproveAnticipationRequest/ (novo)  
**Tipo:** Criar  
**Descrição:** Command com RequestId e dados obrigatórios de decisão (ex.: Observation). Handler: obter usuário atual e role (ICurrentUserService); validar papel (Analista ou Admin); obter solicitação por id; validar estado ANALISE_PENDENTE e executar transição (domínio); persistir entidade e registro de auditoria; retornar resposta com solicitação atualizada (ex.: ApproveAnticipationRequestResponse com Id, Status, etc.). Rejeitar com exceção ou Result quando transição inválida ou sem permissão.  
**Requisito atendido:** CA1 – Aprovação por Analista/Admin; permissões (CA5).

### 7. RejectAnticipationRequestCommand e Handler

**Onde:** LastTechTest.Aplicacao/Anticipation/Commands/RejectAnticipationRequest/ (novo)  
**Tipo:** Criar  
**Descrição:** Command com RequestId e motivo de recusa (obrigatório). Handler: mesma estrutura que Approve; validar papel Analista/Admin; estado ANALISE_PENDENTE; executar Reject no domínio; persistir e auditoria com motivo; retornar resposta.  
**Requisito atendido:** CA3 – Recusa por Analista/Admin; CA5.

### 8. CancelAnticipationRequestCommand e Handler

**Onde:** LastTechTest.Aplicacao/Anticipation/Commands/CancelAnticipationRequest/ (novo)  
**Tipo:** Criar  
**Descrição:** Command com RequestId e opcionalmente motivo. Handler: validar papel Creator (apenas própria solicitação) ou Admin; estado ANALISE_PENDENTE para transição; se já CANCELADA_POR_CREATOR, não alterar estado e retornar resposta de idempotência (mensagem clara “já cancelada”). Persistir transição e auditoria quando cancelamento for aplicado.  
**Requisito atendido:** CA2 – Cancelamento válido e idempotência; CA5.

### 9. Endpoint POST /api/v1/anticipations/{id}/approve

**Onde:** LastTechTest.API/Program.cs  
**Tipo:** Criar  
**Descrição:** Mapear POST para approve com body (ex.: observation). Autorização: RequireRole(Analista, Admin). Chamar ApproveAnticipationRequestCommand; retornar 200/204 com representação da solicitação atualizada; em caso de transição inválida ou estado incorreto, retornar 400/422 com mensagem clara.  
**Requisito atendido:** CA1; exposição via API.

### 10. Endpoint POST /api/v1/anticipations/{id}/reject

**Onde:** LastTechTest.API/Program.cs  
**Tipo:** Criar  
**Descrição:** Mapear POST para reject com body (motivo obrigatório). Autorização: RequireRole(Analista, Admin). Chamar RejectAnticipationRequestCommand; retornar 200 com confirmação e novo estado; 400/422 quando transição inválida.  
**Requisito atendido:** CA3; exposição via API.

### 11. Endpoint POST /api/v1/anticipations/{id}/cancel

**Onde:** LastTechTest.API/Program.cs  
**Tipo:** Criar  
**Descrição:** Mapear POST para cancel (body opcional com motivo). Autorização: RequireRole(Creator, Admin). Handler valida ownership quando Creator. Retornar 200 com solicitação atualizada ou 200 com mensagem de idempotência quando já cancelada; 403 quando Creator tenta cancelar solicitação de outro; 400/422 para estado inválido.  
**Requisito atendido:** CA2; CA5; exposição via API.

### 12. Consulta GET por id e listagem – suporte aos novos estados

**Onde:** LastTechTest.Aplicacao (GetAnticipationRequestById, ListAnticipationRequests), LastTechTest.API (respostas)  
**Tipo:** Integrar  
**Descrição:** Garantir que o status retornado nas respostas (GET by id, listagem, create) inclua os novos valores do enum (APROVADA, RECUSADA, CANCELADA_POR_CREATOR, ANALISE_PENDENTE). ListAnticipationRequests já filtra por status; permitir filtrar por esses valores. Analista deve poder consultar (RA-2); adicionar role Analista aos endpoints GET de antecipação se ainda não estiver.  
**Requisito atendido:** RA-2 reaproveitado; Analista pode consultar em diferentes estados.

---

## Resumo executivo

- **Total de itens de alteração:** 12  
- **Por tipo:** Criar (7), Alterar (4), Integrar (1)  
- **Dependências:** Itens 2 e 3 (domínio) devem existir antes dos handlers (6, 7, 8). Item 4 (auditoria) antes dos handlers. Item 5 (repositório) antes dos handlers. Itens 6–8 antes dos endpoints 9–11. Item 12 pode ser feito em paralelo ou após enum e domínio.
