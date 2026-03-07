# Referência — Construção de Software

**Foco:** Tradução de regras de negócio para código; não cobre arquitetura, testes nem qualidade de código em geral.

**Definição:** Construção de software é a atividade de transformar regras de negócio em código: ler especificações, interpretar condições/restrições/fluxos do domínio e expressá-los de forma precisa e mantível na linguagem de programação.

---

## Uso por agentes de IA

- **Índice rápido:** Conceitos centrais (§1) | Lógica (§2) | Atividades (§3) | Técnicas (§4) | Padrões (§5) | Princípios (§6) | Estruturas e algoritmos (§7) | Normas (§8) | Índice de termos (§9).
- **Como usar:** Para "como expressar regra X no código" → §2 Lógica, §4 Técnicas, §8 Normas. Para "que conceito aplicar" → §1. Para "que fazer ao implementar" → §3. Para "que padrão usar" → §5.
- **Formato das entradas:** Cada item usa **Nome** (termo chave), depois Definição / Quando usar / No código, em bullets.

---

## 1. Conceitos centrais (regra de negócio → código)

| Termo | Definição | No código |
|-------|-----------|-----------|
| **Invariantes de negócio** | Condições que devem ser sempre verdadeiras para um conceito do domínio (ex.: saldo ≥ 0, data fim ≥ data início). | Validações em pontos únicos: construtores, setters, métodos que alteram estado; asserções ou checagens antes/depois de operações. |
| **Pré-condições e pós-condições** | O que deve ser verdade antes de uma operação e o que deve ser verdade depois. | Delimitam responsabilidade do método (ex.: só aprovar se status = Pendente; após aprovar, status = Aprovado). |
| **Estado do domínio** | Conjunto de dados que representa o negócio em um momento (entidade, agregado, valor). | A regra define transições de estado permitidas e o que cada operação faz com esse estado. |
| **Regra como expressão vs. processo** | Regra pode ser expressão (cálculo, condição, fórmula) ou processo (sequência de passos, fluxo). | Expressão → funções puras/expressões. Processo → métodos com condicionais e loops. |
| **Vocabulário do domínio no código** | Nomes refletem termos do negócio. | Tipos, métodos, variáveis e constantes com nomes do domínio: `podeAprovar()`, `calcularMultaAtraso()`, `StatusPedido.PAGO`. |

---

## 2. Lógica (a “engrenagem” da regra no código)

A regra ganha corpo técnico com os elementos da linguagem para decisão, repetição e transformação.

| Elemento | Papel | Exemplo de uso na regra |
|----------|--------|--------------------------|
| **Operadores e expressões** | Construir condições e cálculos. | Aritméticos, comparação, lógicos (e, ou, não), texto. Ex.: `valor >= minimo && valor <= limite`, `preco * quantidade * (1 - desconto)`. |
| **Condições (decisão)** | Ramificar comportamento conforme estado ou inputs. | `if/else`, `switch/match`, ternário. Ex.: desconto só se cliente premium; tratar cada tipo de evento. |
| **Iteração** | Aplicar regra a vários itens ou repetir passo até condição. | `for`, `while`, `forEach`, recursão. Ex.: itens do pedido, parcelas, juros compostos, etapas de fluxo. |
| **Variáveis e constantes** | Valores intermediários e nomes com significado. | `valorComDesconto`, `TAXA_JUROS_MENSAL`; constantes evitam números mágicos. |
| **Métodos/funções e tipos** | Agrupar lógica sob nome do domínio; modelar conceitos. | `calcularTotal()`, `podeEmitirNota()`; tipos `Pedido`, `Parcela`, `RegraDesconto`. |

**Imperativo vs. declarativo:** Imperativo = passo a passo (faça A, depois B). Declarativo = dizer o que se quer (ex.: total = soma dos itens com desconto). Preferir o que deixar a intenção da regra mais clara (ex.: `reduce` em vez de loop com variável mutável).

---

## 3. Atividades de alto nível (da especificação ao código)

