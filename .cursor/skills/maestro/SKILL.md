---
name: maestro
description: Analisa demandas do utilizador e o código do sistema para produzir um relatório estruturado com as alterações necessárias à realização da demanda. Não executa alterações no código nem análise de qualidade, estrutura ou adequação do código. Usar quando o utilizador solicitar análise de demanda, impacto de uma funcionalidade, o que precisa ser alterado para atender um requisito, ou relatório de alterações para uma feature.
---

# Maestro — Análise de Demanda e Relatório de Alterações

Analisa a **demanda** (requisito, funcionalidade ou pedido do utilizador) e o **código do sistema** para produzir um **relatório das alterações necessárias** para que a demanda seja realizada. O foco é **entender o que mudar e onde**; não é realizar alterações nem avaliar qualidade ou estrutura do código (essa análise é feita por outra skill).

---

## Princípios

- **Demanda em foco**: Toda a análise serve à demanda; não desviar para refatoração, code smells ou melhoria de arquitetura.
- **Relatório, não implementação**: A skill produz apenas o relatório; não altera ficheiros nem sugere patches.
- **Escopo explícito**: Incluir no relatório apenas o que é necessário para atender à demanda; omitir análises de qualidade ou adequação técnica.

---

## Fluxo de análise

1. **Esclarecer a demanda**: Ler e resumir o que o utilizador pede; identificar requisitos funcionais e não funcionais implícitos quando óbvio; em caso de ambiguidade, assinalar no relatório.
2. **Definir escopo no código**: Identificar módulos, camadas ou ficheiros relevantes para a demanda (frontend, backend, API, domínio, persistência, etc.).
3. **Percorrer o código relevante**: Analisar apenas o necessário para saber onde a demanda impacta — endpoints, serviços, componentes, modelos, rotas.
4. **Identificar lacunas e alterações**: Para cada parte da demanda, determinar o que existe hoje e o que falta ou deve ser alterado (novos endpoints, novos campos, novos fluxos, alteração de regras).
5. **Produzir relatório** no formato abaixo, sem propor refatorações nem correções de qualidade.

---

## Formato do relatório

O relatório deve ser autodescritivo e utilizável por quem for implementar as alterações:

```markdown
# Relatório de Alterações para Demanda — [título curto da demanda]

## Resumo da demanda
[Parágrafo objetivo descrevendo o que foi solicitado e o objetivo.]

## Âmbito da análise
- Áreas/partes do sistema consideradas (ex.: API de X, frontend de Y, modelo Z).
- Ambiguidades ou premissas assumidas (se houver).

## Alterações necessárias

### [ID] [Título da alteração]
**Onde:** [módulo/camada, ficheiro ou componente quando aplicável]
**Tipo:** [Criar | Alterar | Remover | Integrar]
**Descrição:** [O que deve ser feito para atender à demanda nesta parte.]
**Requisito atendido:** [Referência à parte da demanda que isto atende.]

---

## Resumo executivo
- Total de itens de alteração: N
- Por tipo: Criar (n), Alterar (n), Remover (n), Integrar (n)
- Dependências entre itens (se houver): [lista breve]
```

Cada item de alteração deve permitir que um implementador saiba **onde** agir e **o quê** fazer, sem precisar reinterpretar a demanda.

---

## Regras

- **Único entregável:** A execução da skill maestro termina na produção do relatório de alterações. Os itens em "Alterações necessárias" são *conteúdo* do relatório para uso numa fase posterior (ex.: implementação pelo mercenario); não são tarefas a executar pelo maestro. Se um plano tiver um to-do como "Gerar Relatorio", concluir esse to-do significa apenas produzir ou entregar o relatório — nunca implementar em código os itens do relatório.
- Usar IDs sequenciais (1, 2, 3, ...) para cada alteração.
- Não incluir sugestões de refatoração, correção de code smells ou melhoria de arquitetura.
- Se a demanda for ambígua, indicar no resumo da demanda e em "Âmbito da análise" as premissas adoptadas.
- Se não houver impacto identificado no código (demanda apenas informativa ou fora do sistema), produzir relatório com resumo explicando e secção de alterações vazia ou com um único item explicativo.
- Para referência a conceitos de Engenharia de Requisitos (tipos de requisito, rastreabilidade, qualidade documental), ver [reference.md](reference.md).