---
description: Regra de critério técnico para frontend Angular. Camadas (domain, application, infrastructure, core, shared, features), conceitos fundantes (áreas, módulos, páginas, componentes, directives, serviços) com mapeamento para estrutura de ficheiros e pastas, convenções, DIP, a11y, Signals/RxJS, lifecycle, typed forms, padrão command para backend. Spec antes de código. Aplicar quando criar, refatorar ou evoluir frontend Angular (skill mestre-freire-angular).
alwaysApply: false
---

# Angular Frontend — Regra de critério técnico

## Spec antes de código

- **Entrada obrigatória antes de gerar ou alterar código Angular:** contrato de API (ex.: OpenAPI), relatório de alterações (ex.: maestro) ou demanda com critérios verificáveis.
- Fluxo: **ler spec → planejar** (onde criar ficheiros, que camada, que conceito) **→ implementar → validar** (testes, build).
- Nunca gerar código Angular sem referência a um artefato de spec.

---

## Camadas e dependências permitidas

| Camada | Responsabilidade | Pode depender de |
|--------|------------------|------------------|
| **domain** | Interfaces, tipos, entidades de domínio | — |
| **application** | Casos de uso, orquestração (sem I/O direto) | domain |
| **infrastructure** | HTTP, repositórios, implementações de interfaces | domain, application |
| **core** | Singletons, config global, guards | domain (evitar application/infrastructure) |
| **shared** | Componentes, directives, pipes reutilizáveis | domain, core |
| **features** | Páginas e componentes de feature | domain, application, infrastructure, core, shared |

- **Regra:** dependências apenas para dentro (camada externa depende da interna; domain não depende de ninguém).

---

## Estrutura modular — conceitos e mapeamento para ficheiros/pastas

Estes conceitos **definem a estrutura de ficheiros e pastas, agrupamentos e modularização** do projeto.

### Definições operativas

- **Áreas:** Conjuntos de módulos ligados a roles/permissões de utilizador; geram uma visão de sistema; podem ter UI/layout diferente entre si. **Onde:** pasta por área (ex.: `app/areas/admin/`, `app/areas/creator/`).
- **Módulos:** Menor unidade de software implantável e desenvolvível de forma independente; conjunto coeso de páginas e ações. **Onde:** pasta por módulo (ex.: `app/features/anticipations/`, `app/features/dashboard/`); cada módulo tem `pages/`, `components/`, rotas e eventualmente `services/` locais.
- **Páginas:** Conjuntos de componentes agrupados para uma ação ou conjunto de informações; storytelling em UX/UI. **Onde:** um componente de página por rota (ex.: `features/anticipations/pages/request-list/`); ficheiros `*.component.ts`, `*.component.html`, `*.component.scss`.
- **Componentes:** Partes de código reutilizáveis para montar páginas (conjuntos de elementos, abstrações reutilizáveis). **Onde:** `shared/components/` para globais; `features/<modulo>/components/` para específicos do módulo.
- **Directives:** Estender HTML com comportamento customizado ou transformar elementos (sem template próprio). **Onde:** `shared/directives/` ou `features/<modulo>/directives/`.
- **Serviços:** Lógica reutilizável por camada (domain = interfaces; application = casos de uso; infrastructure = HTTP/repositórios; core = singletons). **Onde:** `app/domain/`, `app/application/`, `app/infrastructure/`, `app/core/` (ou equivalente por pasta de camada).

### Checklist — onde colocar um novo artefato

- [ ] Novo tipo/interface de domínio → `domain/`.
- [ ] Novo caso de uso (orquestração, sem HttpClient) → `application/`.
- [ ] Nova implementação HTTP/repositório → `infrastructure/`.
- [ ] Novo singleton/config global → `core/`.
- [ ] Novo componente reutilizável em vários módulos → `shared/components/`.
- [ ] Nova página (rota, storytelling) → `features/<modulo>/pages/<nome>/`.
- [ ] Novo componente usado só num módulo → `features/<modulo>/components/`.
- [ ] Nova directiva → `shared/directives/` ou `features/<modulo>/directives/`.
- [ ] Novo módulo coeso de funcionalidade → nova pasta em `features/<modulo>/` com `pages/`, `components/`, rotas; considerar lazy loading.

---

## Convenções de componentes e módulos

- **Componentes:** usar `templateUrl` e `styleUrl` (não template inline em páginas com mais de 5 linhas).
- **Standalone:** preferir componentes standalone; módulos quando necessário para lazy loading ou agrupamento.
- **Lazy loading:** por feature/módulo nas rotas (`loadChildren`).
- **Trio de ficheiros:** `.component.ts`, `.component.html`, `.component.scss` para cada componente (exceto directivas).

---

## Serviços por camada (DIP)

