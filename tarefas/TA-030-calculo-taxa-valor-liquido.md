# [TA-030] Regra de cálculo de taxa e valor líquido (5%)

## Contexto
Toda solicitação de antecipação e toda simulação dependem de um cálculo consistente da taxa de antecipação e do valor líquido a ser recebido pelo criador. O desafio define que a taxa é fixa em 5% sobre o valor bruto solicitado, e o sistema deve refletir isso de forma centralizada no domínio.

## Objetivo
Implementar a regra de cálculo de taxa e `valor_liquido` a partir de `valor_solicitado`, com taxa fixa de 5%, garantindo consistência entre criação de solicitação e simulação.

## Escopo
- Incluir neste card:
  - Função/método de domínio ou serviço de domínio responsável por calcular:
    - `taxa = valor_solicitado * 0.05`
    - `valor_liquido = valor_solicitado - taxa` (ou equivalente `valor_solicitado * 0.95`).
  - Definição clara de como tratar casas decimais e arredondamento (ex.: para 2 casas decimais).
  - Uso dessa regra na criação de solicitação e na simulação (sem duplicação de lógica).
- Fica fora deste card:
  - Persistência de resultados (parte de outros cards).
  - Regras de valor mínimo e status (já cobertas em `TA-020`).

## Critérios de aceite (cenários de teste)
- Cenário 1 – Cálculo simples:
  - Dado `valor_solicitado` = R$ 1.000,00,
  - Quando o cálculo for executado,
  - Então a taxa é R$ 50,00 e o `valor_liquido` é R$ 950,00.
- Cenário 2 – Vários valores (incluindo limites):
  - Dado diferentes valores válidos (ex.: R$ 150,00; R$ 10.000,00),
  - Quando o cálculo for executado para cada um,
  - Então o `valor_liquido` é sempre `valor_solicitado * 0.95` com o arredondamento definido.
- Cenário 3 – Reutilização da regra:
  - Dado o uso da regra tanto na criação de solicitação quanto na simulação,
  - Quando ambos os fluxos são exercitados para o mesmo `valor_solicitado`,
  - Então o resultado (`valor_liquido` e taxa) é idêntico.

## Estratégia TDD da tarefa
- Especificar testes unitários para:
  - Função/método de cálculo isolado.
  - Casos de borda (valores grandes, valores com muitas casas decimais, etc.).
- Implementar a função/método de cálculo até que os testes passem.
- Integrar essa função com o fluxo de criação (`TA-020`) e com o fluxo de simulação (`TA-080`) aos poucos, adicionando testes de integração conforme necessário.

## Dependências
- Depende de:
  - `TA-001` – Esqueleto mínimo da aplicação de antecipação.
- É pré-requisito para:
  - `TA-020` – Regra de criação de solicitação (para cálculo de `valor_liquido` ao criar).
  - `TA-080` – Regra de simulação de antecipação (sem persistência).

## Subcards
- (Opcional) `TA-030-01` – Extração de um value object de dinheiro/valor monetário para encapsular arredondamento e operações monetárias, caso o time queira sofisticar a modelagem.*** End Patch
