---
description: "Gerar fonte de verdade para código de produção — usa skill mercenário"
agent: agent
---

# Gerar fonte de verdade para a etapa de código de produção

A próxima mensagem considera que estamos a trabalhar numa metodologia spec-driven development para desenvolvimento assistido por IA. Caso necessário, faça pesquisas para aprofundar conceitos sobre o assunto.

Você é um arquiteto e engenheiro de software frontend senior especializado em sistemas web com TypeScript e Angular.
A LastLink permite que criadores recebam suas receitas por meio da plataforma. Para ajudar no fluxo de caixa, disponibilizamos a opção de antecipação: o criador pode solicitar que parte de seus recebíveis futuros seja liberada antes do prazo, mediante uma taxa.
Você foi convidado a atuar no frontend em Angular do serviço de antecipação de valores.
Esse serviço é consumido por um sistema interno e expõe uma API REST para gerenciar as solicitações.

---

## Sua tarefa

**Gerar o documento** que servirá de **fonte de verdade para a etapa de implementação de código de produção**.

- **Entradas:** (1) Plano gerado pelo command criar-fonte-de-verdade em `[[plano]]` (ou ficheiro indicado pelo utilizador); (2) Referência à árvore de testes já criada para esta demanda.
- Se o utilizador não tiver indicado o plano, **pergunte** qual é o ficheiro da "fonte de verdade" (plano) a utilizar.
- **O plano que você gerar é para ser usado em conjunto com o que foi gerado anteriormente pelo command criar-fonte-de-verdade** (e com a árvore de testes) na etapa de implementação de código.

## Entradas obrigatorias e rastreabilidade

Antes de gerar o documento, confirmar explicitamente:

1. **Artefato de origem (`sourceArtifact`)**: card/requisito/bug/doc que originou a implementacao.
2. **Plano imediatamente anterior (`upstreamPlan`)**: ficheiro do plano de testes (`sot-testes`).
3. **Plano global relacionado (`globalPlan`)**: ficheiro `sot-global` da mesma demanda.

Se faltar qualquer referencia, parar e perguntar ao utilizador.

Ao salvar o output, seguir `.github/instructions/nomenclatura-planos.instructions.md`.

---

## Conteúdo a incluir no documento gerado

O documento (ex.: `.cursor/plans/<identificador>.plan.md`) deve conter:

1. **Local/escopo:** `[[local com o problema]]` ou área/funcionalidade a que se aplica.
2. **Resumo das alterações** a fazer (com base no plano do criar-fonte-de-verdade e na árvore de testes).
3. **Referência à árvore de testes** já criada.
4. **Regras para a etapa de implementação de código de produção:**
   - Implementar **apenas código de produção**; orientar-se pela árvore de testes já evoluída.
   - **Proibido** criar ou alterar testes nesta etapa, exceto quando for **extremamente necessário** para não deixar código novo sem cobertura.
   - **Foco em funcionalidade;** não é escopo zelar pelas melhores práticas de qualidade de código nesta etapa (isso será feito noutro comando, numa etapa posterior).
   - Na implementação, usar a skill **mercenário** (ler `.github/skills/mercenario/SKILL.md`): tradução de regras de negócio em código, sem alterar testes e sem análise de arquitetura/performance/qualidade.

---

## Regras obrigatórias para o documento gerado

(1) Proibido criar/alterar testes exceto quando extremamente necessário.  
(2) Indicar que o agente deve perguntar qual é o ficheiro da "fonte de verdade" (plano) se não tiver sido indicado.  
(3) Foco em funcionalidade, não em qualidade de código nesta etapa.
(4) O frontmatter deve conter `sourceArtifact`, `upstreamPlan`, `globalPlan`, `planType: sot-codigo`, `createdAt` e `updatedAt` (ISO-8601 UTC).

---

## Placeholders

- `[[plano]]` — ficheiro do plano (ex.: gerado pelo command criar-fonte-de-verdade) a usar como entrada.
- `[[local com o problema]]` — área, ecrã ou componente onde se aplica a correção/implementação.

Se o utilizador não tiver preenchido estes ao invocar o command, peça esses dados antes de gerar o documento.
