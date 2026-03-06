## [RF-5] Criar nova solicitação de antecipação no frontend (Creators)

### Contexto e objetivo de negócio

A tela "Minhas solicitações de antecipação" (RF-1) já exibe os botões **"Nova solicitação"** e **"Simular antecipação"**, mas o botão "Nova solicitação" ainda não leva a um fluxo de criação. O backend expõe, desde RA-1 e RC-1, o endpoint **criar solicitação** (`POST /api/v1/anticipations`) com validações de valor mínimo (>= 100), limite por creator (no máximo uma solicitação em análise por creator) e regras de elegibilidade. Falta que o frontend ofereça um **formulário de criação** integrado, para que Creators (e Admin, quando aplicável) possam registrar novas solicitações diretamente na aplicação, com validação inline e feedback claro em caso de sucesso ou erro.

O objetivo desta demanda é implementar, no frontend Angular, o **fluxo de criação de nova solicitação de antecipação**: navegação a partir do botão "Nova solicitação" (RF-1), formulário com valor solicitado e demais campos exigidos pela API, submissão ao backend e atualização da lista após sucesso, com tratamento de erros (validação, uma pendente por creator, etc.).

### User stories de frontend

- **US-CR1 – Abrir formulário de nova solicitação**
  - Como **Creator**, quero **acessar um formulário de nova solicitação a partir do botão "Nova solicitação" na tela Minhas solicitações**, para **preencher os dados e submeter sem sair do contexto da aplicação**.

- **US-CR2 – Informar valor e submeter**
  - Como **Creator**, quero **informar o valor que desejo antecipar e submeter o formulário**, para **criar uma solicitação real e vê-la na minha lista**.

- **US-CR3 – Ver validações antes de enviar**
  - Como **Creator**, quero **ver mensagens de validação no próprio formulário** (valor mínimo, formato, etc.), para **corrigir antes de enviar e evitar erros desnecessários**.

- **US-CR4 – Feedback de sucesso e erro**
  - Como **Creator**, quero **ver mensagem de sucesso e a nova solicitação na lista após criar**, e **em caso de erro (ex.: já tenho uma solicitação em análise) ver a mensagem retornada pelo sistema e um código para suporte**, para **entender o resultado e agir conforme necessário**.

- **US-CR5 – Admin criar em nome de creator (opcional)**
  - Como **Admin**, quero **poder criar uma solicitação em nome de um creator indicado** (quando o fluxo for exposto ao Admin), para **suportar cenários operacionais ou de suporte**.

### Personas / papéis

- **Creator (persona principal)**
  - Usa o botão "Nova solicitação" em RF-1; preenche valor (e campos exigidos); submete e vê a solicitação na lista.
- **Admin (papel secundário)**
  - Pode usar o mesmo fluxo ou um derivado para criar em nome de um creator (conforme API: `creatorId` opcional no body); escopo pode ser limitado a um card futuro se a prioridade for apenas Creator.
- **Analista**
  - Não cria solicitações reais; não é persona deste card.

### Telas, módulos, relatórios e navegação

- **Módulo lógico de frontend**
  - Área/módulo de **Antecipação** (Creator) — mesma área de RF-1.

- **Telas impactadas**
  - **Minhas solicitações (RF-1)**: o botão "Nova solicitação" passa a navegar para uma **tela ou modal de criação** (rota dedicada ou overlay).
  - **Tela/modal "Nova solicitação"**:
    - Formulário com campo **valor solicitado** (obrigatório; unidade conforme API — ex.: valor em reais com duas casas decimais; backend espera `requestedAmount` em decimal).
    - Opcionalmente, para Admin: campo **creator** (identificador do creator para quem se está criando).
    - Botões "Criar solicitação" (ou "Submeter") e "Cancelar" (voltar sem salvar).
    - Validação: valor mínimo 100 (conforme RC-1); valor máximo e formato conforme backend/UI.
    - Após sucesso: mensagem de sucesso, redirecionamento para "Minhas solicitações" ou fechamento do modal e atualização da lista.

- **Navegação**
  - Creator → Minhas solicitações → "Nova solicitação" → formulário → preenche valor → Submeter → sucesso → volta à lista (ou modal fecha) e lista atualizada; ou erro → mensagem na tela.

### Permissões e segurança

