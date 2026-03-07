## [RO-1] Integrar métricas e tracing (OpenTelemetry) com logs estruturados (Serilog)

### Contexto e objetivo de negócio

Serilog já está configurado para logs estruturados em console (`Serilog.AspNetCore` v10, `UseSerilogRequestLogging`), mas o sistema não possui correlação automática entre logs, traces e métricas. Diante de um incidente ou degradação de performance, desenvolvedores e operadores precisam cruzar manualmente informações dispersas — sem saber em qual camada a lentidão ocorre, sem `TraceId` indexado no log e sem métricas de p95 por endpoint.

O objetivo desta demanda é **integrar OpenTelemetry ao backend .NET 10**, completando a base de observabilidade com os três pilares coesos:

- **Logs:** enriquecer o Serilog existente com `TraceId` e `SpanId` automáticos por requisição, sem alterar nenhum log existente.
- **Traces:** instrumentar ASP.NET Core, MediatR handlers e EF Core automaticamente, sem código adicional nos handlers de negócio.
- **Métricas:** expor métricas de latência (p50/p95/p99), throughput e taxa de erro por endpoint, prontas para scrape Prometheus ou exportação OTLP.
- **Configurabilidade por ambiente:** ativar/desativar sem recompilar, mantendo ambiente "Testing" sem overhead algum.
- **Extensibilidade:** exporter plugável (OTLP, Prometheus, Console) com zero mudança de código ao trocar de coletor.

`docs/architecture/ADR-003-observability-and-security.md` já documenta a intenção de adotar OpenTelemetry e registra explicitamente que "métricas e tracing ainda não estão integrados"; esta demanda é a realização concreta dessa intenção.

### User stories

- **US-OB1 — Correlacionar log com trace**
  Como **desenvolvedor**, quero que cada linha de log estruturado carregue automaticamente o `TraceId` e `SpanId` da requisição corrente, para **cruzar rapidamente logs e traces ao investigar uma falha ou comportamento inesperado** sem busca manual por contexto disperso.

- **US-OB2 — Métricas de latência e taxa de erro por endpoint**
  Como **operador de sistema**, quero ter acesso a métricas automáticas de latência e taxa de erros por endpoint (`POST /api/v1/anticipations`, `POST /api/v1/auth/login`, etc.), para **detectar degradação de performance antes que afete usuários**.

- **US-OB3 — Trace distribuído por operação de domínio**
  Como **desenvolvedor**, quero que uma requisição a qualquer endpoint produza um trace com spans cobrindo ao menos as camadas ASP.NET Core, MediatR handler e banco de dados (EF Core), para **localizar a origem de lentidões ou falhas em operações complexas sem instrumentação manual nos handlers**.

- **US-OB4 — Configuração por ambiente sem recompilação**
  Como **desenvolvedor**, quero que a coleta de métricas e tracing possa ser habilitada ou desabilitada por configuração de ambiente (variável ou `appsettings`), para **evitar overhead em testes e CI sem precisar alterar código**.

- **US-OB5 — Exportação pronta para stack de monitoramento**
  Como **operador de sistema**, quero que o sistema exporte dados de observabilidade a coletores padrão da indústria (OTLP, Prometheus, Jaeger/Zipkin), para **integrar com a stack de monitoramento existente ou futura sem refatoração do backend**.

### Personas / papéis afetados

- **Desenvolvedor backend**
  Literacia técnica alta; usa traces e logs correlacionados durante debug e investigação de regressões de performance em desenvolvimento e homologação. Configura e mantém a integração OpenTelemetry.

- **Operador de sistemas / SRE**
  Monitora dashboards em staging/produção; configura alertas baseados em métricas; espera um endpoint de métricas ou coletor OTLP já integrado ao iniciar o container.

- **Analista de qualidade**
  Valida SLOs (latência, disponibilidade) durante testes de carga; usa traces para identificar hotspots em cenários de volume sem precisar alterar código de produção.

### Telas, módulos, relatórios e navegação

**Escopo deste card: exclusivamente backend e infraestrutura de observabilidade. Nenhuma tela de frontend é criada ou alterada neste card.**

