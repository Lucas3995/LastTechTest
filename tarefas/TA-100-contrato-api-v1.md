# [TA-100] Consolidação do contrato público da API de antecipação (v1)

## Contexto
Depois que as regras de negócio estiverem implementadas no domínio e na camada de aplicação, é necessário expor um contrato público de API REST versionado (`v1`) que permita a um sistema interno consumir todas as funcionalidades de antecipação de forma consistente e documentada.

## Objetivo
Consolidar os endpoints públicos da API de antecipação em uma versão `v1`, garantindo que criação, listagem, aprovação/recusa e simulação estejam disponíveis, obedecendo às regras de negócio definidas nos demais cards.

## Escopo
- Incluir neste card:
  - Definição e implementação dos endpoints principais em `/api/v1/...`:
    - `POST /api/v1/solicitacoes` – criar solicitação.
    - `GET /api/v1/solicitacoes` – listar por `creator_id`.
    - `POST` ou `PATCH /api/v1/solicitacoes/{id}/aprovar` – aprovar solicitação.
    - `POST` ou `PATCH /api/v1/solicitacoes/{id}/recusar` – recusar solicitação.
    - `GET /api/v1/solicitacoes/simulacao` – simular antecipação via query params.
  - Mapeamento entre DTOs de entrada/saída e o domínio/casos de uso implementados nos outros cards.
  - Garantia de que as respostas respeitam as regras de negócio:
    - Valor mínimo, status inicial, unicidade, aprovação/recusa, simulação sem persistência, validações.
  - Documentação mínima dos endpoints (ex.: Swagger/OpenAPI com descrições e exemplos).
- Fica fora deste card:
  - Detalhes de autenticação/autorização finos (podem ser acrescentados se houver tempo).
  - Monitoramento/observabilidade avançados.

## Critérios de aceite (cenários de teste)
- Cenário 1 – Fluxo ponta a ponta de criação:
  - Dado um corpo de requisição válido para criação de solicitação,
  - Quando `POST /api/v1/solicitacoes` é chamado,
  - Então o sistema cria a solicitação obedecendo às regras de valor mínimo, status inicial e unicidade, retornando os dados de forma consistente.
- Cenário 2 – Fluxo ponta a ponta de aprovação/recusa:
  - Dado uma solicitação `pendente`,
  - Quando os endpoints de aprovação e recusa são chamados, cada um no seu cenário,
  - Então o status é atualizado corretamente ou erros adequados são retornados quando a operação é inválida.
- Cenário 3 – Fluxo ponta a ponta de simulação:
  - Dado parâmetros válidos para simulação,
  - Quando `GET /api/v1/solicitacoes/simulacao` é chamado,
  - Então o sistema retorna taxa e `valor_liquido` usando a regra de 5%, sem criar solicitações.

## Estratégia TDD da tarefa
- Especificar testes de integração/E2E cobrindo os fluxos ponta a ponta, usando:
  - Servidor em memória/WebApplicationFactory (ou equivalente em .NET).
  - Dados controlados para validar comportamentos esperados.
- Implementar/ajustar controladores/endpoints, DTOs e mapeamentos até que todos os testes de integração passem.
- Refatorar a organização de rotas, versionamento e documentação mantendo a suíte verde.

## Dependências
- Depende de:
  - `TA-001` – Esqueleto mínimo da aplicação de antecipação.
  - `TA-020` – Regra de criação de solicitação.
  - `TA-030` – Regra de cálculo de taxa e valor líquido.
  - `TA-040` – Regra de unicidade de solicitação pendente por creator.
  - `TA-050` – Regra de listagem por `creator_id`.
  - `TA-060` – Regra de aprovação de solicitação.
  - `TA-070` – Regra de recusa de solicitação.
  - `TA-080` – Regra de simulação de antecipação.
  - `TA-090` – Regras de validação de entrada.

## Subcards
- (Opcional) `TA-100-01` – Customização de documentação (descrições, exemplos e agrupamentos no Swagger/OpenAPI) para melhorar DX do time interno.*** End Patch
