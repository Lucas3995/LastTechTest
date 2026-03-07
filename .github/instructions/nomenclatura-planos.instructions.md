---
applyTo: "**/*.plan.md"
---

# Nomenclatura e rastreabilidade de planos

## Objetivo

Garantir que qualquer fonte de verdade/plano seja facil de localizar, ordenar e auditar.

## Padrao de nomenclatura

Todo novo plano em `.cursor/plans/` deve seguir:

`YYYYMMDD_<tipo>_<identificador>.plan.md`

### Tipos permitidos

- `sot-global`
- `sot-testes`
- `sot-codigo`
- `sot-refatoracao`
- `sot-entrega`
- `sot-outro`

### Regras do identificador

- Usar `kebab-case`.
- Preferir referencia de demanda ou escopo (`rf-7-operacao-analista`, `bug-404-minhas-solicitacoes`).
- Sem espacos, acentos ou caracteres especiais.

## Frontmatter minimo obrigatorio

Todo plano deve declarar no frontmatter:

- `name`
- `overview`
- `sourceArtifact`: card/requisito/bug/doc que originou o plano.
- `upstreamPlan`: ficheiro do plano anterior na cadeia (`none` quando plano inicial).
- `planType`: um dos tipos permitidos.
- `createdAt`: ISO-8601 UTC (`YYYY-MM-DDTHH:mm:ss.sssZ`).
- `updatedAt`: ISO-8601 UTC (`YYYY-MM-DDTHH:mm:ss.sssZ`).
- `todos`: nao vazio (ver `.github/instructions/planos-todos.instructions.md`).

## Regras de cadeia

1. `sot-global` deve apontar para `upstreamPlan: none` (ou plano global anterior quando for evolucao da mesma demanda).
2. `sot-testes` deve apontar para um `sot-global`.
3. `sot-codigo` deve apontar para um `sot-testes` e citar o `sot-global` relacionado.
4. `sot-refatoracao` deve apontar para `none` (analise inicial) ou para o ultimo `sot-refatoracao` da mesma trilha.

Se um plano obrigatorio anterior nao for informado, o agente deve perguntar ao operador antes de continuar.

## Compatibilidade com legado

- Planos antigos podem manter nome legado.
- Ao evoluir um plano legado, usar o novo padrao de nome.
- Sempre incluir uma linha no corpo com `Legado relacionado:` quando houver ficheiro antigo equivalente.
