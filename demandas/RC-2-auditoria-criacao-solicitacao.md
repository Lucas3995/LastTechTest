## [RC-2] Registrar auditoria na criação de solicitação de antecipação (RA-1)

### Contexto e objetivo de negócio

O card **RA-1** exige "Auditoria mínima: quem chamou o endpoint (id do usuário, role), timestamp, valores envolvidos." Hoje o endpoint de criação de solicitação (`POST /api/v1/anticipations`) não regista nenhum evento de auditoria na criação; apenas as transições (aprovar, recusar, cancelar) dispõem de interface de auditoria (RA-3). O objetivo desta correção é garantir que **cada criação de solicitação** seja registada em trilha de auditoria com utilizador, role, data/hora e valores relevantes (ex.: valor solicitado, valor líquido, creator_id), permitindo rastreabilidade e conformidade com RA-1.

### User story principal

- Como **admin** ou gestor operacional, quero que **cada criação de solicitação de antecipação seja registada em auditoria** (quem criou, role, quando, valores), para **rastrear e auditar o uso do endpoint de criação** conforme RA-1.

### Personas / papéis afetados

- **Creator**: ao criar solicitação, a chamada fica registada em auditoria (userId do token, role Creator).
- **Admin**: ao criar em nome de um creator, a chamada fica registada (userId do admin, role Admin, creator_id alvo).
- **Operadores de risco / auditoria**: consomem os registos de auditoria para análise e conformidade.

### Telas, módulos, relatórios e navegação

- **Escopo deste card**: apenas API e persistência de auditoria; nenhuma tela ou endpoint de consulta de auditoria (estes podem ser cards futuros).
- **Módulo**: Antecipação de Valores; fluxo de criação em `POST /api/v1/anticipations`.

### Permissões e segurança

- Não há mudança de permissões. O registo de auditoria é interno; apenas quem já pode criar solicitação (Creator/Admin) gera eventos de auditoria de criação. Dados sensíveis devem limitar-se ao necessário (ids, valores, timestamps).

### Fluxos de uso e regras de negócio

#### Regra – Auditoria na criação

- **Regra (RA-1):** Toda criação de solicitação de antecipação deve gerar registo de auditoria com:
  - Identificador do utilizador que chamou o endpoint (id do usuário).
  - Role do utilizador (Creator ou Admin).
  - Timestamp (data/hora) da operação.
  - Valores envolvidos (ex.: valor solicitado, valor líquido, creator_id da solicitação).
- **Comportamento esperado:** Após persistir a entidade `AnticipationRequest`, o handler (ou serviço de auditoria) regista um evento de "criação" com os dados acima, persistido na mesma transação ou em transação adequada para garantir consistência.

#### Opções de desenho

- Estender a tabela/entidade de auditoria existente (ex.: `AnticipationRequestAudit`) para suportar ação "Create" e payload com valores (requestedAmount, netAmount, creatorId), ou criar entidade/contrato específico para auditoria de criação (ex.: `AnticipationRequestCreationAudit`). Manter coesão com o modelo de auditoria já usado em RA-3 quando fizer sentido.

### Critérios de aceitação (testáveis)

- **CA1 – Criação como Creator gera registo de auditoria**
  - Dado um utilizador autenticado com role `Creator` que chama `POST /api/v1/anticipations` com dados válidos,
  - Quando a solicitação é criada com sucesso (201),
  - Então deve existir um registo de auditoria associado à criação contendo: userId do Creator, role "Creator", timestamp, e valores relevantes (ex.: requestedAmount, netAmount, creatorId da solicitação).

- **CA2 – Criação como Admin gera registo de auditoria**
  - Dado um utilizador autenticado com role `Admin` que chama `POST /api/v1/anticipations` com `CreatorId` e dados válidos,
  - Quando a solicitação é criada com sucesso (201),
  - Então deve existir um registo de auditoria contendo: userId do Admin, role "Admin", timestamp, creatorId da solicitação e valores (requestedAmount, netAmount).

