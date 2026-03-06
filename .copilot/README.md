# GitHub Copilot — Setup Equivalente ao Cursor

Bem-vindo! Este diretório contém o setup **GitHub Copilot** equivalente ao seu setup **Cursor** com a metodologia **spec-driven development**.

## 🚀 Início Rápido

### 1. Estrutura

```
.copilot/
├── README.md                                  (este arquivo)
├── MAPEAMENTO-CURSOR-PARA-COPILOT.md         (documento completo de equivalências)
├── instructions/                              (substitui rules do Cursor)
│   ├── 0-metodologia-completa.md
│   ├── 2-frontend-angular.md
│   ├── 3-cards-demandas.md
│   └── 4-planos-todos.md
├── prompt-templates/                          (substitui skills + commands do Cursor)
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
└── scripts/                                   (scripts reutilizáveis)
    └── arauto.sh
```

### 2. Fluxo de Trabalho Típico

Quando tiver uma **demanda**, abra o **GitHub Copilot Chat** em VS Code (atalho: `Ctrl+Shift+I` ou clique no ícone no sidebar) e siga este padrão:

#### Etapa 1: Planejamento

```markdown
# Demanda: RF-1 — Minhas Solicitações

Vou começar o planejamento.

[Cole aqui:.copilot/instructions/0-metodologia-completa.md]

[Cole aqui: .copilot/prompt-templates/tradutor.md]

Demanda: Permitir que creators vejam suas solicitações de antecipação.
```

O Copilot fará perguntas; responda e itere. Resultado: síntese em linguagem de negócio.

#### Etapa 2: Análise de Alterações

```markdown
Resultado do Tradutor: [copie do resultado anterior]

[Cole aqui: .copilot/prompt-templates/maestro.md]

Analisar a demanda acima em relação ao código do projeto.
```

Resultado: relatório de alterações (ID, Onde, Tipo, Descrição).

#### Etapa 3: Criação de Testes

```markdown
Relatório do Maestro: [copie do resultado anterior]

[Cole aqui: .copilot/prompt-templates/quadro-de-recompensas.md]

Criar testes para os itens acima.
```

Resultado: testes em `.spec.ts` ou `*Tests.cs`.

#### Etapa 4: Implementação

```markdown
Testes criados: [referência ao resultado anterior]

[Cole aqui: .copilot/prompt-templates/mercenario.md]

Implementar código para que os testes passem.
```

Resultado: código que faz os testes passarem.

#### Etapa 5: Análise de Qualidade

```markdown
Escopo a analisar: [arquivo ou pasta do passo anterior]

[Cole aqui: .copilot/prompt-templates/batedor-de-codigos.md]
```

Resultado: relatório de inadequações (code smells, SOLID, etc).

#### Etapa 6: Refatoração

```markdown
Relatório do Batedor: [copie do resultado anterior]

[Cole aqui: .copilot/prompt-templates/mestre-freire.md]

Refatorar baseado no relatório acima.
```

Resultado: código refatorado, testes ainda passam.

#### Etapa 7: Entrega

```markdown
[Cole aqui: .copilot/prompt-templates/arauto.md]

Trabalho concluído em RF-1. Proceda com a entrega.
```

Resultado: commit, push, PR criada, workflows monitorados.

---

## 📕 Referência de Templates

### **Instructions** (use como contexto, não prompts)

| Arquivo | Para quê |
|---------|----------|
| `0-metodologia-completa.md` | Entender metodologia; **sempre** cole no início |
| `2-frontend-angular.md` | Ao trabalhar com Angular; referencie com Mestre Freire Angular |
| `3-cards-demandas.md` | Ao criar cards de demanda |
| `4-planos-todos.md` | Ao gerar/atualizar planos |

### **Prompt-Templates** (skills)

| Arquivo | Quando usar |
|---------|-------------|
| `tradutor.md` | Entender demanda em UX/negócio, antes de código |
| `maestro.md` | Análise de demanda; que altera no código |
| `quadro-de-recompensas.md` | Criar testes a partir de relatório |
| `mercenario.md` | Implementar regras de negócio em código |
| `batedor-de-codigos.md` | Analisar código; encontrar code smells |
| `mestre-freire.md` | Refatorar guiado por relatório do Batedor |
| `mestre-freire-angular.md` | Refatorar/criar Angular com critério específico |
| `arauto.md` | Commit, push, PR, CI |

