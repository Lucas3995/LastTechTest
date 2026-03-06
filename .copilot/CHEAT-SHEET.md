# Copilot Setup — Cheat Sheet Rápido

Use este arquivo como **referência rápida** durante o trabalho. Todos os detalhes estão em [MAPEAMENTO-CURSOR-PARA-COPILOT.md](./MAPEAMENTO-CURSOR-PARA-COPILOT.md).

---

## 🎯 Antes de Começar

```markdown
1. Abra o chat do GitHub Copilot (Ctrl+Shift+I)
2. Cole: .copilot/instructions/0-metodologia-completa.md
3. Descreva: "Vou começar a demanda [NOME]"
4. Siga a rotina-completa (veja abaixo)
```

---

## 📋 Rotina Completa (Sequência Recomendada)

### 🔵 FASE 1: PLANEJAMENTO

| Passo | O que fazer | Template a colar | Saída esperada |
|-------|-----------|------------------|----------------|
| 1 | Entender demanda em UX | `tradutor.md` | Síntese de personas, tarefas, fluxos (sem código) |
| 2 | Analisar impacto no código | `maestro.md` | Relatório com IDs, Tipo, Onde, Descrição |
| 3 | Criar testes | `quadro-de-recompensas.md` | Arquivos `*.spec.ts` ou `*Tests.cs` criados |
| ✅ | **Validar com você** | — | Tudo ok? Prosseguir? |

---

### 🟢 FASE 2: IMPLEMENTAÇÃO

| Passo | O que fazer | Template a colar | Saída esperada |
|-------|-----------|------------------|----------------|
| 4 | Implementar código | `mercenario.md` | Código que faz testes passarem |
| 5 | Analisar qualidade | `batedor-de-codigos.md` | Relatório de code smells/SOLID/etc |
| 6 | Refatorar | `mestre-freire.md` | Código melhorado, testes ainda passam |
| 7 | Executar testes | — | `npm run test` ou `dotnet test` — TUDO VERDE |
| ✅ | **Validar com você** | — | Tudo ok? Prosseguir? |

---

### 🟡 FASE 3: ENTREGA

| Passo | O que fazer | Template a colar | Saída esperada |
|-------|-----------|------------------|----------------|
| 8 | Entregar ao remoto | `arauto.md` | PR criada, workflows verdes, em main |

---

## 🚀 Atalhos: Templates por Situação

### 🟦 Preciso criar um PLANO primeiro?

```markdown
# Situação: Tenho uma demanda, mas não sei por onde começar

Cole aqui: .copilot/prompt-templates/criar-fonte-de-verdade.md

[Descreva a demanda]
```

**Resultado:** Plano consolidado em `.cursor/plans/`

---

### 🟩 Preciso gerar CARDS de demanda?

```markdown
# Situação: Devo gerar cards para o backlog

Cole aqui: .copilot/instructions/3-cards-demandas.md
Cole aqui: .copilot/prompt-templates/maestro.md

[Descreva o que quer nos cards]
```

---

### 🟨 Devo REFATORAR o código existente?

```markdown
# Situação: Código tem problemas de qualidade, não nova feature

1. Cole aqui: .copilot/prompt-templates/batalhor-de-codigos.md
   [Indique pasta/arquivo a analisar]
   
   → Resultado: Relatório

2. Cole aqui: .copilot/prompt-templates/mestre-freire.md
   [Cole o relatório]
   
   → Resultado: Código refatorado
```

---

### 🟧 Devo EVOLUIR o Angular (criar/refatorar)?

```markdown
# Situação: Trabalho no frontend Angular

Cole aqui: .copilot/instructions/2-frontend-angular.md
Cole aqui: .copilot/prompt-templates/mestre-freire-angular.md

[Indique Modo: Greenfield | Refactoring | Evolução]
[Forneça Spec/Contrato de API ou Relatório]
```

---

## 📊 Matriz de Templates

```
RULES (Instructions)          → Contexto/Referência
├─ 0-metodologia-completa.md     ← Cole SEMPRE no início
├─ 2-frontend-angular.md          ← Cole ao trabalhar com Angular
├─ 3-cards-demandas.md            ← Cole ao criar cards
└─ 4-planos-todos.md              ← Cole ao gerar planos

SKILLS (Prompt-Templates)    → Invocar em sequência conforme rotina
├─ tradutor.md               ← 1º passo: entender demanda (UX)
├─ maestro.md                ← 2º passo: analisar código (alterações)
├─ quadro-de-recompensas.md  ← 3º passo: criar testes
├─ mercenario.md             ← 4º passo: implementar código
├─ batedor-de-codigos.md     ← 5º passo: analisar qualidade
├─ mestre-freire.md          ← 6º passo: refatorar
├─ mestre-freire-angular.md  ← Variante: para Angular
└─ arauto.md                 ← 7º passo: entregar (commit, PR, CI)

COMMANDS (Estruturados)      → Gerar planos consolidados
├─ criar-fonte-de-verdade.md ← Plano global (Tradutor + Maestro)
├─ criar-plano-arvore-testes.md ← Plano para testes
├─ criar-fonte-verdade-codigo-producao.md ← SoT para implementação
└─ criar-fonte-de-verdade-refactoring.md ← SoT para refactoring
```

---

## 🎮 Exemplo: Implementar RF-1 em 3 Sessões

### Sessão 1: Planejamento (30 min)

