## [RF-2] Tela Lista global de solicitacoes de antecipacao (Admin)

### Contexto e objetivo de negócio

Com os endpoints de criação e consulta de solicitações de antecipação já disponíveis (RA-1, RA-2, RA-3, RA-4 e RC-1), o time interno (Admin/operacional) precisa de uma **tela de frontend** que permita **monitorar o conjunto completo das solicitações**, filtrá-las por critérios relevantes (creator, status, período, valores) e acessar detalhes para suporte e análise operacional.

O objetivo desta demanda é criar a **tela “Lista global de solicitações de antecipação”** no frontend Angular, voltada a Admin (e possivelmente Analista em etapas futuras), com foco em:

- Prover uma visão **consolidada e filtrável** de todas as solicitações.  
- Apoiar **atendimento e operações internas**, permitindo localizar rapidamente solicitações por creator, status ou data.  
- Facilitar a navegação para o **detalhe de qualquer solicitação**.  
- Servir de base para, em cards futuros, incluir ações administrativas (aprovar/recusar, etc.).

### User stories de frontend

- **US-A1 – Listar solicitações globalmente**  
  - Como **Admin**, quero **ver a lista de todas as solicitações de antecipação do sistema**, para **monitorar uso, volumes e status**.

- **US-A2 – Detalhar solicitação de qualquer creator**  
  - Como **Admin**, quero **abrir os detalhes de qualquer solicitação**, para **investigar dúvidas e responder chamados**.

- **US-A3 – Filtrar por creator, status e período**  
  - Como **Admin**, quero **filtrar as solicitações por creator, status e período**, para **enxergar rapidamente subconjuntos relevantes**.

- **US-A4 – Preparar terreno para ações administrativas**  
  - Como **Admin**, quero que a lista global permita **identificar solicitações em estados específicos (por exemplo, em análise)**, para **posteriormente poder executar ações como aprovar, recusar ou outras**, mesmo que essas ações não sejam implementadas neste card.

### Personas / papéis

- **Admin (persona principal)**  
  - Colaborador interno com alta literacia digital, responsável por monitorar solicitações e apoiar áreas de negócio.  
- **Analista (papel futuro)**  
  - Em cards futuros, poderá utilizar a mesma tela ou uma derivada para fila de análise.
- **Creator (impacto indireto)**  
  - Não usa esta tela, mas se beneficia de um monitoramento mais eficiente.

### Telas, módulos, relatórios e navegação

- **Módulo lógico de frontend**
  - Área/módulo de **Antecipação** também serve ao Admin.  
  - A tela deve aparecer no **menu de operações internas**, com rótulo claro (“Solicitações de antecipação – Admin” ou similar).

- **Tela principal deste card**
  - `Tela Lista global de solicitacoes de antecipacao (Admin)`:
    - Página em nível de feature do módulo de Antecipação (ex.: `features/anticipation/pages/admin-list`).  
    - Integrada ao shell/layout existente para usuários internos.

- **Navegação**
  - `Home Admin` → `Lista global de solicitações de antecipação`.  
  - A partir desta tela:
    - Clique em linha → `Tela Detalhe da solicitação (Admin)` (pode compartilhar base com detalhe do Creator, mas com mais permissões e contexto).  
    - Em cards futuros: links para ações administrativas (aprovar/recusar, etc.) ou filas de análise.

### Permissões e segurança

- **Role `Admin`**
  - Tem acesso à tela de lista global.  
  - Pode visualizar solicitações de **qualquer creator**, respeitando políticas internas.  

- **Other roles**
  - `Creator` não deve ter acesso a esta tela; se tentar navegar, o sistema deve negar acesso conforme política.  
  - `Analista` poderá vir a usar esta tela ou uma derivação; este card não altera papéis de backend.

### Fluxos de uso e regras de negócio (UI)

#### Fluxo 1 – Visualizar lista global

1. Admin acessa o módulo de Antecipação e escolhe “Lista global de solicitações”.  
2. A tela mostra:
   - Cabeçalho com título e resumo curto (por exemplo, contagem total, últimos 7 dias).  
   - Barra de filtros (creator, status, período, etc.).  
   - Grid de solicitações com colunas ampliadas (incluindo creator).  
3. Por padrão, a lista pode carregar solicitações de um intervalo recente (ex.: últimos 7/30 dias) para evitar excesso de dados.

#### Fluxo 2 – Filtrar por creator, status e período

1. Admin utiliza:
   - Input de busca por creator (ID, nome de exibição ou ambos).  
   - Seleção de status (incluindo Em análise, Aprovada, Recusada, Cancelada pelo Creator).  
   - Seleção de período.  
2. Ao aplicar filtros:
   - A lista é atualizada.  
   - Indicadores visuais de filtros ativos permanecem visíveis.

#### Fluxo 3 – Ver detalhes de qualquer solicitação