- **domain:** apenas interfaces/abstrações (ex.: `IReportRepository`).
- **application:** serviços que orquestram casos de uso; **não** injetar `HttpClient`; dependem de interfaces do domain.
- **infrastructure:** implementações que chamam HTTP, repositórios concretos; implementam interfaces do domain/application.
- **core:** singletons (auth, config, logging); configuração de app.
- Componentes de página **não** injetam `HttpClient` diretamente; usam serviços de application ou infrastructure.

---

## Regras duras (violações no formato para agente)

Ao analisar código, violações devem ser reportadas assim: `**[PRINCÍPIO]** \`path:linha\` — descrição`.

- **[CAMADA]** `src/app/features/foo/page.component.ts` — componente de página injeta HttpClient diretamente; deve usar serviço de application/infrastructure.
- **[CONVENÇÃO]** `src/app/features/foo/page.component.ts` — template inline com mais de 5 linhas; usar templateUrl.
- **[CAMADA]** `src/app/domain/` — camada domain contém implementação concreta (ex.: chamada HTTP); manter apenas interfaces/tipos.
- **[ESTRUTURA]** `src/app/` — módulo sem pasta `pages/` ou `components/` quando há páginas/componentes soltos; agrupar conforme conceitos (módulo/página/componente).

---

## Acessibilidade (a11y)

- Semântica HTML: usar `<button>`, `<nav>`, `<header>`, `<main>`, `<label>` em vez de `<div>` com click.
- ARIA: quando HTML não basta, usar `[attr.aria-label]`, `role`, `aria-live` conforme necessidade.
- Teclado: todos os interactivos acessíveis por teclado; focos visíveis.
- Leitores de tela: conteúdo dinâmico anunciado (ex.: Angular CDK `LiveAnnouncer`); evitar só feedback visual.
- Referência: Angular a11y (angular.dev/best-practices/a11y), CDK a11y (FocusTrap, FocusMonitor).

---

## Signals vs RxJS

- **Signals:** estado síncrono e fino na UI (flags, computed); preferir para estado local de componente e derived state.
- **RxJS:** fluxos assíncronos (chamadas HTTP, WebSockets, eventos, operadores); manter para tudo o que é async.
- Interop: `toSignal()` para consumir Observable no template; `toObservable()` para aplicar operadores RxJS a um Signal; `takeUntilDestroyed(this.destroyRef)` para subscriptions em componentes (Angular 16+).

---

## Ciclo de vida

- Limpeza em `ngOnDestroy`: subscriptions, listeners, timers para evitar memory leaks.
- Preferir `takeUntilDestroyed(this.destroyRef)` em vez de unsubscribe manual quando possível.
- Directives e serviços com ciclo de vida: respeitar destruição e não reter referências a componentes destruídos.

---

## Typed forms

- Preferir reactive forms tipados (Angular 14+): `FormGroup<T>`, `FormControl<T>` com inferência ou genéricos.
- Evitar `UntypedFormGroup`/`UntypedFormControl` exceto em migração.

---

## Frontend independente do backend — central de ações (Command)

- O frontend deve funcionar sem o backend exceto no momento das requisições.
- **Central de ações de servidor:** um módulo/serviço central gere **todas** as requisições aos backends (design pattern Command: intenção serializável, fila, retry).
- Requisições ao backend são **não garantidas** (rede, indisponibilidade); tratar como ambiente fora do controle do frontend.
- Se o backend falhar ou estiver indisponível: **avisar o utilizador**, **persistir o comando** (ex.: fila local) e permitir **retry** mais tarde; **nunca** quebrar a página (ex.: erro não tratado que impeça navegação ou uso da UI).
- UX: central acessível (ex.: menu superior) onde o utilizador vê processos solicitados e pode continuar com o feedback quando o comando terminar.

---

## Checklist geral (agente IA)

- [ ] Spec/contrato/relatório/demanda disponível antes de implementar.
- [ ] Novos artefatos colocados na camada e pasta corretas (domain/application/infrastructure/core/shared/features; áreas/módulos/páginas/componentes/directives).
- [ ] Componentes com templateUrl e styleUrl; sem HttpClient em componentes de página.
- [ ] Serviços por camada (DIP); interfaces no domain, implementações em infrastructure.
- [ ] A11y: semântica, ARIA quando necessário, teclado, feedback para leitores de tela.
- [ ] Estado: Signals para estado síncrono de UI; RxJS para async (HTTP, WebSockets).
- [ ] Subscriptions com `takeUntilDestroyed` ou limpeza em ngOnDestroy.
- [ ] Formulários tipados (typed forms).
- [ ] Requisições ao backend via central de ações; falha tratada (notificar, persistir comando, retry); páginas não quebradas por falha de rede.
