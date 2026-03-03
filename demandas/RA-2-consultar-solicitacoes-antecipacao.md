## [RA-2] Consultar solicitacoes de antecipacao

### Contexto e objetivo de negócio

Depois de criar solicitações de antecipação (RA-1), é necessário que creators e times internos consigam **consultar** essas solicitações via API para acompanhar status, valores e histórico. O objetivo é expor endpoints de leitura que permitam a cada creator enxergar apenas suas próprias solicitações, enquanto o admin consegue enxergar o conjunto completo, suportando auditoria, monitoramento operacional e integrações futuras, sem ainda envolver telas ou relatórios visuais.

### User story principal

- Como **creator autenticado**, quero **listar e consultar os detalhes das minhas solicitacoes de antecipacao**, para **acompanhar o que ja pedi, seus valores e status**.
- Como **admin**, quero **listar e consultar as solicitacoes de antecipacao de qualquer creator**, para **monitorar o uso da antecipacao, apoiar analise de risco e integrar com fluxos internos**.

### Personas / papéis afetados

- **Creator**: precisa enxergar somente suas próprias solicitações.
- **Admin**: precisa enxergar qualquer solicitação de qualquer creator (superuser).

### Telas, módulos, relatórios e navegação

- **Escopo deste card**:
  - Não haverá telas/front-end; será implementado apenas em **endpoints de API**.
- **Módulo lógico**:
  - Módulo de **Antecipação de Valores** na API, reutilizando o prefixo de RA-1 (por exemplo, `/api/v1/antecipacoes`), respeitando o versionamento atual.
- Telas, grids e dashboards que consumam esses dados ficarão para cards futuros.

### Permissões e segurança

- **Role `Creator`**:
  - Pode **listar** solicitações de antecipação **apenas do próprio creator**.
  - Pode **consultar detalhes** (por id) somente de solicitações vinculadas ao seu `creator_id`.
- **Role `Admin`**:
  - Pode **listar** todas as solicitações de antecipação do sistema, com filtros (por creator, status, período etc.).
  - Pode **consultar detalhes** de qualquer solicitação.
  - Não ganha ações exclusivas além de ter acesso a tudo que o creator tem, porém em escopo global.
- **Outras roles**:
  - Não participam desses endpoints neste card.
- **Segurança geral**:
  - Autenticação via JWT (access/refresh) existente.
  - Autorização por role:
    - Endpoints de consulta exigem role `Creator` ou `Admin`.
    - Para `Creator`, o backend deve forçar filtro por `creator_id` do token (sem confiar em parâmetros de filtro externos para ampliar escopo).
    - Para `Admin`, o backend pode aceitar filtros de qualquer `creator_id`.

### Fluxos de uso e regras de negócio (API)

#### Fluxo 1 – Listar solicitacoes (Creator)

1. Cliente de API chama endpoint de listagem autenticado como `Creator`.
2. Backend identifica `creator_id` via token e ignora qualquer tentativa de informar outro `creator_id` no request.
3. Backend retorna lista paginada (ou equivalente) de solicitações vinculadas àquele creator, com campos essenciais (id, datas, valores, status atual).
4. Nenhuma solicitação de outros creators é retornada.

#### Fluxo 2 – Listar solicitacoes (Admin)

1. Cliente de API chama endpoint de listagem autenticado como `Admin`.
2. Backend aceita filtros gerais (por creator, status, período, etc.) e busca no conjunto completo.
3. Backend retorna lista paginada de solicitações de acordo com os filtros.

#### Fluxo 3 – Consultar detalhes de uma solicitacao (Creator)

1. Cliente de API chama endpoint de consulta por id autenticado como `Creator`.
2. Backend verifica se a solicitação pertence ao `creator_id` do token.
3. Se pertencer, retorna detalhes completos; se não, responde erro de autorização.

#### Fluxo 4 – Consultar detalhes de uma solicitacao (Admin)

1. Cliente de API chama endpoint de consulta por id autenticado como `Admin`.
2. Backend encontra a solicitação, independentemente do creator, e retorna detalhes.

#### Regras de negócio principais (adicionais às já existentes)