### **Prompt-Templates** (commands — crie planos antes de implementar)

| Arquivo | Quando usar |
|---------|-------------|
| `criar-fonte-de-verdade.md` | Criar plano global para demanda (Tradutor + Maestro) |
| `criar-plano-arvore-testes.md` | Criar plano específico para testes |
| `criar-fonte-verdade-codigo-producao.md` | Criar SoT para implementação de código |
| `criar-fonte-de-verdade-refactoring.md` | Criar SoT para guiar refatorações |

---

## ⚡ Dicas de Uso

### 1. **Sempre cole a metodologia primeiro**

```
Abra o chat do Copilot.
Cole .copilot/instructions/0-metodologia-completa.md para contexto.
Descreva o trabalho.
Copilot vai perguntar; valide a cada passo.
```

### 2. **Use referências cruzadas**

Quando um template depende de outro, **copie o resultado do anterior** e cole no próximo:

```
Tradutor gera resultado A
Cole resultado A para o Maestro
Maestro gera relatório B
Cole relatório B para Quadro-de-Recompensas
... e assim por diante
```

### 3. **Não pule etapas da rotina-completa**

A sequência ideal:
```
Planejamento: Tradutor → Maestro → Quadro-de-Recompensas
Implementação: Mercenário → Batedor → Mestre Freire → Testes
Entrega: Arauto
```

### 4. **Valide com perguntas**

Após cada etapa, você recebe um resumo. **Sempre responda:**
- "Está correto?" ou "Falta algo?"
- "Proceda?" ou "Ajuste [detalhe]"

Isso mantém a qualidade e a metodologia.

### 5. **Use @file para referenciar arquivos**

No chat do Copilot, você pode usar:
```
@.copilot/instructions/0-metodologia-completa.md

Vou começar...
```

Copilot lerá o arquivo automaticamente.

### 6. **Testes sempre**

Após **qualquer** alteração de código (Mercenário, Mestre Freire):
```
npm run test           // frontend Angular
dotnet test           // backend .NET
./scripts/frontend-test-docker.sh  // se host não tiver Node 20+
```

---

## 🔧 Customizações Recomendadas

### 1. Criar um atalho no `.vscode/tasks.json`

Para abrir rapidamente os templates, você pode criar uma tarefa:

```json
{
  "label": "Open Copilot Setup",
  "command": "code",
  "args": ["${workspaceFolder}/.copilot/README.md"],
  "presentation": {
    "reveal": "always",
    "panel": "new"
  }
}
```

Execute com `Ctrl+Shift+B` → **Open Copilot Setup**.

### 2. Criar arquivos de contexto por demanda

Quando iniciar uma demanda, crie um arquivo `.md` com contexto:

```
.copilot-sessions/
└── RF-1-minhas-solicitacoes.md

---
# Sessão: RF-1 — Minhas Solicitações

Data: 2026-03-06
Status: Planejamento
Último passo: Tradutor
Próximo passo: Maestro

[cola resultado do Tradutor]
```

Reutilize este arquivo nos chats subsequentes para manter contexto.

### 3. Copiar scripts para uso rápido

Se usar muito o `arauto.sh`, copie e execute diretamente:

```bash
cp .cursor/skills/arauto/scripts/arauto.sh .copilot/scripts/
chmod +x .copilot/scripts/arauto.sh

# Depois, no terminal:
.copilot/scripts/arauto.sh --preview
```

---

## 📚 Documentação Completa

Para détail técnico de cada skill, rule e command, consulte:
- **[MAPEAMENTO-CURSOR-PARA-COPILOT.md](./MAPEAMENTO-CURSOR-PARA-COPILOT.md)** — documento completo com explicações profundas

---

## 💡 Exemplo Prático: Implementar RF-1

### Dia 1: Planejamento

**1️⃣ Abra o chat do Copilot**
```
Ctrl+Shift+I  (em VS Code)
```

**2️⃣ Cole metodologia + tradutor**
```markdown
[Cole .copilot/instructions/0-metodologia-completa.md]

[Cole .copilot/prompt-templates/tradutor.md]

Demanda: RF-1 — Minhas Solicitações de Antecipação
Link: demandas/RF-1-minhas-solicitacoes-antecipacao.md
```

