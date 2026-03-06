# Mapeamento: Cursor Skills/Commands/Rules → GitHub Copilot

## Visão Geral

Este documento mapeia os **8 skills**, **4 commands** e **5 rules** do seu setup Cursor para estruturas equivalentes no GitHub Copilot, preservando a metodologia **spec-driven development** e a **rotina-completa**.

### Estrutura Proposta para Copilot

```
.copilot/
├── README.md                          (este documento)
├── instructions/                      (substituem rules + metodologia)
│   ├── 0-metodologia-completa.md
│   ├── 1-spec-driven-development.md
│   ├── 2-frontend-angular.md
│   ├── 3-cards-demandas.md
│   └── 4-planos-todos.md
├── prompt-templates/                  (substituem commands + skills)
│   ├── tradutor.md
│   ├── maestro.md
│   ├── quadro-de-recompensas.md
│   ├── mercenario.md
│   ├── batedor-de-codigos.md
│   ├── mestre-freire.md
│   ├── mestre-freire-angular.md
│   ├── arauto.md
│   ├── criar-fonte-de-verdade.md
│   ├── criar-plano-arvore-testes.md
│   ├── criar-fonte-verdade-codigo-producao.md
│   └── criar-fonte-de-verdade-refactoring.md
├── scripts/                           (scripts reutilizáveis)
│   └── arauto.sh (copiar de .cursor)
└── contexto-projeto.md                (escopo inicial + contexto)
```

---

## MAPEAMENTO DETALHADO

### PART I: RULES → INSTRUCTIONS

<details>
<summary><strong>Rule 1: metodologia-para-devs.mdc</strong></summary>

#### Equivalente Copilot: `.copilot/instructions/0-metodologia-completa.md`

**Como usar:**
1. Referenciar este arquivo quando iniciar um novo trabalho (demanda, refactoring, etc.)
2. Colar no chat do Copilot como contexto antes de invocar skills/prompts
3. Ou configurar como "Custom Instructions" no VS Code (se a extensão suportar)

**Conteúdo:** Copiar na íntegra de `.cursor/rules/metodologia-para-devs.mdc`, com adição de seção:

```markdown
## Como aplicar no Copilot

Quando solicitar ajuda ao Copilot, agrupe a metodologia com o contexto:

1. **Cole este documento** no chat (ou referencie com @file .copilot/instructions/0-metodologia-completa.md)
2. **Descreva o trabalho:** demanda, refactoring, análise, testes, entrega
3. **Indique a skill/prompt correspondente:** "Use o [Tradutor](#) para...", "Use o [Maestro](#) para...", etc.
4. **Valide com perguntas:** Copilot fará mais perguntas conforme a metodologia; participe ativamente

Exemplo:
```
Aqui está a metodologia que usamos: [colar .copilot/instructions/0-metodologia-completa.md]

Preciso implementar a demanda RF-1. 

Siga a rotina-completa:
1. Tradutor: entenda a demanda em UX/usabilidade
2. Maestro: gere relatório de alterações
3. Quadro-de-recompensas: crie testes
4. Valide comigo antes de seguir
```

</details>

<details>
<summary><strong>Rule 2: angular-frontend.mdc</strong></summary>

#### Equivalente Copilot: `.copilot/instructions/2-frontend-angular.md`

**Como usar:**
1. Referenciar quando criar/refatorar código Angular
2. Colar no chat antes de implementar mudanças no frontend
3. Usar em conjunto com **mestre-freire-angular.md** (prompt-template)

**Conteúdo:** Copiar de `.cursor/rules/angular-frontend.mdc`, com adição:

```markdown
## Forma de usar no Copilot

Ao iniciar qualquer trabalho no frontend Angular, inclua este documento:

```
Contexto técnico do frontend Angular: [colar instruções]

Vou fazer [criar componente | refatorar | evoluir funcionalidade] em [path/do/escopo].
Considere:
- Camadas (domain, application, infrastructure, core, shared, features)
- Estrutura modular: áreas, módulos, páginas, componentes
- Convenções: templateUrl, styleUrl, standalone, lazy loading
- DIP, a11y, Signals/RxJS, lifecycle, typed forms
- Central de ações (Command) para requisições ao backend
```

