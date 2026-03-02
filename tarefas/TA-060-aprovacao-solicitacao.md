# [TA-060] Regra de aprovação de solicitação

## Contexto
Após a criação de uma solicitação de antecipação, um operador interno precisa aprová-la quando os critérios internos forem atendidos. A aprovação altera o status da solicitação e impacta diretamente a disponibilidade de novas solicitações pendentes para o mesmo criador.

## Objetivo
Implementar a regra de aprovação de solicitação, permitindo a transição de `pendente` para `aprovada` e bloqueando aprovações inválidas.

## Escopo
- Incluir neste card:
  - Caso de uso/serviço de aplicação que represente a aprovação de uma solicitação.
  - Regra: apenas solicitações com status `pendente` podem ser aprovadas.
  - Definição clara de erro quando a aprovação for tentada em estados inválidos (`aprovada` ou `recusada`).
  - Atualização de status persistido após aprovação bem-sucedida.
- Fica fora deste card:
  - Lógica de recusa (tratada em `TA-070`).
  - Regras de autenticação/autorização de quem pode aprovar.

## Critérios de aceite (cenários de teste)
- Cenário 1 – Aprovação válida:
  - Dado uma solicitação com status `pendente`,
  - Quando o fluxo de aprovação é executado,
  - Então o status da solicitação passa a `aprovada`.
- Cenário 2 – Aprovação de já aprovada:
  - Dado uma solicitação com status `aprovada`,
  - Quando o fluxo de aprovação é executado novamente,
  - Então a operação falha com erro adequado, sem alterar o registro.
- Cenário 3 – Aprovação de recusada:
  - Dado uma solicitação com status `recusada`,
  - Quando o fluxo de aprovação é executado,
  - Então a operação falha com erro adequado, sem alterar o registro.

## Estratégia TDD da tarefa
- Especificar testes:
  - Unitários para o caso de uso de aprovação, cobrindo os três cenários principais.
  - (Opcional) Testes de integração para o endpoint correspondente.
- Implementar a lógica de transição de estado e persistência até que os testes passem.
- Refatorar, se necessário, para manter a regra bem encapsulada no domínio.

## Dependências
- Depende de:
  - `TA-001` – Esqueleto mínimo da aplicação de antecipação.
  - `TA-010` – Modelagem da entidade de solicitação de antecipação.
- Relaciona-se com:
  - `TA-040` – Regra de unicidade de solicitação pendente por creator (após aprovação, o criador pode voltar a criar nova pendente).
  - `TA-100` – Consolidação do contrato público da API de antecipação (v1), ao expor o endpoint de aprovação.

## Subcards
- (Opcional) `TA-060-01` – Fluxo de aprovação em lote (multi-solicitações), se o escopo for estendido futuramente.*** End Patch
