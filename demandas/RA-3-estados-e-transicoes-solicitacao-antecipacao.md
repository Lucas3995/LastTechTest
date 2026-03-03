## [RA-3] Estados e transições da solicitação de antecipação

### Contexto e objetivo de negócio

Atualmente, as solicitações de antecipação precisam seguir um ciclo de vida claro e auditável, refletindo de forma consistente em que ponto da análise cada solicitação se encontra (aguardando análise, aprovada, recusada, cancelada pelo creator, etc.). A ausência de um modelo de estados explícito e de regras claras de transição pode gerar inconsistências, dúvidas operacionais e dificuldades de auditoria.  
Este card tem como objetivo **definir, modelar e expor via API o ciclo de vida da solicitação de antecipação**, garantindo que apenas transições válidas sejam permitidas, com as devidas permissões por papel (`Creator`, `Analista`, `Admin`) e com trilhas de auditoria adequadas.

### User story principal

- Como **Analista**, quero **aprovar ou recusar solicitações de antecipação em análise**, para **formalizar a decisão da análise de crédito/risco de forma rastreável e consistente**.

### User stories adicionais

- Como **Creator**, quero **cancelar uma solicitação de antecipação minha que ainda esteja pendente de análise**, para **evitar seguir com uma solicitação que não faz mais sentido para mim**.  
- Como **Admin**, quero **poder executar qualquer transição de estado permitida no sistema para qualquer solicitação**, para **atuar em situações excepcionais, correções operacionais ou suporte**, respeitando as mesmas regras de negócio definidas para cada transição.

### Personas / papéis afetados

- **Creator**: usuário que cria solicitações de antecipação e pode cancelá-las em determinados estados.  
- **Analista**: papel responsável por **análises e decisões administrativas** sobre as solicitações (aprovar, recusar, eventualmente outras ações administrativas futuras).  
- **Admin**: superusuário administrativo com acesso a **todas as ações** de todas as roles, inclusive `Creator` e `Analista`, para cenários excepcionais ou de suporte.

### Telas, módulos, relatórios e navegação

- **Escopo deste card**:
  - Não haverá telas/front-end implementados aqui.
  - Este card foca exclusivamente na **exposição de endpoints de API** e na modelagem de domínio/regra de negócio associada.
- **Módulo lógico**:
  - Domínio de **antecipação de recebíveis** / **solicitações de antecipação**, sob o módulo de Antecipação da API.
- Integrações futuras previstas (não implementadas neste card, mas a serem consideradas na modelagem):
  - Telas de gestão de solicitações para `Creator` (minhas solicitações, possibilidade de cancelamento).
  - Tela de fila de análise para `Analista` (pendentes, aprovadas, recusadas).
  - Relatórios ou dashboards de status para áreas de negócio e risco.

### Permissões e segurança

- **Role `Creator`**:
  - Pode **cancelar** apenas **solicitações próprias** em estado `ANALISE_PENDENTE` (ou outro estado explicitado como cancelável pelo Creator em cards futuros, se for o caso, mas inicialmente somente este).
  - Não pode aprovar, recusar ou alterar estados de solicitações de outros usuários.
- **Role `Analista`**:
  - Pode **aprovar** ou **recusar** solicitações em estado `ANALISE_PENDENTE`.  
  - Pode consultar solicitações em diferentes estados conforme regras de leitura definidas no RA-2.
  - Não pode alterar solicitações diretamente para estados que não respeitem as transições definidas neste card.
- **Role `Admin`**:
  - Tem acesso a **todas as ações** que `Creator` e `Analista` possuem no escopo deste card.
  - Pode executar transições de estado em qualquer solicitação, respeitando as **mesmas regras de negócio** e validações aplicáveis a `Creator` e `Analista` (não ignora o modelo de estados; apenas tem mais amplitude de escopo).
- **Segurança geral**:
  - Todos os endpoints devem respeitar o mecanismo de autenticação/autorização já estabelecido no backend (JWT, roles, etc.), reaproveitando a infraestrutura existente.

### Fluxos de uso e regras de negócio (estados e transições)

#### Estados da solicitação de antecipação