Use o prompt-template **mestre-freire-angular.md** para refatoração guiada.
```

</details>

<details>
<summary><strong>Rule 3: cards-demandas.mdc</strong></summary>

#### Equivalente Copilot: `.copilot/instructions/3-cards-demandas.md`

**Como usar:**
1. Cole este arquivo quando precisar gerar cards de demanda
2. Siga o processo iterativo de validação

**Conteúdo:** Copiar de `.cursor/rules/cards-demandas.mdc`, com adição:

```markdown
## Processo no Copilot

1. **Apresentação:** Copilot sugere o card (formatado em Markdown no corpo da resposta)
2. **Validação:** Você valida, sugere mudanças, aprova ou pede ajustes
3. **Iteração:** Copilot atualiza conforme sua preferência
4. **Criação:** Somente após validação final, criamos o arquivo em `demandas/`

Exemplo de diálogo:
```
Você: "Gere um card para a demanda de autenticação com OAuth"
Copilot: [sugere card em Markdown formatado no corpo]
Você: "Remova a seção de tecnologia XYZ, ela está fora de escopo"
Copilot: [atualiza] "Aqui está a versão revisada..."
Você: "Perfeito, crie o arquivo em demandas/"
Copilot: [cria demandas/RA-X-autenticacao-oauth.md]
```

</details>

<details>
<summary><strong>Rule 4: planos-todos.mdc</strong></summary>

#### Equivalente Copilot: `.copilot/instructions/4-planos-todos.md`

**Como usar:**
1. Referenciar quando Copilot gerar ou atualizar planos (`.plan.md`)
2. Garantir que a seção `todos:` está sempre preenchida

**Conteúdo:** Copiar de `.cursor/rules/planos-todos.mdc`

</details>

<details>
<summary><strong>Rule 5: poupar-creditos-com-skills.mdc</strong></summary>

#### Equivalente Copilot: Incorporado em `.copilot/instructions/0-metodologia-completa.md`

**Como usar:**
- Quando uma sequência de ações for repetível, solicite ao Copilot que crie um script reutilizável
- Exemplo: "Crie um script em `.copilot/scripts/` que faça [ação X, Y, Z]"
- Use o script depois para poupar créditos

**Aplicação:** O único script essencial é `arauto.sh` (já existe em `.cursor/skills/arauto/scripts/`); pode ser copiado para `.copilot/scripts/arauto.sh`

</details>

---

### PART II: SKILLS → PROMPT-TEMPLATES

Cada skill Cursor torna-se um **prompt-template** no Copilot. Usa-se colando o template no chat quando necessário.

<details>
<summary><strong>Skill 1: Tradutor</strong></summary>

#### Arquivo: `.copilot/prompt-templates/tradutor.md`

**Propósito:** Análise de requisitos e UX antes de código

**Como usar:**
```markdown
# Tradutor — Análise de Requisitos e UX

[Cole o conteúdo de .cursor/skills/tradutor/SKILL.md]

## Início do trabalho

Você: "[Cole o template acima no chat]

Preciso entender a demanda [nome/descrição].
[Descrição breve ou link para arquivo de demanda]"

Copilot: [Faz perguntas guidadas, cria síntese de contexto, usuários, cenários]

Resultado esperado: Documento com síntese de usabilidade, personas, tarefas, necessidades de informação — **sem falar de código**.
```

**Checklist:**
- [ ] Contexto de negócio entendido
- [ ] Usuários e personas mapeadas
- [ ] Tarefas e fluxos descritos
- [ ] Necessidades de informação claras
- [ ] Documento pronto para próxima etapa (Maestro)

</details>

<details>
<summary><strong>Skill 2: Maestro</strong></summary>

#### Arquivo: `.copilot/prompt-templates/maestro.md`

**Propósito:** Transformar demanda em relatório de alterações no código

**Como usar:**
```markdown
# Maestro — Análise de Demanda e Relatório de Alterações

[Cole o conteúdo de .cursor/skills/maestro/SKILL.md]

## Início do trabalho

Você: "[Cole o template acima no chat]

Demanda: [RA-1, RF-2, etc.]
Resultado do Tradutor (se aplicável): [link ou cole síntese]
Estado do código: [breve resumo ou link para docs/tracability.md]"

Copilot: [Analisa, propõe alterações, cria relatório estruturado]

Resultado: Relatório com ID, Tipo (Criar|Alterar|Remover|Integrar), Descrição, Requisito atendido.
```

**Checklist:**
- [ ] Demanda compreendida (com resultado do Tradutor)
- [ ] Código analisado
- [ ] Alterações mapeadas com IDs
- [ ] Dependências entre itens identificadas
- [ ] Relatório pronto para Quadro-de-Recompensas ou Mercenário

