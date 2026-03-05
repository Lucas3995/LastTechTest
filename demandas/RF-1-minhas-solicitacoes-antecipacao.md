## [RF-1] Tela Minhas solicitacoes de antecipacao (Creator)

### Contexto e objetivo de negócio

Creators da LastLink utilizam o serviço de antecipação de valores para melhorar fluxo de caixa. Após a implementação dos endpoints de criação e consulta de solicitações (RA-1, RA-2 e correção RC-1), falta uma **tela de frontend** que permita ao Creator **visualizar e acompanhar suas próprias solicitações de antecipação**, entender seus status, acessar detalhes e, quando ainda em análise, cancelar o pedido.  

O objetivo desta demanda é criar a **tela “Minhas solicitações de antecipação”** no frontend Angular existente, com foco em:

- Fornecer uma **visão consolidada e filtrável** das solicitações do Creator.  
- Permitir que o usuário **entenda rapidamente o estado atual de cada solicitação** (em análise, aprovada, recusada, cancelada).  
- Oferecer **ponto de partida claro** para acessar detalhes e fluxos de criação/simulação (ligação com futuras telas de RF-2/RF-3).  
- Possibilitar que o Creator **cancele uma solicitação em análise**, respeitando as regras de RA-3.  

### User stories de frontend

- **US-C1 – Listar minhas solicitações**  
  - Como **Creator**, quero **ver uma lista das minhas solicitações de antecipação**, para **acompanhar rapidamente valores, datas e status**.

- **US-C2 – Ver detalhes de uma solicitação**  
  - Como **Creator**, quero **abrir os detalhes de uma solicitação específica**, para **entender melhor valores, taxas, datas e histórico básico**.

- **US-C5 – Cancelar solicitação em análise**  
  - Como **Creator**, quero **cancelar uma solicitação minha que ainda esteja em análise**, para **evitar seguir com um pedido que não faz mais sentido**.

- **US-C6 – Acessar criação/simulação a partir da lista**  
  - Como **Creator**, quero **acessar facilmente as ações de criar nova solicitação e simular antecipação a partir da tela de lista**, para **não precisar “caçar” essas funcionalidades no sistema**.

### Personas / papéis

- **Creator (persona principal)**  
  - Usuário final da plataforma, com literacia digital média, que precisa acompanhar e gerenciar suas solicitações de antecipação.
- **Admin (papel secundário, impacto indireto)**  
  - Não utiliza esta tela diretamente, mas se beneficia de uma experiência melhor do Creator (menos erros, menos chamados de suporte).  
  - Admin terá sua própria tela em RF-2.

### Telas, módulos, relatórios e navegação

- **Módulo lógico de frontend**
  - Área/módulo de **Antecipação** dentro do sistema interno Angular.  
  - Esta tela deve ser acessível a partir do **menu principal** ou área de “Financeiro / Antecipações” existente ou a ser criada.

- **Tela principal deste card**
  - `Tela Minhas solicitacoes de antecipacao (Creator)`:
    - Página em nível de **feature** (ex.: `features/anticipation/pages/my-requests`) conforme convenções Angular do projeto.
    - Integrada ao `shell` existente (layout comum da aplicação interna).

- **Navegação**
  - A partir do menu/shell:
    - `Home Creator` → `Minhas solicitações de antecipação`.
  - A partir desta tela:
    - Clique em linha de grid → `Tela Detalhe da solicitação (Creator)` (pode ser modal, drawer ou página dedicada, mas faz parte do escopo deste card).
    - Ações principais:
      - Botão **“Nova solicitação”** → leva ao fluxo de criação (tela/screen de um card futuro ou reaproveitando RF-3 + RA-1).  
      - Botão **“Simular antecipação”** → leva à tela de simulação (RF-3).

### Permissões e segurança

- **Role `Creator`**
  - Tem acesso à tela “Minhas solicitações de antecipação”.
  - Só pode ver e interagir com **solicitações atreladas ao seu próprio `creator_id`**.
  - Pode acionar cancelamento apenas para solicitações próprias em estado de análise, conforme RA-3.

- **Role `Admin` / outros**
  - Esta tela não é destinada a Admin; Admin terá tela global (RF-2).  
  - Se por algum motivo um Admin navegar até aqui (ex.: permissão ampla), os dados exibidos devem continuar filtrados por `creator_id` do contexto, ou a navegação deve ser restringida conforme política de backend.

### Fluxos de uso e regras de negócio (UI)

#### Fluxo 1 – Ver lista de solicitações (Creator)

1. Creator acessa a tela “Minhas solicitações de antecipação” a partir do menu.  
2. A tela exibe:
   - **Barra de título** com nome da tela e breve descrição.  
   - **Barra de ações principais** com botões “Nova solicitação” e “Simular antecipação”.  
   - **Área de filtros** (período, status; opcionalmente texto livre).  
   - **Grid de solicitações**, inicialmente carregado com as solicitações do creator para um período padrão (ex.: últimos 90 dias).  
