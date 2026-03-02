# [TA-090] Regras de validação de entrada (campos obrigatórios e formatos)

## Contexto
Para garantir robustez e boa experiência de uso das APIs, é necessário validar os dados de entrada dos fluxos de criação, aprovação/recusa e simulação. Isso inclui presença de campos obrigatórios, tipos corretos e formatos válidos, retornando erros claros quando algo estiver incorreto.

## Objetivo
Padronizar a validação de entrada para os principais fluxos de antecipação (criação, aprovação/recusa e simulação), garantindo respostas de erro consistentes e alinhadas às regras de negócio.

## Escopo
- Incluir neste card:
  - Definição de validações para:
    - `creator_id` (presença e formato).
    - `valor_solicitado` (presença, numérico, maior que zero; regra de valor mínimo R$ 100,00 já tratada em `TA-020`, mas pode ser refletida nas mensagens).
    - `data_solicitacao` (presença e formato de data válido).
    - Identificador de solicitação para aprovação/recusa.
  - Padronização das respostas de validação (estrutura de erro, mensagens, códigos).
  - Integração dessas validações nos fluxos de:
    - Criação de solicitação (`TA-020`).
    - Aprovação (`TA-060`).
    - Recusa (`TA-070`).
    - Simulação (`TA-080`).
- Fica fora deste card:
  - Lógicas de negócio em si (valor mínimo, status, unicidade, etc.), que já são cobertas em outros cards.

## Critérios de aceite (cenários de teste)
- Cenário 1 – Campo obrigatório ausente:
  - Dado uma chamada de criação de solicitação sem `creator_id`,
  - Quando o endpoint é chamado,
  - Então o sistema retorna erro de validação indicando campo obrigatório ausente, sem criar solicitação.
- Cenário 2 – Formato inválido:
  - Dado uma chamada de simulação com `data_solicitacao` em formato inválido,
  - Quando o endpoint é chamado,
  - Então o sistema retorna erro de validação adequado, sem executar o fluxo de negócio.
- Cenário 3 – Identificador de solicitação inválido:
  - Dado uma chamada de aprovação/recusa com identificador de solicitação inexistente ou inválido,
  - Quando o endpoint é chamado,
  - Então o sistema retorna erro adequado (ex.: não encontrado ou inválido), sem alterar dados.

## Estratégia TDD da tarefa
- Especificar testes:
  - Unitários para validadores (ex.: usando FluentValidation, conforme a stack sugerida).
  - De integração para endpoints, verificando o formato e o conteúdo das respostas de validação.
- Implementar os validadores e conectá-los aos fluxos de entrada dos endpoints até que todos os testes passem.

## Dependências
- Depende de:
  - `TA-001` – Esqueleto mínimo da aplicação de antecipação.
  - `TA-010` – Modelagem da entidade de solicitação de antecipação (para saber campos necessários).
- Relaciona-se com:
  - `TA-020`, `TA-060`, `TA-070`, `TA-080` – Fluxos que consomem essas entradas.

## Subcards
- (Opcional) `TA-090-01` – Internacionalização/mensagens multilíngues de erro de validação, caso o projeto evolua.*** End Patch
