## Demandas em Markdown

Esta pasta concentra **cards de demanda** em Markdown, usados para transformar o que o negócio pede (texto solto, reuniões, desafios técnicos) em itens claros e rastreáveis para o time de desenvolvimento.

Cada arquivo em `demandas/*.md` representa uma **demanda/unidade de valor** que pode entrar em um board (Kanban, Scrum, GitHub Projects etc.) e depois ser ligada ao código e aos testes.

---

### Estrutura padrão de um card

Cada card deve seguir, na medida do possível, a estrutura abaixo:

- **Título**
  - Formato sugerido: `[RX] Nome curto da demanda orientado a valor`  
    Exemplo: `[RA-1] Criar solicitacao de antecipacao`.

- **Contexto e objetivo de negócio**
  - Pequeno parágrafo explicando a dor / necessidade e o resultado desejado (métrica ou situação-alvo).

- **User story principal**
  - Forma padrão: `Como [persona/papel], quero [acao/resultado] para [beneficio].`
  - Quando fizer sentido, incluir stories adicionais relacionadas.

- **Personas / papéis afetados**
  - Lista de personas ou papéis (ex.: operador de risco, analista financeiro, suporte, TI interno) impactados pela demanda.

- **Telas, módulos, relatórios e navegação**
  - Quais telas novas ou existentes são afetadas (quando houver frontend).
  - Em qual módulo/menu do sistema interno se encaixa.
  - Relatórios, grids, dashboards ou gráficos necessários.
  - Navegação: de onde o usuário chega e para onde pode ir a partir desta funcionalidade.

- **Permissões e segurança**
  - Quem pode **ver**, **criar**, **editar**, **aprovar**, **recusar**, **cancelar** ou **exportar** dados ligados à demanda.
  - Requisitos de autenticação/autorização especiais (ex.: MFA obrigatório, escopo de API, isolamento por `creator_id`).

- **Fluxos de uso e regras de negócio**
  - Passo a passo do fluxo principal (happy path) em linguagem de processo.
  - Regras de negócio relevantes (validações, cálculos, limites, estados permitidos).
  - Variações importantes (ex.: erros de validação, conflitos de estado, cenários-limite).

- **Critérios de aceitação (testáveis)**
  - Lista de critérios verificáveis, de preferência em formato Given–When–Then.
  - Devem permitir que QA/devs criem testes automatizados sem reinterpretar a demanda.

- **Requisitos técnicos/metodológicos aplicáveis**
  - Não são cards separados, e sim **constraints deste card**:
    - Respeitar a arquitetura base (Clean Architecture, CQRS/MediatR, DDD).
    - Seguir a rotina-completa com as skills: implementação (`mercenario`), criação de testes (`quadro-de-recompensas`), análise de código (`batedor-de-codigos`), refatoração (`mestre-freire`).
    - Manter a pirâmide de testes coerente com a criticidade (unitários, integração, E2E).
    - Obedecer às ADRs do projeto (observabilidade, segurança, persistência etc.).

- **Rastreabilidade para código e testes**
  - Espaço para ser preenchido ao longo da execução:
    - Casos de uso / Commands / Queries.
    - Endpoints ou rotas HTTP.
    - Serviços de domínio / infraestrutura.
    - Testes unitários, de integração e E2E relacionados.

- **Dependências e riscos**
  - Outros cards dos quais depende ou que dependem deste.
  - Riscos de negócio, técnicos ou de UX conhecidos.

---

### Convenções e boas práticas ao gerar cards

Ao gerar ou alterar cards nesta pasta, seguir as orientações abaixo. Para processo e convenções detalhados, ver o rule `.cursor/rules/cards-demandas.mdc`.

- **Validação:** Sempre validar com o operador antes de prosseguir; um card por vez; propor o conteúdo do card formatado no corpo da resposta (não apenas em bloco markdown); aguardar validação; só depois criar o ficheiro.
- **Envolvimento:** Fazer mais perguntas (escopo, prioridades, personas); manter o operador parte do processo; oferecer escolhas e validar cada card antes do próximo.
- **Regras de negócio:** Regras da especificação do cliente são **aditivas** às já existentes no projeto.
- **Requisitos técnicos:** Todo card deve incluir na secção correspondente o checklist do projeto (ex.: .NET/C#, SOLID, TDD, DDD, pirâmide de testes).
- **Riscos:** Se um risco for mitigado como requisito explícito no card, removê-lo da lista de riscos.
- **Rastreabilidade:** Manter `docs/tracability.md` actualizado com requisito → casos de uso, endpoints, testes.

---

### Como usar estes cards no fluxo do projeto

1. **Tradutor**  
   - A partir do que o cliente descreve (como no arquivo `InstrucoesProjeto`), atuar como analista de requisitos/UX:
     - Entender objetivos de negócio, personas, jornadas e necessidades de informação.
     - Produzir ou atualizar cards em `demandas/` em linguagem de negócio (telas, módulos, fluxos, relatórios, permissões).

2. **Maestro**  
   - A partir de um card aprovado, mapear:
     - Quais casos de uso (Commands/Queries/Handlers) serão necessários.
     - Quais endpoints / rotas HTTP serão criados ou alterados.
     - Quais entidades, serviços de domínio e integrações são impactados.

3. **Quadro-de-recompensas**  
   - Usar os **critérios de aceitação** do card para criar/atualizar testes:
     - Unitários (domínio, serviços puros).
     - Integração (repositórios, handlers).
     - E2E (fluxos completos HTTP).

4. **Batedor-de-codigos** e **Mestre-freire**  
   - Após a implementação, analisar e refatorar o código mantendo a ligação entre:
     - Card de demanda → requisitos.
     - Casos de uso / endpoints → testes.

---

### Rastreabilidade com `docs/tracability.md`

O arquivo `docs/tracability.md` já mapeia os requisitos de **autenticação** (R1–R5) para casos de uso, endpoints e testes.

Para o domínio de **antecipação de valores**, a ideia é:

- Criar novos requisitos (por exemplo, `RA-1`, `RA-2`, …) representados por cards em `demandas/`.
- Após a implementação de cada card:
  - Atualizar `docs/tracability.md` adicionando uma seção para **Requisitos de antecipacao**, ligando:
    - IDs dos requisitos (ex.: `RA-1 Criar solicitacao de antecipacao`).
    - Casos de uso / handlers.
    - Endpoints HTTP.
    - Testes automatizados.
- Manter o ID do requisito (ex.: `RA-1`) visível tanto no card em `demandas/` quanto na tabela de rastreabilidade.

---

### Sincronização opcional com issues do GitHub

Opcionalmente, os cards em `demandas/` podem ser usados como fonte de verdade para criar issues no GitHub:

- Cada arquivo em `demandas/` contém título, descrição, contexto e critérios de aceitação.
- Um script em `scripts/` (por exemplo, `sync-demandas-issues.sh`) pode:
  - Ler os arquivos de `demandas/`.
  - Criar ou atualizar issues no repositório via `gh issue create`.
  - Aplicar labels e vincular a epics/boards conforme convenção do time.

O uso desse script deve ser documentado no próprio script e aqui no README (como referência de alto nível), sem substituir o fluxo normal de planejamento do time.

