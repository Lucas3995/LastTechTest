# [TA-020] Regra de criação de solicitação (valor mínimo + status inicial)

## Contexto
Quando um criador deseja antecipar parte de seus recebíveis, ele cria uma solicitação de antecipação. O desafio exige que essa criação respeite um valor mínimo e que todas as solicitações iniciem com status `pendente`, garantindo consistência de negócio desde o primeiro passo do fluxo.

## Objetivo
Implementar a regra de criação de solicitação de antecipação garantindo que:
- O valor solicitado seja maior que R$ 100,00.
- Toda solicitação criada tenha status inicial `pendente`.

## Escopo
- Incluir neste card:
  - Fluxo de criação de solicitação no domínio/camada de aplicação (ex.: comando CQRS ou serviço de aplicação).
  - Validação de valor mínimo > R$ 100,00.
  - Definição do status inicial como `pendente` na criação bem-sucedida.
  - Integração mínima com a entidade `SolicitacaoAntecipacao` definida em `TA-010`.
  - Exposição dessa criação via API (endpoint POST) pode ser iniciada aqui ou refinada em `TA-100`, desde que os testes de criação sejam cobertos.
- Fica fora deste card:
  - Regra de unicidade de solicitação pendente por `creator_id` (tratada em `TA-040`).
  - Regras de aprovação/recusa (tratadas em `TA-060` e `TA-070`).
  - Detalhes de autenticação/autorização.

## Critérios de aceite (cenários de teste)
- Cenário 1 – Criação válida:
  - Dado um `creator_id` válido, `valor_solicitado` = R$ 150,00 e uma `data_solicitacao` válida,
  - Quando o caso de uso de criação é executado,
  - Então a solicitação é criada com status `pendente` e retorna sucesso.
- Cenário 2 – Valor igual a 100,00:
  - Dado um `creator_id` válido e `valor_solicitado` = R$ 100,00,
  - Quando o caso de uso de criação é executado,
  - Então a criação falha com erro de validação indicando que o valor deve ser maior que R$ 100,00.
- Cenário 3 – Valor menor que 100,00:
  - Dado um `creator_id` válido e `valor_solicitado` = R$ 50,00,
  - Quando o caso de uso de criação é executado,
  - Então a criação falha com erro de validação adequado.

## Estratégia TDD da tarefa
- Especificar primeiro testes:
  - Unitários para o caso de uso/serviço de criação (cenários de sucesso e falha por valor baixo).
  - De integração (opcional neste card) exercendo o endpoint de criação via HTTP.
- Implementar o mínimo de lógica de validação e criação para fazer os testes passarem.
- Refatorar para padrões de arquitetura (CQRS, validações dedicadas) conforme necessário, mantendo a suíte verde.

## Dependências
- Depende de:
  - `TA-001` – Esqueleto mínimo da aplicação de antecipação.
  - `TA-010` – Modelagem da entidade de solicitação de antecipação.
- É pré-requisito para:
  - `TA-040` – Regra de unicidade de solicitação pendente por creator.
  - `TA-100` – Consolidação do contrato público da API de antecipação (v1).

## Subcards
- (Opcional) `TA-020-01` – Exposição do fluxo de criação especificamente via API HTTP (mapeamento request/response), caso o time queira separar domínio/aplicação da camada de transporte.*** End Patch```} */}
  } */}
*** End Patch```} */}
*** End Patch */}
*** End Patch */}
*** End Patch */}
*** End Patch */}
*** End Patch */}
*** End Patch */}
*** End Patch */}
*** End Patch */}
*** End Patch */}
*** End Patch */}
*** End Patch */}
*** End Patch */}
*** End Patch */}
*** End Patch */}
*** End Patch */}
*** End Patch */}
*** End Patch */}
*** End Patch */}
*** End Patch */} */}
*** End Patch */}
*** End Patch */}
*** End Patch */}
*** End Patch */}
*** End Patch */}
*** End Patch */}
*** End Patch */}
*** End Patch */}
*** End Patch */}
*** End Patch */}
*** End Patch */}
*** End Patch */}
*** End Patch */}
*** End Patch */}
*** End Patch */}
*** End Patch */}
*** End Patch */}