| Atividade | O que fazer |
|-----------|-------------|
| **Ler e interpretar a especificação** | Entender requisitos funcionais, regras explícitas (“o sistema deve…”), restrições; identificar entidades, operações, cenários (fluxo feliz e exceções de negócio). |
| **Identificar entidades, valores e operações** | Mapear substantivos → tipos (classes, structs, value objects); verbos/ações → métodos ou comandos; definir onde cada regra “vive”. |
| **Mapear condições para código** | Transformar “se X então Y” em condicionais, guard clauses ou early returns; cobrir todas as ramificações (incluindo “caso contrário”). |
| **Expressar cálculos e fórmulas** | Traduzir fórmulas de negócio (imposto, multa, desconto) em expressões e funções; usar constantes nomeadas para parâmetros. |
| **Modelar estados e transições** | Quando a regra depende de estado: representar estados (enum, constantes, tipo) e restringir transições (ex.: só “pago” se valor recebido; só cancelar se ainda não enviado). |
| **Tratar exceções de negócio** | Decidir representação: retorno (resultado, optional, Either), exceção de negócio ou valor de erro; manter consistência no tratamento (ex.: pedido recusado por estoque insuficiente). |
| **Revisar aderência** | Garantir que cada regra documentada tem um lugar claro no código e que não há comportamento extra não especificado. |

---

## 4. Técnicas para expressar regras no código

| Técnica | Definição | Quando usar |
|---------|-----------|-------------|
| **Guard clauses** | Validar pré-condições no início do método e sair cedo se violadas (return ou exceção de negócio). | Para deixar o caminho feliz linear e as exceções explícitas. |
| **Early return** | Retornar assim que o resultado for conhecido. | Para reduzir aninhamento de `if/else` e destacar cada ramo da regra. |
| **Extrair condição para método/função** | Dar nome de domínio a uma expressão booleana. | Ex.: `podeAprovar()`, `estaDentroDoPrazo()` — melhora leitura e reuso. |
| **Extrair cálculo para função** | Colocar fórmulas e agregações em funções com nome do negócio. | Ex.: `calcularMulta()`, `calcularTotalComImposto()` — centraliza regra de cálculo. |
| **Tabelas de decisão em código** | Regra “para cada combinação de A, B, C → resultado X”. | Implementar com mapa (chave composta → resultado) ou `switch`/pattern matching em vez de muitos `if`. |
| **Máquina de estados** | Tipo de estado + funções (estado atual, evento) → novo estado ou erro. | Fluxos com estados bem definidos e transições condicionais; regra concentrada nas transições. |
| **Regras como dados** | Regras em estruturas de dados (condição + ação/valor); motor que as interpreta. | Muitas regras configuráveis (descontos por faixa, limites por perfil) em vez de muitos `if` hardcoded. |
| **Constantes e enums para opções** | Status, tipos, códigos e faixas como constantes ou enums. | Comparações e `switch` legíveis; regra não depende de strings ou números soltos. |

---

## 5. Padrões úteis para regras de negócio

| Padrão | Intent | Quando usar na regra |
|--------|--------|------------------------|
| **Strategy** | Intercambiar algoritmos de uma família. | Diferentes formas de calcular desconto, frete, etc.; regra concreta em classe/função, escolhida em tempo de execução. |
| **Specification** | Encapsular condição de negócio em objeto (ex.: `isSatisfiedBy(candidato)`). | Combinar especificações (e, ou, não); reutilizar regra em validações e consultas. |
| **Rule Object / Policy** | Uma classe ou módulo = uma regra ou política. | Ex.: `RegraLimiteCredito`, `PoliticaDescontoAniversario`; orquestração chama regras em vez de lógica inline. |
| **State** | Comportamento muda conforme estado; estados como tipos, lógica por estado. | Quando há muitos `if (status == …)`; pedido rascunho vs. enviado vs. pago. |
| **Template Method** | Esqueleto de fluxo fixo com passos variáveis. | Ex.: calcular base → descontos → impostos; cada passo pode ser sub-regra. |
| **Chain of Responsibility** | Sequência de regras; cada elo processa ou repassa. | Aprovação por valor: até X aprovador A, acima de X aprovador B. |

