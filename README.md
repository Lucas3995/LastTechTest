## Desafio Técnico – LastTechTest: Serviço de Antecipação de Valores

Este repositório contém a implementação de uma API REST para gestão de **solicitações de antecipação de valores** para criadores da LastTechTest, com foco em **clareza de código**, **boas práticas de engenharia**, **testes automatizados** e **estrutura preparada para evoluir**.

O objetivo é permitir que um sistema interno crie, liste, aprove/recuse e simule solicitações de antecipação, seguindo as regras descritas no arquivo `InstrucoesProjeto`.

---

## Stack prevista

- **Backend**: C# / .NET 9
- **Arquitetura**: Arquitetura Limpa, SOLID, DDD, CQRS com MediatR, Repository Pattern
- **Validações**: FluentValidation
- **Autenticação**: JWT com MFA (a ser definido conforme necessidade do desafio)
- **Persistência**: banco em memória com Entity Framework, com opção de alternar para SQLite
- **Testes**: xUnit, Moq, FluentAssertions, Coverlet
- **Containerização**: Docker + Docker Compose

> Observação: parte da estrutura ainda será construída conforme os cards de tarefa em `tarefas/` forem implementados usando TDD.

---

## Estrutura do repositório

- **`backend/`** – Código do backend .NET 9 (solution, projetos em `src/` e testes em `tests/`).
- **`tarefas/`** – Cards de tarefa em markdown.
- **`docker/`** – Arquivos do Docker: `docker-compose.yml` para subir o projeto com um único comando (a partir da raiz: `docker compose -f docker/docker-compose.yml up`).

## Como rodar o projeto

O projeto é executado **via Docker** a partir da **raiz do repositório**:

1. **Clonar o repositório**
   ```bash
   git clone <url-do-repo>
   cd LastTechTest
   ```

2. **Subir a API** (na raiz do projeto)
   ```bash
   docker compose -f docker/docker-compose.yml up
   ```
   Ou em background: `docker compose -f docker/docker-compose.yml up -d`.

   A API fica disponível em **http://localhost:8080**.

3. **Acessar a API**
   - **Swagger UI** (documentação e testes dos endpoints): [http://localhost:8080](http://localhost:8080)
   - **Health check**: `GET http://localhost:8080/health`
   - Endpoints de negócio (criação, listagem, aprovação/recusa, simulação) serão documentados conforme os cards em `tarefas/`.

A documentação OpenAPI (Swagger) é gerada automaticamente e exibida na raiz da API; o documento JSON está em `/swagger/v1/swagger.json`.

---

## Como rodar os testes

A solution e os projetos de teste ficam em `backend/`. Para rodar os testes:

```bash
cd backend
dotnet test
```

Com o tempo, podem ser adicionados:

- Coleta de cobertura com Coverlet.
- Geração de relatório com ReportGenerator.
- Execução automática via GitHub Actions em cada push/PR.

As suítes de testes (unitários, de integração e E2E) serão organizadas conforme os cards de regra de negócio, sempre iniciando a implementação pelos testes.

---

## Organização por cards de tarefa (`tarefas/`)

A pasta `tarefas/` conterá os **cards de tarefa** em formato markdown, cada um representando um conjunto coeso de regras de negócio a ser implementado via TDD.

- Os IDs seguem o padrão `TA-XXX` (por exemplo, `TA-020`) para cards principais.
- Subcards, quando necessários, seguem o padrão `TA-XXX-YY` (por exemplo, `TA-020-01`).

Cada card descreve:

- Contexto e objetivo em linguagem de negócio.
- Escopo da regra de negócio.
- Critérios de aceite como **cenários de teste**.
- Estratégia TDD para atacar a tarefa.
- Dependências entre cards.

Para começar a implementação, abra os arquivos em `tarefas/` e siga o fluxo descrito em cada card, sempre iniciando pelos testes.

---

## Resumo do domínio (a partir de `InstrucoesProjeto`)

Regras principais:

- **Criar solicitação de antecipação**
  - Entrada: `creator_id`, `valor_solicitado`, `data_solicitacao`.
  - Aplicar taxa de **5%** sobre o valor solicitado.
  - Calcular e retornar: `valor_liquido`, `status` (default = `pendente`).
- **Listar solicitações por `creator_id`**.
- **Aprovar ou recusar uma solicitação**
  - Atualizar `status` para `aprovada` ou `recusada`.
- **Simulação de antecipação (GET com query params)**
  - Calcular os valores de antecipação sem criar a solicitação.

Regras de negócio:

- Valor solicitado deve ser **maior que R$ 100,00**.
- Um `creator_id` não pode ter mais de **uma solicitação pendente** ao mesmo tempo.
- Taxa de antecipação fixa: **5%** sobre o valor bruto.
- Toda solicitação inicia com status **`pendente`**.

Essas regras estão decompostas em cards de tarefa na pasta `tarefas/`.