- `ANALISE_PENDENTE` (estado inicial):  
  - Representa uma solicitação recém-criada que **ainda não foi analisada** pelo `Analista`.  
- `APROVADA`:  
  - Representa uma solicitação cuja antecipação foi aprovada pelo `Analista` (ou `Admin` agindo como analista em casos excepcionais).  
- `RECUSADA`:  
  - Representa uma solicitação cuja antecipação foi recusada após análise.  
- `CANCELADA_POR_CREATOR`:  
  - Representa uma solicitação cancelada pelo próprio `Creator` antes da conclusão da análise.

> Observação: outros estados futuros (ex.: “LIQUIDADA”, “EXPIRADA”, etc.) poderão ser adicionados em cards posteriores; este card foca no ciclo básico de análise e cancelamento pelo solicitante.

#### Transições permitidas (happy path e regras)

- De `ANALISE_PENDENTE` para `APROVADA`:
  - Chamado por: `Analista` (ou `Admin`).  
  - Pré-condições:
    - Solicitação está em `ANALISE_PENDENTE`.
    - Informações mínimas de decisão (ex.: motivo, observações) são fornecidas, se exigidas pelo negócio.
  - Efeitos:
    - Status alterado para `APROVADA`.
    - Registro de auditoria com usuário, data/hora, motivo/observação.

- De `ANALISE_PENDENTE` para `RECUSADA`:
  - Chamado por: `Analista` (ou `Admin`).  
  - Pré-condições:
    - Solicitação está em `ANALISE_PENDENTE`.
    - Deve ser registrado pelo menos um motivo de recusa conforme regras de negócio vigentes.
  - Efeitos:
    - Status alterado para `RECUSADA`.
    - Registro de auditoria com usuário, data/hora, motivo de recusa.

- De `ANALISE_PENDENTE` para `CANCELADA_POR_CREATOR`:
  - Chamado por: `Creator` (ou `Admin` atuando em nome do usuário em situações especiais).
  - Pré-condições:
    - Solicitação pertence ao `Creator` autenticado (exceto quando `Admin` estiver atuando explicitamente em contexto administrativo, se assim definido).
    - Solicitação está em `ANALISE_PENDENTE`.
  - Efeitos:
    - Status alterado para `CANCELADA_POR_CREATOR`.
    - Registro de auditoria com usuário, data/hora e, opcionalmente, motivo do cancelamento.

#### Regra geral de transição

- **Não é permitido executar uma transição que mantenha a solicitação no mesmo estado atual** (ex.: tentar aprovar uma solicitação já `APROVADA`, recusar uma já `RECUSADA`, cancelar uma já `CANCELADA_POR_CREATOR`, ou recolocar em `ANALISE_PENDENTE` uma solicitação que já saiu desse estado).  
- Essas tentativas devem ser tratadas como operações inválidas (ou, quando definido como idempotência explícita, com mensagem clara de que a ação já foi realizada), **sem alterar o estado**.

#### Transições não permitidas (exemplos)

- Não é permitido:
  - Aprovar (`APROVADA`) uma solicitação que já esteja `RECUSADA` ou `CANCELADA_POR_CREATOR`.
  - Recusar (`RECUSADA`) uma solicitação que já esteja `APROVADA` ou `CANCELADA_POR_CREATOR`.
  - Cancelar (`CANCELADA_POR_CREATOR`) uma solicitação que já esteja `APROVADA`, `RECUSADA` ou já `CANCELADA_POR_CREATOR`.  
    - Neste caso, deve ser retornada uma resposta clara de que **a solicitação já foi cancelada anteriormente** (mensagem de idempotência/estado final).

### Critérios de aceitação (testáveis)

Use Given–When–Then, considerando apenas API (sem UI).

- **CA1 – Aprovação de solicitação em análise por Analista**  
  - Dado que existe uma solicitação de antecipação em estado `ANALISE_PENDENTE`,  
    e que o usuário autenticado possui papel `Analista` (ou `Admin`),  
  - Quando ele chamar o endpoint de **aprovar solicitação** passando o identificador da solicitação e os dados obrigatórios de decisão,  
  - Então o sistema deve:
    - Atualizar o estado da solicitação para `APROVADA`;
    - Registrar auditoria com usuário, data/hora e informações relevantes da decisão;
    - Retornar a solicitação já atualizada em `APROVADA` (ou uma representação equivalente) com código de status HTTP adequado (ex.: 200/204).

