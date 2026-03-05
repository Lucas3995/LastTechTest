## [RF-3] Tela de simulacao de solicitacao de antecipacao

### Contexto e objetivo de negócio

RA-4 define a funcionalidade de **simulação de solicitação de antecipação (fake, sem persistência)**, reutilizando as mesmas regras de cálculo e validação das solicitações reais (RA-1 + `InstrucoesProjeto`), com cache in-memory e possibilidade de conversão posterior em uma solicitação real.  

Do ponto de vista de experiência do usuário, falta uma **tela de frontend dedicada à simulação**, que:

- Permita ao Creator **experimentar cenários** (valores, prazos, combinações de recebíveis) sem criar registros definitivos.  
- Exiba de forma **clara e confiável** o resultado da simulação (valor líquido, taxas, datas) e a **validade temporal** daquele resultado.  
- Ofereça, quando aplicável, a opção de **converter a última simulação válida em uma solicitação real**, respeitando as regras de negócio (uma pendente por creator, ausência de efeitos persistentes na simulação, validade do código, etc.).  
- Permita a Admin/Analista simular em nome de creators (para suporte), sem criar dados reais.

### User stories de frontend

- **US-S1 – Simular antecipação como Creator**  
  - Como **Creator**, quero **simular uma solicitação de antecipação com os mesmos critérios da solicitação real**, para **entender o valor líquido esperado, taxas e prazos antes de tomar uma decisão**.

- **US-S2 – Ver resultado da simulação com validade clara**  
  - Como **Creator**, quero **ver o resultado da simulação com indicação clara de validade**, para **saber até quando posso confiar naqueles valores para converter em solicitação real**.

- **US-S3 – Converter simulação em solicitação real**  
  - Como **Creator**, quero **converter uma simulação válida em solicitação real**, para **formalizar o pedido sem precisar preencher novamente os dados**.

- **US-S4 – Simular em nome de outro usuário (Admin/Analista)**  
  - Como **Admin/Analista**, quero **simular uma solicitação em nome de um creator**, para **apoiá-lo em dúvidas e orientações sem gerar registros reais**.

### Personas / papéis

- **Creator**  
  - Usa simulação para testar cenários antes de criar solicitação real.  
  - Pode converter simulações em solicitações reais, respeitando as regras de RA-4.

- **Analista**  
  - Usa simulação para reproduzir cenários em nome de creators e apoiar atendimento.  
  - Não converte simulações em solicitações reais (decisão é do Creator/Admin).

- **Admin**  
  - Tem acesso a todas as ações de Creator e Analista, podendo simular e converter em nome de qualquer creator em cenários excepcionais.

### Telas, módulos, relatórios e navegação

- **Módulo lógico de frontend**
  - Área/módulo de **Antecipação** no frontend, em linha com as demais telas RF-1 e RF-2.  
  - Tela acessível a partir de:
    - `Minhas solicitações de antecipação` (RF-1) → ação “Simular antecipação”.  
    - `Home Admin` / `Lista global` (RF-2) → ação “Simular em nome do creator” (para Admin/Analista).

- **Tela principal deste card**
  - `Tela de simulacao de solicitacao de antecipacao`:
    - Página de feature (ex.: `features/anticipation/pages/simulation`).  
    - Composta por:
      - **Formulário de simulação**.  
      - **Painel de resultado** (após simulação).  
      - **Área de conversão** (quando simulação pode ser convertida).

### Permissões e segurança

- **Role `Creator`**
  - Pode simular para si mesmo; identificação de creator vem do token.  
  - Pode converter simulação em solicitação real apenas se:
    - Não houver outra solicitação em aberto.  
    - A simulação for válida e não expirada.  
    - As regras de criação RA-1 + `InstrucoesProjeto` estiverem satisfeitas.

- **Role `Analista`**
  - Pode simular em nome de qualquer creator (indicando explicitamente o creator-alvo).  
  - Não converte simulação em real (ações de conversão não são oferecidas para Analista).

- **Role `Admin`**
  - Pode simular e converter em nome de qualquer creator, sob as mesmas regras de negócio aplicáveis ao Creator (uma pendente, validade, etc.).

### Fluxos de uso e regras de negócio (UI)

#### Fluxo 1 – Simular como Creator

