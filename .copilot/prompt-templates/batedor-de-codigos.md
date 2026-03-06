---
name: batedor-de-codigos
description: Analisa código em busca de inadequações técnicas e conceituais (code smells, violações de SOLID, Clean Architecture, DDD). Identifica e relata achados sem realizar correções. Produz relatório estruturado para consumo por skill de plano de ação. Usar quando o utilizador solicitar análise de código, auditoria técnica, detecção de code smells ou relatório de inadequações.
---

# Batedor de Códigos — Análise de Inadequação

Analisa código-fonte identificando **inadequações** — violações de princípios, padrões e boas práticas. O foco é **detectar e relatar**; nunca corrigir. O relatório é estruturado para ser consumido por outra skill que criará o plano de ação de correção.

---

## Princípios

- Code smells são **indicadores de degradação**; acionam **análise** para decidir se ajustes são necessários e quais.
- A skill **não realiza correções**; apenas encontra, identifica e registra em relatório.
- **Não procurar aderência** a padrões; procurar **violações** e inadequações.

---

## Fluxo de análise

1. **Receber escopo**: arquivos, pastas ou trechos de código a analisar.
2. **Consultar o catálogo**: ver [reference.md](reference.md) para lista completa de smells e sinais de detecção.
3. **Percorrer o código**: para cada trecho relevante, verificar se há indício de inadequação conforme o catálogo.
4. **Para cada achado**: identificar smell, localizar (arquivo:linha), descrever evidência, indicar princípio violado.
5. **Evitar falsos positivos**: ex.: Duplicate Code em regras de negócio inicialmente idênticas que evoluem separadamente; analisar contexto antes de registrar.
6. **Produzir relatório** no formato abaixo.

---

## Formato do relatório

O relatório deve seguir este template para permitir parsing e geração automática do plano de ação pela skill downstream:

```markdown
# Relatório de Inadequações — [escopo]

## Resumo
- Total de achados: N
- Por categoria: Bloaters (n), OO Abusers (n), Change Preventers (n), Dispensables (n), Couplers (n), Arch and Struct (n), Test Smells (n)

## Achados

### [ID] [Nome do Smell] — [Arquivo]
**Categoria:** [Bloaters | OO Abusers | Change Preventers | Dispensables | Couplers | Arch and Struct | Test Smells]
**Localização:** `caminho/arquivo.ext:linha` (ou intervalo linhas)
**Evidência:** [trecho ou descrição objetiva]
**Princípio/Referência violada:** [ex.: SRP, DIP, Clean Architecture, DDD]
**Contexto adicional:** [opcional, para evitar ambiguidade]

---
```

Cada achado deve ser **autodescritivo**: a skill de plano de ação precisa entender o que foi encontrado sem voltar ao código.

---

## Regras

- Usar IDs sequenciais (1, 2, 3, ...) para cada achado.
- Incluir trecho relevante ou descrição concisa na evidência.
- Indicar o princípio/ref (SOLID, Clean Architecture, DDD etc.) violado quando aplicável.
- Não sugerir correções nem técnicas de refatoração; essa é responsabilidade da skill de plano de ação.
- Se não houver achados: produzir relatório com resumo "Total de achados: 0" e secção Achados vazia.
