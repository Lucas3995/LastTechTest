## [RA-1] Criar solicitacao de antecipacao

### Contexto e objetivo de negócio

Creators recebem valores de forma parcelada/ao longo do tempo e querem antecipar parte desses recebimentos para obter liquidez imediata. O objetivo é permitir que um creator autenticado registre, via API, uma solicitação de antecipação real (não simulada), aplicada sobre seus recebíveis elegíveis, com regras claras de validação, cálculo de limite e registro de estado, de forma segura e auditável para uso interno (incluindo admin).

### User story principal

- Como **creator autenticado**, quero **registrar uma solicitacao de antecipacao real** sobre meus recebiveis elegiveis, para **receber parte do valor antes da data original**.
- (Story complementar) Como **admin**, quero **poder utilizar todos os endpoints relacionados a solicitacoes de antecipacao**, para **suportar fluxos administrativos e de auditoria**, mesmo que o consumo atual seja principalmente por sistemas internos.

### Personas / papéis afetados

- **Creator**: usuário final que solicita antecipação sobre seus próprios recebíveis.
- **Admin**: usuário administrativo para cenários especiais, com acesso a **todas as ações que qualquer outra role tiver** (inclusive as ações deste card).

### Telas, módulos, relatórios e navegação

- **Escopo deste card**:
  - Não haverá telas/front-end implementados aqui.
  - Este card foca exclusivamente na **exposição de endpoints de API** e na modelagem de domínio/regra de negócio associada.
- **Módulo lógico**:
  - Módulo de **Antecipação de Valores** na API, com endpoints sob um prefixo coerente (por exemplo, `/api/v1/antecipacoes`), respeitando o versionamento atual.
- Qualquer tela, dashboard, grid ou fluxo de navegação para usuários finais será tratada em cards futuros, reaproveitando esses endpoints.

### Permissões e segurança

- **Role `Creator`**:
  - Pode **criar** solicitações de antecipação via endpoints, **apenas para si mesmo** (vinculadas ao seu próprio identificador de usuário/creator).
  - Pode **consultar** apenas suas próprias solicitações via endpoints de leitura.
- **Role `Admin`**:
  - Deve ter permissão para **chamar todos os endpoints desta demanda** (tanto de criação quanto de consulta), com capacidade de acessar dados de qualquer creator, conforme políticas internas.
  - Não há, neste card, ações exclusivas de admin (como aprovação/recusa); apenas o direito de usar tudo o que o creator pode e consultar qualquer registro.
- **Outras roles**:
  - Podem existir no sistema, mas **não** interagem com os endpoints deste card por enquanto.
- **Segurança geral**:
  - Autenticação via JWT existente (access/refresh).
  - Autorização baseada em roles:
    - Endpoints exigem role `Creator` ou `Admin`.
    - Quando o chamador for `Creator`, o backend deve garantir que a operação seja sempre restrita ao `creator_id` do token.
    - Quando o chamador for `Admin`, ele pode operar sobre qualquer `creator_id`, conforme o escopo do endpoint.

### Fluxos de uso e regras de negócio (API)

#### Happy path – Criar solicitação de antecipação (via endpoint)

1. Cliente de API (por exemplo, front futuro ou outro sistema) chama endpoint de criação de solicitação de antecipação autenticado como `Creator` ou `Admin`.
2. Backend identifica o usuário e sua role a partir do JWT.
3. Backend aplica regras de elegibilidade aos recebíveis indicados (conforme `InstrucoesProjeto`) e limites de valor.
4. Backend calcula valores de antecipação (bruto, taxas, descontos, líquido).
5. Backend registra a solicitação de antecipação em estado inicial (por exemplo, `CRIADA` ou `PENDENTE`) e retorna identificador/protocolo e resumo dos dados.
6. Para chamadas feitas por `Admin`, os mesmos fluxos são válidos, com a diferença de que o admin pode operar sobre qualquer `creator_id` permitido pelo contrato do endpoint.

#### Regras de negócio principais (adicionais às já existentes)

- Somente recebíveis elegíveis podem ser usados (datas/status/limites conforme `InstrucoesProjeto`).
- Respeito a limites máximos por creator, conforme política de risco.
- Solicitações feitas com token de `Creator` são automaticamente associadas ao `creator_id` do token; não é permitido informar outro `creator_id`.
- Solicitações feitas com token de `Admin` podem, se assim definido pelo contrato do endpoint, operar sobre um `creator_id` informado, respeitando regras de segurança do domínio.
- Estados de solicitação contemplam pelo menos o estado inicial; demais estados ficam para cards futuros (aprovação/liquidação etc.).
- Auditoria mínima: quem chamou o endpoint (id do usuário, role), timestamp, valores envolvidos.

#### Preparação para futura simulação (solicitação fake)

- A lógica de **cálculo e validação** da antecipação deve ser implementada em componentes de domínio/serviços reutilizáveis, independentes de persistência.
- O endpoint de criação real apenas orquestra:
  - Validação e cálculo via domínio.
  - Persistência da solicitação.
- Isso deve permitir que um futuro endpoint de **simulação fake**:
  - Reutilize exatamente as mesmas regras/serviços de cálculo e validação.
  - Não persista nada nem altere estados definitivos.

