# [TA-040] Regra de unicidade de solicitação pendente por creator

## Contexto
O desafio estabelece que um mesmo criador não pode ter mais de uma solicitação de antecipação com status pendente ao mesmo tempo. Essa regra evita sobrecarga de risco e inconsistências financeiras, garantindo que o fluxo de antecipação seja controlado.

## Objetivo
Garantir que, para cada `creator_id`, exista no máximo uma solicitação com status `pendente` em qualquer momento.

## Escopo
- Incluir neste card:
  - Regra de negócio que verifica se já existe uma solicitação pendente para o `creator_id` antes de criar uma nova.
  - Integração dessa verificação no fluxo de criação de solicitação (`TA-020`).
  - Definição de erro/mensagem clara quando a criação é bloqueada por já existir uma pendente.
- Fica fora deste card:
  - Detalhes de infraestrutura de persistência (ORM, banco, etc.) além do mínimo necessário para realizar a checagem.
  - Regras de aprovação/recusa em si (tratadas em `TA-060` e `TA-070`), embora o efeito de aprovação/recusa libere o criador para novas pendentes.

## Critérios de aceite (cenários de teste)
- Cenário 1 – Criador sem pendentes:
  - Dado um `creator_id` que não possui solicitações pendentes,
  - Quando o fluxo de criação de solicitação é executado,
  - Então a criação é permitida e a nova solicitação fica com status `pendente`.
- Cenário 2 – Criador com uma pendente:
  - Dado um `creator_id` que já possui uma solicitação `pendente`,
  - Quando uma nova solicitação é tentada para o mesmo `creator_id`,
  - Então a criação é bloqueada e o sistema retorna um erro claro indicando que já existe solicitação pendente.
- Cenário 3 – Solicitações aprovadas/recusadas:
  - Dado um `creator_id` com solicitações somente `aprovada` e/ou `recusada`,
  - Quando uma nova solicitação é criada,
  - Então a criação é permitida e a nova solicitação fica `pendente`.

## Estratégia TDD da tarefa
- Especificar testes para o caso de uso/serviço de criação que:
  - Criem uma solicitação pendente com sucesso.
  - Tentem criar uma segunda pendente para o mesmo `creator_id` e verifiquem o erro.
  - Testem o comportamento quando as solicitações anteriores são aprovadas/recusadas.
- Utilizar dublês de persistência (ex.: repositório em memória ou mocks) para simular a existência de registros prévios.
- Implementar a verificação de unicidade na lógica de criação até que todos os testes passem.

## Dependências
- Depende de:
  - `TA-001` – Esqueleto mínimo da aplicação de antecipação.
  - `TA-010` – Modelagem da entidade de solicitação de antecipação.
  - `TA-020` – Regra de criação de solicitação (valor mínimo + status inicial).
- É pré-requisito para:
  - `TA-100` – Consolidação do contrato público da API de antecipação (v1), pois a API deve respeitar essa regra ao criar solicitações.

## Subcards
- (Opcional) `TA-040-01` – Otimizações de consulta/índices para verificar rapidamente a existência de pendentes em bancos maiores, caso o projeto evolua para um cenário mais realista.*** End Patch
} %}