3. O usuário pode:
   - Ajustar filtros (status, período) e aplicar.  
   - Ordenar por colunas (data, valor, status).  
4. Estado vazio:
   - Se não houver solicitações, a tela exibe mensagem clara (“Você ainda não tem solicitações de antecipação”) e sugere iniciar pelo botão “Nova solicitação” ou “Simular antecipação”.

#### Fluxo 2 – Ver detalhes de uma solicitação

1. Na grid, o Creator clica em uma linha (ou botão de “Ver detalhes”).  
2. A UI abre o detalhe da solicitação (página ou painel modal/drawer):  
   - Mostra valores, taxas, status, datas e informações mínimas de auditoria (quem criou, quando).  
3. Se a solicitação não pertencer ao Creator (cenário de erro ou tentativa maliciosa), a UI deve:
   - Exibir mensagem de acesso negado, ou  
   - Não exibir a solicitação (conforme resposta da API).

#### Fluxo 3 – Cancelar solicitação em análise

1. No detalhe de uma solicitação em estado de análise (`ANALISE_PENDENTE` / `Created`/`Pending` conforme RA-3/RC-1), a tela mostra ação “Cancelar solicitação”.  
2. Ao acionar:
   - É exibido diálogo de confirmação com mensagem clara sobre a irreversibilidade do cancelamento.  
3. Em caso de sucesso:
   - Status muda visualmente para “Cancelada pelo creator”.  
   - A lista é atualizada.  
4. Tentativas repetidas:
   - Se o usuário tentar cancelar novamente uma solicitação já cancelada, a UI exibe mensagem de que a solicitação já está cancelada, sem alterar o estado.

### Critérios de aceitação (testáveis – foco frontend)

- **CA-RF1-1 – Lista restrita ao próprio Creator**
  - Dado que estou autenticado como Creator,  
  - Quando acesso a tela “Minhas solicitações de antecipação”,  
  - Então só devo ver solicitações vinculadas ao meu `creator_id`, com colunas mínimas (ID, datas, valores, status).

- **CA-RF1-2 – Lista vazia com orientação**
  - Dado que não tenho solicitações de antecipação,  
  - Quando acesso a tela,  
  - Então devo ver uma mensagem explicando que ainda não tenho solicitações e CTAs claros para “Nova solicitação” e “Simular antecipação”.

- **CA-RF1-3 – Ordenação e filtros visíveis**
  - Dado que tenho múltiplas solicitações,  
  - Quando aplico filtros (por status, período) e/ou ordeno colunas,  
  - Então a tela deve refletir os filtros/ordenação aplicados de forma clara (indicadores visuais em filtros e cabeçalhos de coluna).

- **CA-RF1-4 – Acesso ao detalhe**
  - Dado que vejo uma solicitação na lista,  
  - Quando clico para ver detalhes,  
  - Então devo ver uma tela/painel com informações completas da solicitação (campos principais de RA-1/RA-2) e, se a solicitação não for minha, receber feedback adequado de acesso negado.

- **CA-RF1-5 – Cancelamento de solicitação em análise**
  - Dado que existe uma solicitação minha em estado de análise,  
  - Quando acesso o detalhe e aciono “Cancelar solicitação” confirmando a ação,  
  - Então o status exibido deve mudar para “Cancelada pelo creator” e a lista deve refletir essa mudança.

- **CA-RF1-6 – Tratamento de cancelamentos repetidos**
  - Dado que uma solicitação já está cancelada pelo Creator,  
  - Quando tento cancelar novamente,  
  - Então devo ver mensagem clara indicando que a solicitação já foi cancelada anteriormente e o status não deve mudar.

### Componentes de UI e comportamento

- **Componente: Cabeçalho da página**
  - Conteúdo:
    - Título: “Minhas solicitações de antecipação”.  
    - Subtítulo curto contextualizando objetivo da tela.

- **Componente: Barra de ações principais**
  - Botões:
    - “Nova solicitação” → inicia fluxo de criação real (RF futuro / integração RA-1).  
    - “Simular antecipação” → navega para tela de simulação (RF-3).
  - Comportamento:
    - Botões sempre visíveis, alinhados ao lado direito do cabeçalho ou abaixo, seguindo convenções de design do projeto.

- **Componente: Filtros**
  - Campos típicos:
    - Período (data inicial/final ou presets como “Últimos 30/90 dias”).  
    - Status (multi seleção: Em análise, Aprovada, Recusada, Cancelada pelo Creator).  
  - Comportamento:
    - Aplicação explícita (botão “Aplicar filtros”) para evitar requisições desnecessárias.  
    - Indicadores de filtros ativos visíveis (chips ou texto).