1. Admin clica em uma linha da grid.  
2. A UI exibe o detalhe da solicitação, com informações análogas ao detalhe do Creator, mas sem restrição por `creator_id`.  
3. Informações adicionais (quando já existirem no backend):
   - Histórico de estados.  
   - Quem aprovou/recusou (para estados finais), etc.

### Critérios de aceitação (testáveis – foco frontend)

- **CA-RF2-1 – Lista global visível para Admin**
  - Dado que estou autenticado como Admin,  
  - Quando acesso a tela de lista global de solicitações,  
  - Então devo ver uma lista com solicitações de múltiplos creators, com colunas mínimas: creator, ID, datas, valores, status.

- **CA-RF2-2 – Bloqueio para Creator**
  - Dado que estou autenticado como Creator,  
  - Quando tento acessar diretamente a URL da lista global,  
  - Então devo receber negação de acesso (mensagem de autorização) e não visualizar dados globais.

- **CA-RF2-3 – Filtros por creator, status, período**
  - Dado que estou autenticado como Admin e tenho um conjunto variado de solicitações,  
  - Quando aplico filtros por creator, status ou período,  
  - Então a lista deve refletir apenas os registros que atendem aos filtros e os filtros ativos devem ficar claros na UI.

- **CA-RF2-4 – Acesso ao detalhe de qualquer solicitação**
  - Dado que estou autenticado como Admin,  
  - Quando clico em uma solicitação específica na lista,  
  - Então devo ver o detalhe completo, independentemente do creator.

- **CA-RF2-5 – Preparação para ações administrativas**
  - Dado que a tela está implementada,  
  - Quando observo solicitações em estado de análise,  
  - Então a UI deve deixá-las identificáveis (por exemplo, status destacado), de forma que seja trivial adicionar, em cards futuros, ações como aprovar/recusar diretamente a partir desta visão ou do detalhe.

### Componentes de UI e comportamento

- **Cabeçalho da página (Admin)**
  - Título: “Solicitações de antecipação – visão Admin” (ou frase similar).  
  - Subtítulo: breve explicação do propósito (monitorar solicitações de todos os creators).

- **Barra de filtros avançados**
  - Campos:
    - Busca por creator (campo de texto com autocomplete ou seletor).  
    - Status (multi-seleção).  
    - Período (intervalo de datas / presets).  
    - Opcional: filtros por valor mínimo/máximo.  
  - Comportamento:
    - Botões “Aplicar filtros” e “Limpar filtros”.  
    - Indicadores visuais de filtros ativos (chips/tag list).

- **Grid global de solicitações**
  - Colunas:
    - Creator (nome/identificador).  
    - ID da solicitação.  
    - Data da solicitação.  
    - Valor bruto, valor líquido.  
    - Status.  
  - Comportamento:
    - Ordenação por colunas principais.  
    - Paginação clara (número de páginas, total de registros).  
    - Clique em linha → detalhe.

- **Componente de detalhe (Admin)**
  - Pode reutilizar o componente de detalhe de RF-1, mas com:
    - Indicação do creator.  
    - Espaço para exibir históricos/ações administrativas futuras.

### Requisitos visuais, UX e acessibilidade (UX/UI)

- **Hierarquia visual**
  - Diferenciar claramente o contexto Admin de outras telas (título explícito).  
  - Filtros em área bem delimitada, com campos alinhados e legíveis mesmo em telas amplas.

- **Clareza em dados densos**
  - Linhas de tabela com espaçamento adequado e zebra-striping quando necessário.  
  - Alinhamento à direita para números, à esquerda para textos (creator, status).  
  - Uso de fonte monoespaçada para colunas de valores pode ser considerado para leitura mais rápida.

- **Mensagens e feedback**
  - Mensagens de “nenhum resultado encontrado” após aplicar filtros devem sugerir revisar os filtros.  
  - Erros de carregamento devem indicar possibilidade de recarregar ou ajustar parâmetros.

- **Acessibilidade**
  - Navegação via teclado incluindo filtros e grid.  
  - Foco visível ao alternar filtros e navegar nas linhas da tabela.  
  - Contraste adequado entre texto e fundo, especialmente em tags de status e cabeçalhos de coluna.

### Requisitos técnicos/metodológicos aplicáveis (frontend Angular)

- **Referências de arquitetura Angular**
  - Aplicar a regra `.cursor/rules/angular-frontend.mdc` como critério técnico obrigatório para o frontend.
  - Usar a skill `mestre-freire-angular` como guia padrão para criação e evolução do código Angular desta tela.
- **Boas práticas da stack Angular**
  - Respeitar as camadas `domain`, `application`, `infrastructure`, `core`, `shared`, `features`, mantendo:
    - Páginas de visão Admin em `features` (por exemplo, página de lista global de solicitações).  
    - Services/facades em `application`, responsáveis por orquestrar casos de uso de listagem e filtros globais.  
    - Acessos HTTP e integrações em `infrastructure`, atrás de interfaces bem definidas.
  - Não injetar `HttpClient` diretamente em componentes de página; depender sempre de services/facades de `application` que, por sua vez, usam interfaces/implementações em `infrastructure`.
  - Utilizar **forms tipados**, padrões de estado com Signals/RxJS e boas práticas de a11y conforme descrito na regra Angular do projeto.
