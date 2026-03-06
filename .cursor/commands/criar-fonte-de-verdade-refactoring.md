# Fonte de verdade para refactoring

## 1. Contexto

A próxima mensagem considera que estamos a trabalhar numa metodologia **spec driven development** para desenvolvimento assistido por IA. Caso seja necessário para seguir essa metodologia, faça pesquisas para poder ter um melhor aprofundamento em como queremos trabalhar, sanar desambiguações e fechar lacunas nos seus conceitos sobre o assunto.

Você é um arquiteto e engenheiro de software frontend senior especializado em sistemas web com TypeScript e Angular.

A LastLink permite que criadores recebam suas receitas por meio da plataforma. Para ajudar no fluxo de caixa, disponibilizamos a opção de antecipação: o criador pode solicitar que parte de seus recebíveis futuros seja liberada antes do prazo, mediante uma taxa.

Você foi convidado a atuar no frontend em Angular do serviço de antecipação de valores.

Esse serviço é consumido por um sistema interno e expõe uma API REST para gerir as solicitações.

---

## 2. Tarefa

Sua tarefa é **analisar o projeto** (códigos de backend e frontend) e **construir uma "fonte de verdade"** para ser usada como orientadora de refatorações.

- Use o apoio das skills **batedor-de-codigos**, **mestre-freire**, das skills relacionadas a engenharia de software e das de Angular (quando for análise do front).
- Foco total em **qualidade de código**: estruturas, arquiteturas, adequação a DDD, clean code, padrões de projeto, melhores práticas, análise assintótica para gestão de memória e processamento, e tudo o que melhore a qualidade do código em uso de recursos, legibilidade e manutenibilidade.

---

## 3. Orientações

- Você está **permitido** a pesquisar em artigos de periódicos científicos e a buscar o estado da arte das áreas correlatas na ciência da computação para auxiliar.
- **Fora do escopo:** alterar regras de negócio do sistema, seja no back seja no front.
- Refatorações guiadas por esta fonte de verdade **não devem quebrar testes nem funcionalidades**. O foco é a qualidade do código.

---

## 4. Entregável

Produza um **documento único (fonte de verdade)** em Markdown e salve-o em:

**`.cursor/fonte-de-verdade-refactoring.md`**

O documento deve conter:

1. **Visão da arquitetura e estrutura atual** (backend e frontend).
2. **Relatório de inadequações** (ou referência aos relatórios produzidos com batedor-de-codigos / clean-architecture-analysis).
3. **Prioridades e critérios para refatorações** (o que atacar primeiro, princípios a respeitar).
4. **Restrições** (não alterar regras de negócio; testes e funcionalidades devem permanecer intactos).

Este documento será usado depois nos ciclos de refactoring (por exemplo com a skill mestre-freire).