- **CA2 – Cancelamento de solicitação em análise pelo Creator e tratamento de cancelamentos repetidos**  
  - Cenário 1 – Cancelamento válido:
    - Dado que existe uma solicitação de antecipação pertencente ao `Creator` autenticado em estado `ANALISE_PENDENTE`,  
    - Quando o `Creator` chamar o endpoint de **cancelar solicitação** passando o identificador,  
    - Então o sistema deve:
      - Atualizar o estado para `CANCELADA_POR_CREATOR`;
      - Registrar auditoria de cancelamento;
      - Retornar a solicitação já atualizada em `CANCELADA_POR_CREATOR` (ou confirmação equivalente).

  - Cenário 2 – Tentativa de cancelamento após já ter sido cancelada:  
    - Dado que existe uma solicitação de antecipação pertencente ao `Creator` autenticado em estado `CANCELADA_POR_CREATOR`,  
    - Quando o `Creator` tentar chamar novamente o endpoint de **cancelar solicitação**,  
    - Então o sistema deve:
      - **Não alterar o estado** (permanece `CANCELADA_POR_CREATOR`);
      - Informar de forma clara na resposta que **a solicitação já foi cancelada anteriormente** (com mensagem específica indicando que a ação já foi realizada);
      - Retornar um código HTTP compatível com essa situação (por exemplo, 200 com mensagem de idempotência ou 409/422, conforme padrão do projeto).

- **CA3 – Recusa de solicitação em análise por Analista**  
  - Dado que existe uma solicitação de antecipação em estado `ANALISE_PENDENTE`,  
    e que o usuário autenticado possui papel `Analista` (ou `Admin`),  
  - Quando ele chamar o endpoint de **recusar solicitação** passando o identificador e o motivo da recusa,  
  - Então o sistema deve:
    - Atualizar o estado para `RECUSADA`;
    - Registrar auditoria com motivo de recusa;
    - Retornar confirmação da recusa e o novo estado.

- **CA4 – Bloqueio de transições inválidas (incluindo A → A)**  
  - Dado que existe uma solicitação de antecipação em qualquer estado final (`APROVADA`, `RECUSADA`, `CANCELADA_POR_CREATOR`) ou em um estado que não comporte a transição solicitada,  
  - Quando qualquer usuário (`Creator`, `Analista` ou `Admin`) tentar chamar endpoints de transição que não sejam permitidos a partir daquele estado, ou que **resultariam no mesmo estado atual** (ex.: aprovar uma solicitação já `APROVADA`, cancelar uma já `CANCELADA_POR_CREATOR`),  
  - Então o sistema deve:
    - **Recusar a operação**;
    - Não alterar o estado atual;
    - Retornar uma mensagem clara informando que a transição é inválida para o estado atual ou que a ação já foi realizada;
    - Respeitar os códigos de status HTTP definidos pelo padrão do projeto (por exemplo, 400/422 ou 200 com mensagem de idempotência, conforme decisão de padrão).

- **CA5 – Aplicação de permissões por papel**  
  - Dado que:
    - O usuário tem papel `Creator`,  
  - Quando tentar aprovar ou recusar uma solicitação por endpoints administrativos,  
  - Então o sistema deve recusar a operação por falta de permissão.  

  - Dado que:
    - O usuário tem papel `Analista`,  
  - Quando tentar aprovar ou recusar a partir de estados diferentes de `ANALISE_PENDENTE`,  
  - Então o sistema deve recusar a operação por estado inválido.

  - Dado que:
    - O usuário tem papel `Admin`,  
  - Quando ele chamar qualquer endpoint de transição respeitando as regras de negócio (estado atual e pré-condições),  
  - Então o sistema deve permitir a transição, mesmo que a solicitação pertença a outro usuário.

### Requisitos técnicos/metodológicos aplicáveis

#### Modelagem explícita de estados

