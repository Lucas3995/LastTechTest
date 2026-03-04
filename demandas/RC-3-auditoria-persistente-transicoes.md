## [RC-3] Ativar auditoria persistente nas transições de solicitação (RA-3)

### Contexto e objetivo de negócio

O card **RA-3** exige "Registro de auditoria com usuário, data/hora, motivo/observação" em cada transição de estado (aprovar, recusar, cancelar). O fluxo de transições já invoca `IAnticipationAuditService.RecordTransitionAsync` no `AnticipationTransitionExecutor`, e existe uma implementação persistente (`AnticipationAuditService` em LastTechTest.Persistencia) que grava na tabela `AnticipationRequestAudits`. Porém, na API está registado **NoOpAnticipationAuditService**, pelo que em execução normal as transições **não** geram registos persistidos. O objetivo desta correção é **ativar a auditoria persistente** nas transições (aprovar, recusar, cancelar), por configuração ou troca de registo, garantindo que os eventos fiquem gravados conforme RA-3.

### User story principal

- Como **analista** ou gestor operacional, quero que **cada aprovação, recusa e cancelamento de solicitação seja registado em base de dados** (quem fez, quando, motivo/observação), para **auditar decisões e transições** conforme RA-3.

### Personas / papéis afetados

- **Analista**: ao aprovar ou recusar, a ação fica registada em auditoria.
- **Creator**: ao cancelar a própria solicitação, a ação fica registada.
- **Admin**: ao executar qualquer transição, a ação fica registada.
- **Operadores de risco / auditoria**: consomem os registos para análise e conformidade.

### Telas, módulos, relatórios e navegação

- **Escopo deste card**: configuração/registro de serviços e persistência de auditoria; endpoints de transição já existentes (`POST .../approve`, `.../reject`, `.../cancel`). Nenhuma tela nem endpoint novo de consulta de auditoria (cards futuros).
- **Módulo**: Antecipação de Valores; fluxos de transição RA-3.

### Permissões e segurança

- Não há mudança de permissões. Quem já pode aprovar, recusar ou cancelar continua com as mesmas regras; apenas se ativa a persistência dos eventos de auditoria que já são produzidos pelo executor.

### Fluxos de uso e regras de negócio

#### Regra – Auditoria persistente nas transições

- **Regra (RA-3):** Cada transição (Aprovar, Recusar, Cancelar) deve gerar um registo de auditoria persistido com:
  - RequestId da solicitação.
  - Ação (Approve, Reject, Cancel).
  - UserId do utilizador que executou a ação.
  - Data/hora (AtUtc).
  - Motivo ou observação (ReasonOrObservation), quando aplicável.
- **Comportamento esperado:** Em ambiente( s) definidos (ex.: produção, staging), o container deve usar `AnticipationAuditService` (Persistencia) em vez de `NoOpAnticipationAuditService`, de forma que cada chamada a approve/reject/cancel persista um registo em `AnticipationRequestAudits`. Em testes unitários ou de integração que não queiram efeitos colaterais, pode manter-se o NoOp por configuração ou substituição em teste.

#### Opções de desenho

- **Registro por ambiente:** Em `Program.cs` (ou configuração por ambiente), registrar `IAnticipationAuditService` com `AnticipationAuditService` quando não for ambiente de teste; em ambiente "Testing" ou quando config indicar, manter `NoOpAnticipationAuditService`.
- **Transação:** O executor já chama `_auditService.RecordTransitionAsync` antes de `_repository.SaveChangesAsync()`; garantir que o `AnticipationAuditService` usa o mesmo `DbContext` que o repositório (já é o caso se o mesmo scope injetar ambos), para que uma única transação persista a alteração de estado e o registo de auditoria.

### Critérios de aceitação (testáveis)

- **CA1 – Aprovar persiste registo de auditoria**
  - Dado uma solicitação em estado Pending e um utilizador com role Analista ou Admin,
  - Quando o endpoint `POST /api/v1/anticipations/{id}/approve` é chamado com sucesso (200),
  - Então deve existir um registo em `AnticipationRequestAudits` (ou equivalente) com RequestId, Action "Approve", UserId, AtUtc e observação quando fornecida.

- **CA2 – Recusar persiste registo de auditoria**
  - Dado uma solicitação em estado Pending e um utilizador com role Analista ou Admin,
  - Quando o endpoint `POST /api/v1/anticipations/{id}/reject` é chamado com Reason e retorna 200,
  - Então deve existir um registo de auditoria com Action "Reject", UserId, AtUtc e Reason preenchido.