1. Creator chega à tela de simulação a partir de RF-1 (ou menu).  
2. Preenche o formulário com:
   - Valor desejado para antecipação.  
   - Seleção de contratos/recebíveis (se aplicável pela API).  
   - Datas ou outros parâmetros exigidos pela API.  
3. Ao submeter:
   - A tela mostra validações inline (valor mínimo, limites, elegibilidade).  
   - Se inválido, exibe mensagens claras, sem salvar nada.  
   - Se válido, exibe o **resultado da simulação** (painel de resultado) e salva a simulação em cache (transparente ao usuário).

#### Fluxo 2 – Ver resultado e validade

1. Após simulação bem-sucedida, o painel mostra:
   - Valor bruto, taxa aplicada, valor líquido.  
   - Datas relevantes (data de antecipação, vencimentos).  
   - Texto indicando:
     - Validade da simulação (hora até a qual o usuário deve considerar aqueles valores).  
     - Que a simulação não cria solicitação real.  
2. Opcionalmente, exibir um **identificador de simulação** (simulationCode) caso o design e a API assim prevejam.

#### Fluxo 3 – Converter simulação em solicitação real

1. A partir de uma simulação ainda válida (no painel de resultado), a tela oferece ação “Criar solicitação real com base nesta simulação”.  
2. Ao acionar:
   - UI confirma a intenção (diálogo de confirmação).  
   - Backend verifica regras de negócio (ausência de solicitação em aberto, regras RA-1 + `InstrucoesProjeto`, validade da simulação, etc.).  
3. Em caso de sucesso:
   - A tela informa que a solicitação real foi criada e oferece link para “Minhas solicitações” (RF-1).  
4. Em caso de falha:
   - UI exibe mensagens claras para cada cenário:
     - Simulação expirada.  
     - Já existe solicitação em aberto.  
     - Regras de negócio deixaram de ser satisfeitas.

#### Fluxo 4 – Simular em nome de outro usuário (Admin/Analista)

1. Admin/Analista chega à tela de simulação com contexto de creator-alvo (ou seleciona o creator na tela).  
2. Preenche parâmetros da simulação e submete.  
3. Recebe resultado semelhante ao do Creator, **sem** opção de conversão (para Analista; Admin pode ter).  
4. Pode compartilhar informações com o creator (por exemplo, via suporte).

### Critérios de aceitação (testáveis – foco frontend)

- **CA-RF3-1 – Simulação válida para Creator**  
  - Dado que estou autenticado como Creator em condições válidas para antecipação,  
  - Quando preencho os parâmetros de simulação corretamente e submeto,  
  - Então devo ver um painel com valores simulados e a validade exposta, sem criação de solicitação real.

- **CA-RF3-2 – Mensagens claras para simulação inválida**  
  - Dado que informo parâmetros que violam alguma regra de negócio (ex.: valor ≤ 100),  
  - Quando submeto a simulação,  
  - Então devo ver mensagens de erro claras junto aos campos relevantes e nenhuma solicitação real deve ser criada.

- **CA-RF3-3 – Ausência de efeitos persistentes na simulação**  
  - Dado que realizo uma ou mais simulações (Creator/Admin/Analista),  
  - Quando verifico minhas solicitações em RF-1 ou RF-2,  
  - Então não devo ver novas solicitações criadas enquanto não converter explicitamente (fluxo de conversão).

- **CA-RF3-4 – Conversão bem-sucedida a partir de simulação válida**  
  - Dado que existe uma simulação ainda válida (sem expirar, não utilizada, sem solicitação em aberto),  
  - Quando aciono “Criar solicitação real com base na simulação”,  
  - Então a UI deve indicar sucesso, e a nova solicitação deve surgir em “Minhas solicitações” com valores idênticos aos da simulação.

- **CA-RF3-5 – Conversão rejeitada (expirada, já utilizada, já existe pendente ou regras violadas)**  
  - Dado cada um dos cenários de rejeição previstos em RA-4 (expirada, já utilizada, já há pendente, regras deixaram de ser satisfeitas),  
  - Quando tento converter,  
  - Então devo receber mensagens específicas explicando o motivo e nenhuma solicitação real deve ser criada.

- **CA-RF3-6 – Simulação por Admin/Analista em nome de outro creator**  
  - Dado que estou autenticado como Admin ou Analista e escolho um creator-alvo,  
  - Quando efetuo uma simulação válida em nome desse creator,  
  - Então devo ver o resultado normalmente, identificar claramente em nome de quem a simulação foi feita e não criar solicitação real.

