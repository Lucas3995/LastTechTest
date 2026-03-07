---
description: "Criar plano (fonte de verdade) para a árvore de testes — usa skill quadro-de-recompensas"
agent: agent
---

# Criar plano (fonte de verdade) para a árvore de testes

## Propósito

Produzir um **documento de plano** que serve de fonte de verdade específica para a **criação da árvore de testes** no frontend Angular (correção de bugs ou feature). Este plano será usado em conjunto com o plano global (do comando criar-fonte-de-verdade) na etapa posterior "criação de TESTES". **Modo plano:** não criar nem alterar código; apenas analisar e produzir o plano.

---

## Contexto

Trabalhamos em metodologia **spec-driven development** para desenvolvimento assistido por IA. Você é um arquiteto e engenheiro de software frontend sénior (TypeScript/Angular). A LastLink permite que criadores recebam suas receitas pela plataforma; o serviço de antecipação (criador solicita antecipação de recebíveis mediante taxa) é consumido por um sistema interno e expõe API REST. Você atua no frontend Angular desse serviço.

---

## Pré-condição

- O **plano global** (resultado do comando **criar-fonte-de-verdade**) já existe e é contexto obrigatório.
- Se o utilizador não indicar o ficheiro do plano global, **pergunte** qual é o ficheiro da fonte de verdade global (ex.: em `.cursor/plans/` ou caminho que ele fornecer).
- Parâmetros opcionais após o comando: caminho do plano global; local do problema (ex.: "listagem das solicitações do creator logado") para afinar o âmbito.

## Entradas obrigatorias e rastreabilidade

Antes de gerar o plano da arvore de testes, confirmar explicitamente:

1. **Artefato de origem (`sourceArtifact`)**: card/requisito/bug que originou a demanda.
2. **Plano imediatamente anterior (`upstreamPlan`)**: ficheiro do plano global (`sot-global`) que esta sendo decomposto para testes.

Se qualquer item estiver ausente, parar e perguntar ao utilizador.

Ao salvar o output, seguir `.github/instructions/nomenclatura-planos.instructions.md`.

---

## Modo de operação

- **Apenas produzir plano — sem editar código.** Use a ferramenta CreatePlan (ou documento markdown equivalente) para gerar o output.
- Ao produzir o plano, use os critérios da skill **quadro-de-recompensas** (formato relatório de tarefas para a secção de alterações) e da skill **mestre-freire-angular** (estrutura de camadas e convenções do frontend Angular; regra `.github/instructions/angular-frontend.instructions.md`) para que o output seja utilizável na etapa "criação de TESTES".
- Consulte as skills **quadro-de-recompensas** e **mestre-freire-angular** quando for definir a estrutura da árvore de testes.
- Sugestão de nome do ficheiro de output: `plano-arvore-testes_<identificador>.plan.md` em `.cursor/plans/`.

---

## Conteúdo do plano a gerar

O plano deve incluir, alinhado aos exemplos em `.cursor/plans/` e `docs/frontend-rf1-minhas-solicitacoes-arvore-testes.md`:

1. **Referência ao plano global** — Nome/ficheiro e resumo do problema ou demanda.
2. **Âmbito do frontend** — Páginas, componentes, fachadas, serviços, respeitando a estrutura e convenções da skill mestre-freire-angular (camadas, áreas, módulos; regra angular-frontend).
3. **Mapeamento** — Itens do plano global (ex.: alterações 4.1, 4.2, …) → ramos/casos de teste (unit, integração, E2E).
4. **Estrutura da árvore de testes** — Quais ficheiros `*.spec.ts` (e E2E se aplicável), cenários por tipo (unit/integ/e2e), nomes sugeridos de testes quando fizer sentido.
5. **Secção em formato relatório de tarefas** (compatível com **quadro-de-recompensas**): por item, identificador, Onde, Tipo (Criar | Alterar | Remover | Integrar), Descrição, Requisito atendido — para que a etapa "criação de TESTES" possa invocar a skill quadro-de-recompensas com este plano.
6. **Ordem sugerida** para criar/evoluir os testes (para uso na etapa "criação de TESTES").

---

## Uso posterior

Este plano (e o plano global) serão usados em conjunto no comando/prompt de **"criação de TESTES"**: aquele passo usa este plano como `[[planoComAsOrientacoes]]` e cria apenas a árvore de testes (skill quadro-de-recompensas), sem código de produção — salvo o mínimo extremamente necessário para o build. Se o utilizador não indicar o ficheiro da fonte de verdade na etapa de criação de testes, o agente deve perguntar.

No frontmatter do plano gerado, incluir no minimo:

- `sourceArtifact`
- `upstreamPlan`
- `planType: sot-testes`
- `createdAt` e `updatedAt` em ISO-8601 UTC