- **Metodologias de desenvolvimento**
  - Praticar **spec-driven development**, tratando este card como fonte de verdade de UI/UX/testes; implementações e testes devem ser rastreáveis aos IDs de critérios de aceitação (CA-RF2-x) e às seções de componentes descritas acima.
  - Praticar **TDD**, definindo testes de frontend (unitários, de integração e E2E) com base nos critérios de aceitação e na seção "Diretrizes de testes", implementando o código somente após os testes estarem especificados.
  - Integrar as skills `maestro` (planejamento técnico a partir do card), `mestre-freire-angular` (implementação/refino Angular) e `quadro-de-recompensas` (árvore de testes) no fluxo de trabalho.

#### Artefatos Angular esperados

- Página de feature para "Lista global de solicitações de antecipação" na área/módulo de Antecipação voltada a Admin (por exemplo, `features/anticipation/pages/admin-list`).
- Componentes de UI:
  - Tabela global de solicitações com colunas ampliadas (incluindo creator).  
  - Componente de detalhe de solicitação para Admin (podendo reutilizar base do detalhe de RF-1, com campos adicionais).  
  - Componentes de filtros avançados (creator, status, período, valores, etc.).
- Serviço/facade em `application` para listagem global de solicitações, compartilhando modelos com RF-1 quando possível.
- Uso ou criação, em `shared`, de pipes/directives para formatação monetária, de data, de identificação de creator e de status, reaproveitando componentes já existentes.

### Diretrizes de testes para o frontend

- **Unitários**
  - Componentes responsáveis por formatar colunas (creator, status, valores).  
  - Lógica de construção de parâmetros de filtro antes da chamada de API.

- **Integração**
  - Interação entre filtros, grid e serviço de dados, com backend mockado.  
  - Verificação de que parâmetros corretos são enviados conforme filtros ativos.

- **E2E**
  - Cenário 1: Admin acessa a lista global, vê solicitações e aplica filtros por creator/status.  
  - Cenário 2: Admin abre o detalhe de uma solicitação qualquer.  
  - Cenário 3: Creator tenta acessar a URL da lista global e recebe negação de acesso.  

Testes devem ser alinhados com CA-RF2-x e escritos de forma que um agente de IA consiga derivá-los diretamente desta seção e dos critérios.

### Spec para agentes de IA

- **Seções que são spec principal**
  - User stories de frontend (US-A1–A4).  
  - Critérios CA-RF2-x.  
  - Componentes de UI + diretrizes de testes.  

- **Nomenclatura sugerida**
  - Página: `AnticipationAdminRequestsPage`.  
  - Grid: `AnticipationAdminRequestsTable`.  
  - Testes E2E: `anticipation-admin-requests.e2e.spec.ts`.

- **Uso recomendado**
  - Agentes devem ler este card como **fonte única de verdade de UI/UX Admin** para lista global, sem inferir requisitos além dos aqui descritos e dos cards RA/RC relacionados.

### Dependências e riscos

- **Dependências**
  - RA-1, RA-2, RA-3, RA-4, RC-1 (camadas de backend e contratos).  
  - RF-1 (pode compartilhar componentes de detalhe).  

- **Riscos**
  - Possível sobrecarga de dados caso a tela traga volume muito alto sem paginação/filtros adequados; mitigação: exigir filtros de período padrão e paginação eficiente.

### Rastreabilidade

- **Backend**: RA-1, RA-2, RA-3, RA-4, RC-1.  
- **Frontend**: RF-2 mapeado em `docs/tracability.md` para componentes/rotas e testes específicos de Admin, incluindo o fluxo de trabalho com skills `maestro`, `mestre-freire-angular`, `quadro-de-recompensas` e `clean-architecture-analysis` aplicado ao frontend.

- **Integração com skills e árvore de testes**
  - Este card serve de entrada direta para:
    - `maestro`: definição de casos de uso, rotas Angular, áreas e componentes Admin a partir desta spec de frontend.  
    - `mestre-freire-angular`: implementação e evolução das páginas/serviços de Admin respeitando `.cursor/rules/angular-frontend.mdc` e as camadas definidas.  
    - `clean-architecture-analysis`: verificação posterior de que os novos artefatos de frontend Admin respeitam as camadas e princípios de Arquitetura Limpa.
  - O conteúdo de "Diretrizes de testes" e dos critérios CA-RF2-x deve ser usado por `quadro-de-recompensas` para gerar uma árvore de testes completa (unitários, integração e E2E) sem precisar reinterpretar a demanda.