- **CA3 – Cancelar persiste registo de auditoria**
  - Dado uma solicitação em estado Pending pertencente ao Creator autenticado (ou Admin),
  - Quando o endpoint `POST /api/v1/anticipations/{id}/cancel` é chamado com sucesso (200),
  - Então deve existir um registo de auditoria com Action "Cancel", UserId e AtUtc.

- **CA4 – Transação consistente**
  - Dado uma chamada a approve/reject/cancel que altera o estado da solicitação,
  - Quando a operação é bem-sucedida,
  - Então tanto a alteração de estado na entidade como o registo em `AnticipationRequestAudits` devem estar persistidos; em caso de falha após gravar auditoria e antes de SaveChanges, ou falha de SaveChanges, o comportamento deve ser consistente (rollback da transação quando usar o mesmo DbContext).

- **CA5 – Testes que usam NoOp não dependem de persistência de auditoria**
  - Testes de integração ou E2E que hoje usam `NoOpAnticipationAuditService` continuam a passar; testes que verificam auditoria persistente devem usar (ou configurar) o `AnticipationAuditService` real contra base de dados (ex.: in-memory ou SQLite de teste).

### Requisitos técnicos/metodológicos aplicáveis

- **TDD obrigatório:** ajustar ou criar testes que validem a persistência de auditoria (integração com DB) e garantir que a ativação por configuração não quebra testes existentes.
- **Pirâmide completa:**
  - **Unit:** manter testes de domínio/executor que possam continuar com NoOp.
  - **Integração:** testes que usam `AnticipationAuditService` e DbContext para verificar que approve/reject/cancel gravam em `AnticipationRequestAudits` (CA1–CA4).
  - **E2E:** opcional; chamada HTTP a approve/reject/cancel e verificação em base de dados de que o registo de auditoria existe.
- **SOLID e 3 princípios de coesão de componentes:** sem alterar a assinatura do executor; apenas a implementação injectada de `IAnticipationAuditService` muda (NoOp vs persistente) conforme ambiente/configuração.
- **Clean Architecture:** alterações limitadas à composição/registro em API (Program.cs) e possivelmente testes; Persistencia já contém `AnticipationAuditService` e configuração da tabela.
- **Boas práticas .NET/C#**, uso do mesmo `DbContext` no mesmo scope para transação única.
- **Referências:** RA-3 (auditoria nas transições); `docs/tracability.md` deve ser atualizado após implementação.
- **Rotina-completa:** implementação (`mercenario`), testes (`quadro-de-recompensas`), análise (`batedor-de-codigos`), refatoração (`mestre-freire`); validar testes antes da entrega (`arauto`).

### Rastreabilidade para código e testes (a preencher na implementação)

- **Casos de uso:** mesmos handlers e executor de RA-3; alteração apenas no registro de `IAnticipationAuditService`.
- **Infraestrutura:** `LastTechTest.Persistencia.Services.AnticipationAuditService`; tabela `AnticipationRequestAudits`; `Program.cs` (registro condicional por ambiente ou config).
- **Testes a criar/alterar:**
  - Integração: cenários de approve/reject/cancel com `AnticipationAuditService` real, verificando registos em `AnticipationRequestAudits`.
  - E2E: opcional – verificação de que após approve/reject/cancel via API existe registo de auditoria (quando API estiver configurada com auditoria persistente no ambiente de teste).
- **Configuração:** documentar em README ou configuração como ativar auditoria persistente por ambiente (ex.: `UseAnticipationAudit = true` ou ausência de ambiente "Testing").

### Dependências e riscos

- **Dependências:** RA-3 (executor, handlers, endpoints de transição); entidade `AnticipationRequestAudit` e migração já existentes; `ApplicationDbContext` com `AnticipationRequestAudits`.
- **Riscos:** Em testes que partilham o mesmo DbContext e esperam contagem de entidades, a nova escrita em `AnticipationRequestAudits` pode afectar asserts; mitigação com asserts explícitos sobre auditoria ou isolamento de testes. Em produção, garantir que o mesmo DbContext/transação é usado para solicitação e auditoria (já é o caso no executor).
- **Decisão:** Definir em que ambientes a auditoria persistente está ativa (ex.: todos exceto "Testing") e documentar.