### Componentes de UI e comportamento

- **Formulário de simulação**
  - Campos:
    - Valor solicitado.  
    - Seleção de contratos/recebíveis (se aplicável, ex.: multi-select ou pick list).  
    - Datas/parametrizações adicionais exigidas pela API.  
    - Para Admin/Analista: campo para identificar o creator-alvo (autocomplete ou seleção).
  - Comportamento:
    - Validações inline (valor mínimo, formatos, obrigatoriedade).  
    - Botão “Simular” habilitado somente quando campos essenciais estiverem preenchidos.

- **Painel de resultado de simulação**
  - Conteúdo:
    - Valor bruto, taxa aplicada, valor líquido.  
    - Datas relevantes.  
    - Texto de validade (por exemplo: “Simulação válida até 11:40”).  
  - Comportamento:
    - Exibido apenas após uma simulação bem-sucedida.  
    - Atualizado a cada nova simulação (última simulação substitui anteriores).

- **Área de conversão em solicitação real**
  - Ação:
    - Botão “Criar solicitação real com base nesta simulação” para Creator (e Admin quando aplicável).  
  - Comportamento:
    - Desabilitado ou oculto quando não há simulação válida ou quando há solicitação em aberto.  
    - Diálogo de confirmação antes da conversão.  
    - Feedback de sucesso/falha alinhado a RA-4.

### Requisitos visuais, UX e acessibilidade (UX/UI)

- **Clareza e confiança**
  - A tela deve comunicar claramente que é uma **simulação**, não uma solicitação real.  
  - Resultados financeiros devem ser apresentados com destaque, utilizando tipografia e espaçamento que favoreçam leitura rápida e confiança.

- **Formulário amigável**
  - Campos com rótulos descritivos, placeholders úteis e textos de ajuda curtos.  
  - Mensagens de erro específicas (ex.: “O valor deve ser maior que R$ 100,00” em vez de códigos técnicos), reaproveitando quando possível as mensagens de domínio retornadas pelo backend (RA-4) e sempre oferecendo um **código/ID de suporte** associado ao erro para facilitar atendimento.

- **Acessibilidade**
  - Foco adequado ao abrir o painel de resultado e ao exibir mensagens de erro.  
  - Navegação via teclado plenamente funcional para campos e ações de simulação/conversão.  
  - Contraste adequado para textos e elementos-chave (especialmente valores e mensagens de validade).

### Requisitos técnicos/metodológicos aplicáveis (frontend Angular)

- **Referências de arquitetura Angular**
  - Aplicar a regra `.cursor/rules/angular-frontend.mdc` como critério técnico obrigatório para o frontend.
  - Usar a skill `mestre-freire-angular` como guia padrão para criação e evolução do código Angular desta tela.
- **Boas práticas da stack Angular**
  - Respeitar as camadas `domain`, `application`, `infrastructure`, `core`, `shared`, `features`, mantendo:
    - Página de simulação em `features` (por exemplo, `features/anticipation/pages/simulation`).  
    - Services/facades em `application` responsáveis por orquestrar chamadas de simulação e conversão, encapsulando regras de negócio de RA-4.  
    - Acessos HTTP e integrações em `infrastructure`, atrás de interfaces bem definidas.
  - Não injetar `HttpClient` diretamente em componentes de página; depender sempre de services/facades de `application` que, por sua vez, usam interfaces/implementações em `infrastructure`.
  - Utilizar **forms tipados**, padrões de estado com Signals/RxJS e boas práticas de a11y conforme descrito na regra Angular do projeto.
- **Metodologias de desenvolvimento**
  - Praticar **spec-driven development**, tratando este card como fonte de verdade de UI/UX/testes; implementações e testes devem ser rastreáveis aos IDs de critérios de aceitação (CA-RF3-x) e às seções de componentes descritas acima.
  - Praticar **TDD**, definindo testes de frontend (unitários, de integração e E2E) com base nos critérios de aceitação e na seção "Diretrizes de testes", implementando o código somente após os testes estarem especificados.
  - Integrar as skills `maestro` (planejamento técnico a partir do card), `mestre-freire-angular` (implementação/refino Angular) e `quadro-de-recompensas` (árvore de testes) no fluxo de trabalho.
  - Garantir que **nenhum bloco `try/catch` suprima silenciosamente erros** de simulação ou conversão: o frontend deve extrair do payload de erro dados como `message`, `code` e `traceId` (quando disponíveis) e usá-los para montar mensagens ricas ao usuário e ao suporte.

