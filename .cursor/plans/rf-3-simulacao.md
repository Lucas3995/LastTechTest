# PLANO / FONTE DE VERDADE — RF-3: Simulação de Solicitação de Antecipação (Frontend)

**Versão:** 1.0  
**Data:** 6 de março de 2026  
**Autor:** GitHub Copilot (agente especializado)  
**Status:** Pronto para Testes & Implementação  

---

## § 1. RESUMO EXECUTIVO (TL;DR)

### Objetivo
Implementar uma **tela de simulação de antecipação** que permita a **Creator**, **Admin** e **Analista** experimentar cenários de antecipação com base nas mesmas regras de cálculo/validação do backend (RA-4), apresentando resultados claros e, opcionalmente, permitindo conversão em solicitação real.

### Escopo de Negócio
- ✅ **Quem**: Creator (simula para si), Analista (simula em nome de creator), Admin (simula e converte em nome de qualquer creator)
- ✅ **O quê**: Formulário + resultado simulado + opção de conversão
- ✅ **Onde**: Área/módulo "Antecipação" do frontend (Feature: `anticipation`)
- ✅ **Por quê**: Reduzir fricção na experimentação de cenários; aumentar confiança antes de criar solicitação real
- ✅ **Como**: Reutilizar HTTP service existente de antecipação; integrar APIs de simulação-conversão do backend (RA-4)

### Artefatos Esperados (Frontend)
1. **Nova rota**: `/anticipation/simulation`
2. **Nova página**: `features/anticipation/pages/simulation/`
   - Componente: `AnticipationSimulationPageComponent`
3. **Novos componentes**:
   - `AnticipationSimulationFormComponent` — formulário de entrada
   - `AnticipationSimulationResultPanelComponent` — painel de resultado
   - (internos) controles de conversão condicionados a role/estado
4. **Nova facade** ou extensão da existente:
   - `AnticipationSimulationFacade` — orquestração de simulação + conversão
5. **Extensão do HTTP service**:
   - Métodos para `simulateAnticipation()` e `convertSimulationToReal()`
6. **Extensão do domain**:
   - Tipos/interfaces para `AnticipationSimulation`, `SimulationResult`, `ConversionResult`

### Critério de Pronto
- Rotas definidas em `app.routes.ts`
- Componentes hierarquizados com testes unitários
- Facade implementada com estado via Signals
- HTTP service + types completados
- Testes E2E cobrinndo os 5 cenários principais (CA-RF3-1 a CA-RF3-6)
- Integração com navegação (links a partir de RF-1, RF-2 mencionados na demanda)

---

## § 2. FASE TRADUTOR — Análise de Negócio e UX

### 2.1 Contexto & Personas

#### Personas Envolvidas
1. **Creator** — Criador de conteúdo; quer testar cenários antes de enviar solicitação real
2. **Analista** — Papel de apoio; simula em nome de others para suporte; não converte
3. **Admin** — Superuser; acesso a todas ações

#### User Stories
| ID | Persona | Ação | Benefício |
|----|---------|------|-----------|
| US-S1 | Creator | Simular antecipação com mesmos critérios da real | Entender valor líquido, taxas, prazos antes de decidir |
| US-S2 | Creator | Ver resultado com indicação clara de validade | Saber até quando confiar nos valores para converter |
| US-S3 | Creator | Converter simulação válida em solicitação real | Formalizar pedido sem preencher dados novamente |
| US-S4 | Analista/Admin | Simular em nome de outro creator | Apoiar atendimento sem criar registros reais |

#### Fluxos de Uso

**Fluxo 1: Simulação como Creator**
```
1. Creator acessa tela (a partir de RF-1 "Simular antecipação" ou menu)
2. Preenche: valor solicitado, contratos/recebíveis, datas (conforme API exigir)
3. Submete: "Simular"
4. Validações inline (valor mínimo, elegibilidade)
5. Se válida: exibe "Painel de Resultado" com valores, validade, e opção "Criar solicitação real"
6. Se inválida: mensagens erro localizadas a cada campo
```

**Fluxo 2: Conversão em Solicitação Real**
```
1. Creator vê simulação válida no painel
2. Clica "Criar solicitação real com base nesta simulação"
3. Diálogo de confirmação
4. Backend verifica: não existe solicitação em aberto, validade OK, regras OK
5. Se OK: solicitação criada (valores idênticos aos da simulação); feedback sucesso + link RF-1
6. Se falha: mensagem específica (expirada? pendente? regras violadas?)
```

**Fluxo 3: Simulação por Analista em nome de Creator**
```
1. Analista acessa tela (de admin/lista global ou menu)
2. **Seleciona creator-alvo** (autocomplete ou pick list)
3. Preenche parâmetros de simulação
4. Submete
5. Resultado exibido normalmente, sem opção de conversão (Analista não converte)
6. Texto: "Simulação realizada em nome de [Creator Name]"
```