- **Role `Creator`**
  - Pode acessar o fluxo de criação e criar **apenas para si mesmo** (o backend associa ao `creator_id` do token; o frontend não envia `creatorId` quando o usuário é Creator).
- **Role `Admin`**
  - Pode criar para si ou para um creator indicado, conforme contrato do endpoint (body com `creatorId` opcional); o backend valida permissões.
- **Autenticação**
  - O endpoint `POST /api/v1/anticipations` exige autenticação; o frontend usa o token existente.

### Fluxos de uso e regras de negócio (UI)

#### Fluxo 1 – Creator cria nova solicitação (happy path)

1. Creator está em "Minhas solicitações" e clica em "Nova solicitação".
2. Abre a tela ou modal do formulário de criação.
3. Preenche o valor solicitado (ex.: 1.000,00) respeitando o mínimo 100 e o máximo permitido (conforme backend).
4. Clica em "Criar solicitação" (ou "Submeter").
5. O frontend valida (valor >= 100, formato); se inválido, exibe mensagens junto aos campos e não envia.
6. Se válido, envia `POST /api/v1/anticipations` com body `{ "requestedAmount": 1000 }` (sem `creatorId` para Creator).
7. Backend retorna 201 com Id, Protocol, NetAmount, Status.
8. Frontend exibe mensagem de sucesso (ex.: "Solicitação criada com sucesso."), fecha o modal ou redireciona para a lista e recarrega a lista de solicitações para exibir a nova entrada.

#### Fluxo 2 – Erro: já existe solicitação em análise

1. Creator que já tem uma solicitação em estado Pending tenta criar outra.
2. Backend retorna 400 (ou 422) com mensagem do tipo "Já existe uma solicitação em análise para este creator."
3. Frontend exibe a mensagem retornada e, se disponível, código/ID para suporte; o formulário permanece aberto para ajuste ou cancelamento.

#### Fluxo 3 – Erro: valor inválido

1. Creator informa valor menor que 100 (ou fora dos limites).
2. Preferencialmente o frontend valida antes de enviar e exibe "Valor mínimo é 100" (ou equivalente).
3. Se a API retornar 400 por validação, exibir a mensagem do backend.

#### Regras de negócio (UI)

- Valor solicitado: **mínimo 100** (unidade alinhada ao backend — ex.: reais); máximo conforme política (backend/RC-1).
- Creator: não envia `creatorId`; backend usa o do token.
- Admin: pode enviar `creatorId` opcional se o formulário expuser esse campo.
- Uma única solicitação em análise por creator: o backend rejeita segunda criação; o frontend deve exibir claramente a mensagem de erro.

### Critérios de aceitação (testáveis – foco frontend)

- **CA-RF5-1 – Botão "Nova solicitação" leva ao formulário**
  - Dado que estou autenticado como Creator e estou na tela Minhas solicitações,  
  - Quando clico em "Nova solicitação",  
  - Então devo ser levado à tela ou modal de criação (formulário com campo de valor e ação de submeter).

- **CA-RF5-2 – Validação de valor mínimo no frontend**
  - Dado que estou no formulário de nova solicitação e informo valor menor que 100,  
  - Quando tento submeter (ou ao sair do campo),  
  - Então devo ver mensagem de validação (ex.: valor mínimo 100) e a requisição não deve ser enviada.

- **CA-RF5-3 – Submissão válida cria e atualiza lista**
  - Dado que estou como Creator, não tenho solicitação em análise, preencho valor >= 100 e clico em Criar,  
  - Quando a API retorna sucesso (201),  
  - Então devo ver mensagem de sucesso e a lista de "Minhas solicitações" deve incluir a nova solicitação (ou redirecionar para a lista com ela visível).

- **CA-RF5-4 – Erro "já existe solicitação em análise" exibido**
  - Dado que já tenho uma solicitação em análise e abro o formulário de nova solicitação,  
  - Quando preencho valor válido e submeto,  
  - Então a API pode retornar erro; a UI deve exibir a mensagem retornada pelo backend e, se disponível, código para suporte.

- **CA-RF5-5 – Cancelar volta sem criar**
  - Dado que estou no formulário de nova solicitação,  
  - Quando clico em Cancelar (ou equivalente),  
  - Então o formulário deve fechar ou voltar à lista sem enviar requisição e sem alterar a lista.

