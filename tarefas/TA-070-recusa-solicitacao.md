# [TA-070] Regra de recusa de solicitação

## Contexto
Nem todas as solicitações de antecipação serão aprovadas; algumas precisam ser recusadas com base em critérios internos (risco, política, inconsistências de dados etc.). A recusa altera o status da solicitação e pode liberar o criador para realizar uma nova solicitação pendente no futuro.

## Objetivo
Implementar a regra de recusa de solicitação, permitindo a transição de `pendente` para `recusada` e impedindo recusas inválidas.

## Escopo
- Incluir neste card:
  - Caso de uso/serviço de aplicação que represente a recusa de uma solicitação.
  - Regra: apenas solicitações com status `pendente` podem ser recusadas.
  - Definição clara de erro quando a recusa for tentada em estados inválidos (`aprovada` ou `recusada`).
  - Atualização de status persistido após recusa bem-sucedida.
- Fica fora deste card:
  - Lógica de aprovação (tratada em `TA-060`).
  - Detalhamento dos motivos de recusa além do necessário para o desafio (pode ser estendido futuramente).

## Critérios de aceite (cenários de teste)
- Cenário 1 – Recusa válida:
  - Dado uma solicitação com status `pendente`,
  - Quando o fluxo de recusa é executado,
  - Então o status da solicitação passa a `recusada`.
- Cenário 2 – Recusa de já recusada:
  - Dado uma solicitação com status `recusada`,
  - Quando o fluxo de recusa é executado novamente,
  - Então a operação falha com erro adequado, sem alterar o registro.
- Cenário 3 – Recusa de aprovada:
  - Dado uma solicitação com status `aprovada`,
  - Quando o fluxo de recusa é executado,
  - Então a operação falha com erro adequado, sem alterar o registro.

## Estratégia TDD da tarefa
- Especificar testes:
  - Unitários para o caso de uso de recusa, cobrindo os três cenários principais.
  - (Opcional) Testes de integração para o endpoint correspondente.
- Implementar a lógica de transição de estado e persistência até que os testes passem.
- Refatorar conforme necessário para manter a regra bem encapsulada e simétrica à aprovação.

## Dependências
- Depende de:
  - `TA-001` – Esqueleto mínimo da aplicação de antecipação.
  - `TA-010` – Modelagem da entidade de solicitação de antecipação.
- Relaciona-se com:
  - `TA-040` – Regra de unicidade de solicitação pendente por creator (após recusa, o criador pode voltar a criar nova pendente).
  - `TA-100` – Consolidação do contrato público da API de antecipação (v1), ao expor o endpoint de recusa.

## Subcards
- (Opcional) `TA-070-01` – Registro de motivo estruturado de recusa (códigos/mensagens), caso o escopo do desafio seja ampliado.*** End Patch
