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

## Como rodar o projeto localmente (visão geral)

Assim que a solução .NET for criada (card `TA-001`), o fluxo esperado para execução local será:

1. **Clonar o repositório**
   ```bash
   git clone <url-do-repo>
   cd LastTechTest
   ```

2. **Subir via Docker Compose** (quando os arquivos de containerização estiverem presentes)
   ```bash
   docker compose up --build
   ```

3. **Rodar a API localmente com .NET CLI** (modo alternativo, sem Docker)
   ```bash
   dotnet restore
   dotnet build
   dotnet run --project src/LastTechTest.Antecipacao.Api
   ```

4. **Acessar a API**
   - Endpoint de saúde (exemplo do card `TA-001`): `GET /health`
   - Endpoints de negócio (criação, listagem, aprovação/recusa, simulação) serão documentados conforme os cards `TA-020` a `TA-100`.

> Enquanto a solução ainda não estiver criada, estes comandos são uma **referência de intenção** e podem precisar de ajuste conforme o nome final dos projetos/pastas.

---

## Como rodar os testes

Após a criação dos projetos de teste (durante a implementação dos cards com TDD), o fluxo previsto é:

```bash
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