</details>

<details>
<summary><strong>Skill 3: Quadro de Recompensas</strong></summary>

#### Arquivo: `.copilot/prompt-templates/quadro-de-recompensas.md`

**Propósito:** Criar testes a partir de relatório de tarefas

**Como usar:**
```markdown
# Quadro de Recompensas — Criação de Testes a partir de Relatório

[Cole o conteúdo de .cursor/skills/quadro-de-recompensas/SKILL.md]

## Início do trabalho

Você: "[Cole o template acima no chat]

Relatório de tarefas (do Maestro):
[Cole ou referencie demandas/RA-X-*.md ou relatório gerado]

Foco: [backend | frontend | ambos]"

Copilot: [Mapeia tarefas a testes, cria estrutura de testes]

Resultado: Arquivos *.spec.ts ou *Tests.cs com cobertura completa.
```

**Interação:**
```
Você: "Crie testes para a demanda RF-1"
      [colar relatório do maestro]
      
Copilot: "Identifiquei N itens de alteração. Vou criar testes em:
         - src/app/features/anticipations/pages/request-list/request-list.component.spec.ts
         - src/app/features/anticipations/components/.../..."
         [mostra estrutura]

Você: "Perfeito, crie os arquivos" (ou "Ajuste [detalhe]")

Copilot: [Executa npm run test ou ng test no final]
```

**Checklist:**
- [ ] Relatório de tarefas fornecido (do Maestro)
- [ ] Testes mapeados (um ou mais por item)
- [ ] Casos de teste derivados (happy path, edge cases)
- [ ] Testes criados em arquivos .spec.ts ou *Tests.cs
- [ ] Suíte executada e verde

</details>

<details>
<summary><strong>Skill 4: Mercenário</strong></summary>

#### Arquivo: `.copilot/prompt-templates/mercenario.md`

**Propósito:** Traduzir regras de negócio em código

**Como usar:**
```markdown
# Mercenário — Tradução de Regras de Negócio para Código

[Cole o conteúdo de .cursor/skills/mercenario/SKILL.md]

## Início do trabalho

Você: "[Cole o template acima no chat]

Testes já criados: [referência a demanda/arquivo de testes]
Relatório do Maestro: [link ou resumo]
Foco: Implementar código para que os testes passem"

Copilot: [Identifica lacunas, implementa regras, traduz condições]

Resultado: Código de produção aderente aos testes, sem alterar specs.
```

**Fluxo típico:**
```
Você: "Use o Mercenário para implementar RF-1"
      [colar ou referenciar testes + relatório]

Copilot: "Identifiquei que preciso criar/alterar:
         - [arquivo 1] — implementar cálculo de taxa
         - [arquivo 2] — integrar validação de saldo
         ..."

Você: "Implemente" (ou "Mostre o plano antes")

Copilot: [Cria/altera arquivo, explica regra por regra]
         [Propõe: "Próximo passo: executar testes?"]

Você: "Execute os testes"

Copilot: [Executa npm run test ou dotnet test]
```

**Checklist:**
- [ ] Testes já existem e definem comportamento
- [ ] Relatório de alterações fornecido
- [ ] Nenhum teste foi alterado
- [ ] Regras traduzidas em condicionais, cálculos, transições
- [ ] Testes passam

</details>

<details>
<summary><strong>Skill 5: Batedor de Códigos</strong></summary>

#### Arquivo: `.copilot/prompt-templates/batedor-de-codigos.md`

**Propósito:** Analisar código e identificar inadequações (code smells, SOLID, etc.)

**Como usar:**
```markdown
# Batedor de Códigos — Análise de Inadequação

[Cole o conteúdo de .cursor/skills/batedor-de-codigos/SKILL.md]

## Início do trabalho

Você: "[Cole o template acima no chat]

Escopo a analisar: [caminho/arquivo(s)]
Foco: [code smells | SOLID | Clean Architecture | DDD]"

Copilot: [Percorre código, identifica inadequações]

Resultado: Relatório estruturado com smells, localizações, princípios violados.
```