- **Componente: Grid de solicitações**
  - Colunas mínimas:
    - Identificador (ID ou número de protocolo).  
    - Data da solicitação.  
    - Valor bruto solicitado.  
    - Valor líquido estimado.  
    - Status (tag com rótulo + cor).  
  - Comportamento:
    - Ordenação por colunas com indicação visual (setas).  
    - Paginação ou scroll com indicação de quantos registros estão sendo exibidos.  
    - Clique em linha abre detalhe.

- **Componente: Detalhe da solicitação**
  - Conteúdo:
    - Seções para informações financeiras, status e auditoria básica.  
    - Ação “Cancelar solicitação” quando aplicável.
  - Comportamento:
    - Diálogo de confirmação para cancelamento.  
    - Mensagens de erro/sucesso alinhadas com padrão de notificações do app.

### Requisitos visuais, UX e acessibilidade (UX/UI)

- **Hierarquia visual**
  - Título da tela com maior destaque, seguido de subtítulo explicativo.  
  - Grid em destaque no centro da tela, filtros em área claramente demarcada acima.  
  - Status visualmente diferenciados com **tags coloridas**, sempre com texto legível (por exemplo: “Em análise”, “Aprovada”).

- **Consistência**
  - Mesmos estilos de botões, tags de status e padrões de grid utilizados em outras áreas do sistema.  
  - Linguagem de textos consistente com RA-1 a RA-4 (mesmos termos para estados e ações).

- **Mensagens**
  - Evitar jargões técnicos; explicar regras de negócio (valor mínimo, uma pendente) com textos simples em tooltips ou mensagens contextuais, quando necessário.  
  - Mensagens de erro devem mencionar o que o usuário pode fazer em seguida (ex.: “Tente novamente mais tarde”, “Recarregue a página”).  
  - Quando houver erro de backend, a mensagem apresentada ao usuário deve combinar **contexto** (o que estava sendo feito), **motivo de negócio** (mensagem segura retornada pela API, quando existir) e um **código/ID de suporte** (por exemplo, `traceId` ou `code` retornado pelo backend), para facilitar atendimento.

- **Acessibilidade**
  - Navegação completa via teclado:  
    - Ordem de foco lógica (cabeçalho → ações principais → filtros → grid).  
    - Foco visível em todos os elementos interativos.  
  - Cores:
    - Status diferenciados por cor, mas sempre com texto; contraste adequado para fundos e textos.  
  - Estrutura semântica:
    - Uso de landmarks (região de conteúdo principal, navegação) de acordo com convenções do projeto para facilitar leitores de tela.

### Requisitos técnicos/metodológicos aplicáveis (frontend Angular)

- **Referências de arquitetura Angular**
  - Aplicar a regra `.cursor/rules/angular-frontend.mdc` como critério técnico obrigatório para o frontend.
  - Usar a skill `mestre-freire-angular` como guia padrão para criação e evolução do código Angular desta tela.
- **Boas práticas da stack Angular**
  - Respeitar as camadas `domain`, `application`, `infrastructure`, `core`, `shared`, `features`, mantendo:
    - Páginas em `features` (por exemplo, página de "Minhas solicitações de antecipação").  
    - Services/facades em `application`, responsáveis por orquestrar casos de uso de listagem, filtros e cancelamento.  
    - Acessos HTTP e integrações em `infrastructure`, atrás de interfaces bem definidas.
  - Não injetar `HttpClient` diretamente em componentes de página; depender sempre de services/facades de `application` que, por sua vez, usam interfaces/implementações em `infrastructure`.
  - Utilizar **forms tipados**, padrões de estado com Signals/RxJS e boas práticas de a11y conforme descrito na regra Angular do projeto.
- **Metodologias de desenvolvimento**
  - Praticar **spec-driven development**, tratando este card como fonte de verdade de UI/UX/testes; implementações e testes devem ser rastreáveis aos IDs de critérios de aceitação (CA-RF1-x) e às seções de componentes descritas acima.
  - Praticar **TDD**, definindo testes de frontend (unitários, de integração e E2E) com base nos critérios de aceitação e na seção "Diretrizes de testes", implementando o código somente após os testes estarem especificados.
  - Integrar as skills `maestro` (planejamento técnico a partir do card), `mestre-freire-angular` (implementação/refino Angular) e `quadro-de-recompensas` (árvore de testes) no fluxo de trabalho.
  - Tratar erros de forma a **não suprimir exceções em blocos `try/catch`**: o frontend deve sempre registrar o erro bruto (para debug) e expor ao usuário mensagens derivadas do payload de erro do backend (contexto + motivo seguro + código/ID para suporte), evitando mensagens genéricas isoladas.

#### Artefatos Angular esperados

