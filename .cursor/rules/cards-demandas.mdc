---
description: Aplicar quando for gerar ou alterar cards de demanda em `demandas/`. Define processo de validação com o operador, envolvimento contínuo, convenções de conteúdo e rastreabilidade.
alwaysApply: false
---

# Cards de demanda em `demandas/`

## Validação com o operador (foco principal)

- **Sempre validar com o operador antes de prosseguir.** Nunca criar ficheiros de card em massa sem validação.
- Para cada card: anunciar o que vai ser criado, apresentar o conteúdo **formatado no corpo da resposta** (não como único bloco de código markdown), aguardar validação ou orientação do operador e **só então** criar o ficheiro em `demandas/`.

## Envolvimento do operador (menos alienação)

- **Fazer mais perguntas** ao longo do processo: clarificar escopo, prioridades, personas, critérios de aceitação; propor opções e pedir preferência (ex.: "prefere que este card inclua X ou fique para outro?").
- Manter o operador **parte do processo**: resumir o que se entendeu antes de escrever o card; oferecer escolhas (ex.: fatiar por fluxo vs por persona); validar cada card antes de passar ao próximo.

## Processo de geração

- Gerar cards **um de cada vez**.

## Regras de negócio

- Regras vindas de especificação do cliente (ex.: `InstrucoesProjeto`) são **adições** às regras já existentes no sistema; não substituem.

## Requisitos técnicos em todo card

Incluir na secção "Requisitos técnicos/metodológicos aplicáveis":

- Boas práticas .NET/C#, design patterns quando fizer sentido, estruturas de dados, análise assintótica e gestão de processamento/memória.
- SOLID e os 3 princípios de coesão de componentes.
- **TDD** e **DDD** obrigatórios.
- **Pirâmide completa**: testes unitários, de integração e E2E.

## Riscos vs requisitos

- Se um risco for tratado **tornando-o requisito explícito** no card, **remover** esse item da secção "Dependências e riscos" (não manter duplicado).

## Rastreabilidade

- Após criar ou alterar cards de requisitos (ex.: RA-x), actualizar `docs/tracability.md` com o mapeamento: requisito → casos de uso (commands/queries/handlers), endpoints, testes (unit, integração, E2E).