**Exemplo:**
```
Você: "Use o Batedor de Códigos para analisar src/app/application/"
      [colar regra angular-frontend.mdc se houver]

Copilot: "Analisando...
         
         # Relatório de Inadequações — src/app/application/
         
         ## Resumo
         - Total de achados: 4
         - Por categoria: Change Preventers (2), OO Abusers (1), ...
         
         ## Achados
         
         ### 1 — Violação de DIP — arquivo.ts
         ..."

Você: "Perfeito, use o Mestre Freire para corrigir"
```

**Checklist:**
- [ ] Escopo definido
- [ ] Análise completa (não parar na metade)
- [ ] Relatório autossuficiente (sem voltar ao código)
- [ ] Princípios/categorias claros
- [ ] Pronto para Mestre Freire

</details>

<details>
<summary><strong>Skill 6: Mestre Freire</strong></summary>

#### Arquivo: `.copilot/prompt-templates/mestre-freire.md`

**Propósito:** Refatoração guiada por relatório, sem alterar comportamento

**Como usar:**
```markdown
# Mestre Freire — Refatoração Guiada por Relatório

[Cole o conteúdo de .cursor/skills/mestre-freire/SKILL.md]

## Início do trabalho

Você: "[Cole o template acima no chat]

Relatório de inadequações (do Batedor):
[Cole ou referencie o relatório]

Comece com o primeiro achado."

Copilot: [Identifica técnica de refatoração, aplica em passos pequenos]

Resultado esperado: Código refatorado, testes ainda passam, comportamento inalterado.
```

**Fluxo:**
```
Você: "Use o Mestre Freire com o relatório acima"

Copilot: "Lendo relatório...
         
         Achado 1 — [Violation Name]
         Técnica: [Extract Method | Extract Class | ...]
         
         Vou refatorar em passos pequenos. Passo 1:
         [mostra mudança]
         
         Próximo: executar testes?"

Você: "Execute os testes"

Copilot: [npm run test / dotnet test]
         "Todos os testes continuam passando ✓
         
         Prosseguindo com Achado 2..."
```

**Checklist:**
- [ ] Relatório recebido e compreendido
- [ ] Achados em ordem lógica (dependências)
- [ ] Cada achado refatorado em passos pequenos
- [ ] Testes executados após cada passo
- [ ] Nenhuma adição/remoção de funcionalidade
- [ ] Nenhum arquivo de teste alterado

</details>

<details>
<summary><strong>Skill 7: Mestre Freire — Angular</strong></summary>

#### Arquivo: `.copilot/prompt-templates/mestre-freire-angular.md`

**Propósito:** Refatoração ou criação Angular com critério específico

**Como usar:**
```markdown
# Mestre Freire — Angular

[Cole o conteúdo de .cursor/skills/mestre-freire-angular/SKILL.md]

## Início do trabalho (modo Greenfield)

Você: "[Cole o template]
      [Cole também .copilot/instructions/2-frontend-angular.md]
      
      Modo: Greenfield
      Spec/Contrato: [OpenAPI ou especificação de API]
      Estrutura desejada: [áreas, módulos, etc.]"

Copilot: [Cria estrutura, confirma, implementa]

Resultado: App Angular scaffolding conforme regra angular-frontend.

## Uso (modo Refatoração)

Você: "[Cole o template + instrução angular]
      
      Modo: Refatoração
      Relatório: [do batedor-de-codigos]"

Copilot: [Aplica mestre-freire + critério Angular]

## Uso (modo Evolução)

Você: "[Cole o template + instrução angular]
      
      Modo: Evolução
      Demanda: [RF-1, RF-2, etc.]
      Contrato de API: [se houver mudança]"

Copilot: [Implementa respeitando regra e reference]
```

**Checklist:**
- [ ] Spec/contrato fornecido (Greenfield) ou relatório (Refactor)
- [ ] Modo indicado (Greenfield | Refactor | Evolução)
- [ ] Regra angular-frontend consultada
- [ ] Nenhum `*.spec.ts` alterado
- [ ] Testes executados (npm run test ou ng test)
- [ ] Build passou

</details>

<details>
<summary><strong>Skill 8: Arauto</strong></summary>

#### Arquivo: `.copilot/prompt-templates/arauto.md`

**Propósito:** Entrega ao repositório (commit, push, PR, CI)