- Página de feature para "Minhas solicitações de antecipação" na área/módulo de Antecipação (por exemplo, `features/anticipation/pages/my-requests`).
- Componentes de UI:
  - Tabela de solicitações do Creator (grid principal da tela).  
  - Componente de detalhe da solicitação (página, modal ou drawer, conforme diretrizes de design).  
  - Componentes de filtros (período, status, outros campos necessários).
- Serviço/facade em `application` responsável por listar, filtrar e cancelar solicitações do Creator, consumindo os contratos de RA-1, RA-2 e RA-3.
- Uso ou criação, em `shared`, de pipes/directives para formatação monetária, de data e de status, reaproveitando o que já existir sempre que possível.

### Diretrizes de testes para o frontend

Para cada critério de aceitação, planejar uma **árvore de testes** cobrindo:

- **Testes unitários (frontend)**  
  - Formatação de dados (por exemplo, componentes que formatam valores monetários e status).  
  - Lógica de exibição de mensagens de estado (vazia, erro).

- **Testes de integração (frontend + backend fake ou contratos)**  
  - Interação entre componentes de filtro, grid e serviços de dados (mock de API).  
  - Confirmação de que filtros e ordenação refletem corretamente os resultados exibidos.

- **Testes E2E (Playwright ou equivalente)**  
  - Cenário 1: Creator com lista não vazia acessa a tela, filtra por status, abre detalhes.  
  - Cenário 2: Creator sem solicitações acessa a tela e vê mensagem de vazio + CTA.  
  - Cenário 3: Creator cancela uma solicitação em análise e vê status atualizado.  
  - Cenário 4: Creator tenta cancelar novamente uma solicitação já cancelada e recebe mensagem apropriada.  

Os testes devem ser derivados direta e explicitamente dos critérios CA-RF1-x e desta seção, permitindo que um agente de IA (via skill `quadro-de-recompensas`) consiga gerar a suíte com mínima ambiguidade.

### Spec para agentes de IA

- **Seções-chave para spec-driven development**
  - User stories de frontend (base para funcionalidades).  
  - Critérios de aceitação (CA-RF1-x) – fonte de verdade para comportamentos esperados.  
  - Componentes de UI e comportamento – guia para estrutura de tela e estados.  
  - Diretrizes de testes – definição de tipos e cenários de testes.  

- **Padrões de nomenclatura sugeridos**
  - Componentes de página: algo como `AnticipationMyRequestsPage` (ou equivalente no padrão do projeto).  
  - Componentes de grid: `AnticipationRequestsTable`.  
  - Arquivos de teste E2E: `anticipation-my-requests.e2e.spec.ts` (seguindo convenções atuais).  

- **Uso por agentes**
  - Agentes devem:
    - Ler user stories + critérios de aceitação para entender o que implementar.  
    - Usar a seção de componentes de UI como blueprint para criar os componentes Angular.  
    - Usar a seção de testes para gerar árvore de testes (unit, integração, E2E) alinhada a CA-RF1-x.  

### Dependências e riscos

- **Dependências**
  - RA-1, RA-2, RA-3, RC-1 (backend de criação, consulta, estados, validações de valor mínimo e “uma pendente”).  
  - RF-3 (simulação) para ação “Simular antecipação” (integração de navegação).

- **Riscos**
  - Risco de inconsistência entre termos usados na UI e os estados do backend; mitigação: alinhar termos de status com RA-3 e docs de backend.  
  - Risco de sobrecarga de informação na tela (muitos campos na grid); mitigação: priorizar colunas essenciais e usar detalhe para informações completas.

### Rastreabilidade

- **Requisitos backend relacionados**
  - RA-1, RA-2, RA-3, RC-1.  
- **Futuros mapeamentos em `docs/tracability.md`**
  - RF-1 → rotas Angular de “Minhas solicitações de antecipação”.  
  - RF-1 → componentes de página/grid/detalhe.  
  - RF-1 → testes unitários, de integração e E2E de frontend correspondentes.
  - RF-1 → fluxo de trabalho com skills `maestro`, `mestre-freire-angular`, `quadro-de-recompensas` e `clean-architecture-analysis` aplicado ao frontend.

- **Integração com skills e árvore de testes**
  - Este card serve de entrada direta para:
    - `maestro`: definição de casos de uso, rotas Angular, áreas e componentes a partir desta spec de frontend.  
    - `mestre-freire-angular`: implementação e evolução das páginas/serviços respeitando `.cursor/rules/angular-frontend.mdc` e as camadas definidas.  
    - `clean-architecture-analysis`: verificação posterior de que os novos artefatos de frontend respeitam as camadas e princípios de Arquitetura Limpa.
  - O conteúdo de "Diretrizes de testes" e dos critérios CA-RF1-x deve ser usado por `quadro-de-recompensas` para gerar uma árvore de testes completa (unitários, integração e E2E) sem precisar reinterpretar a demanda.

