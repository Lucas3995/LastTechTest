# Árvore de testes — RF-1 Minhas solicitações de antecipação (frontend Angular)

## 1. Escopo

- **Demanda**: `demandas/RF-1-minhas-solicitacoes-antecipacao.md`
- **Tela**: Minhas solicitações de antecipação (Creator)
- **Stack**: Angular 20 + Playwright (E2E)
- **Metodologia**: Spec-driven development + TDD
- **Critérios de aceitação cobertos**: `CA-RF1-1` a `CA-RF1-6`

Esta árvore descreve, em nível de casos de teste, o que deve ser exercitado em **unitários**, **integração** e **E2E**, servindo como guia para geração dos arquivos de teste (`*.spec.ts`) e dos testes E2E de Playwright.

---

## 2. Mapa CA → tipos de teste e artefatos

| CA | Foco | Unit | Integração | E2E | Artefatos principais |
|----|------|------|------------|-----|----------------------|
| **CA-RF1-1** | Lista restrita ao Creator | Formatação, colunas, eventual filtro local por `creator_id` | Página + façade + serviço de listagem por `creator_id` | Fluxo Creator autenticado vendo só suas solicitações | Página RF-1, façade de listagem, serviço HTTP de listagem |
| **CA-RF1-2** | Lista vazia com orientação | Componente de estado vazio (mensagem + CTAs) | Página recebendo lista vazia do serviço e exibindo estado vazio | Creator sem solicitações vendo mensagem de vazio + CTAs | Página RF-1, componente de vazio, façade/serviço |
| **CA-RF1-3** | Filtros e ordenação visíveis | Funções puras de filtro/ordenar + indicadores visuais | Filtros ↔ façade ↔ serviço ↔ grid | Fluxo aplicando filtros/ordem com UI coerente | Componentes de filtros, grid, façade de listagem |
| **CA-RF1-4** | Acesso ao detalhe | Renderização de detalhe + mensagem de acesso negado | Clique na grid → carregar detalhe via serviço | Fluxo abrir detalhe válido + erro de backend | Componente de detalhe, façade de detalhe, serviço HTTP |
| **CA-RF1-5** | Cancelar em análise | Lógica `canCancel` + mensagem de confirmação | Clique em cancelar → chamada de serviço → atualização de lista/detalhe | Fluxo de cancelamento bem-sucedido | Componente de detalhe, façade de cancelamento, serviço HTTP |
| **CA-RF1-6** | Cancelamentos repetidos | Ocultar/desabilitar ação + mensagem “já cancelada” | Resposta idempotente tratada sem mudar status | Tentativa de recancelar com mensagem adequada | Os mesmos de cancelamento, em cenário já-cancelado |

---

## 3. Árvore detalhada por critério de aceitação

### 3.1 CA-RF1-1 — Lista restrita ao próprio Creator

- **[CA-RF1-1] Nó raiz: Lista restrita ao Creator**
  - **[UNIT] Formatação e colunas**
    - `should_normalize_requests_to_table_rows_CA_RF1_1`
    - `should_format_amounts_and_dates_CA_RF1_1`
    - `should_build_minimum_columns_CA_RF1_1` (ID, datas, valores, status)
  - **[INTEG] Página + façade + serviço**
    - `should_call_list_service_with_creator_id_CA_RF1_1`
    - `should_render_rows_returned_by_service_CA_RF1_1`
    - `should_filter_out_requests_from_other_creators_CA_RF1_1`
  - **[E2E] Fluxo Creator autenticado**
    - `creator_sees_only_own_requests_in_list_CA_RF1_1`

### 3.2 CA-RF1-2 — Lista vazia com orientação

- **[CA-RF1-2] Nó raiz: Lista vazia**
  - **[UNIT] Componente de estado vazio**
    - `should_show_empty_message_when_no_requests_CA_RF1_2`
    - `should_show_new_request_and_simulation_ctas_CA_RF1_2`
  - **[INTEG] Página com service retornando lista vazia**
    - `should_switch_from_grid_to_empty_state_on_empty_list_CA_RF1_2`
    - `should_keep_ctas_available_on_empty_state_CA_RF1_2`
    - `should_replace_empty_state_with_grid_when_requests_appear_CA_RF1_2`
  - **[E2E] Fluxo Creator sem solicitações**
    - `creator_without_requests_sees_empty_state_and_ctas_CA_RF1_2`

### 3.3 CA-RF1-3 — Ordenação e filtros visíveis

