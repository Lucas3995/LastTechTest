## [RC-1] Corrigir validações no endpoint de criar solicitação de antecipação

### Contexto e objetivo de negócio

Duas regras de negócio definidas no **InstrucoesProjeto** não estão aplicadas no endpoint de criação de solicitação de antecipação, gerando comportamento incorreto: (1) um creator pode criar várias solicitações enquanto já possui uma em análise; (2) é possível criar solicitações com valor solicitado menor ou igual a R$ 100,00. O objetivo desta correção é garantir que o endpoint `POST /api/v1/anticipations` rejeite valor menor ou igual a 100 e bloqueie nova solicitação quando o creator já tiver uma em estado "em análise" (Created ou Pending), alinhando o sistema às regras de negócio e à rastreabilidade dos requisitos RA-1 a RA-3.

### User story principal

- Como **creator** (ou consumidor da API), quero que o sistema **impeça criar nova solicitação enquanto existir uma em análise** e **rejeite valor solicitado menor ou igual a R$ 100**, para que as regras de negócio estejam corretas e auditáveis.

### Personas / papéis afetados

- **Creator**: afetado pelas duas validações ao criar solicitação.
- **Admin**: ao criar solicitação em nome de um creator, as mesmas validações aplicam-se (valor mínimo e "uma pendente por creator").

### Telas, módulos, relatórios e navegação

- **Escopo deste card**: apenas API; nenhuma tela ou front-end.
- **Módulo**: Antecipação de Valores; endpoint `POST /api/v1/anticipations`.

### Permissões e segurança

- Não há mudança de permissões. As validações aplicam-se igualmente a Creator e Admin (Admin ao criar para um creator também deve respeitar "uma pendente por creator" e valor mínimo).

### Fluxos de uso e regras de negócio

#### Bug 1 – Uma solicitação "pendente" por creator

- **Regra (InstrucoesProjeto):** Um creator não pode ter mais de uma solicitação **em análise** ao mesmo tempo.
- **Definição de "em análise":** status `Created` ou `Pending` (equivalente a ANALISE_PENDENTE nos cards RA-3), conforme `AnticipationTransitionRules.IsAnalysisPending` no domínio.
- **Comportamento esperado:** Se já existir uma solicitação nesses estados para o `creator_id` efetivo da requisição, o endpoint deve recusar com **400** e mensagem clara (ex.: já existe solicitação em aberto); **não** deve criar segunda solicitação.

#### Bug 2 – Valor mínimo

- **Regra (InstrucoesProjeto):** Valor solicitado deve ser **estritamente maior que R$ 100,00**.
- **Comportamento esperado:** Valores menores ou iguais a 100 devem ser rejeitados. Validação no command (FluentValidation); resposta **400** com mensagem clara (ex.: "Requested amount must be greater than 100" ou equivalente em português).

### Critérios de aceitação (testáveis)

- **CA1 – Valor <= 100 rejeitado (validação)**
  - Dado um utilizador autenticado (Creator ou Admin) e dados válidos exceto `RequestedAmount` <= 100,
  - Quando o cliente chama `POST /api/v1/anticipations` com esse valor,
  - Então o backend deve retornar **400** e **não** criar solicitação; a mensagem deve indicar que o valor deve ser maior que 100.

- **CA2 – Valor > 100 aceite na validação**
  - Dado um utilizador autenticado e recebíveis elegíveis,
  - Quando o cliente chama o endpoint com `RequestedAmount` > 100 (ex.: 100.01 ou 101),
  - Então a validação de valor mínimo deve passar (outras regras, como limite ou elegibilidade, podem ainda falhar).

- **CA3 – Creator com solicitação já em análise não pode criar outra**
  - Dado um creator que já possui **exatamente uma** solicitação em estado `Created` ou `Pending`,
  - Quando o cliente chama `POST /api/v1/anticipations` para esse creator com dados que seriam válidos (valor > 100, elegibilidade, limite),
  - Então o backend deve recusar com **400** e mensagem clara (ex.: já existe solicitação em aberto); **não** deve criar segunda solicitação.