- **Visualização externa:**
  - Jaeger UI: visualização de traces distribuídos.
  - Prometheus + Grafana: dashboards de métricas de endpoints e operações de domínio.
  - `docker-compose.observability.yml` (override) inclui esses serviços localmente, ativado com `docker compose -f docker/docker-compose.yml -f docker/docker-compose.observability.yml up`.

- **Ponto de coleta interno:**
  - Endpoint `/metrics` (scrape Prometheus) — exposto apenas em rede interna ou protegido.
  - Exporter OTLP apontando para coletor configurável via `appsettings`.
  - Exporter Console disponível para desenvolvimento local imediato sem infraestrutura adicional.

### Permissões e segurança

- Nenhuma mudança de permissões na API pública ou nas regras de autorização existentes.
- Endpoint `/metrics` (Prometheus scrape) deve ser exposto apenas em rede interna ou protegido por autenticação básica / IP allowlist — não expor publicamente sem proteção.
- `TraceId` pode ser incluído nas respostas de erro da API como campo `traceId` no payload; esta demanda cria a infraestrutura que torna esse valor confiável e rastreável (os cards RF-x já recomendam expor esse campo ao usuário para suporte).
- Spans de EF Core devem omitir conteúdo de queries que possam expor dados sensíveis em produção: configurar `SetDbStatementForText = false` em ambientes não-Development e documentar na ADR.

### Fluxos de uso e regras de negócio

#### Fluxo 1 — Desenvolvedor investigando falha em criação de antecipação

1. Uma requisição a `POST /api/v1/anticipations` retorna 500.
2. O desenvolvedor abre o console de logs; localiza a linha de erro — que agora carrega `TraceId` e `SpanId` automaticamente.
3. No Jaeger/Zipkin, pesquisa pelo `TraceId`; vê a cascata de spans:
   - `HTTP POST /api/v1/anticipations` (span raiz, ASP.NET Core)
   - `MediatR: CreateAnticipationRequestCommand` (span filho, handler)
   - `EF Core: INSERT AnticipationRequests` (span neto, banco)
4. Identifica que o span de EF Core demorou 1,8 s (lock ou query lenta) — sem alterar nenhum código de handler ou domínio.

#### Fluxo 2 — Operador monitorando saúde em produção

1. Painel Grafana exibe:
   - `http.server.request.duration` por endpoint e código HTTP (p50, p95, p99).
   - Taxa de requisições 5xx por operação (aprovação, criação, login).
   - Contagem acumulada de solicitações criadas (métrica custom de negócio, se implementada neste card ou em evolução futura).
2. Alerta dispara quando p95 de `POST /api/v1/anticipations/{id}/approve` ultrapassa 300 ms.
3. Operador abre o trace do request problemático diretamente a partir do ID de correlação.

#### Fluxo 3 — Trace de ponta a ponta em operação de aprovação

1. `POST /api/v1/anticipations/{id}/approve` é chamado.
2. Trace gerado cobre automaticamente:
   - Span raiz: ASP.NET Core — HTTP request.
   - Span filho: `MediatR: ApproveAnticipationRequestCommand`.
   - Span filho: `EF Core: SELECT AnticipationRequests WHERE Id = ?`.
   - Span filho: `EF Core: UPDATE AnticipationRequests + INSERT AnticipationRequestAudits` (quando RC-3 estiver ativo).
3. Todos os spans aparecem correlacionados no mesmo `TraceId`; os logs da mesma requisição trazem o mesmo `TraceId` e `SpanId` correspondente.

#### Fluxo 4 — CI rodando testes sem overhead de observabilidade

1. CI executa `dotnet test` com `ASPNETCORE_ENVIRONMENT=Testing`.
2. O código de registro de OpenTelemetry verifica a configuração e **não registra** nenhum exporter nem TracerProvider ativo.
3. Todos os testes existentes passam sem side effects de spans, métricas ou Activity listeners em memória.

### Critérios de aceitação (testáveis)

- **CA-RO1-1 — Correlação TraceId/SpanId nos logs**
  Dado que a integração está habilitada (`Observability:Enabled = true`) e uma requisição HTTP é realizada a qualquer endpoint,
  Quando o log estruturado dessa requisição é inspecionado,
  Então cada linha deve conter os campos `TraceId` e `SpanId` referentes à requisição corrente (valores não nulos e não vazios).