- **[CA-RF1-3] Nó raiz: Filtros e ordenação**
  - **[UNIT] Funções puras e indicadores**
    - `should_filter_requests_by_status_CA_RF1_3`
    - `should_filter_requests_by_period_CA_RF1_3`
    - `should_sort_requests_by_date_and_amount_CA_RF1_3`
    - `should_mark_active_filters_and_sorted_column_CA_RF1_3`
  - **[INTEG] Filtros ↔ façade ↔ grid**
    - `should_apply_status_filter_and_update_grid_CA_RF1_3`
    - `should_apply_period_filter_and_update_grid_CA_RF1_3`
    - `should_apply_sorting_and_update_grid_order_CA_RF1_3`
    - `should_preserve_filter_values_after_reload_CA_RF1_3`
  - **[E2E] Fluxo com múltiplas solicitações**
    - `creator_applies_filters_and_sorting_and_sees_consistent_ui_CA_RF1_3`

### 3.4 CA-RF1-4 — Acesso ao detalhe da solicitação

- **[CA-RF1-4] Nó raiz: Detalhe**
  - **[UNIT] Componente de detalhe**
    - `should_render_main_fields_from_request_model_CA_RF1_4`
    - `should_show_access_denied_message_when_flagged_CA_RF1_4`
  - **[INTEG] Grid → detalhe via façade/serviço**
    - `should_load_request_details_on_row_click_CA_RF1_4`
    - `should_show_access_denied_when_service_returns_403_or_404_CA_RF1_4`
  - **[E2E] Fluxos de detalhe**
    - `creator_opens_request_detail_and_sees_expected_information_CA_RF1_4`
    - `creator_sees_friendly_error_when_detail_service_fails_CA_RF1_4`

### 3.5 CA-RF1-5 — Cancelamento de solicitação em análise

- **[CA-RF1-5] Nó raiz: Cancelamento em análise**
  - **[UNIT] Lógica de habilitação e mensagem**
    - `should_allow_cancel_only_for_analysis_statuses_CA_RF1_5`
    - `should_build_irreversible_cancel_confirmation_message_CA_RF1_5`
  - **[INTEG] Detalhe + façade + serviço de cancelamento**
    - `should_call_cancel_service_with_correct_id_CA_RF1_5`
    - `should_update_status_to_canceled_by_creator_on_success_CA_RF1_5`
    - `should_show_error_message_and_keep_status_on_cancel_failure_CA_RF1_5`
  - **[E2E] Fluxo completo de cancelamento**
    - `creator_cancels_pending_request_and_sees_updated_status_CA_RF1_5`

### 3.6 CA-RF1-6 — Tratamento de cancelamentos repetidos

- **[CA-RF1-6] Nó raiz: Cancelamentos repetidos**
  - **[UNIT] Lógica de idempotência na UI**
    - `should_disable_or_hide_cancel_action_for_canceled_status_CA_RF1_6`
    - `should_generate_already_canceled_message_CA_RF1_6`
  - **[INTEG] Resposta idempotente do serviço**
    - `should_not_change_status_when_service_reports_already_canceled_CA_RF1_6`
    - `should_show_already_canceled_message_on_repeated_cancel_CA_RF1_6`
  - **[E2E] Fluxo de recancelamento**
    - `creator_attempts_to_cancel_again_and_sees_already_canceled_message_CA_RF1_6`

---

## 4. Ramos transversais: UX, acessibilidade e robustez

### 4.1 Acessibilidade (a11y)

- **[A11Y-1] Navegação por teclado**
  - `a11y_tab_order_follows_header_actions_filters_grid_detail_dialog`
  - `a11y_focus_visible_on_all_interactive_elements`
  - `a11y_focus_returns_to_meaningful_element_after_closing_detail_or_dialog`
- **[A11Y-2] Semântica e estrutura**
  - `a11y_main_landmarks_and_headings_present_on_page`
  - `a11y_status_tags_have_text_labels_besides_color`
  - `a11y_automated_accessibility_scan_reports_no_critical_issues`

### 4.2 Mensagens e erros

- **[ERR-1] Falha de backend na listagem**
  - `should_show_generic_error_message_when_list_service_fails`
  - `should_allow_user_to_retry_list_after_error`
- **[ERR-2] Falha de backend no detalhe**
  - `should_show_friendly_error_when_detail_service_fails`
  - `should_keep_navigation_usable_after_detail_error`