**3️⃣ Valide com perguntas**
```
"Entendi corretamente que o creator quer:
 - Ver lista de suas solicitações
 - Filtrar por status
 - Ver detalhes de cada solicitação
 
Correto?"
```

**4️⃣ Copie o resultado, Cole maestro**
```markdown
[Cole .copilot/prompt-templates/maestro.md]

Resultado do Tradutor:
[cole resultado do passo anterior]

Código atual do projeto:
- Backend: LastTechTest.API
- Frontend: frontend/
```

... e assim por diante até gerar planos completos.

### Dia 2: Implementação

**1️⃣ Cole Quadro-de-Recompensas**
```markdown
[Cole .copilot/prompt-templates/quadro-de-recompensas.md]

Relatório do Maestro:
[cole resultado de Dia 1]
```

**2️⃣ Cole Mercenário**
```markdown
[Cole .copilot/prompt-templates/mercenario.md]

Testes já criados:
[referência aos testes criados acima]

Implementar código para passar nos testes.
```

**3️⃣ Execute testes**
```
npm run test  // se o Copilot pedir
```

### Dia 3: Qualidade e Entrega

**1️⃣ Cole Batedor-de-Codigos**
```markdown
[Cole .copilot/prompt-templates/batedor-de-codigos.md]

Escopo: src/app/features/anticipations/
```

**2️⃣ Cole Mestre Freire**
```markdown
[Cole .copilot/prompt-templates/mestre-freire.md]

Relatório (do Batedor):
[cole resultado acima]
```

**3️⃣ Cole Arauto**
```markdown
[Cole .copilot/prompt-templates/arauto.md]

Trabalho concluído em RF-1. Entregar?
```

---

## 📝 Checklist de Setup

- [ ] Pasta `.copilot/` criada
- [ ] Subpastas `instructions/`, `prompt-templates/`, `scripts/` criadas
- [ ] Todos os `.md` copiados de `.cursor/` (ou criados a partir de MAPEAMENTO)
- [ ] Script `arauto.sh` copiado (opcional)
- [ ] README.md pronto (este arquivo)
- [ ] Primeiro teste: rode `criar-fonte-de-verdade.md` com uma demanda pequena
- [ ] Validou testes após implementação
- [ ] Entregou com sucesso via `arauto.md`

---

## 🤝 Próximos Passos

1. **Leia** [MAPEAMENTO-CURSOR-PARA-COPILOT.md](./MAPEAMENTO-CURSOR-PARA-COPILOT.md) para entender cada equivalência
2. **Copie** templates de `.cursor/` para `.copilot/` conforme necessário
3. **Teste** com uma pequena demanda (ex.: RF-1 ou uma correção rápida)
4. **Itere** — ajuste naming, organize sessions, customize conforme use

---

## 📞 Dúvidas Frequentes

### P: Por que não usar Cursor em vez de Copilot?
**R:** Você está usando Copilot para compatibilidade e versatilidade. Este setup mantém **toda a metodologia e estrutura** do Cursor, apenas adaptadas para Copilot.

### P: Preciso colar o template inteiro toda vez?
**R:** Sim, para máximo contexto no chat. Alternativamente, use `@.copilot/prompt-templates/[skill].md` para referenciar.

### P: E se o template for muito grande?
**R:** Divida em múltiplos chats. Cole a metodologia uma vez; cole cada skill conforme necessário.

### P: Como reproduzir a invocação `/skill` do Cursor?
**R:** No Copilot, você **cola o template**. Equivalente direto não existe; a metodologia é a mesma, apenas a interface muda.

### P: Posso usar Cursor e Copilot ao mesmo tempo?
**R:** Sim! Use `.cursor/` quando quiser a experiência nativa do Cursor, e `.copilot/` para Copilot. São equivalentes.

---

## 📄 Licença e Uso

Este setup é **derivado do seu Cursor setup** e segue a mesma metodologia. Sinta-se livre para:
- Customizar conforme sua preferência
- Adicionar novos templates
- Manter em sincronismo com `.cursor/` para referência

---

**Criado em:** 6 de março de 2026  
**Versão:** 1.0  
**Status:** Pronto para uso  

---

**Dúvidas? Consulte [MAPEAMENTO-CURSOR-PARA-COPILOT.md](./MAPEAMENTO-CURSOR-PARA-COPILOT.md) para detalhes profundos.**
