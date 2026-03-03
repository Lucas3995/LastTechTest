## ADR-002 – Uso de SQLite como banco placeholder

### Contexto

O projeto precisa de um banco simples para desenvolvimento, testes (incluindo E2E) e execução em Docker, mas deve **permanecer apto a trocar de banco** (por exemplo, PostgreSQL ou SQL Server) sem impacto nas camadas internas de domínio/aplicação.

### Decisão

- Utilizar **SQLite** como banco padrão:
  - Connection string default: `Data Source=lasttechtest.db` (ajustada para volume no Docker).
  - `ApplicationDbContext` configurado em `LastTechTest.Persistencia` com provider SQLite.
- Isolar o conhecimento de SQLite:
  - Entidades de domínio não possuem atributos de EF Core.
  - Mapeamentos e configurações vivem apenas em `LastTechTest.Persistencia`.
  - Repositórios expõem apenas interfaces do domínio (`IUserRepository`, `IUserTokenRepository`).

### Consequências

- **Positivas**:
  - Ambiente de desenvolvimento e testes extremamente leve e portável.
  - Pipeline de CI simples: não exige subir instâncias de banco externas.
  - Fácil migração futura para outro provider apenas ajustando `ApplicationDbContext` e connection strings.
- **Negativas**:
  - Alguns comportamentos de concorrência/transações diferem de bancos relacionais tradicionais.
  - Pode ser necessário ajustar migrações e scripts quando for adotado um banco definitivo.