- **[ERR-3] Falha de backend no cancelamento**
  - `should_show_error_and_keep_status_on_cancel_network_failure`
  - `should_allow_retry_or_close_after_cancel_error`

### 4.3 Robustez e volume de dados

- **[PERF-1] Volume moderado de registros**
  - `should_render_grid_with_many_rows_without_ui_freeze`
  - `should_keep_pagination_or_scroll_responsive_with_many_rows`

---

## 5. Autenticação nas requisições — Plano 401

Ramo derivado do relatório-guia 401 (plano `relatório-guia_404_minhas_solicitações_8e4af525.plan.md`): garante que as chamadas à API de antecipação incluam o JWT no header `Authorization: Bearer <accessToken>`, eliminando 401 Unauthorized quando o utilizador está logado. Referência de contrato: [docs/auth-contract.md](auth-contract.md).

**Requisito atendido (plano):** Eliminar 401 nas chamadas à API de antecipação (e a qualquer recurso protegido sob a mesma base URL) quando o usuário está logado (itens 4.1, 4.2 e secção 6 do plano).

### 5.1 UNIT — Interceptor de token (auth-token.interceptor)

| Caso de teste | Comportamento esperado | Rastreabilidade |
|---------------|------------------------|------------------|
| `should_add_Authorization_Bearer_header_when_request_url_starts_with_API_base_and_session_has_accessToken` | Requisição cuja URL começa por `API_BASE_URL` e existe sessão com `accessToken` → request clonado com header `Authorization: Bearer <token>`. | Plano 4.1, 4.2, Validação §6 |
| `should_not_add_Authorization_header_when_request_to_API_base_but_no_session_or_no_accessToken` | Requisição à API base sem sessão (ou sem `accessToken`) → request segue sem adicionar header (inalterado). | Plano 4.1 |
| `should_not_add_Authorization_header_when_request_url_does_not_start_with_API_base` | Requisição a URL que não começa por `API_BASE_URL` → não adicionar header (não interferir com outros origens). | Plano 4.1 |
| `should_clear_session_and_redirect_to_login_on_401_response` (opcional) | Se 4.3 for implementado: resposta 401 → limpar sessão e/ou redirecionar para login. | Plano 4.3 |

**Artefato:** `frontend/src/app/core/auth/auth-token.interceptor.spec.ts` (o interceptor em produção será criado numa etapa posterior; os testes são especificação executável TDD).

### 5.2 E2E — Fluxo autenticado (lista carrega após login)

| Caso de teste | Comportamento esperado | Rastreabilidade |
|---------------|------------------------|------------------|
| `creator_logs_in_then_navigates_to_my_requests_and_page_loads_without_401` | Após login (página de login com credenciais de teste), navegar para `/anticipation/my-requests`; verificar que a página carrega com sucesso: lista de solicitações ou estado vazio visível (sem 401 na experiência do utilizador). | Critérios de aceitação §1, Validação §6 |

**Artefato:** [frontend/e2e/anticipation-my-requests.spec.ts](frontend/e2e/anticipation-my-requests.spec.ts).

### 5.3 Independência dos testes existentes

Os testes do **AnticipationRequestsHttpService** (`anticipation-requests.http.service.spec.ts`) usam `HttpClientTestingModule` sem interceptor e devem permanecer independentes do interceptor (isolamento por mock do serviço ou HttpClient sem interceptor no TestBed). Os testes da **página** Minhas solicitações (`anticipation-my-requests.spec.ts`) mockam `AnticipationRequestsHttpService`, portanto não dependem do interceptor; nenhuma alteração obrigatória nesses specs para o plano 401.

---

## 6. Contrato API — correção 404 (relatório minhas-solicitações)

Ramo derivado do relatório de alterações 404 (plano `relatório_404_minhas_solicitações_c5d4fe06.plan.md`): testes que garantem o alinhamento do frontend com o backend (URLs, query params, corpo de resposta, cancelamento, mapeamento de status). Cobertura dos itens 1–5 do relatório.

| Item plano | Foco | Tipo | Casos de teste |
|------------|------|------|----------------|
| **1** | URL e método da listagem | Unit (infra) | `should_call_GET_anticipations_base_url_for_list_plan_404` |
| **2** | Query params da listagem | Unit (infra) | `should_send_fromUtc_toUtc_status_page_pageSize_in_list_plan_404` |
| **3** | Contrato de resposta (items/totalCount) | Unit (infra) | `should_map_response_items_and_totalCount_to_domain_list_plan_404` |
| **4** | URL do cancelamento | Unit (infra) | `should_call_POST_anticipations_id_cancel_for_cancel_plan_404` |
| **5** | Mapeamento status backend ↔ frontend | Unit (infra) | `should_map_filter_status_to_backend_enum_values_plan_404`, `should_map_backend_status_to_domain_status_plan_404` |

