## ADR-003 – Observabilidade e requisitos de segurança em monólito SQLite

### Contexto

O backend precisa expor **sinais mínimos de observabilidade** (logs, correlação, métricas simples) e tratar **requisitos não funcionais de segurança** alinhados ao uso de JWT e autenticação baseada em token, sem depender de ferramentas pagas.

### Decisão

- **Observabilidade**:
  - Usar `Serilog.AspNetCore` com `UseSerilogRequestLogging` para logs estruturados em console.
  - Padronizar logs de eventos sensíveis (login, falha de login, refresh, logout) via handlers/camadas de aplicação em ciclos futuros.
  - Deixar pontos de extensão preparados para métricas (latência, taxa de erro de endpoints) e futura adoção de OpenTelemetry.
- **Segurança**:
  - JWT com:
    - Chave simétrica configurável (`Jwt:Secret`).
    - `Issuer` e `Audience` explícitos.
    - `ClockSkew` reduzido (1 minuto).
    - Tokens de acesso de curta duração e refresh tokens regenerados por `ITokenService`.
  - Estrutura para:
    - Lockout após múltiplas falhas de login (a ser completado em evoluções).
    - Registro de eventos sensíveis em logs estruturados.

### Consequências

- **Positivas**:
  - Observabilidade básica pronta para ambientes de desenvolvimento/CI.
  - Segurança alinhada a práticas comuns de JWT, com espaço para endurecimento incremental (ex.: OWASP ASVS).
- **Negativas**:
  - Métricas e tracing ainda não estão integrados a uma stack completa (Prometheus/Grafana, por exemplo).
  - Lockout e políticas avançadas dependem de iterações futuras para ajuste fino do domínio e das regras de negócio.

