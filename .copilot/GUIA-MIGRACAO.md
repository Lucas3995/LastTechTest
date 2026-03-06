# Guia de Migração: .cursor → .copilot

Use este arquivo para **copiar** cada rule/skill/command do Cursor para o Copilot.

---

## 📋 Instruções Gerais

```bash
# 1. Navegar para raiz do projeto
cd /media/belo/BeloSSD_2/Processos_Sel/LastLink/LastTechTest

# 2. Criar estrutura .copilot (se não existir)
mkdir -p .copilot/instructions .copilot/prompt-templates .copilot/scripts

# 3. Copiar cada arquivo conforme tabela abaixo
# 4. Adaptar conteúdo (adicionar seção "Como usar no Copilot")
```

---

## 🔀 Mapeamento de Arquivos

### ✅ Rules → Instructions (Contexto/Referência)

| Origem (.cursor/) | Destino (.copilot/) | Status | Comando |
|---|---|---|---|
| `.cursor/rules/metodologia-para-devs.mdc` | `.copilot/instructions/0-metodologia-completa.md` | ✅ CRIADO | Via [MAPEAMENTO-CURSOR-PARA-COPILOT.md](#) |
| `.cursor/rules/angular-frontend.mdc` | `.copilot/instructions/2-frontend-angular.md` | ⏳ FAZER | `cp .cursor/rules/angular-frontend.mdc .copilot/instructions/2-frontend-angular.md` |
| `.cursor/rules/cards-demandas.mdc` | `.copilot/instructions/3-cards-demandas.md` | ⏳ FAZER | `cp .cursor/rules/cards-demandas.mdc .copilot/instructions/3-cards-demandas.md` |
| `.cursor/rules/planos-todos.mdc` | `.copilot/instructions/4-planos-todos.md` | ⏳ FAZER | `cp .cursor/rules/planos-todos.mdc .copilot/instructions/4-planos-todos.md` |
| `.cursor/rules/poupar-creditos-com-skills.mdc` | Incorporado em `0-metodologia-completa.md` | ⏳ FAZER | Manualmente (seção "Economizar Créditos") |

---

### ✅ Skills → Prompt-Templates (Componentes Reutilizáveis)

| Origem (.cursor/) | Destino (.copilot/) | Status | Comando |
|---|---|---|---|
| `.cursor/skills/tradutor/SKILL.md` | `.copilot/prompt-templates/tradutor.md` | ⏳ FAZER | `cp .cursor/skills/tradutor/SKILL.md .copilot/prompt-templates/tradutor.md` |
| `.cursor/skills/maestro/SKILL.md` | `.copilot/prompt-templates/maestro.md` | ⏳ FAZER | `cp .cursor/skills/maestro/SKILL.md .copilot/prompt-templates/maestro.md` |
| `.cursor/skills/quadro-de-recompensas/SKILL.md` | `.copilot/prompt-templates/quadro-de-recompensas.md` | ⏳ FAZER | `cp .cursor/skills/quadro-de-recompensas/SKILL.md .copilot/prompt-templates/quadro-de-recompensas.md` |
| `.cursor/skills/mercenario/SKILL.md` | `.copilot/prompt-templates/mercenario.md` | ⏳ FAZER | `cp .cursor/skills/mercenario/SKILL.md .copilot/prompt-templates/mercenario.md` |
| `.cursor/skills/batedor-de-codigos/SKILL.md` | `.copilot/prompt-templates/batedor-de-codigos.md` | ⏳ FAZER | `cp .cursor/skills/batedor-de-codigos/SKILL.md .copilot/prompt-templates/batedor-de-codigos.md` |
| `.cursor/skills/mestre-freire/SKILL.md` | `.copilot/prompt-templates/mestre-freire.md` | ⏳ FAZER | `cp .cursor/skills/mestre-freire/SKILL.md .copilot/prompt-templates/mestre-freire.md` |
| `.cursor/skills/mestre-freire-angular/SKILL.md` | `.copilot/prompt-templates/mestre-freire-angular.md` | ⏳ FAZER | `cp .cursor/skills/mestre-freire-angular/SKILL.md .copilot/prompt-templates/mestre-freire-angular.md` |
| `.cursor/skills/arauto/SKILL.md` | `.copilot/prompt-templates/arauto.md` | ⏳ FAZER | `cp .cursor/skills/arauto/SKILL.md .copilot/prompt-templates/arauto.md` |

**Nota:** Após copiar, **adicione uma seção "Como usar no Copilot"** em cada arquivo (veja exemplos em [MAPEAMENTO-CURSOR-PARA-COPILOT.md](./MAPEAMENTO-CURSOR-PARA-COPILOT.md)).

---

### ✅ Commands → Prompt-Templates (Fluxos Estruturados)

| Origem (.cursor/) | Destino (.copilot/) | Status | Comando |
|---|---|---|---|
| `.cursor/commands/criar-fonte-de-verdade.md` | `.copilot/prompt-templates/criar-fonte-de-verdade.md` | ⏳ FAZER | `cp .cursor/commands/criar-fonte-de-verdade.md .copilot/prompt-templates/criar-fonte-de-verdade.md` |
| `.cursor/commands/criar-plano-arvore-testes.md` | `.copilot/prompt-templates/criar-plano-arvore-testes.md` | ⏳ FAZER | `cp .cursor/commands/criar-plano-arvore-testes.md .copilot/prompt-templates/criar-plano-arvore-testes.md` |
| `.cursor/commands/criar-fonte-verdade-codigo-producao.md` | `.copilot/prompt-templates/criar-fonte-verdade-codigo-producao.md` | ⏳ FAZER | `cp .cursor/commands/criar-fonte-verdade-codigo-producao.md .copilot/prompt-templates/criar-fonte-verdade-codigo-producao.md` |
| `.cursor/commands/criar-fonte-de-verdade-refactoring.md` | `.copilot/prompt-templates/criar-fonte-de-verdade-refactoring.md` | ⏳ FAZER | `cp .cursor/commands/criar-fonte-de-verdade-refactoring.md .copilot/prompt-templates/criar-fonte-de-verdade-refactoring.md` |

---

### ✅ Scripts (Opcional)

| Origem | Destino | Status | Comando |
|---|---|---|---|
| `.cursor/skills/arauto/scripts/arauto.sh` | `.copilot/scripts/arauto.sh` | ⏳ FAZER | `cp .cursor/skills/arauto/scripts/arauto.sh .copilot/scripts/arauto.sh && chmod +x .copilot/scripts/arauto.sh` |

---

## 🚀 Plano de Execução

### Fase 1: Criar Estrutura (5 min)

```bash
mkdir -p .copilot/instructions
mkdir -p .copilot/prompt-templates
mkdir -p .copilot/scripts
touch .copilot/context-projeto.md
```

### Fase 2: Copiar Rules → Instructions (10 min)

```bash
# Copiar 4 rules
cp .cursor/rules/angular-frontend.mdc .copilot/instructions/2-frontend-angular.md
cp .cursor/rules/cards-demandas.mdc .copilot/instructions/3-cards-demandas.md
cp .cursor/rules/planos-todos.mdc .copilot/instructions/4-planos-todos.md

# Nota: 0-metodologia-completa.md já foi criado via MAPEAMENTO
# Nota: poupar-creditos-com-skills.mdc já está incorporado em 0-metodologia-completa.md
```

### Fase 3: Copiar Skills → Prompt-Templates (10 min)

```bash
# Copiar 8 skills
cp .cursor/skills/tradutor/SKILL.md .copilot/prompt-templates/tradutor.md
cp .cursor/skills/maestro/SKILL.md .copilot/prompt-templates/maestro.md
cp .cursor/skills/quadro-de-recompensas/SKILL.md .copilot/prompt-templates/quadro-de-recompensas.md
cp .cursor/skills/mercenario/SKILL.md .copilot/prompt-templates/mercenario.md
cp .cursor/skills/batedor-de-codigos/SKILL.md .copilot/prompt-templates/batedor-de-codigos.md
cp .cursor/skills/mestre-freire/SKILL.md .copilot/prompt-templates/mestre-freire.md
cp .cursor/skills/mestre-freire-angular/SKILL.md .copilot/prompt-templates/mestre-freire-angular.md
cp .cursor/skills/arauto/SKILL.md .copilot/prompt-templates/arauto.md
```

### Fase 4: Copiar Commands → Prompt-Templates (10 min)

```bash
# Copiar 4 commands
cp .cursor/commands/criar-fonte-de-verdade.md .copilot/prompt-templates/criar-fonte-de-verdade.md
cp .cursor/commands/criar-plano-arvore-testes.md .copilot/prompt-templates/criar-plano-arvore-testes.md
cp .cursor/commands/criar-fonte-verdade-codigo-producao.md .copilot/prompt-templates/criar-fonte-verdade-codigo-producao.md
cp .cursor/commands/criar-fonte-de-verdade-refactoring.md .copilot/prompt-templates/criar-fonte-de-verdade-refactoring.md
```

### Fase 5: Copiar Scripts (5 min)

```bash
cp .cursor/skills/arauto/scripts/arauto.sh .copilot/scripts/arauto.sh
chmod +x .copilot/scripts/arauto.sh
```

### Fase 6: Copiar Contexto do Projeto (5 min)

```bash
# Copiar arquivos de contexto
cp .cursor/escopo_inicial.txt .copilot/contexto-escopo.txt
cp .cursor/guia-angular.txt .copilot/contexto-guia-angular.txt

# (Opcional) Consolidar em um só arquivo
cat .copilot/contexto-escopo.txt .copilot/contexto-guia-angular.txt > .copilot/contexto-projeto.md
```

### Fase 7: Validação (5 min)

```bash
# Verificar se tudo foi copiado
ls -la .copilot/instructions/ | wc -l  # Deve ter ≥ 4 arquivos
ls -la .copilot/prompt-templates/ | wc -l  # Deve ter ≥ 12 arquivos
ls -la .copilot/scripts/ | wc -l  # Deve ter ≥ 1 arquivo

# Verificar se README e CHEAT-SHEET foram criados
ls -la .copilot/{README,CHEAT-SHEET,MAPEAMENTO}* # Deve existir
```

---

## 📝 Adaptações Após Cópia

Cada arquivo copiado deve ter uma **seção adicional** "Como usar no Copilot". Veja exemplos em [MAPEAMENTO-CURSOR-PARA-COPILOT.md](#).

### Exemplo para cada tipo:

#### Para Instructions (Rules)

```markdown
---
description: [manter igual]
alwaysApply: [manter igual]
---

# [Título Original]

[Conteúdo original da .mdc]

## Como usar no Copilot

[Nova seção explicando como usar em chats do Copilot]
```

#### Para Prompt-Templates (Skills)

```markdown
---
name: [titulo]
description: [manter igual]
---

# [Título Original]

[Conteúdo original do SKILL.md]

## Como começar no Copilot

Você: "[Cole este template no chat do Copilot]
      [Descreva o que quer fazer]"

Copilot: [responde com análise/testes/código/etc]
```

#### Para Prompt-Templates (Commands)

```markdown
# [Comando Original da CLI]

[Conteúdo original do comando]

## Como usar no Copilot

Ao invés de `/comando`, cole este template:

```
[Instruções de como colar e invocar]
```
```

---

## ✅ Checklist Pós-Cópia

```bash
# Rodar este checklist após completar todas as cópias

cat > /tmp/copilot-setup-check.sh << 'EOF'
#!/bin/bash

echo "=== Copilot Setup Validation ==="

# Instructions
echo -n "✓ Instructions (4 files): "
ls .copilot/instructions/*.md 2>/dev/null | wc -l

# Prompt-Templates (12+)
echo -n "✓ Prompt-Templates (12+ files): "
ls .copilot/prompt-templates/*.md 2>/dev/null | wc -l

# Scripts
echo -n "✓ Scripts (1+ files): "
ls .copilot/scripts/* 2>/dev/null | wc -l

# README e guias
echo -n "✓ README.md: "
[ -f .copilot/README.md ] && echo "✓" || echo "✗ FALTA"

echo -n "✓ CHEAT-SHEET.md: "
[ -f .copilot/CHEAT-SHEET.md ] && echo "✓" || echo "✗ FALTA"

echo -n "✓ MAPEAMENTO-CURSOR-PARA-COPILOT.md: "
[ -f .copilot/MAPEAMENTO-CURSOR-PARA-COPILOT.md ] && echo "✓" || echo "✗ FALTA"

echo ""
echo "=== Setup Complete ==="
echo "Próximo passo: Review adaptações em cada arquivo (seção 'Como usar no Copilot')"

EOF

chmod +x /tmp/copilot-setup-check.sh
/tmp/copilot-setup-check.sh
```

---

## 🔍 Diferenças Esperadas

Após a cópia, algumas diferenças são esperadas:

| Item | Cursor | Copilot |
|------|--------|---------|
| **Invocação** | `/comando` (CLI) | Cole template em chat |
| **Contexto** | Automático (Cursor AI) | Manual (@file ou paste) |
| **Sequência** | Automática (agent mode) | Manual (você gerencia) |
| **Output** | No VS Code | No chat |
| **Persistência** | Em projeto com .cursor/ | Em projeto com .copilot/ |

---

## 🎯 Próximas Ações

1. **Execute Fase 1-7** acima (40 min)
2. **Valide** com checklist
3. **Teste piloto**: pegue uma demanda pequena e rode rotina-completa com Copilot
4. **Itere**: ajuste naming, organize sessions, customize

---

## 📞 Dúvidas Frequentes Durante Migração

### P: "Preciso adaptar os conteúdos dos arquivos?"

**R:** Não muito. Os conteúdos são reutilizáveis como-estão. Apenas **adicione uma seção** "Como usar no Copilot" ao final de cada copiado. Veja exemplos em [MAPEAMENTO-CURSOR-PARA-COPILOT.md](#).

### P: "Alguns arquivos têm reference.md — preciso copiar?"

**R:** Opcionalmente sim. Copie em `.copilot/reference/` se quiser, para consulta offline. Não é obrigatório para usar Copilot.

### P: "Preciso sincronizar .cursor/ e .copilot/?

**R:** Não é necessário. Use `.copilot/` para Copilot e `.cursor/` para Cursor. Se alterar um, atualize manualmente no outro se quiser manter sync. Sugestão: use `.cursor/` como principal e copie mudanças para `.copilot/` quando necessário.

### P: "Posso deletar .cursor/ após copiar?"

**R:** Não recomendado. Mantenha ambos; são ambientes equivalentes. Assim você pode alternar entre Cursor e Copilot sem perder setup.

---

## 📊 Sumário da Migração

```
Origem: .cursor/ (8 skills, 4 commands, 5 rules)
         +
Destino: .copilot/ (novos instructions, prompt-templates, scripts)

Status Inicial:  ✅ README, ✅ CHEAT-SHEET, ✅ MAPEAMENTO criados
                ⏳ 17 arquivos aguardando cópia de .cursor/

Ações Necessárias:
  1. ✅ Criar estrutura de pastas
  2. ⏳ Copiar 4 instructions
  3. ⏳ Copiar 8 prompt-templates (skills)
  4. ⏳ Copiar 4 prompt-templates (commands)
  5. ⏳ Copiar scripts (arauto.sh)
  6. ⏳ Adaptar cada arquivo (seção "Como usar no Copilot")
  7. ✅ Validar com checklist

Tempo Estimado: 50—60 minutos
Dificuldade: Baixa (copy-paste + mínimas adaptações)
```

---

**Por onde começar? Execute os comandos da Fase 1-5, depois volte aqui para próximas ações.**

Criado em: 6 de março de 2026