**Outros:** Padrões **criacionais** (Factory, Builder) quando a criação do objeto de negócio exige regras (validações, defaults, composição). **Estruturais** (Adapter, Facade) para integrar regras externas sem misturar com a lógica do domínio.

---

## 6. Princípios na escrita (mindset)

| Princípio | Regra para o agente |
|-----------|----------------------|
| **DRY** | Não duplicar a mesma regra. Se condição ou cálculo aparece em mais de um ponto, extrair para função/método ou tipo; alterar em um só lugar. |
| **KISS** | Preferir a solução mais simples que implemente corretamente a regra. Não introduzir abstrações ou padrões “por precaução”. |
| **YAGNI** | Implementar só o que a demanda e a especificação pedem. Não codificar regras ou cenários “para o futuro” sem requisito. |

---

## 7. Estruturas de dados e algoritmos (a serviço da regra)

| Aspecto | Orientação |
|---------|------------|
| **Estruturas de dados** | Listas → sequências ordenadas (itens do pedido, parcelas). Mapas/dicionários → busca por chave (regras por tipo de cliente, configurações por perfil). Conjuntos → unicidade (ids já processados). Filas → ordem de processamento importa. Escolha errada dificulta expressão da regra ou desempenho. |
| **Algoritmos** | Sequência de passos da regra: busca, filtragem, ordenação, agregação (soma, média, contagem), transformação (map). Preferir construções idiomáticas da linguagem (iteradores, streams) quando deixarem a intenção mais clara. |

Memória (stack, heap) em geral importa para desempenho/alocação; a regra continua expressa com condições, loops, funções e tipos.

---

## 8. Normas e boas práticas (tradução da regra para código)

| Norma | Aplicação |
|-------|------------|
| **Um lugar para cada regra** | Cada regra de negócio tem um único ponto no código onde está implementada. Evitar duplicação. |
| **Nomes do domínio** | Tipos, métodos, variáveis e constantes refletem vocabulário do negócio. Evitar genéricos (`dado`, `processar`, `valor1`); preferir termos precisos (`Pedido`, `calcularMultaAtraso`, `limiteCredito`). |
| **Condições explícitas** | Evitar negações duplas e condições difíceis de ler. Preferir `if (podeAprovar)` em vez de `if (!naoPodeAprovar)`. |
| **Sem números/strings mágicos** | Usar constantes ou enums nomeados (ex.: `LIMITE_MAXIMO_PARCELAS`, `Status.Cancelado`). |
| **Exceções de negócio claras** | Quando a operação não pode ser feita por regra, deixar explícito no nome (ex.: `PedidoRecusadoEstoqueInsuficiente`) e no tratamento. |
| **Métodos/funções focados** | Uma responsabilidade de negócio identificável por método; se mistura várias regras, extrair para funções auxiliares com nomes do domínio. |
| **Imutabilidade quando possível** | Para valores e entidades do domínio, preferir retornar novos valores/estados em vez de mutar; facilita raciocinar sobre a regra. |

---

## 9. Índice de termos (para busca por agentes)

- **Atividades:** ler especificação, identificar entidades/operações, mapear condições, expressar cálculos, modelar estados/transições, tratar exceções de negócio, revisar aderência.
- **Conceitos:** invariantes de negócio, pré-condições, pós-condições, estado do domínio, regra como expressão, regra como processo, vocabulário do domínio.
- **Lógica:** operadores, expressões, condições, decisão, iteração, variáveis, constantes, métodos, funções, tipos, imperativo, declarativo.
- **Técnicas:** guard clauses, early return, extrair condição, extrair cálculo, tabela de decisão, máquina de estados, regras como dados, constantes, enums.
- **Padrões:** Strategy, Specification, Rule Object, Policy, State, Template Method, Chain of Responsibility, Factory, Builder, Adapter, Facade.
- **Princípios:** DRY, KISS, YAGNI.
- **Normas:** um lugar por regra, nomes do domínio, condições explícitas, sem números mágicos, exceções de negócio claras, métodos focados, imutabilidade.

---

**Objetivo:** O código deve ser tradução fiel e legível da regra de negócio, permitindo alterações com segurança e clareza.