**Como usar:**
```markdown
# Arauto — Entrega ao Repositório Remoto

[Cole o conteúdo de .cursor/skills/arauto/SKILL.md]

## Início do trabalho

Você: "[Cole o template]
      Trabalho concluído em [demanda/feature].
      Entregue?"

Copilot: "Vou verificar status, criar mensagem de commit, push e PR.
         
         ## Resumo do que será commitado:
         [mostra diff resumido]
         
         Mensagem de commit (sugestão):
         [tipo(escopo): descrição]
         [corpo do commit]
         
         Titulo da PR:
         [tipo(escopo): descrição]
         
         Deseja prosseguir?"

Você: "Sim, proceda" (ou "Ajuste [detalhe]")

Copilot: [Executa git add -A, commit, push, gh pr create, gh run watch]
         [Aguarda workflows do PR]
         
         "Resultado: ✓ Entrega concluída. Workflows verdes.
         PR URL: [link]"
```

**Script auxiliar:**
- Copiar `.cursor/skills/arauto/scripts/arauto.sh` para `.copilot/scripts/arauto.sh`
- Usar quando fluxo for muito repetitivo

**Checklist:**
- [ ] Alterações prontas (código, testes, documentação)
- [ ] Branch criada (não em main/master)
- [ ] Testes do projeto passam
- [ ] Docker compose build passa
- [ ] Mensagem de commit clara
- [ ] PR criada e workflows monitorados
- [ ] Workflows verdes antes de marcar "concluído"

</details>

---

### PART III: COMMANDS → PROMPT-TEMPLATES ESTRUTURADOS

<details>
<summary><strong>Command 1: criar-fonte-de-verdade</strong></summary>

#### Arquivo: `.copilot/prompt-templates/criar-fonte-de-verdade.md`

**Propósito:** Criar plano como "source of truth" para spec-driven development

**Como usar:**
```markdown
# Criar Fonte de Verdade (Spec-Driven Development)

[Cole o conteúdo de .cursor/commands/criar-fonte-de-verdade.md]

## Fluxo de uso no Copilot

### Etapa 1: Levantamento

Você: "[Cole o template]
      
      Demanda: [RA-1, RF-1, etc.] ou [descrição]
      Localidade/escopo: [página web, módulo, etc.]
      
      Use o Tradutor puis o Maestro para criar o plano." 

Copilot: 
1. **Tradutor:** Entende em linguagem de negócio, UX, personas
2. **Maestro:** Analisa código, cria relatório de alterações
3. **Resultado:** Documento de plano consolidado

### Etapa 2: Validação

Você: "Validei o plano. Próximos passos?"

Copilot: "Próximos passos para a rotina-completa:
         1. Usar criar-plano-arvore-testes para derivar testes
         2. Usar quadro-de-recompensas para criar testes
         3. Usar mercenario para implementar
         4. Usar batedor-de-codigos + mestre-freire para refatorar
         5. Usar arauto para entregar"
```

**Resultado esperado:**
- Plano consolidado em Markdown (salvar como `.cursor/plans/[identificador].plan.md`)
- Pronto para ser referenciado nas próximas etapas

</details>

<details>
<summary><strong>Command 2: criar-plano-arvore-testes</strong></summary>

#### Arquivo: `.copilot/prompt-templates/criar-plano-arvore-testes.md`

**Propósito:** Criar plano específico para árvore de testes (fonte de verdade para testes)

**Como usar:**
```markdown
# Criar Plano (Fonte de Verdade) para Árvore de Testes

[Cole o conteúdo de .cursor/commands/criar-plano-arvore-testes.md]

## Fluxo de uso

Você: "[Cole o template]
      
      Plano global (do criar-fonte-de-verdade):
      [link ou referencie arquivo em .cursor/plans/]
      
      Escopo: [listagem das solicitações | criação de solicitação | etc.]"

Copilot: "Lendo plano global...
         
         Criando plano para árvore de testes:
         
         # Plano — Árvore de Testes [identificador]
         
         ## Referência ao plano global
         [resumo]
         
         ## Âmbito do frontend
         [páginas, componentes, serviços]
         
         ## Mapeamento de alterações → testes
         [item 4.1 → testes unit em X, integração em Y, etc.]
         
         ## Estrutura da árvore
         [lista de .spec.ts a criar/evoluir]
         
         ## Relatório de tarefas (formato quadro-de-recompensas)
         [tabela com ID, Onde, Tipo, Descrição, Requisito]
         
         ## Ordem sugerida
         [sequência para criar/evoluir testes]"

Você: "Perfeito, salve em .cursor/plans/"

Copilot: [Cria arquivo com nome sugerido]
```