### 6.1 Detalhamento — listagem (itens 1–3)

- **[PLAN-404-LIST] Listagem alinhada ao backend**
  - `should_call_GET_anticipations_base_url_for_list_plan_404` — Serviço HTTP chama **GET** `${apiBaseUrl}/api/v1/anticipations` (sem segmento `/minhas`).
  - `should_send_fromUtc_toUtc_status_page_pageSize_in_list_plan_404` — Parâmetros de query enviados: `fromUtc`, `toUtc`, `status`, `page`, `pageSize` (nomes e formatos do backend).
  - `should_map_response_items_and_totalCount_to_domain_list_plan_404` — Resposta `{ items, totalCount }` é mapeada para array de domínio; campos do item (id, creatorId, status, valores, datas) mapeados para `AnticipationRequest`.

### 6.2 Detalhamento — cancelamento (item 4)

- **[PLAN-404-CANCEL] Cancelamento alinhado ao backend**
  - `should_call_POST_anticipations_id_cancel_for_cancel_plan_404` — Serviço HTTP chama **POST** `${apiBaseUrl}/api/v1/anticipations/{id}/cancel` (segmento `cancel`, não `cancelamento`).

### 6.3 Detalhamento — status (item 5)

- **[PLAN-404-STATUS] Mapeamento de status**
  - `should_map_filter_status_to_backend_enum_values_plan_404` — Ao montar filtro de listagem, status do domínio (ex.: PENDING) é convertido para o valor esperado pelo backend (int ou string conforme contrato).
  - `should_map_backend_status_to_domain_status_plan_404` — Ao receber list/detail, string ou int de status do backend é mapeada para `AnticipationRequestStatus`.

**Artefato:** [AnticipationRequestsHttpService](frontend/src/app/infrastructure/anticipation/anticipation-requests.http.service.ts) — spec em `anticipation-requests.http.service.spec.ts`.

**Estado até implementação do plano:** Os 6 casos acima estão implementados no spec. Enquanto o código de produção não for alterado conforme o relatório 404, 5 testes falham (3 por URL/params incorretos, 2 por mapeamento de resposta/status). Após a implementação das alterações no serviço HTTP, a suíte deve ficar verde.

---

## 7. Esquema de rastreabilidade sugerido

Estrutura lógica para futura documentação/automação (ex.: em `docs/tracability.md` ou arquivo dedicado), ligando requisitos, componentes Angular e testes.

Exemplo de esquema conceitual:

```yaml
rf1:
  ca_rf1_1:
    requirement: "Lista restrita ao próprio Creator"
    components:
      - AnticipationMyRequestsPageComponent
      - AnticipationRequestsTableComponent
      - AnticipationMyRequestsFacade
      - AnticipationRequestsService
    tests:
      unit:
        - should_normalize_requests_to_table_rows_CA_RF1_1
        - should_format_amounts_and_dates_CA_RF1_1
        - should_build_minimum_columns_CA_RF1_1
      integration:
        - should_call_list_service_with_creator_id_CA_RF1_1
        - should_render_rows_returned_by_service_CA_RF1_1
        - should_filter_out_requests_from_other_creators_CA_RF1_1
      e2e:
        - creator_sees_only_own_requests_in_list_CA_RF1_1
  ca_rf1_2:
    requirement: "Lista vazia com orientação"
    # ...
```

Padrões de nome recomendados para os testes (a serem usados nos `*.spec.ts` e specs E2E):

- **Unit**: `should_<comportamento>_CA_RF1_X`
- **Integração**: `should_<fluxo>_integration_CA_RF1_X`
- **E2E**: `creator_<fluxo>_e2e_CA_RF1_X`

Com isso, qualquer agente ou desenvolvedor consegue:

- Partir de um `CA-RF1-x` e encontrar rapidamente quais componentes e serviços o exercitam.
- Verificar, pelo nome do teste, qual critério de aceitação está sendo coberto.
- Evoluir a árvore (novos estados, novos filtros) adicionando apenas novos ramos locais, mantendo coesão e rastreabilidade.