- **CA-RO1-2 — Trace gerado por requisição HTTP**
  Dado que a integração está habilitada,
  Quando uma requisição a `POST /api/v1/anticipations` é completada com sucesso,
  Então deve existir um trace com ao menos 2 spans: span raiz de recebimento HTTP (ASP.NET Core) e span filho do handler MediatR correspondente, correlacionados pelo mesmo `TraceId`.

- **CA-RO1-3 — Span de banco de dados presente no trace**
  Dado que a integração está habilitada com instrumentação EF Core,
  Quando uma operação que acessa o banco de dados é executada como parte de um request,
  Então o trace deve incluir ao menos um span filho referente à operação de banco (query ou command), correlacionado ao span pai do handler.

- **CA-RO1-4 — Métricas de latência HTTP disponíveis**
  Dado que a integração de métricas está habilitada,
  Quando o endpoint de métricas (ex.: `/metrics`) ou o exporter OTLP for consultado após ao menos uma requisição,
  Então métricas de latência HTTP (`http.server.request.duration` conforme semconv OpenTelemetry) devem estar presentes para os endpoints instrumentados, com atributos de método HTTP e status.

- **CA-RO1-5 — Sem overhead em ambiente "Testing"**
  Dado que `Observability:Enabled = false` ou o ambiente é "Testing",
  Quando a API é iniciada,
  Então nenhum TracerProvider ativo, MeterProvider ativo ou exporter deve ser registrado (verificável por ausência de `ActivitySource` ativo e ausência de listeners de métricas do OTEL).

- **CA-RO1-6 — Testes existentes continuam passando**
  Dado que a integração é ativada apenas em ambientes não-Testing,
  Quando a suíte de testes existente é executada com `ASPNETCORE_ENVIRONMENT=Testing`,
  Então todos os testes existentes devem passar sem nenhuma alteração motivada exclusivamente pela adição de observabilidade.

- **CA-RO1-7 — TraceId exposto em payload de erro (melhoria de suporte)**
  Dado que uma requisição resulta em erro tratado (4xx/5xx) e a integração está habilitada,
  Quando o payload de erro retornado pela API é inspecionado,
  Então o campo `traceId` deve estar presente com o valor do `TraceId` ativo da requisição, permitindo correlação por parte do suporte técnico.

### Requisitos técnicos/metodológicos aplicáveis

- **Pacotes OpenTelemetry recomendados para .NET 10:**
  - `OpenTelemetry.Extensions.Hosting`
  - `OpenTelemetry.Instrumentation.AspNetCore`
  - `OpenTelemetry.Instrumentation.Http`
  - `OpenTelemetry.Instrumentation.EntityFrameworkCore`
  - `OpenTelemetry.Exporter.OpenTelemetryProtocol` (OTLP — exportação para coletor)
  - `OpenTelemetry.Exporter.Prometheus.AspNetCore` (scrape `/metrics`, se adotado)
  - `Serilog.Enrichers.OpenTelemetry` (correlação `TraceId`/`SpanId` nos logs Serilog)

- **Integração com Serilog existente:**
  - Enriquecer o `LoggerConfiguration` bootstrap em `Program.cs` (antes de `CreateBuilder`) com `.Enrich.WithOpenTelemetry()` para adicionar `TraceId` e `SpanId` a todas as linhas de log.
  - Preservar `UseSerilogRequestLogging()` já existente — o enriquecimento é puramente aditivo e não altera logs já em uso.

- **Configuração por ambiente (`appsettings.json`):**
  - Nova seção `Observability`:
    ```json
    {
      "Observability": {
        "Enabled": false,
        "ServiceName": "LastTechTest.API",
        "Exporter": "console",
        "OtlpEndpoint": "http://localhost:4317"
      }
    }
    ```
  - Em `Program.cs`, o registro de `AddOpenTelemetry()` é condicional a `Observability:Enabled` e/ou a ambiente != "Testing".
  - `appsettings.Development.json` habilita com `Enabled: true` e `Exporter: console` para uso imediato em desenvolvimento local sem infraestrutura adicional.