- **CA4 – Creator sem solicitação em análise pode criar**
  - Dado um creator **sem** solicitação em `Created` ou `Pending` (nenhuma ou apenas em Approved/Rejected/CanceledByCreator),
  - Quando o cliente chama o endpoint com dados válidos,
  - Então o backend deve criar a solicitação e retornar **201**.

- **CA5 – Admin ao criar para um creator respeita a mesma regra**
  - Dado um Admin que chama o endpoint com `CreatorId` de um creator que já tem uma solicitação em análise,
  - Quando o cliente envia o POST com dados válidos,
  - Então o backend deve recusar com **400** (mesma regra: um creator não pode ter mais de uma pendente).

### Requisitos técnicos/metodológicos aplicáveis

- **TDD obrigatório:** escrever ou ajustar testes **antes** ou em par com a implementação; árvore de testes completa com cobertura adequada (objetivo 100% nos pontos alterados).
- **Pirâmide completa:**
  - **Unit:** validador (valor mínimo; eventualmente regra de "uma pendente" se extraída para domínio/serviço).
  - **Integração:** handler com repositório real ou mock – cenários CA3, CA4, CA5.
  - **E2E:** chamadas HTTP aos cenários de rejeição (valor <= 100, já tem pendente) e de sucesso (valor > 100, sem pendente).
- **SOLID e 3 princípios de coesão de componentes:** manter validações e regra "uma pendente" em camadas adequadas (validador para valor mínimo; handler ou serviço de domínio para "já existe pendente"), sem espalhar lógica na API.
- **Clean Architecture / CQRS:** alterações limitadas ao command de criação (validador + handler); repositório pode ganhar método `HasPendingByCreatorAsync(creatorId)` se fizer sentido para clareza e performance (uma query em vez de listar).
- **Boas práticas .NET/C#**, design patterns quando fizer sentido, estruturas de dados e análise assintótica.
- **Referências:** RA-1, RA-2, RA-3 em `demandas/`; `docs/tracability.md` deve ser atualizado após implementação (mapeamento RC-1 → command, validator, handler, testes).
- **Rotina-completa:** implementação (`mercenario`), criação de testes (`quadro-de-recompensas`), análise de código (`batedor-de-codigos`), refatoração (`mestre-freire`); validar testes antes da entrega (`arauto`).

### Rastreabilidade para código e testes (a preencher na implementação)

- **Casos de uso:** `CreateAnticipationRequestCommand` + `CreateAnticipationRequestCommandHandler`; `CreateAnticipationRequestCommandValidator`.
- **Domínio/infra:** possivelmente `IAnticipationRequestRepository.HasPendingByCreatorAsync(creatorId)` (ou uso de `ListAsync` com status Created/Pending e pageSize=1 para existência).
- **Testes a criar/alterar:**
  - Unit: `CreateAnticipationRequestCommandValidatorTests` – valor 0, negativo, 100 (rejeitado), 100.01 ou 101 (aceite).
  - Unit (se houver serviço/regra de domínio para "uma pendente"): testes da regra.
  - Integração: `CreateAnticipationRequestHandlerIntegrationTests` – creator já com pendente → falha; creator sem pendente → sucesso; admin criando para creator com pendente → falha.
  - E2E: `AnticipationE2ETests` – POST com 50 ou 100 → 400; POST com 100.01 ou 101 sem pendente → 201; POST com 101 com pendente → 400.

### Dependências e riscos

- **Dependências:** RA-1 (endpoint e command existentes), RA-2 (consulta), RA-3 (estados). InstrucoesProjeto como fonte das regras.
- **Riscos:** Nenhum crítico; tratar "uma pendente" em termos dos estados `Created` e `Pending` já usados no RA-3.