### Critérios de aceitação (testáveis)

- **CA1 – Criar solicitação válida (Creator)**
  - Dado um JWT válido com role `Creator` e recebíveis elegíveis associados a esse creator,
  - Quando um cliente chama o endpoint de criação de solicitação com dados válidos,
  - Então o backend deve criar a solicitação, persistir em estado inicial e retornar um identificador/protocolo no corpo da resposta.

- **CA2 – Admin com acesso às mesmas ações**
  - Dado um JWT válido com role `Admin`,
  - Quando um cliente chama o mesmo endpoint de criação de solicitação,
  - Então o backend deve permitir a criação da solicitação (respeitando as regras de negócio) e retornar identificador/protocolo, podendo operar sobre qualquer `creator_id` previsto pelo contrato do endpoint.

- **CA3 – Restrições de autorização para Creator**
  - Dado um JWT válido com role `Creator`,
  - Quando o cliente tenta criar ou consultar solicitações vinculadas a outro `creator_id`,
  - Então o backend deve rejeitar a operação com erro de autorização.

- **CA4 – Validações de negócio conforme InstrucoesProjeto**
  - Dado um creator cujos recebíveis não atendem às condições de elegibilidade ou limites,
  - Quando o endpoint de criação é chamado com esses dados,
  - Então o backend deve responder com erro de validação contendo motivo claro (por exemplo, valor acima do limite, recebível não elegível).

- **CA6 – Estrutura de erro rastreável para frontend e suporte**
  - Dado que ocorre qualquer erro na criação de solicitação (validação, autorização ou erro interno),
  - Quando o backend responde com falha,
  - Então a resposta deve conter, além do status HTTP adequado, um payload estruturado com campos como `code`, `message` (mensagem de domínio segura para exibição) e `traceId` (ou identificador equivalente), permitindo que o frontend exiba mensagens ricas ao usuário e que o suporte consiga correlacionar o caso com logs.

- **CA5 – Preparação para simulação**
  - Dado o código de domínio/aplicação responsável por cálculo/validação da antecipação,
  - Quando analisamos os testes unitários e de integração,
  - Então deve ser possível exercitar essas regras sem necessidade de persistir entidades, demonstrando que um endpoint de simulação pode ser criado apenas orquestrando esses componentes.

### Requisitos técnicos/metodológicos aplicáveis

#### Arquitetura, design e qualidade de código

- Respeitar **Clean Architecture**, **CQRS + MediatR** e **DDD** já adotados no projeto.
- Seguir **boas práticas de .NET e C#**, usando recursos idiomáticos da linguagem e do framework (tipos adequados, async/await, cancellation etc.).
- Usar **design patterns** quando necessário (por exemplo, Strategy para cálculos), evitando overengineering.
- Escolher **estruturas de dados adequadas** e cuidar de **complexidade assintótica** de operações relevantes, visando eficiência de processamento e memória.
- Aplicar rigorosamente os princípios **SOLID**.
- Considerar os **3 princípios de coesão de componentes** (agrupamento de classes por fechamento comum, reuso comum etc.), de forma que os componentes de antecipação sejam coesos e bem encapsulados.

- Padrão de tratamento de erros:
  - Handlers e endpoints devem **nunca descartar informações relevantes de exceções**; em vez disso, mapear erros de domínio para códigos e mensagens claras em um payload de erro consistente (incluindo `code` e `traceId`), mantendo detalhes sensíveis apenas em logs internos.

#### Metodologia

- Usar **TDD obrigatório**: escrever testes antes ou ao mesmo tempo que as implementações, deixando-os guiar o design.
- Modelar o domínio usando **DDD**, com linguagem ubíqua consistente com o contexto de antecipação.
- Seguir a rotina do repositório: `mercenario` (implementação), `quadro-de-recompensas` (testes), `batedor-de-codigos` (análise), `mestre-freire` (refatoração).
- Manter pirâmide de testes saudável (foco forte em testes unitários e de integração para serviços de domínio e handlers MediatR; E2E cobrindo o fluxo principal de API quando apropriado).

### Rastreabilidade para código e testes (a preencher depois)

- Commands/queries e handlers MediatR para criação e consulta de solicitações.
- Endpoints HTTP versionados (por exemplo, `POST /api/v1/antecipacoes`, `GET /api/v1/antecipacoes/{id}` ou equivalentes).
- Serviços de domínio para cálculo/validação reutilizáveis em simulação.
- Testes unitários de domínio, testes de integração dos handlers/endpoints e, futuramente, testes E2E de fluxo completo da API.

### Dependências e riscos

- **Dependências**
  - Infraestrutura de autenticação/autorização com roles `Creator` e `Admin`.
  - Modelagem de recebíveis e regras de elegibilidade descritas em `InstrucoesProjeto`.

- **Riscos**
  - Se a separação entre lógica de negócio e persistência não for bem feita, o card de simulação futuro exigirá refatores grandes.
  - Se a política de uso de `Admin` não estiver bem comunicada, pode haver risco de uso indevido em produção; os testes e logs devem reforçar visibilidade.

