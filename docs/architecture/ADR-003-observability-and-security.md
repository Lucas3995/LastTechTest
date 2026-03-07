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
  - ~~Métricas e tracing ainda não estão integrados a uma stack completa (Prometheus/Grafana, por exemplo).~~ → Resolvido pela atualização RO-1 abaixo.
  - Lockout e políticas avançadas dependem de iterações futuras para ajuste fino do domínio e das regras de negócio.

---

### Atualização RO-1 — Integração OpenTelemetry (2026-03-07)

**Contexto:** A ADR original registrava que "métricas e tracing ainda não estão integrados". A demanda RO-1 resolve esse ponto, integrando OpenTelemetry ao backend .NET 10 para completar os três pilares de observabilidade (logs correlacionados, traces distribuídos, métricas de endpoints).

**Decisões tomadas:**

- **Pacotes adotados:**
  - `OpenTelemetry.Extensions.Hosting` 1.15.0
  - `OpenTelemetry.Instrumentation.AspNetCore` 1.15.0
  - `OpenTelemetry.Instrumentation.Http` 1.15.0
  - `OpenTelemetry.Instrumentation.EntityFrameworkCore` 1.15.0-beta.1 (pré-release; único disponível para .NET 10)
  - `OpenTelemetry.Exporter.Console` 1.15.0
  - `OpenTelemetry.Exporter.OpenTelemetryProtocol` 1.15.0
  - `OpenTelemetry.Exporter.Prometheus.AspNetCore` 1.15.0-beta.1
  - `Serilog.Enrichers.OpenTelemetry` 1.0.1
  - Justificativa: pacotes oficiais do OpenTelemetry SDK para .NET, com versões fixadas no `.csproj` para reprodutibilidade.

- **Estratégia de configuração por ambiente:**
  - Toggle `Observability:Enabled` (boolean) em `appsettings.json` — desativado por defeito (seguro).
  - Defesa em profundidade: ambiente `Testing` sempre inibe o registro de providers, mesmo que `Enabled=true` seja configurado acidentalmente.
  - `appsettings.Development.json` habilita com `Enabled=true` e `Exporter=Console` para uso imediato em desenvolvimento local sem infraestrutura adicional.

- **`SetDbStatementForText` (EF Core):**
  - Na versão 1.15.0-beta.1, a propriedade `SetDbStatementForText` não é exposta na API pública. Por defeito, parâmetros SQL NÃO são capturados nos spans, o que satisfaz R4 (sem exposição de queries com dados sensíveis). Quando a propriedade for promovida a estável, configurar `true` apenas em Development.

- **Política de exposição do `/metrics`:**
  - Endpoint Prometheus (`/metrics`) mapeado condicionalmente: apenas quando `Observability:Enabled=true`, `Exporter=Prometheus` e ambiente ≠ `Testing`.
  - Em produção, proteger com rede interna ou IP allowlist antes de expor — nunca expor publicamente sem proteção.

- **`traceId` em payloads de erro:**
  - Todas as respostas de erro (4xx/5xx) incluem o campo `traceId` com o valor de `Activity.Current?.TraceId`, permitindo correlação imediata por suporte técnico.
  - Quando a integração está desativada (sem Activity ativa), o campo é `null` — comportamento seguro e inofensivo.

- **`ActivitySource` nomeado:**
  - `"LastTechTest.API"` reservado para spans customizados futuros, instanciado e registrado exclusivamente na camada API.
  - Camadas `Dominio` e `Aplicacao` NÃO importam pacotes OpenTelemetry.

- **Serilog — enriquecimento aditivo:**
  - `.Enrich.With(new OpenTelemetryTraceIdEnricher())` e `.Enrich.With(new OpenTelemetrySpanIdEnricher())` adicionados ao bootstrap `LoggerConfiguration` em `Program.cs`.
  - `UseSerilogRequestLogging()` permanece sem alteração.
  - O enricher é sempre registrado (não condicional): quando não há Activity ativa, os campos ficam ausentes — inofensivo.