- **CA-RF5-6 – Erro de rede/API exibido**
  - Dado que submeto o formulário com dados válidos,  
  - Quando a API retorna erro (400, 403, 422, 500, etc.),  
  - Então a UI deve exibir mensagem de erro (incluindo texto do backend quando seguro) e código/ID para suporte quando disponível.

### Componentes de UI e comportamento

- **Formulário de criação**
  - Campo "Valor solicitado" (obrigatório): numérico, mínimo 100, formato monetário conforme locale (ex.: 1.000,00).
  - Opcional (Admin): campo "Creator" (seletor ou input) para indicar para quem se está criando.
  - Botão primário "Criar solicitação" (ou "Submeter"); desabilitado enquanto validação falhar ou durante o envio.
  - Botão secundário "Cancelar": fecha modal ou navega de volta à lista sem submeter.
- **Feedback**
  - Sucesso: toast ou mensagem inline "Solicitação criada com sucesso."; em seguida, lista atualizada ou redirecionamento.
  - Erro: área de alerta com mensagem e código de suporte; formulário permanece preenchido para nova tentativa ou correção.

### Requisitos técnicos/metodológicos aplicáveis (frontend Angular)

- **Referências de arquitetura Angular**
  - Aplicar `.cursor/rules/angular-frontend.mdc` e a skill `mestre-freire-angular`.
- **Camadas**
  - **Domain**: estender `AnticipationRequestsPort` com `createRequest(payload)` — payload com `requestedAmount` (valor numérico alinhado ao backend; ex.: em reais) e opcionalmente `creatorId` para Admin; tipo de retorno com id, protocol, status, netAmount (ou equivalente ao backend).
  - **Infrastructure**: implementar POST `.../api/v1/anticipations` com body `{ requestedAmount, creatorId? }`; mapear resposta para domínio.
  - **Application**: facade de "Minhas solicitações" (ou facade dedicado) expõe `createRequest(...)`; em sucesso, limpa erro, mostra feedback e recarrega lista (e fecha modal/volta); em erro, define mensagem apresentável.
  - **Features**: componente ou página "Nova solicitação" com formulário reativo (typed forms), validação mínima 100 e submissão; rota dedicada (ex.: `anticipation/my-requests/new`) ou modal; botão "Nova solicitação" em RF-1 ligado a `router.navigate` ou abertura do modal.
- **Metodologias**
  - TDD e spec-driven; critérios CA-RF5-x e esta seção são base para testes.
  - Não suprimir erros: em tratamento de erro, registrar o erro original e exibir mensagem de UI com contexto, motivo da API e código de suporte.

### Diretrizes de testes para o frontend

- **Unitários**
  - Port/facade: createRequest chama o port com payload correto; em sucesso atualiza lista e mensagem; em erro define mensagem.
  - Formulário: valor < 100 marca campo inválido; valor >= 100 permite submit; Cancelar não chama o port.
- **Integração**
  - Serviço HTTP: POST com body correto; mapeamento da resposta para modelo de domínio.
  - Facade + componente: após create bem-sucedido, lista contém a nova solicitação (ou estado atualizado).
- **E2E (opcional)**
  - Creator acessa Minhas solicitações, clica em Nova solicitação, preenche valor 500, submete e vê a nova solicitação na lista.

### Rastreabilidade

- **Backend**: RA-1 (endpoint `POST /api/v1/anticipations`), RC-1 (validação valor >= 100 e uma pendente por creator).
- **Frontend**: RF-5 mapeado em `docs/tracability.md` para port, HTTP service, facade, componente/página de criação, rota e testes.
- **Dependências**: RF-1 (Minhas solicitações e botão "Nova solicitação"); RA-1 e RC-1 (contratos e regras no backend).

### Dependências e riscos

- **Dependências**
  - RF-1 (tela Minhas solicitações e botão "Nova solicitação").
  - RA-1 (criação de solicitação), RC-1 (validações de valor e uma pendente por creator).
- **Riscos**
  - Unidade de valor (centavos vs reais): o backend usa decimal em reais; o frontend deve enviar no mesmo formato (ex.: 1000 para 1.000,00 reais) e exibir conforme locale; mitigação: documentar no card e no código a convenção adotada.