- **CA3 – Auditoria não altera resposta nem comportamento do endpoint**
  - Dado qualquer chamada válida ao endpoint de criação,
  - Quando a auditoria é registada (ou falha de forma controlada),
  - Então a resposta HTTP e o contrato da API permanecem os mesmos (201 com id, protocol, netAmount, status); falhas de auditoria devem ser tratadas conforme política do projeto (log e/ou falha da requisição, conforme decisão).

- **CA4 – Persistência em mesma transação ou consistente**
  - Dado que a criação da solicitação e o registo de auditoria são executados,
  - Quando a operação é bem-sucedida,
  - Então tanto a solicitação como o registo de auditoria devem estar persistidos; em caso de rollback da transação principal, o registo de auditoria não deve ficar órfão (ou seja, usar mesma transação ou padrão outbox/compensación conforme decisão arquitectural).

### Requisitos técnicos/metodológicos aplicáveis

- **TDD obrigatório:** escrever ou ajustar testes antes ou em par com a implementação; cobertura adequada nos pontos alterados.
- **Pirâmide completa:**
  - **Unit:** serviço ou componente que monta o evento de auditoria de criação (payload, dados do utilizador).
  - **Integração:** handler de criação com repositório e serviço de auditoria – verificar persistência da solicitação e do registo de auditoria (CA1, CA2, CA4).
  - **E2E:** (opcional) chamada HTTP de criação e verificação indireta (ex.: consulta a dados de auditoria em teste de integração com DB).
- **SOLID e 3 princípios de coesão de componentes:** auditoria de criação em camada de aplicação ou infraestrutura, sem espalhar lógica na API; reutilizar ou estender contrato de auditoria existente quando coerente.
- **Clean Architecture / CQRS:** alterações no `CreateAnticipationRequestCommandHandler` e possivelmente novo método no serviço de auditoria (ex.: `RecordCreationAsync`) ou extensão do contrato existente; persistência de auditoria na camada adequada.
- **Boas práticas .NET/C#**, estruturas de dados adequadas.
- **Referências:** RA-1 (auditoria mínima); `docs/tracability.md` deve ser atualizado após implementação.
- **Rotina-completa:** implementação (`mercenario`), testes (`quadro-de-recompensas`), análise (`batedor-de-codigos`), refatoração (`mestre-freire`); validar testes antes da entrega (`arauto`).

### Rastreabilidade para código e testes (a preencher na implementação)

- **Casos de uso:** `CreateAnticipationRequestCommand` + `CreateAnticipationRequestCommandHandler` (invocação do serviço de auditoria após criar a entidade).
- **Domínio/infra:** extensão de `IAnticipationAuditService` (ex.: `RecordCreationAsync`) ou novo contrato/serviço de auditoria de criação; entidade/tabela de auditoria de criação ou extensão de `AnticipationRequestAudit` com ação "Create" e campos de valores.
- **Testes a criar/alterar:**
  - Unit: componente que constrói o evento de auditoria de criação.
  - Integração: `CreateAnticipationRequestHandlerIntegrationTests` (ou equivalente) – após criar solicitação, verificar existência de registo de auditoria com userId, role, timestamp e valores.
  - E2E: opcional, conforme estratégia do projeto.

### Dependências e riscos

- **Dependências:** RA-1 (endpoint e handler de criação); modelo de auditoria existente (RA-3) para reutilizar padrão quando aplicável.
- **Riscos:** Aumento de escrita em base de dados por solicitação; mitigação com escrita eficiente e mesma transação quando possível. Falha ao gravar auditoria não deve corromper a criação sem política clara (ex.: log + alerta ou falha da requisição).
- **Decisão:** Se a auditoria de criação partilhar tabela com a de transições, garantir que ações (Create vs Approve/Reject/Cancel) e payloads estejam bem definidos para consultas futuras.