- **Clean Architecture — onde alterar:**
  - Alterações concentradas em `LastTechTest.API` (wiring/registro em `Program.cs`, nova classe `OpenTelemetryExtensions` em `Configuration/`).
  - `LastTechTest.Infrastrutura` pode encapsular configuração de exporters se surgir necessidade de abstração; neste card, a configuração direta em `API` é suficiente.
  - `Dominio` e `Aplicacao` **não devem** referenciar pacotes de tracing/métricas. Spans customizados futuros em camadas internas devem usar `ActivitySource` nomeado, instanciado e registrado exclusivamente na camada `API`.

- **Docker:**
  - `docker/docker-compose.observability.yml` com Jaeger `all-in-one` (porta 16686 UI, 4317 OTLP gRPC) e Prometheus + Grafana para demonstração local.
  - Uso: `docker compose -f docker/docker-compose.yml -f docker/docker-compose.observability.yml up --build`.

- **ADR:**
  - Atualizar `docs/architecture/ADR-003-observability-and-security.md` registrando: pacotes escolhidos, estratégia de configuração por ambiente, nível de detalhe de spans EF Core (`SetDbStatementForText`), política de exposição do `/metrics` e disponibilidade de `traceId` em respostas de erro. Se o volume de decisões justificar, criar `ADR-004` dedicado a observabilidade com OpenTelemetry.

- **TDD obrigatório:**
  - Escrever testes antes ou em par com a implementação; cobertura adequada nos pontos alterados.
  - Usar `InMemoryExporter<Activity>` do SDK OpenTelemetry para testes de integração sem infraestrutura externa.

- **Rotina-completa:**
  - Implementação (`mercenario`) → testes (`quadro-de-recompensas`) → análise (`batedor-de-codigos`) → refatoração (`mestre-freire`) → CI (`arauto`).
  - Validar que o CI continua verde antes da entrega; a fitness function de cobertura mínima (30%) não deve regredir.

#### Artefatos esperados

- `LastTechTest.API/Configuration/OpenTelemetryExtensions.cs` — método de extensão `AddLastTechTestObservability(this IHostApplicationBuilder builder)` registrando tracing e métricas condicionalmente com base na configuração.
- `LastTechTest.API/Program.cs` — chamada a `AddLastTechTestObservability` e enricher Serilog adicionado ao bootstrap logger.
- `LastTechTest.API/LastTechTest.API.csproj` — referências aos pacotes OpenTelemetry listados acima.
- `LastTechTest.API/appsettings.json` — nova seção `Observability` com defaults seguros (`Enabled: false`).
- `LastTechTest.API/appsettings.Development.json` — override com `Enabled: true` e `Exporter: console`.
- `docs/architecture/ADR-003-observability-and-security.md` — atualização registrando a decisão de adotar OpenTelemetry (ou novo `ADR-004`).
- `docs/tracability.md` — mapeamento `RO-1 → OpenTelemetryExtensions.cs → testes de integração`.
- `docker/docker-compose.observability.yml` — Jaeger `all-in-one` + Prometheus + Grafana para demonstração local completa com um único comando.
- `LastTechTest.Testes/` — novos testes de integração de observabilidade (ver seção "Diretrizes de testes").

### Diretrizes de testes

- **Unitários:**
  - Verificar que o método de extensão `AddLastTechTestObservability` não lança exceção e registra o `TracerProvider` e `MeterProvider` esperados quando `Enabled = true`.
  - Verificar que quando `Enabled = false` (ou ambiente = "Testing"), nenhum provider ativo é registrado (CA-RO1-5).
  - Verificar que o enricher Serilog adiciona campos `TraceId` e `SpanId` não nulos quando há uma `Activity` ativa no contexto.

- **Integração:**
  - Usando `WebApplicationFactory` com `InMemoryExporter<Activity>` (pacote `OpenTelemetry.Exporter.InMemory`):
    - Após `POST /api/v1/anticipations`, verificar presença de ao menos 2 spans com relação pai-filho esperada (CA-RO1-2).
    - Verificar que um dos spans corresponde à instrumentação EF Core (CA-RO1-3).
    - Verificar que o log emitido durante o request carrega `TraceId` e `SpanId` não nulos, usando sink de captura em memória (CA-RO1-1).
  - Verificar que, com `Enabled = false`, nenhum span é registrado no `InMemoryExporter` (CA-RO1-5).

