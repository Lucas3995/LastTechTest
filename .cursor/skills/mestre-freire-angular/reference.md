# Referência — Mestre Freire Angular

Extensão Angular do mapeamento achado → técnica. Formato orientado a consumo por agente IA (listas, checklists, paths explícitos).

---

## Regra de critério técnico (Angular)

- **Ficheiro:** `.cursor/rules/angular-frontend.mdc`
- **Conteúdo relevante:** camadas (domain, application, infrastructure, core, shared, features), definições operativas de áreas/módulos/páginas/componentes/directives/serviços e mapeamento para estrutura de ficheiros e pastas, convenções (templateUrl, styleUrl), serviços por camada (DIP), regras duras, a11y, Signals/RxJS, lifecycle, typed forms, central de ações (Command).

---

## Fluxo spec-first (para agente IA)

- **Quando há contrato OpenAPI:** gerar ou validar tipos e serviços de infraestrutura a partir do contrato; DTOs e interfaces alinhados à spec.
- **Quando há relatório de alterações ou demanda:** garantir critérios verificáveis (ex.: "página X faz Y", "serviço Z implementa interface W") antes de implementar; validar resultado contra esses critérios.
- **Ordem:** ler spec → planejar (onde criar ficheiros, que camada/conceito) → implementar → validar (testes, build).

---

## DIP e camadas no Angular

Ao tratar achados que mencionem **DIP**, **Clean Architecture** ou **serviços por camada** em código Angular:

- **Serviços por camada:** domain (interfaces/abstrações), application (casos de uso, sem HttpClient), infrastructure (implementações HTTP, repositórios), core (singletons, config). Componentes em features usam serviços de application ou infrastructure, não HttpClient direto.
- Consultar `.cursor/rules/angular-frontend.mdc` para o mapeamento completo e dependências permitidas.

---

## Estrutura modular (conceitos → ficheiros/pastas)

Definições alinhadas ao guia; **mapeamento para estrutura de ficheiros e pastas e para agrupamentos/modularização**:

- **Áreas:** conjuntos de módulos por roles/permissões; pasta por área (ex.: `app/areas/admin/`, `app/areas/creator/`).
- **Módulos:** menor unidade implantável; conjunto coeso de páginas/ações; pasta por módulo em `features/<modulo>/` com `pages/`, `components/`, rotas.
- **Páginas:** storytelling UX; um componente de página por rota em `features/<modulo>/pages/<nome>/`.
- **Componentes:** reutilizáveis para montar páginas; `shared/components/` (globais) ou `features/<modulo>/components/` (do módulo).
- **Directives:** extensão de HTML (comportamento sem template); `shared/directives/` ou `features/<modulo>/directives/`.
- **Serviços:** por camada em `domain/`, `application/`, `infrastructure/`, `core/`.

Uso pelo agente: decidir **onde** criar cada artefato e **quando** abrir novo módulo ou feature em Greenfield e Evolução.

---

## Testes no Angular

- **Ficheiros de teste:** não alterar `*.spec.ts`. A skill mestre-freire (e esta) mantém testes intocáveis.
- **Comando:** `npm run test` ou `ng test`. Executar após cada passo (ou lote coerente) de refatoração; a suíte deve permanecer verde.
- Se um teste falhar após refatoração, corrigir o **código de produção**, não o spec.

---

## Acessibilidade (a11y)

- **Checklist mínimo:** semântica HTML (`<button>`, `<nav>`, `<label>`, etc.); ARIA onde HTML não basta; teclado (interactivos acessíveis, foco visível); feedback para leitores de tela.
- **LiveAnnouncer** (Angular CDK) para anunciar conteúdo dinâmico.
- **Referência:** Angular a11y (angular.dev/best-practices/a11y), CDK a11y (FocusTrap, FocusMonitor).

---

## Signals vs RxJS

- **Signals:** estado síncrono e fino na UI (flags de componente, computed); preferir para estado local e derived state.
- **RxJS:** fluxos assíncronos (HTTP, WebSockets, eventos, operadores); manter para tudo o que é async.
- **Interop:** `toSignal()` para consumir Observable no template; `toObservable()` para aplicar operadores RxJS a Signal; `takeUntilDestroyed(this.destroyRef)` para subscriptions em componentes (Angular 16+).

---

## Ciclo de vida

- **Limpeza em `ngOnDestroy`:** subscriptions, listeners, timers para evitar memory leaks.
- **Preferir `takeUntilDestroyed(this.destroyRef)`** em vez de unsubscribe manual quando possível (Angular 16+).
- Directives e serviços: respeitar destruição; não reter referências a componentes destruídos.

---

## Typed forms

- Preferir **FormGroup/FormControl tipados** (Angular 14+); inferência ou genéricos.
- Evitar `UntypedFormGroup`/`UntypedFormControl` exceto em migração.

---

## Central de ações / Command

- Todas as requisições ao backend passam por um **módulo/serviço central** (design pattern Command).
- **Falha de rede/backend:** notificar utilizador, persistir comando (fila), permitir retry; **nunca** quebrar a página.
- UX: central acessível (ex.: menu superior) onde o utilizador vê processos solicitados e obtém feedback quando o comando terminar.