#### Artefatos Angular esperados

- Página de feature para "Simulação de solicitação de antecipação" na área/módulo de Antecipação (por exemplo, `features/anticipation/pages/simulation`).
- Componentes de UI:
  - Formulário de simulação (`AnticipationSimulationForm`).  
  - Painel de resultado (`AnticipationSimulationResultPanel`).  
  - Área/controles de conversão em solicitação real quando aplicável.
- Serviços/facades em `application` para:
  - Simular antecipação usando RA-4 (incluindo cache/validação).  
  - Converter simulação em solicitação real usando RA-1/RA-4, respeitando regras de "uma pendente" e validade.
- Uso ou criação, em `shared`, de pipes/directives para formatação monetária, de datas, de mensagens de validade e de estados de simulação/conversão.

### Diretrizes de testes para o frontend

- **Unitários**
  - Funções de formatação de valores simulação/resultado.  
  - Lógica de exibição de botões de conversão (habilitado/desabilitado).

- **Integração**
  - Interação entre formulário, serviço de simulação e painel de resultado (mock de backend RA-4).  
  - Cenários de erro de negócio (valor mínimo, limites).

- **E2E**
  - Cenário 1: Creator realiza simulação válida e vê resultado com validade.  
  - Cenário 2: Creator tenta simular com valor inválido e recebe erro adequado.  
  - Cenário 3: Creator converte uma simulação válida em solicitação real e depois encontra a solicitação em RF-1.  
  - Cenário 4: Creator tenta converter simulação expirada, já utilizada ou com pendente existente e recebe mensagens específicas.  
  - Cenário 5: Admin/Analista simula em nome de outro creator sem criar solicitação real.

### Spec para agentes de IA

- **Seções-chave**
  - User stories (US-S1–S4), critérios CA-RF3-x, componentes de UI e diretrizes de testes.  

- **Nomenclatura sugerida**
  - Página: `AnticipationSimulationPage`.  
  - Componentes internos: `AnticipationSimulationForm`, `AnticipationSimulationResultPanel`.  
  - Testes E2E: `anticipation-simulation.e2e.spec.ts`.

- **Uso por agentes**
  - Agentes devem usar este card como spec única para a experiência de simulação e conversão em frontend, derivando componentes, rotas e testes diretamente deste texto, vinculado às regras de RA-4.

### Dependências e riscos

- **Dependências**
  - RA-1, RA-4 e `InstrucoesProjeto` (regras de cálculo/validação, lógica de cache e conversão).  
  - RF-1 (integração com “Minhas solicitações”) e RF-2 (para fluxo de Admin/Analista).

- **Riscos**
  - Divergência entre UI e regras de backend sobre validade e conversão; mitigação: alinhar texto de validade com a lógica real de cache e revalidação.  
  - Confusão entre “simular” e “criar solicitação real”; mitigação: rótulos claros e seções visuais distintas.

### Rastreabilidade

- **Backend**: RA-1, RA-4, RC-1, `InstrucoesProjeto`.  
- **Frontend**: RF-3 mapeado em `docs/tracability.md` para componentes, rotas e testes de simulação/conversão, incluindo o fluxo de trabalho com skills `maestro`, `mestre-freire-angular`, `quadro-de-recompensas` e `clean-architecture-analysis` aplicado ao frontend.

- **Integração com skills e árvore de testes**
  - Este card serve de entrada direta para:
    - `maestro`: definição de casos de uso, rotas Angular, áreas e componentes de simulação/conversão a partir desta spec de frontend.  
    - `mestre-freire-angular`: implementação e evolução da página de simulação e serviços associados respeitando `.cursor/rules/angular-frontend.mdc` e as camadas definidas.  
    - `clean-architecture-analysis`: verificação posterior de que os novos artefatos de frontend de simulação/conversão respeitam as camadas e princípios de Arquitetura Limpa.
  - O conteúdo de "Diretrizes de testes" e dos critérios CA-RF3-x deve ser usado por `quadro-de-recompensas` para gerar uma árvore de testes completa (unitários, integração e E2E) sem precisar reinterpretar a demanda.

