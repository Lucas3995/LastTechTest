## ADR-001 – Adoção de Clean Architecture em monólito .NET 10

### Contexto

O backend deve servir como **modelo de referência** para futuros projetos, privilegiando manutenibilidade, testabilidade e independência de frameworks, alinhado às instruções de `instrucoes tecnicas` e à referência [OmniSuite API](https://github.com/DuoMasterGestaoTecnologia/nueva_api/blob/main/README.md).

### Decisão

- Organizar a solução em projetos:
  - `LastTechTest.Dominio` – regras de negócio puras, entidades, enums e interfaces.
  - `LastTechTest.Aplicacao` – casos de uso (CQRS + MediatR), orquestração de domínio.
  - `LastTechTest.Persistencia` – EF Core 10 + SQLite, `ApplicationDbContext` e repositórios.
  - `LastTechTest.Infrastrutura` – serviços técnicos (JWT, hashing, MFA, email, key generator).
  - `LastTechTest.API` – camada HTTP (endpoints, autenticação, Swagger).
  - `LastTechTest.Testes` – testes unitários, integração e E2E.
- Respeitar a regra de dependência:
  - Domínio não referencia frameworks externos.
  - Aplicação depende apenas de Domínio.
  - Persistência e Infraestrutura implementam interfaces de Domínio/Aplicação.
  - API depende de todas as demais, conectando-as via DI.

### Consequências

- **Positivas**:
  - Facilita evolução do domínio sem impacto em frameworks.
  - Torna casos de uso mais testáveis (unit, integration, E2E).
  - Permite trocar persistência ou serviços externos com menor acoplamento.
- **Negativas**:
  - Curva de entrada maior para projetos muito simples.
  - Mais projetos na solução exigem disciplina na navegação e organização.