**Saída esperada:**
- Arquivo `.cursor/plans/plano-arvore-testes_[identificador].plan.md`
- Pronto para `quadro-de-recompensas` na etapa "criação de TESTES"

</details>

<details>
<summary><strong>Command 3: criar-fonte-verdade-codigo-producao</strong></summary>

#### Arquivo: `.copilot/prompt-templates/criar-fonte-verdade-codigo-producao.md`

**Propósito:** Gerar SoT para etapa de implementação de código

**Como usar:**
```markdown
# Gerar Fonte de Verdade para Etapa de Código de Produção

[Cole o conteúdo de .cursor/commands/criar-fonte-verdade-codigo-producao.md]

## Fluxo de uso

Você: "[Cole o template]
      
      Plano global: [.cursor/plans/plano-*.plan.md]
      Árvore de testes: [.cursor/plans/plano-arvore-testes_*.plan.md]
      ou arquivo de testes já criado: [referência]"

Copilot: "Lendo planos...
         
         # Fonte de Verdade — Código de Produção
         
         ## Local/Escopo
         [área, componente, módulo]
         
         ## Resumo de alterações
         [com base nos planos anteriores]
         
         ## Referência à árvore de testes
         [link ou resumo]
         
         ## Regras obrigatórias nesta etapa
         - Implementar apenas código de produção
         - Orientar-se pela árvore de testes
         - Proibida alteração de testes
         - Foco em funcionalidade (não em qualidade de código agora)
         - Usar skill Mercenário"

Você: "Crie o arquivo"

Copilot: [Salva em .cursor/plans/plano-codigo_[id].plan.md]
```

**Saída esperada:**
- Arquivo `.cursor/plans/[identificador]-codigo.plan.md`
- Orientação clara para etapa de implementação com Mercenário

</details>

<details>
<summary><strong>Command 4: criar-fonte-de-verdade-refactoring</strong></summary>

#### Arquivo: `.copilot/prompt-templates/criar-fonte-de-verdade-refactoring.md`

**Propósito:** Criar SoT para guiar refatorações técnicas

**Como usar:**
```markdown
# Fonte de Verdade para Refactoring

[Cole o conteúdo de .cursor/commands/criar-fonte-de-verdade-refactoring.md]

## Fluxo de uso

Você: "[Cole o template]
      
      Escopo: [backend, frontend, ambos]
      Foco: [architecture | clean code | SOLID | DDD | performance]"

Copilot: "Analisando o projeto...
         
         ## Visão da arquitetura atual
         [estrutura backend e frontend]
         
         ## Relatório de inadequações
         [referência aos achados ou execução do Batedor de Códigos]
         
         ## Prioridades e critérios
         [o que atacar primeiro, princípios a respeitar]
         
         ## Restrições
         [não alterar regras, testes devem passar, funcionalidades intactas]"

Você: "Qualquer complemento?"

Copilot: "Se desejar, posso executar o Batedor de Códigos para um
         relatório detalhado de inadequações, que depois o
         Mestre Freire corrigirá. Deseja?"
```

**Saída esperada:**
- Arquivo `.copilot/fonte-de-verdade-refactoring.md` (consolidado)
- Pronto para ciclos de refactoring com Mestre Freire

</details>

---

## PART IV: INSTRUÇÕES DE USO NO COPILOT

### Como organizar o workflow com Copilot

#### 1. **Inicializar um novo trabalho**

```bash
# Crie um arquivo de contexto para a sessão
cat > /tmp/contexto-sessao.md << EOF
# Contexto da Sessão

Demanda: [RA-1, RF-1, etc.]
Status: [iniciando, em andamento, revisão, entrega]
Último passo: [qual skill usou por último]
Próximo passo: [qual skill vai usar]
EOF

# Cole no chat do Copilot:
## Início da implementação de [DEMANDA]

[Cole .copilot/instructions/0-metodologia-completa.md]

Contexto: [resumo do que vai fazer]
Próximo passo: Use o [SKILL] para [AÇÃO]
```

#### 2. **Invocar uma skill específica**

```bash
# No chat do Copilot:

[Cole .copilot/prompt-templates/[skill].md]

[Descreva o trabalho e forneça entradas]

```

#### 3. **Fazer referências cruzadas entre skills**

```bash
# Quando uma skill depende de output de outra:

Você: "Use o Tradutor para entender [demanda]"
[Copilot faz análise]

Você: "Use o Maestro com o resultado do Tradutor para alterações"
[Copilot cria relatório]

Você: "Use o Quadro-de-Recompensas com o relatório para criar testes"
[Copilot cria testes]

# E assim por diante...
```