- Só podem ser consultadas solicitações existentes, criadas via RA-1 ou funcionalidades futuras.
- Status e campos expostos devem ser consistentes com a modelagem de domínio de antecipação (datas, valores, status, creator etc.).
- Logs e auditoria devem registrar quem consultou o quê (especialmente para admin).

### Critérios de aceitação (testáveis)

- **CA1 – Listagem restrita para Creator**
  - Dado um JWT válido com role `Creator`,
  - Quando o cliente chama o endpoint de listagem sem filtros de creator,
  - Então o backend deve retornar apenas solicitações vinculadas ao `creator_id` do token.

- **CA2 – Bloqueio de acesso cruzado (Creator)**
  - Dado um JWT válido com role `Creator`,
  - Quando o cliente tenta consultar por id uma solicitação pertencente a outro `creator_id`,
  - Então o backend deve retornar erro de autorização.

- **CA3 – Listagem global para Admin**
  - Dado um JWT válido com role `Admin`,
  - Quando o cliente chama o endpoint de listagem com ou sem filtros,
  - Então o backend deve retornar solicitações de qualquer creator conforme os filtros aplicados.

- **CA4 – Consulta por id para Admin**
  - Dado um JWT válido com role `Admin`,
  - Quando o cliente consulta por id uma solicitação existente,
  - Então o backend deve retornar os detalhes completos, independentemente do creator.

- **CA5 – Paginacao / limites de resposta**
  - Dado um conjunto grande de solicitações,
  - Quando o endpoint de listagem é chamado,
  - Então o backend deve aplicar paginação/limites padrão (ou outra estratégia definida) para evitar respostas excessivamente grandes.

### Requisitos técnicos/metodológicos aplicáveis

#### Arquitetura, design e qualidade de código

- Reutilizar o contexto de domínio e estrutura definida em RA-1 (entidades, agregados, serviços de domínio).
- Implementar a parte de leitura em conformidade com **Clean Architecture** e **CQRS + MediatR** (queries/handlers específicos para listagem e detalhes).
- Seguir **boas práticas de .NET e C#**, usando recursos idiomáticos da linguagem e do framework.
- **Desempenho das consultas (obrigatório)**:
  - As consultas devem ser projetadas para **não serem mal otimizadas**:
    - Usar projeções (`Select`) para retornar apenas os campos necessários.
    - Evitar N+1 (por exemplo, usando includes ou carregamento adequado).
    - Aplicar paginação/limites e filtros na própria consulta ao banco (sem paginar em memória).
    - Considerar índices e chaves de busca típicas (por creator, status, data).
    - Garantir complexidade assintótica adequada para os cenários esperados (por exemplo, O(log n) ou O(n) controlado em conjuntos filtrados).
  - Testes de integração devem cobrir cenários de volume razoável, para garantir que o comportamento continue aceitável.
- Seguir princípios **SOLID** e os **3 princípios de coesão de componentes**, mantendo consultas de antecipação coesas e separadas de outras preocupações.

#### Metodologia

- **TDD obrigatório** nas queries/handlers e endpoints de leitura.
- Modelagem alinhada a **DDD**.
- Seguir a rotina do repositório: `mercenario` (implementação), `quadro-de-recompensas` (testes), `batedor-de-codigos` (análise), `mestre-freire` (refatoração).
- Manter pirâmide de testes saudável:
  - Testes unitários para consultas de domínio.
  - Testes de integração para queries/handlers e endpoints.
  - E2E apenas quando necessário para cobrir o fluxo completo de leitura.

### Rastreabilidade para código e testes (a preencher depois)

- Queries/handlers MediatR para:
  - Listagem paginada de solicitações.
  - Consulta por id.
- Endpoints HTTP (por exemplo, `GET /api/v1/antecipacoes`, `GET /api/v1/antecipacoes/{id}`).
- Testes unitários e de integração cobrindo CA1–CA5.

### Dependências e riscos

- **Dependências**
  - Entidades/infra de persistência de solicitações criadas em RA-1.
  - Infraestrutura de autenticação/autorização com roles `Creator` e `Admin`.

- **Riscos**
  - **Risco assumido (Admin)**: uso indevido de poderes amplos do usuário admin em produção; por enquanto, esse risco é conhecido e aceito, sem mitigação específica neste card além de rastreabilidade/logs básicos.

