# [TA-050] Regra de listagem de solicitações por `creator_id`

## Contexto
O sistema interno da LastTechTest precisa consultar o histórico de solicitações de antecipação de um criador para análise, atendimento e auditoria. A API deve fornecer uma listagem clara e consistente dessas solicitações, com campos relevantes expostos.

## Objetivo
Disponibilizar uma forma consistente de listar todas as solicitações de antecipação associadas a um determinado `creator_id`, retornando as informações necessárias para o sistema interno.

## Escopo
- Incluir neste card:
  - Caso de uso/serviço de aplicação para listar solicitações por `creator_id`.
  - Definição dos campos retornados (ex.: `id`, `creator_id`, `valor_solicitado`, `valor_liquido`, `status`, `data_solicitacao`).
  - Critérios de ordenação (ex.: por `data_solicitacao` desc, se fizer sentido).
  - Endpoint GET correspondente na API (ex.: `GET /api/v1/solicitacoes?creator_id=...`).
- Fica fora deste card:
  - Qualquer alteração nas regras de criação, aprovação/recusa ou simulação.
  - Paginação, filtros adicionais ou relatórios avançados (podem ser tratados em extensões futuras).

## Critérios de aceite (cenários de teste)
- Cenário 1 – Creator sem solicitações:
  - Dado um `creator_id` sem solicitações registradas,
  - Quando o endpoint/caso de uso de listagem é chamado,
  - Então o sistema retorna uma lista vazia (ex.: `[]`) com sucesso.
- Cenário 2 – Creator com múltiplas solicitações:
  - Dado um `creator_id` com solicitações em estados variados (`pendente`, `aprovada`, `recusada`),
  - Quando o endpoint/caso de uso de listagem é chamado,
  - Então o sistema retorna todas as solicitações associadas a esse `creator_id` com os campos definidos, na ordem esperada.
- Cenário 3 – Validação de `creator_id`:
  - Dado uma chamada com `creator_id` em formato inválido ou ausente,
  - Quando o endpoint é chamado,
  - Então o sistema retorna erro de validação adequado (ver alinhamento com `TA-090`).

## Estratégia TDD da tarefa
- Especificar testes:
  - Unitários para o caso de uso/serviço de listagem, usando repositórios ou fontes de dados em memória/mocks.
  - De integração do endpoint HTTP, verificando o contrato de resposta.
- Implementar primeiro o caso de uso usando uma fonte de dados simples (ex.: repositório em memória),
  - depois conectar à persistência real se/quando existir, mantendo os testes verdes.

## Dependências
- Depende de:
  - `TA-001` – Esqueleto mínimo da aplicação de antecipação.
  - `TA-010` – Modelagem da entidade de solicitação de antecipação.
- Relaciona-se com:
  - `TA-090` – Regras de validação de entrada, para padronizar erros de `creator_id` inválido.

## Subcards
- (Opcional) `TA-050-01` – Paginação e filtros adicionais (por status, datas, etc.), se o escopo do desafio for estendido.*** End Patch