#### 4. **Executar a rotina-completa**

```bash
# Fluxo recomendado por demanda

1. PLANEJAMENTO
   ├─ Tradutor (se houver UI/usabilidade)
   ├─ Maestro (sempre)
   ├─ Quadro-de-Recompensas (cria testes)
   └─ Validar com você

2. IMPLEMENTAÇÃO
   ├─ Mercenário (implementa código)
   ├─ Executar testes (npm run test / dotnet test)
   ├─ Batedor-de-Codigos (analisa)
   ├─ Mestre Freire (refatora)
   ├─ Executar testes novamente
   └─ Validar com você

3. ENTREGA
   ├─ Arauto (commit, push, PR)
   └─ Monitorar CI até verde
```

---

## PART V: ESTRUTURA DE ARQUIVOS PROPOSTA

### Criar a estrutura no projeto:

```bash
mkdir -p .copilot/instructions
mkdir -p .copilot/prompt-templates
mkdir -p .copilot/scripts
```

### Arquivos a criar:

| Arquivo | Origem | Descrição |
|---------|--------|-----------|
| `.copilot/instructions/0-metodologia-completa.md` | `.cursor/rules/metodologia-para-devs.mdc` | Metodologia de trabalho |
| `.copilot/instructions/2-frontend-angular.md` | `.cursor/rules/angular-frontend.mdc` | Critério técnico Angular |
| `.copilot/instructions/3-cards-demandas.md` | `.cursor/rules/cards-demandas.mdc` | Processo de cards |
| `.copilot/instructions/4-planos-todos.md` | `.cursor/rules/planos-todos.mdc` | Formato de todos em planos |
| `.copilot/prompt-templates/tradutor.md` | `.cursor/skills/tradutor/SKILL.md` | Skill Tradutor |
| `.copilot/prompt-templates/maestro.md` | `.cursor/skills/maestro/SKILL.md` | Skill Maestro |
| `.copilot/prompt-templates/quadro-de-recompensas.md` | `.cursor/skills/quadro-de-recompensas/SKILL.md` | Skill Quadro-de-Recompensas |
| `.copilot/prompt-templates/mercenario.md` | `.cursor/skills/mercenario/SKILL.md` | Skill Mercenário |
| `.copilot/prompt-templates/batedor-de-codigos.md` | `.cursor/skills/batedor-de-codigos/SKILL.md` | Skill Batedor-de-Códigos |
| `.copilot/prompt-templates/mestre-freire.md` | `.cursor/skills/mestre-freire/SKILL.md` | Skill Mestre Freire |
| `.copilot/prompt-templates/mestre-freire-angular.md` | `.cursor/skills/mestre-freire-angular/SKILL.md` | Skill Mestre Freire Angular |
| `.copilot/prompt-templates/arauto.md` | `.cursor/skills/arauto/SKILL.md` | Skill Arauto |
| `.copilot/prompt-templates/criar-fonte-de-verdade.md` | `.cursor/commands/criar-fonte-de-verdade.md` | Command 1 |
| `.copilot/prompt-templates/criar-plano-arvore-testes.md` | `.cursor/commands/criar-plano-arvore-testes.md` | Command 2 |
| `.copilot/prompt-templates/criar-fonte-verdade-codigo-producao.md` | `.cursor/commands/criar-fonte-verdade-codigo-producao.md` | Command 3 |
| `.copilot/prompt-templates/criar-fonte-de-verdade-refactoring.md` | `.cursor/commands/criar-fonte-de-verdade-refactoring.md` | Command 4 |
| `.copilot/scripts/arauto.sh` | `.cursor/skills/arauto/scripts/arauto.sh` | Script de entrega (opcional, já existe no Cursor) |
| `.copilot/contexto-projeto.md` | `.cursor/escopo_inicial.txt` + `.cursor/guia-angular.txt` | Contexto do projeto |

---

## PART VI: EQUIVALÊNCIAS RÁPIDAS

### Quando você usa Cursor, use Copilot assim:

| Ação no Cursor | Equivalente no Copilot |
|---|---|
| `/criar-fonte-de-verdade` | Cole `.copilot/prompt-templates/criar-fonte-de-verdade.md` |
| `/criar-plano-arvore-testes` | Cole `.copilot/prompt-templates/criar-plano-arvore-testes.md` |
| `/criar-fonte-verdade-codigo-producao` | Cole `.copilot/prompt-templates/criar-fonte-verdade-codigo-producao.md` |
| `/criar-fonte-de-verdade-refactoring` | Cole `.copilot/prompt-templates/criar-fonte-de-verdade-refactoring.md` |
| **Skill: Tradutor** | Cole `.copilot/prompt-templates/tradutor.md` |
| **Skill: Maestro** | Cole `.copilot/prompt-templates/maestro.md` |
| **Skill: Quadro-de-Recompensas** | Cole `.copilot/prompt-templates/quadro-de-recompensas.md` |
| **Skill: Mercenário** | Cole `.copilot/prompt-templates/mercenario.md` |
| **Skill: Batedor-de-Códigos** | Cole `.copilot/prompt-templates/batedor-de-codigos.md` |
| **Skill: Mestre Freire** | Cole `.copilot/prompt-templates/mestre-freire.md` |
| **Skill: Mestre Freire Angular** | Cole `.copilot/prompt-templates/mestre-freire-angular.md` |
| **Skill: Arauto** | Cole `.copilot/prompt-templates/arauto.md` |
| **Rule: metodologia-para-devs** | Referencie `.copilot/instructions/0-metodologia-completa.md` |
| **Rule: angular-frontend** | Referencie `.copilot/instructions/2-frontend-angular.md` |
| **Rule: cards-demandas** | Referencie `.copilot/instructions/3-cards-demandas.md` |
| **Rule: planos-todos** | Referencie `.copilot/instructions/4-planos-todos.md` |
| **Custom Instructions (Cursor)** | Use `.copilot/` como contexto para cada sessão |

---

## PART VII: DICAS E BOAS PRÁTICAS

### ✅ Fazer
- **Sempre colar o template inteira** no chat (para máximo contexto)
- **Usar referências cruzadas** entre skills (tradutor → maestro → quadro-recompensas)
- **Validar com perguntas** a cada etapa (Copilot fará mais perguntas; engaje-se)
- **Guardar planos** em `.cursor/plans/` com nomes descritivos
- **Executar testes** após cada skill que toque em código
- **Usar Docker** se o ambiente local não permitir rodar testes (frontend: `./scripts/frontend-test-docker.sh`)

### ❌ Não fazer
- **Não pular a validação** com você; a metodologia requer participação
- **Não alterar testes** (exceto em Quadro-de-Recompensas); eles são a rede de proteção
- **Não pular refactoring** após implementação; sempre executar Batedor → Mestre Freire
- **Não confundir skills** — cada uma tem escopo específico (Maestro não implementa, Mercenário não analisa qualidade)
- **Não fazer refactoring sem relatório** — Mestre Freire deve receber relatório do Batedor

---

## PART VIII: PRÓXIMOS PASSOS

### Ações recomendadas:

1. **Crie a pasta `.copilot/`** na raiz do projeto
2. **Copie os templates** de `.cursor/` para `.copilot/`:
   - Rules → instructions/
   - Skills → prompt-templates/
   - Commands → prompt-templates/
   - Scripts → scripts/
3. **Adapte cada arquivo** com a seção "Como usar no Copilot" (veja exemplos acima)
4. **Crie um README** em `.copilot/README.md` com instruções de uso
5. **Valide a estrutura** com um trabalho piloto (pequena demanda)
6. **Itere** conforme aprenda o melhor modo de usar Copilot com essa metodologia

---

## RESUMO

| Conceito | Cursor | Copilot |
|----------|--------|---------|
| **Metodologia** | `.cursor/rules/` | `.copilot/instructions/` |
| **Skills** | `.cursor/skills/*/SKILL.md` | `.copilot/prompt-templates/*.md` |
| **Commands** | `.cursor/commands/*.md` | `.copilot/prompt-templates/*.md` |
| **Invocação** | `/comando` ou `Cmd+K` + skill name | Cole template no chat |
| **Contexto** | Automático (Cursor AI Context) | Manual (cole arquivo ou referencie com @) |
| **Rotina-completa** | Skills em sequência (Cursor coordena) | Seu chat gerencia sequência (cole cada template, valide, avance) |
| **Testes/build** | Automático com regra mestre-freire | Manual ou via script (avisar Copilot para executar) |

---

**Criado em:** 6 de março de 2026  
**Versão:** 1.0  
**Status:** Pronto para implementação