### 2.2 Requisitos Funcionais (Mapeados de RF-3)

#### CA-RF3-1: Simulação Válida
- ✅ Creator simula com parâmetros válidos
- ✅ Resultado exibe: valor bruto, taxa, valor líquido, datas
- ✅ Resultado inclui "Válida até [HH:mm]" (20 minutos antes da expiração real RA-4)
- ✅ Nenhuma solicitação real criada
- ✅ HTTP 200 + dados de resultado

#### CA-RF3-2: Erros de Validação Localizados
- ✅ Parâmetros inválidos → mensagens por campo
- ✅ Valor mínimo? → "Mínimo R$ 100,00"
- ✅ Elegibilidade? → Mensagem do backend extraída e apresentada
- ✅ Sem criação de solicitação real
- ✅ HTTP 400/422 + lista de erros

#### CA-RF3-3: Sem Efeitos Persistentes
- ✅ Múltiplas simulações não criam solicitações em RF-1
- ✅ Cache é in-memory no backend (invisível ao usuário)
- ✅ Última simulação fica "pronta" para conversão (user não vê)

#### CA-RF3-4: Conversão Bem-Sucedida
- ✅ Simulação válida → clica "Criar solicitação real"
- ✅ Backend valida: não há pendente, validade OK, regras OK
- ✅ Solicitação criada com valores **idênticos** aos da simulação
- ✅ Feedback: "Solicitação criada com sucesso!" + link "Ir para Minhas Solicitações"
- ✅ HTTP 201/200 + dados de solicitação criada

#### CA-RF3-5: Rejeição Conversão (5 cenários)
- ✅ **Expirada**: "Simulação expirou. Realize uma nova simulação."
- ✅ **Já utilizada**: "Esta simulação já foi utilizada para criar uma solicitação."
- ✅ **Pendente existente**: "Você já possui uma solicitação em aberto. Conclua ou cancele-a antes."
- ✅ **Regras violadas**: "As condições de negócio mudaram. Realize uma nova simulação."
- ✅ HTTP 422 + motivo específico

#### CA-RF3-6: Admin/Analista Simula em Nome de
- ✅ Seleciona creator-alvo
- ✅ Simula normalmente
- ✅ Resultado mostrado; sem opção de conversão para Analista
- ✅ Texto: "Simulação realizada em nome de [creator]"
- ✅ Admin pode converter; Analista não

### 2.3 UX e Requisitos Visuais

#### Componentes de UI

**Formulário de Simulação**
- Campos: valor solicitado, contratos/recebíveis (multi-select?), datas
- Para Admin/Analista: autocomplete de creator-alvo
- Validações inline (feedback visual em tempo real)
- Botão "Simular" (habilitado quando campos essenciais preenchidos)
- Botão "Limpar" (opcional, reinicia formulário)