```markdown
[Abra GitHub Copilot Chat]

Cole .copilot/instructions/0-metodologia-completa.md

Cole .copilot/prompt-templates/tradutor.md

Demanda: RF-1 — Minhas Solicitações
Link: demandas/RF-1-minhas-solicitacoes-antecipacao.md

[Copilot: Faz perguntas, você responde]
[Resultado: Síntese de UX]

Cole .copilot/prompt-templates/maestro.md

[Cole resultado do Tradutor acima]

[Copilot: Analisa código, gera relatório]
[Resultado: Alterações necessárias com IDs]

Cole .copilot/prompt-templates/quadro-de-recompensas.md

[Cole relatório do Maestro]

[Copilot: Cria testes]
[Resultado: *.spec.ts criados, testes rodando]
```

### Sessão 2: Implementação (1h)

```markdown
[Novo chat]

Cole .copilot/instructions/0-metodologia-completa.md

Cole .copilot/prompt-templates/mercenario.md

Testes criados em: src/app/features/anticipations/pages/...
Relatório do Maestro: [cole resumo de Sessão 1]

[Copilot: Implementa código]
[Resultado: npm run test — TUDO VERDE]

Cole .copilot/prompt-templates/batedor-de-codigos.md

Escopo: src/app/features/anticipations/

[Copilot: Analisa, encontra smells]
[Resultado: Relatório]

Cole .copilot/prompt-templates/mestre-freire.md

[Cole relatório do Batedor]

[Copilot: Refatora em passos]
[Você: cd frontend && npm run test]
[Resultado: Tudo verde]
```

### Sessão 3: Entrega (15 min)

```markdown
[Novo chat]

Cole .copilot/instructions/0-metodologia-completa.md

Cole .copilot/prompt-templates/arauto.md

Trabalho concluído em RF-1. Entregar?

[Copilot: Mostra status, diff, prepara commit/PR]
[Você: "Sim, proceda"]

[Resultado: PR criada, workflows monitorados, em main]
```

---

## ⚠️ Regras de Ouro

| Regra | Por quê |
|-------|---------|
| **Sempre cole metodologia primeiro** | Garante contexto completo |
| **Validar a cada etapa** | Evita divergências; mantém qualidade |
| **Nunca alterar testes** | Eles são a rede de proteção |
| **Sempre executar testes após código** | Garante que nada quebrou |
| **Não pular refactoring** | Qualidade não é "depois" |
| **Usar rotina-completa em ordem** | Cada passo prepara o próximo |

---

## 🆘 Problemas Comuns

| Problema | Solução |
|----------|---------|
| "Copilot não prosseguiu" | É esperado! **Responda com validação:** "Ajuste [detalhe]" ou "Perfeito, prossiga" |
| "Testes falharam após implementação" | Use **Batedor e Mestre Freire** para refatorar; testes são a verdade |
| "Não sei se implementei tudo" | Verifique **relatório do Maestro** — cada item tem ID; marque completados |
| "Frontend quebrou após push" | Execute `npm run test` localmente antes de **arauto**; use Docker se host não tiver Node 20+ |
| "PR workflow deu erro" | Copilot vai informar; interprete como **nova demanda** (criar fix, passar por rotina-completa novamente) |

---

## 📞 Linha Rápida de Ajuda

```markdown
Coloquei [template], [descrevi o trabalho], mas Copilot [comportamento inesperado].

Como procedo?

---

Resposta padrão:
1. Cole .copilot/instructions/0-metodologia-completa.md novamente
2. Descreva o ponto onde ficou (ex.: "Estou em Maestro, preciso que...")
3. Copilot vai recontextualizar.
```

---

## 📝 Variações do Fluxo

### Fluxo Curto (Bug Simples)

```
1. Batedor (analisa código)
2. Mestre Freire (refatora)
3. Testes (valida)
4. Arauto (entrega)
```

### Fluxo Médio (Feature pequena)

```
1. Maestro (alterações)
2. Quadro-de-Recompensas (testes)
3. Mercenário (código)
4. Testes
5. Arauto (entrega)
```

### Fluxo Completo (Feature grande)

```
1. Tradutor (UX)
2. Maestro (alterações)
3. Quadro-de-Recompensas (testes)
4. Mercenário (código)
5. Batedor (análise)
6. Mestre Freire (refatoração)
7. Testes
8. Arauto (entrega)
```

---

## 🔗 Referências Cruzadas

Para **passar resultado de um template para o próximo:**

```markdown
# Template A gerou:
[resultado em Markdown]

[Cole novo template B]

Cole o resultado do passo anterior:
[resultado]

Continuar com [B]...
```

---

## ✅ Checklist Antes de Entregar

- [ ] Código implementado (Mercenário)
- [ ] Testes passam (npm run test / dotnet test)
- [ ] Código refatorado (Mestre Freire)
- [ ] Testes ainda passam (executar novamente)
- [ ] Docker compose build passa
- [ ] Nenhum teste foi alterado (exceto Quadro-de-Recompensas)
- [ ] Nenhum `console.log` ou `Debug.WriteLine` solto
- [ ] Documentação atualizada (se aplicável)
- [ ] Pronto para Arauto

---

## 🚀 Próxima Ação

1. **Abra VS Code**
2. **Pressione `Ctrl+Shift+I`** para abrir GitHub Copilot Chat
3. **Cola este arquivo**: [MAPEAMENTO-CURSOR-PARA-COPILOT.md](./MAPEAMENTO-CURSOR-PARA-COPILOT.md)
4. **Ou comece direto**: Cole `.copilot/instructions/0-metodologia-completa.md` e descreva sua demanda

---

**Boa codificação! 🎉**

Tudo pronto para a rotina-completa com GitHub Copilot.

---

*Criado em: 6 de março de 2026*  
*Versão: 1.0*