- Implementar o modelo de estados de forma **explícita e centralizada** (por exemplo, enum/Value Object de status no domínio, e/ou um pequeno “state machine” interna) para evitar espalhar lógica de estados em múltiplos pontos.
- As regras de transição devem ficar claras em nível de domínio/aplicação, evitando que controllers conheçam detalhes de transição.

#### Arquitetura e padrões

- Respeitar a **Clean Architecture** já adotada no projeto (camadas de domínio, aplicação, infraestrutura, API).  
- Utilizar **CQRS + MediatR** para commands/queries relacionados às mudanças de estado, seguindo o padrão já existente no backend.  
- Aplicar **DDD** para modelar o agregado de solicitação de antecipação, incorporando o estado e as regras de transição como lógica de domínio, não apenas como flags de banco.  
- Seguir **SOLID** e os **3 princípios de coesão de componentes** (common closure, common reuse) na organização das classes relacionadas a estados e transições.  
- Utilizar boas práticas de **.NET e C#**, empregando design patterns quando fizer sentido (por exemplo, Strategy/State para transições, se adequado).

#### Performance, estruturas de dados e persistência

- Reutilizar o modelo de dados de solicitações definido em RA-1/RA-2, sem introduzir acoplamentos indevidos entre API, domínio e infraestrutura.  
- Considerar a complexidade assintótica das operações de consulta e atualização de estado, evitando cargas desnecessárias do agregado (carregar apenas o necessário para decidir a transição).  
- Garantir que operações de mudança de estado sejam otimizadas e consistentes (incluindo locks/concorrência, se necessário, conforme padrões do projeto).

#### Metodologia de testes (TDD + pirâmide completa)

- Utilizar **estritamente TDD**: escrever os testes antes da implementação, guiando o desenho do domínio e das transições de estado.  
- Implementar **toda a pirâmide de testes** para este card:
  - **Testes unitários**: validar regras de transição do domínio (ex.: “de ANALISE_PENDENTE para APROVADA por Analista é permitido”, “de CANCELADA_POR_CREATOR para APROVADA não é permitido”).  
  - **Testes de integração**: garantir que handlers/commands, repositórios e contexto de persistência (EF Core/SQLite) se comportam corretamente durante as transições, incluindo auditoria.  
  - **Testes E2E**: validar os fluxos principais via API (requisições HTTP simulando usuários `Creator`, `Analista`, `Admin`), cobrindo os critérios de aceitação CA1–CA5.  
- Usar as ferramentas padrão do projeto (xUnit, FluentAssertions, Coverlet, etc.) e manter cobertura alinhada à criticidade da funcionalidade.

### Rastreabilidade para código e testes (a preencher ao longo do ciclo)

- Casos de uso / commands:
  - Ex.: `ApproveAnticipationRequestCommand`, `RejectAnticipationRequestCommand`, `CancelAnticipationRequestCommand`.  
- Endpoints/rotas HTTP:
  - Ex.: `POST /api/v1/anticipations/{id}/approve`, `POST /api/v1/anticipations/{id}/reject`, `POST /api/v1/anticipations/{id}/cancel`.  
- Serviços de domínio:
  - Componente responsável pela lógica de estados e transições da solicitação de antecipação.  
- Testes:
  - Testes unitários de domínio de estados/transições.  
  - Testes de integração com banco de dados.  
  - Testes E2E cobrindo os critérios CA1–CA5.

### Dependências e riscos

- **Dependências**:
  - Depende de:
    - **RA-1 – Criar solicitação de antecipação** (modelo básico de solicitação e criação).  
    - **RA-2 – Consultar solicitações de antecipação** (estrutura de consulta/filtragem, reaproveitada para verificar estados).
- **Riscos**:
  - Risco de entendimento incorreto do fluxo real de análise de negócio; mitigação: revisar o modelo de estados com stakeholders de negócio e ajustar em novos cards, se necessário.  
  - Risco de usos inadequados por `Admin` (por ter acesso a todas as ações), aceito como risco de operação por enquanto e eventualmente mitigado em cards futuros (auditoria avançada, segregação de deveres, etc.).

