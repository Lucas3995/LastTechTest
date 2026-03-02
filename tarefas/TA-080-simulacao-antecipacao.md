# [TA-080] Regra de simulação de antecipação (sem persistência)

## Contexto
Para ajudar o criador a decidir se vale a pena solicitar a antecipação, o sistema deve oferecer um endpoint de simulação que calcule quanto ele receberia líquido, sem gravar nenhuma solicitação. O desafio pede explicitamente um endpoint GET com query params para esse fim.

## Objetivo
Implementar a simulação de antecipação que, a partir de parâmetros de entrada, calcule a taxa e o `valor_liquido` usando as mesmas regras de negócio da criação de solicitação, sem criar registros persistidos.

## Escopo
- Incluir neste card:
  - Função/serviço de aplicação para simular antecipação, recebendo os parâmetros necessários (ex.: `creator_id`, `valor_solicitado`, `data_solicitacao`).
  - Uso da mesma regra de cálculo de taxa e `valor_liquido` definida em `TA-030`.
  - Endpoint GET de simulação, com query params para os campos de entrada (ex.: `GET /api/v1/solicitacoes/simulacao?creator_id=...&valor_solicitado=...`).
  - Garantia de que nenhuma entidade de `SolicitacaoAntecipacao` seja criada/persistida durante a simulação.
- Fica fora deste card:
  - Regras de valor mínimo e unicidade de pendente (aplicadas na criação real, não na simulação), a menos que o time decida explicitamente aplicá-las também à simulação.

## Critérios de aceite (cenários de teste)
- Cenário 1 – Simulação básica:
  - Dado um conjunto de parâmetros válidos (incluindo `valor_solicitado` > R$ 100,00),
  - Quando o endpoint de simulação é chamado,
  - Então o sistema retorna um payload contendo `valor_solicitado`, taxa calculada e `valor_liquido`, de acordo com a regra de 5%.
- Cenário 2 – Não persistência:
  - Dado que não existem solicitações registradas,
  - Quando o endpoint de simulação é chamado várias vezes com diferentes parâmetros,
  - Então, após as chamadas, continua não existindo nenhuma solicitação registrada no sistema.
- Cenário 3 – Consistência com cálculo central:
  - Dado um determinado `valor_solicitado`,
  - Quando simulação e criação real usam o mesmo cálculo,
  - Então o `valor_liquido` e a taxa retornados pela simulação são idênticos aos que seriam aplicados na criação.

## Estratégia TDD da tarefa
- Especificar testes:
  - Unitários para o serviço de simulação, usando diretamente a função central de cálculo de taxa/valor líquido (`TA-030`).
  - De integração para o endpoint GET, verificando parâmetros, resposta e ausência de efeitos colaterais.
- Implementar a lógica de simulação até que os testes passem, garantindo que nenhuma camada de persistência é acionada para criar registros.

## Dependências
- Depende de:
  - `TA-001` – Esqueleto mínimo da aplicação de antecipação.
  - `TA-030` – Regra de cálculo de taxa e valor líquido (5%).
- Relaciona-se com:
  - `TA-090` – Regras de validação de entrada, para garantir query params válidos.
  - `TA-100` – Consolidação do contrato público da API de antecipação (v1), ao expor o endpoint de simulação.

## Subcards
- (Opcional) `TA-080-01` – Versão avançada da simulação, considerando múltiplas parcelas/recebíveis, caso o escopo seja ampliado.*** End Patch