**Painel de Resultado**
- Exibido apenas após simulação bem-sucedida
- Layout horizontal/grid:
  - **Col 1 (Valores)**: Valor solicitado, Taxa (%), Valor líquido (R$) — em destaque (typography XL ou cards)
  - **Col 2 (Datas/Info)**: Data de antecipação, Data de vencimento (se aplicável)
  - **Col 3 (Status)**: Badge "Válida até 11:40" (com cor (alerta se < 10 min?)
- Texto: "Esta é uma simulação. Nenhuma solicitação foi criada."

**Área de Conversão**
- Botão "Criar solicitação real com base nesta simulação" (Creator/Admin apenas)
- Desabilitado se: não há simulação, simulação expirada, já há pendente
- Clique → diálogo de confirmação
- Success → feedback + link "Ir para Minhas Solicitações"
- Erro → mensagem contextualizada (vide CA-RF3-5)

#### Acessibilidade & Clareza

- **Rótulos descritivos**: "Valor que deseja antecipar (R$)" em vez de "Valor"
- **Textos de ajuda**: pequenos, inline, focados (ex.: "Você pode antecipar até 90% dos seus recebíveis")
- **Mensagens de erro**: específicas, com **código de suporte** (`traceId` do backend, se disponível)
- **Foco**: ao exibir painel de resultado, foco automático no título/resumo
- **Navegação:** teclado funciona; contraste WCAG AA mínimo
- **Indicadores visuais**: loading spinner ao enviar, disabled buttons ao processar

---

## § 3. FASE MAESTRO — Análise Técnica e Mapeamento

### 3.1 Estado Atual do Frontend

#### Estrutura Existente (Referência)

| Aspecto | Atual | Observação |
|---------|-------|-----------|
| **Rotas** | `anticipation/my-requests`, `anticipation/my-requests/new`, `anticipation/list` | Nova: `anticipation/simulation` |
| **Página existente** | `features/anticipation/pages/my-requests/`, `new-request/`, `admin-list/` | Pattern: componentes standalone, TemplateUrl/StyleUrl, DI de facade |
| **Facades** | `AnticipationMyRequestsFacade`, `AnticipationAdminListFacade` | Usam Signals para estado; injetam porta; pattern: `loading`, `errorMessage`, `requests`, etc. |
| **HTTP Service** | `AnticipationRequestsHttpService` (implementa `AnticipationRequestsPort`) | Métodos: `listMyRequests`, `listGlobalRequests`, `getRequestDetail`, `cancelRequest`, `approveRequest`, `rejectRequest`, `createRequest` |
| **Domain Types** | `AnticipationRequest`, `AnticipationRequestStatus`, `AnticipationRequestsPort`, `CreateAnticipationRequestPayload`, `CreateAnticipationRequestResult` | Interface porta define contrato HTTP |
| **Componentes** | Form, table, detail, filters, empty-state | Pattern: importam domain e application; recebem dados de facade |

#### Padrões Identificados (a seguir)

1. **Página standalone**: injeta facade, expõe signals como props, chama métodos em eventos (ngOnInit, button clicks)
2. **Facade via DI**: importa porta (interface), chama métodos como `firstValueFrom(port.method(...))`
3. **Tratamento de erro**: fallback a `defaultErrorPresentation`; setter `setErrorFromPresentation()` ou similar
4. **Formulários**: `FormGroup` + `FormControl` com validadores TypeScript
5. **Templates**: reusam componentes (tabla, filters) em múltiplas páginas

#### Pontos de Integração Necessários

- **Rotas**: adicionar rota `/anticipation/simulation` em [app.routes.ts](./src/app/app.routes.ts)
- **Componentes**: criar hierarquia em [features/anticipation/pages/simulation/](./src/app/features/anticipation/pages/simulation/)
- **Facade**: criar nova ou estender [AnticipationMyRequestsFacade](./src/app/application/anticipation/anticipation-my-requests.facade.ts)
- **HTTP Service**: adicionar métodos a [AnticipationRequestsHttpService](./src/app/infrastructure/anticipation/anticipation-requests.http.service.ts)
- **Domain**: adicionar tipos a [domain/anticipation.ts](./src/app/domain/anticipation.ts)

### 3.2 Mapeamento de Alterações (Maestro Report)

#### ALTERAÇÃO #1: Estender Domain com Tipos de Simulação
**Tipo**: Criar  
**Arquivo**: [frontend/src/app/domain/anticipation.ts](frontend/src/app/domain/anticipation.ts)  
**O quê**:
```markdown
- Interface `AnticipationSimulation` (simulationCode, validUntil, grossAmountCents, feesAmountCents, netAmountCents, datas relevantes)
- Interface `SimulationResult` (estendendo AnticipationSimulation)
- Interface `ConversionResult` (id, protocol, status, netAmount — para retorno de criação de solicitação real a partir de simulação)
- Type `SimulationError` (para erros de conversão: "SIMULATION_EXPIRED", "ALREADY_USED", "PENDING_EXISTS", "RULES_VIOLATED")
- Estender porta `AnticipationRequestsPort`: adicionar `simulateAnticipation()` e `convertSimulationToReal()`
```
**Justificativa**: Tipagem strong; facilita testes e documentação de contrato HTTP.

---

#### ALTERAÇÃO #2: Estender HTTP Service com Métodos de Simulação & Conversão
**Tipo**: Alterar  
**Arquivo**: [frontend/src/app/infrastructure/anticipation/anticipation-requests.http.service.ts](frontend/src/app/infrastructure/anticipation/anticipation-requests.http.service.ts)  
**O quê**:
```markdown
- Método `simulateAnticipation(payload: SimulateAnticipationPayload): Observable<SimulationResult>`
  - POST `/api/v1/anticipations/simulations`
  - Payload: { requestedAmount, creatorId? (se Admin/Analista) }
  - Retorno: { simulationCode, validUntil, grossAmountCents, ... }
  - Mapeamento: backend retorna ISO dates; converter para Date local

- Método `convertSimulationToReal(simulationCode: string, creatorId?: string): Observable<ConversionResult>`
  - POST `/api/v1/anticipations/simulations/{simulationCode}/confirm`
  - Payload: { creatorId? } (se Admin/Analista)
  - Retorno: { id, protocol, status, netAmount, ... } (ou erro 422 com motivo)
  - Mapeamento & tratamento de erro: extrair `message`, `code`, `traceId` (conforme RA-4) 
```
**Justificativa**: Escalação do contrato HTTP; reutiliza pattern existente.

---

#### ALTERAÇÃO #3: Criar Nova Facade para Simulação & Conversão
**Tipo**: Criar  
**Arquivo**: [frontend/src/app/application/anticipation/anticipation-simulation.facade.ts](frontend/src/app/application/anticipation/anticipation-simulation.facade.ts)  
**O quê**:
```markdown
Classe `AnticipationSimulationFacade` (@Injectable):
- Signals:
  - _simulationResult: signal<AnticipationSimulation | null>
  - _loading: signal<boolean>
  - _errorMessage: signal<string | null>
  - _errorSupportId: signal<string | null>
  - _infoMessage: signal<string | null>
  - _canConvert: signal<boolean> (derivado de simulationResult + user role)
  - _creatorIdForSimulation: signal<string | null> (para Admin/Analista)
  
- ReadOnly signals expostos como props

- Métodos (async):
  - `simulate(payload: SimulateAnticipationPayload): Promise<void>`
    - chama `httpService.simulateAnticipation()`
    - atualiza _simulationResult e _infoMessage
    - captura erro, constrói apresentação via ERROR_PRESENTATION_BUILDER
    - trata timeout/rede normalmente
    
  - `convertToRealRequest(): Promise<void>`
    - verifica se há simulação válida
    - chama `httpService.convertSimulationToReal(simulationCode)`
    - em sucesso: UPDATE _infoMessage, limpa _simulationResult (simulação "usada")
    - em erro: extrai motivo específico (expirada, pendente, regras), seta _errorMessage
    
  - `setCreatorIdForSimulation(creatorId: string)` (para Admin/Analista)
  
  - `reset()` limpa estado (formulário reset manual)

- Injeção:
  - injeta ANTICIPATION_REQUESTS_PORT (porta)
  - injeta ERROR_PRESENTATION_BUILDER
  - padrão: cópia de AnticipationMyRequestsFacade
```
**Justificativa**: Separação de concerns (façade dedicada à simulação); reutilização de padrão existente (Signals, falhasRxJS, error handling).

---

#### ALTERAÇÃO #4: Criar Página de Simulação (Componente Principal)
**Tipo**: Criar  
**Arquivo**: [frontend/src/app/features/anticipation/pages/simulation/anticipation-simulation-page.component.ts](frontend/src/app/features/anticipation/pages/simulation/anticipation-simulation-page.component.ts) (+ .html, .scss)  
**O quê**:
```markdown
Componente standalone `AnticipationSimulationPageComponent`:
- Imports: CommonModule, ReactiveFormsModule, AnticipationSimulationFormComponent, AnticipationSimulationResultPanelComponent, AuthService (para verificar role)
- Injeção: AnticipationSimulationFacade, AuthService, Router
- Props (do facade):
  - simulationResult, loading, errorMessage, infoMessage, canConvert, errorSupportId
- Métodos:
  - `ngOnInit()`: reset facade state, carregar dados iniciais (se necessário, ex.: lista de contratos)
  - `onSimulate(payload)`: chama facade.simulate(payload)
  - `onConvertClick()`: diálogo confirmação → chama facade.convertToRealRequest()
  - `onConvertSuccess()`: router.navigate(['anticipation', 'my-requests'])
- Template:
  - Header: "Simulação de Antecipação"
  - InfoBox: "Esta é uma simulação. Nenhuma solicitação foi criada."
  - FormComponent: passar facade e métodos
  - ResultPanel: mostrar se simulationResult() não null
  - ConversionPanel: mostrar se canConvert() e (user.role === 'Creator' || 'Admin')
  - ErrorBox: se errorMessage()
```
**Justificativa**: Página-container reutilizando padrão existente (RF-1, RF-5); composição de componentes menores.

---

#### ALTERAÇÃO #5: Criar Componente de Formulário de Simulação
**Tipo**: Criar  
**Arquivo**: [frontend/src/app/features/anticipation/components/anticipation-simulation-form/anticipation-simulation-form.component.ts](frontend/src/app/features/anticipation/components/anticipation-simulation-form/anticipation-simulation-form.component.ts) (+ .html, .scss)  
**O quê**:
```markdown
Componente `AnticipationSimulationFormComponent`:
- Input:
  - @Input() isSubmitting: boolean (via signal)
  - @Input() errorMessage?: string
  - @Input() creatorIdForSimulation?: string (se Admin/Analista é default)
  
- Output:
  - @Output() submit = new EventEmitter<SimulateAnticipationPayload>()
  - @Output() creatorSelected = new EventEmitter<string>() (para admin)
  
- Form (FormGroup):
  - requestedAmount: FormControl (Validators.required, Validators.min(100))
  - creatorId: FormControl (se isAdmin/Analista)
  - [outros campos conforme RA-4 exigir, ex.: contratos, datas]
  
- Template:
  - Campos com labels descritivos, placeholders, help text
  - Validação inline (ngIf form.get('field').errors)
  - Botão "Simular" (disabled se form.invalid || isSubmitting)
  - Botão "Limpar" (reset form)
  - Se Admin/Analista: campo de seleção de creator (autocomplete)
  
- Comportamento:
  - ngOnInit: carregar lista de contratos (if needed) via HTTP
  - onSubmit: validação, emit() com payload
```
**Justificativa**: Formulário isolável para testes; reutilizável se necessário.

---

#### ALTERAÇÃO #6: Criar Componente de Painel de Resultado
**Tipo**: Criar  
**Arquivo**: [frontend/src/app/features/anticipation/components/anticipation-simulation-result-panel/anticipation-simulation-result-panel.component.ts](frontend/src/app/features/anticipation/components/anticipation-simulation-result-panel/anticipation-simulation-result-panel.component.ts) (+ .html, .scss)  
**O quê**:
```markdown
Componente `AnticipationSimulationResultPanelComponent`:
- Input:
  - @Input() simulationResult: AnticipationSimulation (required)
  - @Input() canConvert: boolean
  - @Input() userRole: string
  - @Input() isConverting: boolean (para loading)
  
- Output:
  - @Output() convertClick = new EventEmitter<SimulationResult>()
  
- Template:
  - Header: "Resultado da Simulação"
  - Grid (2-3 cols):
    - Col 1: Valor solicitado, Taxa (%), Valor líquido (R$) — em destaque (typography XL ou cards)
    - Col 2: Data de antecipação, Data de vencimento (se aplicável)
    - Col 3: Badge "Válida até 11:40" (com cor (alerta se < 10 min?)
  - Info box: "Esta é uma simulação. Nenhuma solicitação foi criada."
  - Botão "Criar solicitação real com base nesta simulação" (if canConvert && userRole in ['Creator', 'Admin'])
    - Desabilitado se isConverting || !canConvert
  - [Opcionalmente] colapser: exibir "Detalhes técnicos" (simulationCode, createdAt)
  
- Comportamento:
  - Efeito: monitora validUntil; se expirou, desabilita botão + mostra aviso
```
**Justificativa**: Exibição isolada; facilitahidden lógica condicional de display.

---

#### ALTERAÇÃO #7: Criar Componente de Diálogo de Confirmação de Conversão
**Tipo**: Criar (ou reutilizar via Angular Material Dialog, se disponível)  
**Arquivo**: [frontend/src/app/features/anticipation/components/confirm-conversion-dialog/confirm-conversion-dialog.component.ts](frontend/src/app/features/anticipation/components/confirm-conversion-dialog/confirm-conversion-dialog.component.ts) (opcional)  
**O quê**:
```markdown
Componente simples de confirmação:
- Input: simulationResult (para exibir resumo: "Criar solicitação real com valor líquido de R$ X?")
- Output: confirm, cancel
- Botões: "Criar", "Cancelar"

OU usar NgxMatDialog se disponível.
```
**Justificativa**: UX clara; evita ações acidentais.

---

#### ALTERAÇÃO #8: Adicionar Rota de Simulação
**Tipo**: Alterar  
**Arquivo**: [frontend/src/app/app.routes.ts](frontend/src/app/app.routes.ts)  
**O quê**:
```markdown
Adicionar rota:
{
  path: 'anticipation/simulation',
  canActivate: [AuthGuard],
  data: { requiredRoles: ['Creator', 'Admin', 'Analista'] },
  loadComponent: () => import('./features/anticipation/pages/simulation/...').then(m => m.AnticipationSimulationPageComponent)
}

Ou, se preferir subrotas (nested):
{
  path: 'anticipation',
  canActivate: [AuthGuard],
  children: [
    { path: 'my-requests', ... },
    { path: 'my-requests/new', ... },
    { path: 'list', ... },
    { path: 'simulation', data: { requiredRoles: ['Creator', 'Admin', 'Analista'] }, ... }
  ]
}
```
**Justificativa**: Coerência com rotas existentes; lazy loading via loadComponent.

---

#### ALTERAÇÃO #9: Exportar Novos Tipos e Facade em Index Modules
**Tipo**: Alterar  
**Arquivos**:
- [frontend/src/app/domain/index.ts](frontend/src/app/domain/index.ts) — exportar `AnticipationSimulation`, `SimulationResult`, `ConversionResult`, tipos de erro
- [frontend/src/app/application/anticipation/index.ts](frontend/src/app/application/anticipation/index.ts) — exportar `AnticipationSimulationFacade`  
**O quê**: Expose novos tipos e facade para componentes.

---

#### ALTERAÇÃO #10: Atualizar Template HTML da Página Principal
**Tipo**: Alterar (opcional, UX)  
**Arquivo**: [frontend/src/app/features/anticipation/pages/my-requests/anticipation-my-requests-page.component.html](frontend/src/app/features/anticipation/pages/my-requests/anticipation-my-requests-page.component.html)  
**O quê**:
```markdown
Adicionar botão/link "Simular antecipação" apontando para rota `/anticipation/simulation`:
<button (click)="router.navigate(['anticipation', 'simulation'])">Simular Antecipação</button>
```
**Justificativa**: Integração de UX; permite que Creator acesse simulação a partir de RF-1.

---

#### ALTERAÇÃO #11: Atualizar Página de Admin/Lista Global
**Tipo**: Alterar (opcional, UX)  
**Arquivo**: [frontend/src/app/features/anticipation/pages/admin-list/](...)  
**O quê**:
```markdown
Adicionar ação "Simular em nome do criador" (Admin/Analista) que:
- Seleciona creator (já selecionado no contexto?)
- Navega para `/anticipation/simulation?creatorId=XXX`
- Façade pré-popula creatorIdForSimulation
```
**Justificativa**: Acesso direto para Admin/Analista.

---

### 3.3 Matriz de Dependências

```
┌─ Domain (anticipation.ts)
│  ├─ AnticipationSimulation, SimulationResult, ConversionResult
│  └─ AnticipationRequestsPort (extend simulateAnticipation, convertSimulationToReal)
│
├─ Infrastructure (anticipation-requests.http.service.ts)
│  ├─ Implementa métodos HTTP
│  └─ Mapeia backend payload → domain types
│
├─ Application (anticipation-simulation.facade.ts)
│  ├─ Injeta AnticipationRequestsPort
│  ├─ Gerencia estado via Signals
│  └─ Orquestra simulação + conversão
│
├─ Core (app.routes.ts)
│  └─ Define rota '/anticipation/simulation'
│
└─ Features (pages/simulation/ + components/)
   ├─ Página container injeta facade
   ├─ Form component recebe payload
   ├─ Result panel exibe resultado
   └─ Diálogo confirmação (opt)
```

---

## § 4. CONSOLIDAÇÃO — Alterações Necessárias (Relatório Estruturado)

### 4.1 Resumo de Alterações

| ID | Tipo | Localização | O Quê | Precedência | Complexidade |
|----|------|-------------|-------|-------------|--------------|
| **A1** | Criar | domain/anticipation.ts | Tipos de simulação (interfaces) | 1 | Baixa |
| **A2** | Alterar | infrastructure/..http.service.ts | Métodos HTTP simulação/conversão | 2 | Média |
| **A3** | Criar | application/...simulation.facade.ts | Façade com Signals | 3 | Média |
| **A4** | Criar | features/.../pages/simulation/ | Página container | 4 | Média |
| **A5** | Criar | features/.../components/simulation-form/ | FormComponent isolado | 5 | Baixa |
| **A6** | Criar | features/.../components/simulation-result-panel/ | ResultPanel isolado | 5 | Baixa |
| **A7** | Criar (opt) | features/.../components/confirm-dialog/ | Diálogo confirmação | 6 | Muito Baixa |
| **A8** | Alterar | app.routes.ts | Rota '/anticipation/simulation' | 7 | Muito Baixa |
| **A9** | Alterar | domain/index.ts, application/index.ts | Exports novos tipos | 8 | Muito Baixa |
| **A10** | Alterar (opt) | features/.../my-requests/...html | Botão "Simular" | 9 | Muito Baixa |
| **A11** | Alterar (opt) | features/.../admin-list/ | Ação "Simular em nome de" | 10 | Baixa |

---

### 4.2 Detalhes por Alteração

Já mapeado em **§3.2** acima. Vide **ALTERAÇÃO #1 até #11** para descrições completas.

---

## § 5. VERIFICAÇÃO — Mapeamento de Critérios de Aceitação

### Rastreabilidade RF-3 ↔ Código & Testes

| CA-RF3 | Requisito | Componentes Testados | Teste Unitário | Teste E2E |
|--------|-----------|----------------------|-----------------|-----------|
| **CA-RF3-1** | Simulação válida com resultado | Form, HTTP service, Facade, ResultPanel | simulateAnticipation() retorna SimulationResult | Creator preenche form, submete, vê resultado |
| **CA-RF3-2** | Mensagens de erro localizadas | Form validators, HTTP error handling | Form.invalid triggers msg | Creator submete com dados inválidos, vê erros |
| **CA-RF3-3** | Sem efeitos persistentes | HTTP service (mock), Facade | simulateAnticipation() não persiste | Múltiplas simulações, RF-1 sem solicitações novas |
| **CA-RF3-4** | Conversão bem-sucedida | convertSimulationToReal(), Facade | HTTP call returns ConversionResult | Creator simula, converte, vê em RF-1 |
| **CA-RF3-5** | Rejeição conversão (5 cenários) | HTTP error responses, Facade error handling | Error code → message map | Admin tenta converter expirada/pendente, vê mensagem |
| **CA-RF3-6** | Admin/Analista simula em nome de | creatorIdForSimulation, Form field | Form creatorId field, HTTP payload | Analista seleciona creator, simula, vê resultado |

---

## § 6. Decisões e Ambiguidades

### 6.1 Decisões Tomadas (com Justificativa)

| # | Decisão | Justificativa | Alternativa (Rejeitada) |
|---|---------|---------------|------------------------|
| **D1** | Nova Façade dedicada (`AnticipationSimulationFacade`) em vez de estender existente | Separação clara de responsabilidades; fluxo de simulação é distinto de "meus requisitos". Facilita testes e manutenção futura. | Estender `AnticipationMyRequestsFacade`; comprometeria SRP |
| **D2** | Componentes separados (Form, ResultPanel) em vez de tudo em uma página | Testabilidade; reutilização; compatibilidade com shared library; padrão do projeto (ex: RF-1). | Tudo em uma página; acoplamento alto |
| **D3** | Rota `/anticipation/simulation` com requiredRoles incluindo 'Analista' | RA-4 defineque Analista pode simular. Role check em qualquer rota garante segurança. | Sem role check; confiar no backend |
| **D4** | Error handling via `ERROR_PRESENTATION_BUILDER` (conforme existente) | Reutilização de padrão; mensagens consistentes; suporte e traceId. | Strings hardcoded de erro |
| **D5** | Conversão via diálogo de confirmação (UX clara) | Evita ações acidentais; RA-4 exige revalidação de regras; confirmação comunica gravidade. | Submit direto; falta confirmação |
| **D6** | Armazenar `simulationCode` no Painel (opcional exibição técnica) | Facilita debugging; usuário pode compartilhar code com suporte se necessário. | Ocultar; reduz rastreabilidade |

### 6.2 Ambiguidades Residuais (a Clarificar com Stakeholders)

| # | Ambigüidade | Impacto | Sugestão de Resolução |
|---|-------------|--------|----------------------|
| **AmbA1** | Quais são exatamente os "contratos/recebíveis" que Creator seleciona no formulário? RA-4 menciona "conforme regras de RA-1", mas RF-3 não especifica UI. | Design do Form; complexidade do controle. | ✋ Verificar RA-1 ("Criar solicitação") para entender quais campos são expostos. Presumir que seguem o mesmo padrão. Se RA-1 não propõe UI, assumir valor simples (sem seleção de contratos) por ora; expansão posterior. |
| **AmbA2** | Backend retorna `validUntil` como ISO 8601? E a lógica "20 minutos antes" é cálculo do backend ou frontend? | Desincronía de relógios; UX confusa se implementado errado. | ✋ Assumir que backend retorna `validUntil` já ajustado (20 min antes). Frontend exibe direto. Se backend retornar `expiresAt` (hora real), frontend calcula `validUntil = expiresAt - 20min`. Alinhar com RA-4 ao implementar testes. |
| **AmbA3** | Admin pode simular em nome de creator E também converter? Ou só Creator/Analista simulam e Admin supervisiona? | Fluxo de Admin; permissões. | ✋ RA-4 afirma: "Admin tem acesso a TODAS ações de Creator e Analista". Portanto, Admin: simula + converte. Implementar control: if (userRole === 'Admin' \|\| userRole === 'Creator') → mostrar botão conversão. Testar com canActivate roles. |
| **AmbA4** | Se Creator tem simulação válida mas cria nova simulação antes de converter, a primeira é descartada. O frontend avisa que a anterior se perdeu? | UX: nova simulação silenciosamente sobrescreve antiga. | ✋ OK por design (RA-4 especifica: última simulação por creator). Feedback: nada muda (Frontend carro); se usuário voltar ao formulário, resultado anterior desaparece. Opcional: toast de info "Anterior simulação descartada." |
| **AmbA5** | Qual é o contrato exato de `POST /api/v1/anticipations/simulations` e `POST /api/v1/anticipations/simulations/{code}/confirm`? Os payloads/responses que descrevi estão corretos? | Implementação do HTTP service; mapeamentoBackend tipos. | ✋ Validar contra RA-4 backend ou contrato OpenAPI do backend. RF-3 frontend não redefine; reusa. Provisoriamente, seguir schema em RA-4 (seção "Fluxos de uso"). |

### 6.3 Notas de Implementação

1. **Segurança (AuthGuard + Roles)**: Rota protegida por `AuthGuard` E field `requiredRoles`. Backend revalida tudo na chamada HTTP. Frontend é apenas UI.
2. **Acessibilidade**: Validações inline (aria-describedby), foco em diálogos, imagens alt. Testar com screen reader.
3. **Performance**:HTTP service usa `map`. Facade usa `firstValueFrom` (async/await). Se necessário, adicionar retry logic via RxJS retry operator no HTTP service.
4. **Cache Frontend**: Não há cache extra no frontend. Cada submit gera HTTP call. Backend cache (RA-4) é invisible.
5. **i18n**: Textos hardcoded neste plano. Projeto usará i18n actual (ex.: ngx-translate?). Strings de error do backend (do `buildErrorPresentation`).

---

## § 7. Referências & Rastreabilidade

### Documentação Origem

| Documento | Seção | Relevância |
|-----------|-------|-----------|
| `demandas/RF-3-simulacao-solicitacao-antecipacao.md` | User stories, CAs, componentes | Spec principal |
| `demandas/RA-4-simulacao-solicitacao-antecipacao.md` | Fluxos backend, cache, regras | Dependência backend |
| `frontend/src/app/app.routes.ts` | Rotas existentes | Padrão de rotas |
| `frontend/src/app/application/anticipation/*.facade.ts` | Padrão de façade | Template código |
| `frontend/src/app/features/anticipation/pages/my-requests/` | Página exemplo | Pattern página |
| `frontend/src/app/domain/anticipation.ts` | Types atuais | Extensão domain |
| `.cursor/rules/angular-frontend.mdc` | Critério técnico | Obrigatório |
| `.cursor/skills/maestro/SKILL.md` | Processo maestro | Metodologia |
| `docs/tracability.md` | Rastreabilidade | Integração |

### Arquivos Afetados (Checklist)

- [ ] [frontend/src/app/domain/anticipation.ts](frontend/src/app/domain/anticipation.ts) — Alterar: adicionar tipos
- [ ] [frontend/src/app/infrastructure/anticipation/anticipation-requests.http.service.ts](frontend/src/app/infrastructure/anticipation/anticipation-requests.http.service.ts) — Alterar: métodos HTTP
- [ ] [frontend/src/app/application/anticipation/](frontend/src/app/application/anticipation/) — **Criar**: anticipation-simulation.facade.ts
- [ ] [frontend/src/app/features/anticipation/pages/](frontend/src/app/features/anticipation/pages/) — **Criar**: simulation/
- [ ] [frontend/src/app/features/anticipation/components/](frontend/src/app/features/anticipation/components/) — **Criar**: simulation-form/, simulation-result-panel/, confirmconversion-dialog/ (opt)
- [ ] [frontend/src/app/app.routes.ts](frontend/src/app/app.routes.ts) — Alterar: add rota
- [ ] [frontend/src/app/domain/index.ts](frontend/src/app/domain/index.ts) — Alterar: export tipos
- [ ] [frontend/src/app/application/anticipation/index.ts](frontend/src/app/application/anticipation/index.ts) — Alterar: export façade
- [ ] [frontend/src/app/features/anticipation/pages/my-requests/...html](frontend/src/app/features/anticipation/pages/my-requests/) — Alterar (opt): link "Simular"
- [ ] [frontend/src/app/features/anticipation/pages/admin-list/](frontend/src/app/features/anticipation/pages/admin-list/) — Alterar (opt): ação "Simular em nome de"

---

## § 8. Próximos Passos (Fluxo de Trabalho)

**Ordem de Execução Recomendada:**

1. **Testes**: Usar `quadro-de-recompensas` skill para gerar árvore de testes (unit, integração, E2E) baseada em CAs RF-3. Arquivo: `.cursor/plans/rf-3-tests-tree.md` (output).
2. **Implementação**: Seguir `mestre-freire-angular` para implementar em ordem de precedência (A1 → A11); TDD: testes roxa → código grén.
3. **Validação**: Executar suíte de testes após cada fase; verificar cobertura (target: >80% para lógica, 100% para componentes).
4. **Integração**: Alinhar com testes E2E; garantir que fluxo fim-a-fim (simulação → conversão → RF-1) funciona.
5. **Documentação**: Atualizar [docs/tracability.md](docs/tracability.md) com rotas, componentes, testes criados; marcar RF-3 como "Implementado".

---

## § 9. Conclusão

Este plano consolida a demanda **RF-3 (Simulação de Solicitação de Antecipação)** em um **artefato de referência** pronto para:

✅ **Especificação de Testes** (usando `quadro-de-recompensas`)  
✅ **Implementação TDD** (usando `mestre-freire-angular`)  
✅ **Validação de Arquitetura** (usando `clean-architecture-analysis`)  

**Documento gerado:**Fonte de verdade (✓ Completo)  
**Estado:** Pronto para TESTES & IMPLEMENTAÇÃO  
**Última atualização:** 6 de março de 2026  

---