- **Regressão:**
  - Executar a suíte completa existente de backend com `ASPNETCORE_ENVIRONMENT=Testing` e confirmar que todos os testes continuam passando sem alterações (CA-RO1-6).
  - A cobertura mínima do CI (30%) não deve regredir após as alterações.

### Spec para agentes de IA

- **Seções que são spec principal:** User stories (US-OB1–OB5) + Critérios CA-RO1-x + Artefatos esperados + Requisitos técnicos.

- **Nomenclatura sugerida:**
  - Classe de extensão: `OpenTelemetryExtensions` em namespace `LastTechTest.API.Configuration`.
  - Config model: classe `ObservabilityOptions` com propriedades `Enabled`, `ServiceName`, `Exporter` (enum: `Console`, `Otlp`, `Prometheus`), `OtlpEndpoint`.
  - `ActivitySource` nomeado para spans customizados futuros: `"LastTechTest.API"`.
  - Arquivo de testes: `ObservabilityIntegrationTests.cs` em `LastTechTest.Testes/`.

- **Uso recomendado:**
  - Este card é a **fonte única de verdade** para a integração de observabilidade do backend.
  - A implementação deve permanecer circunscrita às camadas `API` e eventualmente `Infrastrutura`; `Dominio` e `Aplicacao` não importam pacotes de OTEL diretamente.
  - Agentes devem ler os critérios CA-RO1-x como spec verificável e derivar os testes diretamente dessa seção, sem inferir comportamentos além do aqui descrito.

### Dependências e riscos

- **Dependências:**
  - RC-3 (auditoria persistente de transições): não bloqueante para esta demanda, mas quando ativa, os spans EF Core exibirão a escrita de auditoria — o que é benéfico para observabilidade completa de operações de transição.
  - Serilog já configurado (`Serilog.AspNetCore` v10): não bloqueante; apenas adicionar o enricher `Serilog.Enrichers.OpenTelemetry`.

- **Riscos:**
  - **Overhead em CI/testes:** Activity listeners ativos podem causar flakiness em testes de timing; mitigação com `Enabled = false` em "Testing" (CA-RO1-5), garantida por configuração antes da execução de testes.
  - **Compatibilidade de pacotes:** Alguns pacotes `OpenTelemetry.Instrumentation.*` são pré-release e têm cadência de release acelerada; mitigação com versões fixadas no `.csproj` e verificação explícita de compatibilidade com .NET 10 antes da adoção.
  - **Coletor ausente em dev:** Exportação OTLP exige um coletor em execução; sem coletor, spans são descartados silenciosamente (comportamento padrão do SDK). Documentar claramente no README e na ADR para evitar confusão em desenvolvimento local — o exporter `console` em Development elimina esse problema para o desenvolvedor local.
  - **Exposição de SQL em traces:** Spans EF Core podem expor queries SQL em texto claro; mitigação com `SetDbStatementForText = false` em produção, configurável por ambiente e documentado na ADR.

### Rastreabilidade

- **ADR relacionada:** `docs/architecture/ADR-003-observability-and-security.md` (seção "Métricas e tracing ainda não integrados" — esta demanda resolve explicitamente esse ponto).
- **README:** Seção "Próximos passos sugeridos" — referência direta: "Integrar métricas e tracing (OpenTelemetry) com a base de logs estruturados (Serilog já configurado)."
- **Futuros mapeamentos em `docs/tracability.md`:**
  - `RO-1` → `LastTechTest.API/Configuration/OpenTelemetryExtensions.cs`
  - `RO-1` → `LastTechTest.API/Program.cs` (bootstrap logger + DI)
  - `RO-1` → `appsettings.json` / `appsettings.Development.json` (seção Observability)
  - `RO-1` → `LastTechTest.Testes/ObservabilityIntegrationTests.cs`
- **Integração com skills:**
  - `maestro`: mapeamento de alterações de código (Program.cs, csproj, appsettings, ADR, testes) — ver relatório de alterações gerado em paralelo a este card.
  - `quadro-de-recompensas`: testes derivados dos CA-RO1-x (unitários, integração, regressão).
  - `batedor-de-codigos` + `mestre-freire`: análise e refatoração pós-implementação, verificando que OTEL não vazou para camadas de Domínio ou Aplicação.
  - `arauto`: commit, push, PR e validação do CI após implementação.