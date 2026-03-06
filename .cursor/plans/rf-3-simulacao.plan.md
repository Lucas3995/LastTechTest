# PLANO DE IMPLEMENTAÇÃO — RF-3: Simulação de Solicitação de Antecipação (Frontend)

**Versão:** 1.0  
**Data:** 6 de março de 2026  
**Gerado por:** Comando `criar-fonte-verdade-codigo-producao`  
**Fonte de Verdade:** `.cursor/plans/rf-3-simulacao.md` (Maestro Report)  
**Status:** Pronto para Implementação  

---

## § 1. LOCAL E ESCOPO

### 1.1 Área de Aplicação
- **Feature:** `anticipation` (Antecipação de Recebíveis)
- **Funcionalidade:** RF-3 — Simulação de Solicitação de Antecipação
- **Localização no Código:** `frontend/src/app/features/anticipation/`
- **Componentes Afetados:**
  - Domain layer: tipos de simulação
  - Infrastructure layer: HTTP service
  - Application layer: nova façade
  - Features layer: página + componentes

### 1.2 Personas Envolvidas
- **Creator**: Simula antecipação para si; pode converter em solicitação real
- **Analista**: Simula em nome de Creator; não converte
- **Admin**: Acesso irrestrito; simula + converte em nome de qualquer Creator

### 1.3 Rota Principal
```
GET  /anticipation/simulation              (Page com formulário + resultado)
POST /api/v1/anticipations/simulations     (Backend: simulação)
POST /api/v1/anticipations/simulations/{code}/confirm  (Backend: conversão)
```

---

## § 2. RESUMO DAS ALTERAÇÕES

Este plano implementa **11 alterações principais** (A1 → A11) que materializam a demanda RF-3 em código de produção.

### 2.1 Matriz de Alterações

| ID | Tipo | Módulo | Descrição | Prioridade |
|----|------|--------|-----------|-----------|
| **A1** | Criar | Domain | Tipos de simulação (`AnticipationSimulation`, `SimulationResult`, `ConversionResult`) | 🔴 M1 |
| **A2** | Alterar | Infrastructure | Métodos HTTP (`simulateAnticipation()`, `convertSimulationToReal()`) | 🔴 M1 |
| **A3** | Criar | Application | Nova façade (`AnticipationSimulationFacade`) com Signals | 🔴 M1 |
| **A4** | Criar | Features (Page) | Página container (`AnticipationSimulationPageComponent`) | 🔴 M2 |
| **A5** | Criar | Features (Component) | Formulário isolado (`AnticipationSimulationFormComponent`) | 🟡 M2 |
| **A6** | Criar | Features (Component) | Painel de resultado (`AnticipationSimulationResultPanelComponent`) | 🟡 M2 |
| **A7** | Criar | Features (Component) | Diálogo confirmação (opcional) | 🟢 M3 |
| **A8** | Alterar | Core | Rota `/anticipation/simulation` em `app.routes.ts` | 🟡 M2 |
| **A9** | Alterar | Domain/Application | Exports em `index.ts` (tipos + façade) | 🟢 M3 |
| **A10** | Alterar | Features (UX) | Link "Simular antecipação" em RF-1 (opcional) | 🟢 M3 |
| **A11** | Alterar | Features (UX) | Ação "Simular em nome de" em admin list (opcional) | 🟢 M3 |

**Legenda:** 🔴 = Critical (M1), 🟡 = Important (M2), 🟢 = Nice-to-have (M3)

---

## § 3. REFERÊNCIA À ÁRVORE DE TESTES

**Arquivo de testes:** `.cursor/plans/rf-3-testes.md` (ou árvore equivalente gerada por `quadro-de-recompensas`)

### 3.1 Escopo de Cobertura de Testes

Os testes devem cobrir os **6 cenários principais de aceitação** (CA-RF3-1 a CA-RF3-6):

1. **CA-RF3-1**: Simulação válida → resultado exibido (unitário + E2E)
2. **CA-RF3-2**: Erros de validação → mensagens localizadas (unitário)
3. **CA-RF3-3**: Sem efeitos persistentes → múltiplas simulações OK (integração)
4. **CA-RF3-4**: Conversão bem-sucedida → solicitação criada (E2E)
5. **CA-RF3-5**: Rejeição conversão (5 cenários: expirada, já usada, pendente, regras, etc.) (unitário + E2E)
6. **CA-RF3-6**: Admin/Analista simula em nome de Creator (unitário + E2E)

### 3.2 Estrutura Esperada de Testes (Referência)

```
frontend/
  src/
    app/
      domain/
        ├─ anticipation.spec.ts
        │  └─ Tipos: AnticipationSimulation, SimulationResult, ConversionResult
        │
      infrastructure/
        ├─ anticipation-requests.http.service.spec.ts
        │  ├─ simulateAnticipation() → HTTP 200 + payload mapping
        │  └─ convertSimulationToReal() → HTTP 200/422 + error handling
        │
      application/
        ├─ anticipation-simulation.facade.spec.ts
        │  ├─ simulate() → atualiza signals, trata erros
        │  ├─ convertToRealRequest() → valida, chama HTTP, navegação
        │  └─ loadContracts() (se necessário)
        │
      features/
        anticipation/
          pages/
            simulation/
              ├─ anticipation-simulation-page.component.spec.ts
              │  └─ Integração: form + result panel + facade
              │
          components/
            anticipation-simulation-form/
              ├─ anticipation-simulation-form.component.spec.ts
              │  ├─ Form validation (required, min value, etc.)
              │  └─ Submit emit + creatorId field (admin)
              │
            anticipation-simulation-result-panel/
              ├─ anticipation-simulation-result-panel.component.spec.ts
              │  ├─ Display result data
              │  ├─ Validade check + disable conversion se expirada
              │  └─ Convert button click emit
              │
      e2e/
        └─ rf-3-simulacao.e2e.spec.ts
           ├─ Creator simula + converte com sucesso
           ├─ Creator simula com erro (validação)
           ├─ Conversão bloqueada (expirada, pendente, etc.)
           ├─ Analista simula sem conversão
           └─ Admin simula + converte
```

### 3.3 Critério de Pronto
- ✅ Testes unitários podem falhar inicialmente (TDD: red → green)
- ✅ Testes E2E descrevem o fluxo fim-a-fim (navegação, UI, navegação para RF-1)
- ✅ **Nenhum teste deve ser criado/modificado DURANTE a implementação**, exceto se absolutamente necessário para não deixar código novo sem cobertura

---

## § 4. REGRAS DE IMPLEMENTAÇÃO (MERCENÁRIO SKILL)

### 4.1 Mandatos Obrigatórios

#### 🚫 PROIBIÇÕES

1. **NÃO criar ou alterar testes** durante esta etapa
   - Exceção: **SOMENTE** se novo código fica sem cobertura (será indicado)
   - Deve-se buscar testar código existente antes de criar nova lógica

2. **NÃO refatorar** código por "qualidade"
   - Sem reformat, sem reorganização de imports, sem rename de variáveis
   - Foco: funcionalidade + regras de negócio

3. **NÃO analisar** arquitetura, performance, padrões avançados
   - Aplicar padrões **já existentes** no projeto (ex.: Signals, Facades, RxJS)
   - Tradução direta de regras → código

4. **NÃO adicionar** dependências, pacotes ou configurações
   - Usar apenas o que já está disponível no projeto

#### ✅ FOCO EXCLUSIVO

1. **Tradução de Regras de Negócio (RF-3) → Código**
   - Ler cada CA (CA-RF3-1 a CA-RF3-6)
   - Implementar lógica exata para fazer teste passar

2. **Reutilização de Padrões Existentes**
   - Facades com Signals (ver `AnticipationMyRequestsFacade`)
   - HTTP service com porta interface (ver `AnticipationRequestsHttpService`)
   - Componentes standalone (ver `MyRequestsPageComponent`)
   - Tratamento de erro via `ERROR_PRESENTATION_BUILDER`

3. **Funcionalidade + Clareza**
   - Código legível
   - Comentários onde lógica de negócio é sutil
   - Nomes de variáveis/métodos alinhados com RF-3

### 4.2 Metodologia: Mercenário

A **skill mercenária** aplica-se aqui: você é um "tradutor de regras".

**Processo:**
```
1. Ler critério de aceitação (CA-RF3-X)
2. Ler teste correspondente (ou usar testes como especificação)
3. Identificar código necessário
4. Implementar em código (repousando em estruturas existentes)
5. Verificar: teste passa? Requisito atendido?
```

**Não é escopo:**
- Propor mudanças arquiteturais
- Otimizar algoritmos
- Melhorar nomes/padrões "por princípios"
- Indicar refatorações
- Adicionar logging/telemetria

---

## § 5. DETALHES TÉCNICOS POR ALTERAÇÃO

### **ALTERAÇÃO A1: Domain — Tipos de Simulação**

**Arquivo:** `frontend/src/app/domain/anticipation.ts`

**O quê implementar:**

```typescript
// 1. Interface para resultado de simulação
export interface AnticipationSimulation {
  simulationCode: string;
  validUntil: Date; // ISO 8601 → convertido para Date
  grossAmountCents: number; // valor bruto em centavos
  feesAmountCents: number; // taxa em centavos
  netAmountCents: number; // valor líquido em centavos
  anticipationDate: Date;
  expirationDate?: Date; // se aplicável
  creatorId?: string; // para rastreabilidade
}

export type SimulationResult = AnticipationSimulation;

// 2. Interface para resultado de conversão (criação de solicitação real)
export interface ConversionResult {
  id: string;
  protocol: string;
  status: AnticipationRequestStatus; // reutilizar enum existente
  netAmountCents: number;
  createdAt: Date;
}

// 3. Payload de solicitação de simulação
export interface SimulateAnticipationPayload {
  requestedAmountCents: number; // valor solicitado
  creatorId?: string; // para Admin/Analista (opcional)
  contractIds?: string[]; // se necessário (conforme conforme RA-4)
}

// 4. Type para erros de conversão (descreve os possíveis motivos)
export type SimulationConversionError = 
  | 'SIMULATION_EXPIRED'
  | 'ALREADY_USED'
  | 'PENDING_EXISTS'
  | 'RULES_VIOLATED'
  | 'UNKNOWN';

// 5. Estender porta existente com novos métodos
// (Adicionar ao `AnticipationRequestsPort` interface)
// simulateAnticipation(payload: SimulateAnticipationPayload): Observable<SimulationResult>;
// convertSimulationToReal(simulationCode: string, creatorId?: string): Observable<ConversionResult>;
```

**Orientações de implementação:**
- ✅ Copiar estrutura de tipos já presentes no arquivo
- ✅ Usar `Date` para datas (JS nativo; Angular mapeia ISO 8601 automaticamente)
- ✅ Nomes de campos em camelCase
- ✅ Comentários curtos explicando centavos (moeda brasileira)
- ✅ Deixar porta `AnticipationRequestsPort` aberta para métodos novos (próxima alteração)

---

### **ALTERAÇÃO A2: Infrastructure — HTTP Service**

**Arquivo:** `frontend/src/app/infrastructure/anticipation/anticipation-requests.http.service.ts`

**O quê implementar:**

```typescript
// Dentro da classe AnticipationRequestsHttpService

// 1. Método para simular antecipação
simulateAnticipation(payload: SimulateAnticipationPayload): Observable<SimulationResult> {
  // POST /api/v1/anticipations/simulations
  return this.httpClient.post<any>('/api/v1/anticipations/simulations', {
    requestedAmountCents: payload.requestedAmountCents,
    creatorId: payload.creatorId || undefined,
    contractIds: payload.contractIds || undefined,
  }).pipe(
    map(response => ({
      simulationCode: response.simulationCode,
      validUntil: new Date(response.validUntil), // ISO string → Date
      grossAmountCents: response.grossAmountCents,
      feesAmountCents: response.feesAmountCents,
      netAmountCents: response.netAmountCents,
      anticipationDate: new Date(response.anticipationDate),
      expirationDate: response.expirationDate ? new Date(response.expirationDate) : undefined,
      creatorId: response.creatorId,
    }))
  );
}

// 2. Método para converter simulação em solicitação real
convertSimulationToReal(simulationCode: string, creatorId?: string): Observable<ConversionResult> {
  // POST /api/v1/anticipations/simulations/{code}/confirm
  const payload = creatorId ? { creatorId } : {};
  return this.httpClient.post<any>(
    `/api/v1/anticipations/simulations/${simulationCode}/confirm`,
    payload
  ).pipe(
    map(response => ({
      id: response.id,
      protocol: response.protocol,
      status: response.status as AnticipationRequestStatus,
      netAmountCents: response.netAmountCents,
      createdAt: new Date(response.createdAt),
    }))
  );
}
```

**Orientações de implementação:**
- ✅ Reutilizar padrão existente de `httpClient.post`
- ✅ Mapear ISO strings → `Date` via `new Date(response.field)`
- ✅ Garantir que payload seja enviado como JSON (httpClient padrão)
- ✅ Sem retry logic por enquanto (RA-4 define comportamento de retry no backend)
- ✅ Deixar erro HTTP passar para camada de aplicação (Facade) para tratamento centralizado
- ✅ Adicionar estes dois métodos à **interface `AnticipationRequestsPort`** também

---

### **ALTERAÇÃO A3: Application — Façade de Simulação**

**Arquivo:** `frontend/src/app/application/anticipation/anticipation-simulation.facade.ts` (criar)

**O quê implementar:**

```typescript
import { Injectable, Signal, signal, computed } from '@angular/core';
import { firstValueFrom } from 'rxjs';
import { 
  AnticipationSimulation, 
  SimulationResult, 
  ConversionResult,
  SimulateAnticipationPayload,
  ANTICIPATION_REQUESTS_PORT,
  AnticipationRequestsPort
} from '@domain';
import { ERROR_PRESENTATION_BUILDER, ErrorPresentation } from '@core';

@Injectable()
export class AnticipationSimulationFacade {
  
  // Signals de estado
  private readonly _simulationResult = signal<AnticipationSimulation | null>(null);
  private readonly _loading = signal<boolean>(false);
  private readonly _errorMessage = signal<string | null>(null);
  private readonly _errorSupportId = signal<string | null>(null); // traceId
  private readonly _infoMessage = signal<string | null>(null);
  private readonly _creatorIdForSimulation = signal<string | null>(null);
  private readonly _isConverting = signal<boolean>(false);

  // Computed: pode converter?
  private readonly _canConvert = computed(() => {
    const result = this._simulationResult();
    if (!result) return false;
    
    // Verifica se ainda válida (validUntil > agora)
    const now = new Date();
    return result.validUntil > now;
  });

  // Público (read-only)
  readonly simulationResult: Signal<AnticipationSimulation | null> = this._simulationResult.asReadonly();
  readonly loading: Signal<boolean> = this._loading.asReadonly();
  readonly errorMessage: Signal<string | null> = this._errorMessage.asReadonly();
  readonly errorSupportId: Signal<string | null> = this._errorSupportId.asReadonly();
  readonly infoMessage: Signal<string | null> = this._infoMessage.asReadonly();
  readonly creatorIdForSimulation: Signal<string | null> = this._creatorIdForSimulation.asReadonly();
  readonly isConverting: Signal<boolean> = this._isConverting.asReadonly();
  readonly canConvert: Signal<boolean> = this._canConvert;

  constructor(
    private port: AnticipationRequestsPort,
    private errorBuilder: ErrorPresentation
  ) {}

  async simulate(payload: SimulateAnticipationPayload): Promise<void> {
    this._loading.set(true);
    this._errorMessage.set(null);
    this._errorSupportId.set(null);
    this._infoMessage.set(null);

    try {
      const result = await firstValueFrom(
        this.port.simulateAnticipation(payload)
      );
      this._simulationResult.set(result);
      this._infoMessage.set('Simulação realizada com sucesso.');
    } catch (error) {
      // Extrai mensagem de erro via ERROR_PRESENTATION_BUILDER
      const presentation = this.errorBuilder.build(error);
      this._errorMessage.set(presentation.message);
      this._errorSupportId.set(presentation.supportId); // traceId
    } finally {
      this._loading.set(false);
    }
  }

  async convertToRealRequest(): Promise<void> {
    const result = this._simulationResult();
    if (!result) {
      this._errorMessage.set('Nenhuma simulação válida para converter.');
      return;
    }

    // Valida novamente se expirou
    if (!this._canConvert()) {
      this._errorMessage.set('Simulação expirou. Realize uma nova simulação.');
      return;
    }

    this._isConverting.set(true);
    this._errorMessage.set(null);

    try {
      const conversionResult = await firstValueFrom(
        this.port.convertSimulationToReal(
          result.simulationCode,
          this._creatorIdForSimulation() || undefined
        )
      );
      
      // Sucesso: limpa simulação (foi "usada")
      this._simulationResult.set(null);
      this._infoMessage.set(
        `Solicitação criada com sucesso! Protocolo: ${conversionResult.protocol}`
      );
      // Componente deverá navegar para RF-1 após este sucesso
      
    } catch (error) {
      // Trata erros específicos de conversão
      const presentation = this.errorBuilder.build(error);
      
      // Se erro 422, tentar extrair motivo específico
      if (error?.status === 422) {
        const body = error.error;
        if (body?.code === 'SIMULATION_EXPIRED') {
          this._errorMessage.set('Simulação expirou. Realize uma nova simulação.');
        } else if (body?.code === 'ALREADY_USED') {
          this._errorMessage.set('Esta simulação já foi utilizada para criar uma solicitação.');
        } else if (body?.code === 'PENDING_EXISTS') {
          this._errorMessage.set('Você já possui uma solicitação em aberto. Conclua ou cancele-a antes.');
        } else if (body?.code === 'RULES_VIOLATED') {
          this._errorMessage.set('As condições de negócio mudaram. Realize uma nova simulação.');
        } else {
          this._errorMessage.set(presentation.message);
        }
      } else {
        this._errorMessage.set(presentation.message);
      }
      this._errorSupportId.set(presentation.supportId);
    } finally {
      this._isConverting.set(false);
    }
  }

  setCreatorIdForSimulation(creatorId: string): void {
    this._creatorIdForSimulation.set(creatorId);
  }

  reset(): void {
    this._simulationResult.set(null);
    this._loading.set(false);
    this._errorMessage.set(null);
    this._errorSupportId.set(null);
    this._infoMessage.set(null);
    this._creatorIdForSimulation.set(null);
    this._isConverting.set(false);
  }
}
```

**Orientações de implementação:**
- ✅ Copiar padrão de `AnticipationMyRequestsFacade` (signals, computed, DI)
- ✅ Usar `firstValueFrom()` para converter Observable → Promise (async/await)
- ✅ Tratamento centralizado de erro via `ERROR_PRESENTATION_BUILDER`
- ✅ Métodos async (compatível com TypeScript + templates Angular)
- ✅ Lógica de "pode converter?" em computed signal (reativa)
- ✅ Mensagens de erro com base em backend codes (422 específicos)

---

### **ALTERAÇÃO A4: Features (Page) — Página de Simulação**

**Arquivo:** `frontend/src/app/features/anticipation/pages/simulation/anticipation-simulation-page.component.ts` (criar)

**Também criar:**
- `anticipation-simulation-page.component.html`
- `anticipation-simulation-page.component.scss`

**TypeScript:**

```typescript
import { Component, OnInit, OnDestroy, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { AnticipationSimulationFormComponent } from '../../components/simulation-form/anticipation-simulation-form.component';
import { AnticipationSimulationResultPanelComponent } from '../../components/simulation-result-panel/anticipation-simulation-result-panel.component';
import { AnticipationSimulationFacade } from '@application';
import { AuthService } from '@core'; // ou seu serviço de autenticação
import { SimulateAnticipationPayload } from '@domain';

@Component({
  selector: 'app-anticipation-simulation-page',
  standalone: true,
  imports: [
    CommonModule,
    AnticipationSimulationFormComponent,
    AnticipationSimulationResultPanelComponent
  ],
  templateUrl: './anticipation-simulation-page.component.html',
  styleUrl: './anticipation-simulation-page.component.scss',
  providers: [AnticipationSimulationFacade]
})
export class AnticipationSimulationPageComponent implements OnInit, OnDestroy {
  
  private facade = inject(AnticipationSimulationFacade);
  private authService = inject(AuthService);
  private router = inject(Router);

  // Expor signals para template
  simulationResult = this.facade.simulationResult;
  loading = this.facade.loading;
  errorMessage = this.facade.errorMessage;
  infoMessage = this.facade.infoMessage;
  errorSupportId = this.facade.errorSupportId;
  canConvert = this.facade.canConvert;
  isConverting = this.facade.isConverting;
  userRole = this.authService.currentUserRole(); // ou signal similar

  ngOnInit(): void {
    // Reset façade ao entrar na página
    this.facade.reset();
    
    // Se Admin/Analista, verificar se tem creatorId na rota/query param
    // e pré-popular
    // (implementar conforme necessidade)
  }

  ngOnDestroy(): void {
    // Cleanup opcional
  }

  async onSimulate(payload: SimulateAnticipationPayload): Promise<void> {
    await this.facade.simulate(payload);
  }

  async onConvertClick(): Promise<void> {
    // Diálogo de confirmação
    const confirmed = confirm(
      'Deseja criar uma solicitação real com base nesta simulação?'
    );
    
    if (confirmed) {
      await this.facade.convertToRealRequest();
      
      // Se sucesso (infoMessage atualizada), navegue
      if (this.facade.infoMessage()) {
        // Aguarda 2s para exibir feedback, depois navega
        setTimeout(() => {
          this.router.navigate(['anticipation', 'my-requests']);
        }, 2000);
      }
    }
  }

  onCreatorSelected(creatorId: string): void {
    // Para Admin/Analista: registrar creator selecionado
    this.facade.setCreatorIdForSimulation(creatorId);
  }
}
```

**HTML:**

```html
<div class="simulation-container">
  <header class="page-header">
    <h1>Simulação de Antecipação</h1>
    <p class="info-text">Teste diferentes cenários sem criar solicitações reais.</p>
  </header>

  <!-- Info Box -->
  <div class="info-box">
    <p>💡 Esta é uma simulação. Nenhuma solicitação foi criada.</p>
  </div>

  <!-- Error Box -->
  @if (errorMessage()) {
    <div class="error-box">
      <p class="error-message">{{ errorMessage() }}</p>
      @if (errorSupportId()) {
        <p class="error-support">ID de suporte: {{ errorSupportId() }}</p>
      }
    </div>
  }

  <!-- Info Message (success) -->
  @if (infoMessage()) {
    <div class="success-box">
      <p class="success-message">{{ infoMessage() }}</p>
    </div>
  }

  <!-- Formulário -->
  <section class="form-section">
    <app-anticipation-simulation-form
      [isSubmitting]="loading()"
      [userRole]="userRole()"
      (submit)="onSimulate($event)"
      (creatorSelected)="onCreatorSelected($event)"
    ></app-anticipation-simulation-form>
  </section>

  <!-- Painel de Resultado -->
  @if (simulationResult()) {
    <section class="result-section">
      <app-anticipation-simulation-result-panel
        [simulationResult]="simulationResult()!"
        [canConvert]="canConvert()"
        [userRole]="userRole()"
        [isConverting]="isConverting()"
        (convertClick)="onConvertClick()"
      ></app-anticipation-simulation-result-panel>
    </section>
  }
</div>
```

**SCSS:**

```scss
.simulation-container {
  max-width: 900px;
  margin: 0 auto;
  padding: 2rem;

  .page-header {
    margin-bottom: 2rem;

    h1 {
      font-size: 2rem;
      margin-bottom: 0.5rem;
    }

    .info-text {
      color: var(--color-text-secondary);
      font-size: 0.95rem;
    }
  }

  .info-box,
  .error-box,
  .success-box {
    padding: 1rem;
    margin-bottom: 2rem;
    border-radius: 0.5rem;
    font-size: 0.95rem;
  }

  .info-box {
    background-color: var(--color-info-light);
    color: var(--color-info-dark);
  }

  .error-box {
    background-color: var(--color-error-light);
    color: var(--color-error-dark);

    .error-support {
      font-size: 0.85rem;
      margin-top: 0.5rem;
      opacity: 0.8;
    }
  }

  .success-box {
    background-color: var(--color-success-light);
    color: var(--color-success-dark);
  }

  .form-section,
  .result-section {
    margin-bottom: 3rem;
  }
}
```

**Orientações de implementação:**
- ✅ Standalone component (pattern existente no projeto)
- ✅ Injetar façade, auth, router
- ✅ Expostos signals directly no template (Angular 17+)
- ✅ Métodos async: `onSimulate()`, `onConvertClick()`
- ✅ Reutilizar estrutura de páginas existentes (RF-1, RF-5)
- ✅ Confirmação simples com `confirm()` ou dialog mais elaborado (A7)
- ✅ NavigateSmart após conversão bem-sucedida

---

### **ALTERAÇÃO A5: Form Component**

**Arquivo:** `frontend/src/app/features/anticipation/components/simulation-form/anticipation-simulation-form.component.ts` (criar)

**Também criar:**
- `anticipation-simulation-form.component.html`
- `anticipation-simulation-form.component.scss`

**TypeScript:**

```typescript
import { Component, Input, Output, EventEmitter, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { SimulateAnticipationPayload } from '@domain';
import { HttpClient } from '@angular/common/http'; // para carregar contratos se necessário

@Component({
  selector: 'app-anticipation-simulation-form',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './anticipation-simulation-form.component.html',
  styleUrl: './anticipation-simulation-form.component.scss'
})
export class AnticipationSimulationFormComponent implements OnInit {
  
  @Input() isSubmitting: boolean = false;
  @Input() userRole: string = 'Creator'; // 'Creator', 'Analista', 'Admin'
  @Input() errorMessage?: string;
  
  @Output() submit = new EventEmitter<SimulateAnticipationPayload>();
  @Output() creatorSelected = new EventEmitter<string>();

  private fb = inject(FormBuilder);
  private http = inject(HttpClient);

  form!: FormGroup;

  ngOnInit(): void {
    this.initializeForm();
  }

  initializeForm(): void {
    const controls: any = {
      requestedAmountCents: ['', [Validators.required, Validators.min(10000)]], // R$ 100,00 em centavos
    };

    // Se Admin ou Analista, adicionar campo de seleção de creator
    if (this.userRole !== 'Creator') {
      controls.creatorId = ['', Validators.required];
    }

    // Se necessário, adicionar mais campos (contratos, datas, etc.)
    // conforme RA-4 especificar

    this.form = this.fb.group(controls);
  }

  onSubmit(): void {
    if (this.form.invalid) {
      return; // Validação inline no template já marca erro
    }

    const payload: SimulateAnticipationPayload = {
      requestedAmountCents: this.form.get('requestedAmountCents')?.value,
      creatorId: this.form.get('creatorId')?.value || undefined,
    };

    this.submit.emit(payload);
  }

  onCreatorChange(creatorId: string): void {
    this.creatorSelected.emit(creatorId);
  }

  onReset(): void {
    this.form.reset();
  }

  getErrorMessage(fieldName: string): string {
    const control = this.form.get(fieldName);
    if (!control || !control.errors) return '';

    if (control.errors['required']) {
      return 'Este campo é obrigatório.';
    }
    if (control.errors['min']) {
      return `Valor mínimo: R$ 100,00`;
    }
    return 'Campo inválido.';
  }
}
```

**HTML:**

```html
<form [formGroup]="form" (ngSubmit)="onSubmit()" class="simulation-form">
  <fieldset>
    <legend>Dados da Simulação</legend>

    <!-- Valor Solicitado -->
    <div class="form-group">
      <label for="amountInput">
        Valor que deseja antecipar (R$)
        <span class="required">*</span>
      </label>
      <input
        id="amountInput"
        type="number"
        formControlName="requestedAmountCents"
        placeholder="Ex: 50000 (R$ 500,00)"
        class="form-input"
        [class.invalid]="form.get('requestedAmountCents')?.touched && form.get('requestedAmountCents')?.invalid"
        [disabled]="isSubmitting"
      />
      @if (form.get('requestedAmountCents')?.touched && form.get('requestedAmountCents')?.errors) {
        <span class="error-text">
          {{ getErrorMessage('requestedAmountCents') }}
        </span>
      }
      <small class="help-text">Você pode antecipar até 90% dos seus recebíveis.</small>
    </div>

    <!-- Creator (para Admin/Analista) -->
    @if (userRole !== 'Creator') {
      <div class="form-group">
        <label for="creatorInput">
          Criador
          <span class="required">*</span>
        </label>
        <input
          id="creatorInput"
          type="text"
          formControlName="creatorId"
          placeholder="Buscar criador..."
          class="form-input"
          [disabled]="isSubmitting"
          (change)="onCreatorChange($event.target.value)"
        />
        @if (form.get('creatorId')?.touched && form.get('creatorId')?.errors) {
          <span class="error-text">
            {{ getErrorMessage('creatorId') }}
          </span>
        }
      </div>
    }

    <!-- Campos adicionais conforme RA-4 (contratos, datas, etc.) -->
    <!-- [Adicionar aqui se necessário] -->
  </fieldset>

  <!-- Botões -->
  <div class="form-actions">
    <button type="submit" class="btn btn-primary" [disabled]="form.invalid || isSubmitting">
      @if (isSubmitting) {
        <span>Simulando...</span>
      } @else {
        <span>Simular</span>
      }
    </button>
    <button type="button" class="btn btn-secondary" (click)="onReset()">
      Limpar
    </button>
  </div>
</form>
```

**SCSS:**

```scss
.simulation-form {
  padding: 1.5rem;
  border: 1px solid var(--color-border);
  border-radius: 0.5rem;
  background-color: var(--color-bg-secondary);

  fieldset {
    border: none;
    padding: 0;
    margin: 0;

    legend {
      font-size: 1.1rem;
      font-weight: 600;
      margin-bottom: 1.5rem;
    }
  }

  .form-group {
    margin-bottom: 1.5rem;

    label {
      display: block;
      margin-bottom: 0.5rem;
      font-weight: 500;
      color: var(--color-text-primary);

      .required {
        color: var(--color-error);
      }
    }

    .form-input {
      width: 100%;
      padding: 0.75rem;
      border: 1px solid var(--color-border);
      border-radius: 0.25rem;
      font-size: 1rem;

      &:focus {
        outline: none;
        border-color: var(--color-primary);
        box-shadow: 0 0 0 3px rgba(var(--color-primary-rgb), 0.1);
      }

      &.invalid {
        border-color: var(--color-error);
      }

      &:disabled {
        background-color: var(--color-bg-disabled);
        color: var(--color-text-disabled);
      }
    }

    .error-text {
      display: block;
      margin-top: 0.25rem;
      color: var(--color-error);
      font-size: 0.85rem;
    }

    .help-text {
      display: block;
      margin-top: 0.5rem;
      color: var(--color-text-secondary);
      font-size: 0.85rem;
    }
  }

  .form-actions {
    display: flex;
    gap: 1rem;
    margin-top: 2rem;

    .btn {
      padding: 0.75rem 1.5rem;
      border: none;
      border-radius: 0.25rem;
      font-size: 1rem;
      font-weight: 500;
      cursor: pointer;
      transition: all 0.2s;

      &:disabled {
        opacity: 0.5;
        cursor: not-allowed;
      }

      &.btn-primary {
        background-color: var(--color-primary);
        color: white;

        &:hover:not(:disabled) {
          background-color: var(--color-primary-dark);
        }
      }

      &.btn-secondary {
        background-color: var(--color-border);
        color: var(--color-text-primary);

        &:hover:not(:disabled) {
          background-color: var(--color-bg-disabled);
        }
      }
    }
  }
}
```

**Orientações de implementação:**
- ✅ FormGroup com validadores simples (required, min)
- ✅ Validação inline em template (ngIf errors)
- ✅ Campos adicionais (contratos, datas) podem ser adicionados posteriormente conforme RA-4
- ✅ Emitir payload via `@Output()` na submissão
- ✅ Criador selecionável para Admin/Analista
- ✅ Botão de reset limpa o formulário

---

### **ALTERAÇÃO A6: Result Panel Component**

**Arquivo:** `frontend/src/app/features/anticipation/components/simulation-result-panel/anticipation-simulation-result-panel.component.ts` (criar)

**Também criar:**
- `anticipation-simulation-result-panel.component.html`
- `anticipation-simulation-result-panel.component.scss`

**TypeScript:**

```typescript
import { Component, Input, Output, EventEmitter, OnInit, effect } from '@angular/core';
import { CommonModule } from '@angular/common';
import { AnticipationSimulation } from '@domain';

@Component({
  selector: 'app-anticipation-simulation-result-panel',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './anticipation-simulation-result-panel.component.html',
  styleUrl: './anticipation-simulation-result-panel.component.scss'
})
export class AnticipationSimulationResultPanelComponent implements OnInit {
  
  @Input() simulationResult!: AnticipationSimulation;
  @Input() canConvert: boolean = false;
  @Input() userRole: string = 'Creator';
  @Input() isConverting: boolean = false;

  @Output() convertClick = new EventEmitter<void>();

  isExpired: boolean = false;
  validUntilDisplay: string = '';
  minUntilExpiration: number = 0;

  ngOnInit(): void {
    this.updateExpiration();
  }

  updateExpiration(): void {
    const now = new Date();
    this.isExpired = this.simulationResult.validUntil <= now;
    
    // Calcula tempo até expiração
    const diffMs = this.simulationResult.validUntil.getTime() - now.getTime();
    this.minUntilExpiration = Math.max(0, Math.floor(diffMs / 60000));
    
    // Formato: "Válida até 14:30" ou "Expirada"
    if (this.isExpired) {
      this.validUntilDisplay = 'Expirada';
    } else {
      const hours = this.simulationResult.validUntil.getHours().toString().padStart(2, '0');
      const mins = this.simulationResult.validUntil.getMinutes().toString().padStart(2, '0');
      this.validUntilDisplay = `Válida até ${hours}:${mins}`;
    }
  }

  onConvertClick(): void {
    if (this.canConvert && !this.isConverting) {
      this.convertClick.emit();
    }
  }

  // Conversão de centavos para reais formatado
  formatCurrency(cents: number): string {
    const reais = cents / 100;
    return reais.toLocaleString('pt-BR', {
      style: 'currency',
      currency: 'BRL'
    });
  }

  // Porcentagem de taxa
  getFeePercentage(): string {
    if (this.simulationResult.grossAmountCents === 0) return '0%';
    const pct = (this.simulationResult.feesAmountCents / this.simulationResult.grossAmountCents) * 100;
    return pct.toFixed(2) + '%';
  }

  // Mostrar botão de conversão?
  shouldShowConvertButton(): boolean {
    return (this.userRole === 'Creator' || this.userRole === 'Admin') && 
           !this.isExpired &&
           this.canConvert;
  }
}
```

**HTML:**

```html
<div class="result-panel">
  <header class="result-header">
    <h2>Resultado da Simulação</h2>
  </header>

  <div class="result-grid">
    <!-- Coluna 1: Valores -->
    <section class="column values-column">
      <h3>Valores</h3>
      <div class="value-item">
        <span class="label">Valor Solicitado</span>
        <span class="value">{{ formatCurrency(simulationResult.grossAmountCents) }}</span>
      </div>
      <div class="value-item">
        <span class="label">Taxa</span>
        <span class="value value-emphasis">{{ getFeePercentage() }}</span>
      </div>
      <div class="value-item value-highlight">
        <span class="label">Valor Líquido</span>
        <span class="value value-large">{{ formatCurrency(simulationResult.netAmountCents) }}</span>
      </div>
    </section>

    <!-- Coluna 2: Datas -->
    <section class="column dates-column">
      <h3>Datas</h3>
      <div class="value-item">
        <span class="label">Data de Antecipação</span>
        <span class="value">
          {{ simulationResult.anticipationDate | date:'dd/MM/yyyy' }}
        </span>
      </div>
      @if (simulationResult.expirationDate) {
        <div class="value-item">
          <span class="label">Data de Vencimento</span>
          <span class="value">
            {{ simulationResult.expirationDate | date:'dd/MM/yyyy' }}
          </span>
        </div>
      }
    </section>

    <!-- Coluna 3: Status -->
    <section class="column status-column">
      <h3>Status</h3>
      <div [class.validity-badge]="true" [class.valid]="!isExpired" [class.expired]="isExpired">
        <span class="badge-text">{{ validUntilDisplay }}</span>
        @if (!isExpired && minUntilExpiration < 10) {
          <span class="badge-warning">⚠️ Vencendo em breve</span>
        }
      </div>
    </section>
  </div>

  <!-- Info de Simulação -->
  <div class="info-box">
    <p>ℹ️ Esta é uma simulação. Nenhuma solicitação foi criada ainda.</p>
  </div>

  <!-- Botão de Conversão -->
  @if (shouldShowConvertButton()) {
    <div class="conversion-actions">
      <button 
        class="btn btn-primary" 
        (click)="onConvertClick()"
        [disabled]="isConverting"
      >
        @if (isConverting) {
          <span>Criando solicitação...</span>
        } @else {
          <span>Criar solicitação real com base nesta simulação</span>
        }
      </button>
    </div>
  }

  <!-- Detalhes técnicos (colapser opcional) -->
  <details class="technical-details">
    <summary>Detalhes técnicos</summary>
    <div class="details-content">
      <p><strong>Código de simulação:</strong> {{ simulationResult.simulationCode }}</p>
      <p><strong>Criador:</strong> {{ simulationResult.creatorId || 'Você' }}</p>
    </div>
  </details>
</div>
```

**SCSS:**

```scss
.result-panel {
  padding: 2rem;
  border: 1px solid var(--color-border-success);
  border-radius: 0.5rem;
  background-color: var(--color-bg-secondary);

  .result-header {
    margin-bottom: 2rem;

    h2 {
      margin: 0;
      font-size: 1.5rem;
      color: var(--color-text-primary);
    }
  }

  .result-grid {
    display: grid;
    grid-template-columns: repeat(auto-fit, minmax(250px, 1fr));
    gap: 2rem;
    margin-bottom: 2rem;

    .column {
      h3 {
        font-size: 0.95rem;
        font-weight: 600;
        margin-bottom: 1rem;
        color: var(--color-text-secondary);
        text-transform: uppercase;
        letter-spacing: 0.05em;
      }

      .value-item {
        display: flex;
        flex-direction: column;
        margin-bottom: 1.5rem;

        .label {
          font-size: 0.85rem;
          color: var(--color-text-secondary);
          margin-bottom: 0.25rem;
        }

        .value {
          font-size: 1.25rem;
          font-weight: 600;
          color: var(--color-text-primary);
        }

        .value-emphasis {
          color: var(--color-primary);
        }

        .value-large {
          font-size: 1.75rem;
          color: var(--color-success);
        }
      }

      &.values-column .value-highlight {
        padding: 1rem;
        background-color: rgba(var(--color-success-rgb), 0.05);
        border-radius: 0.25rem;
      }
    }
  }

  .validity-badge {
    display: inline-block;
    padding: 0.75rem 1rem;
    border-radius: 0.25rem;
    font-weight: 600;
    font-size: 0.95rem;

    &.valid {
      background-color: rgba(var(--color-success-rgb), 0.1);
      color: var(--color-success);
      border: 1px solid var(--color-success);
    }

    &.expired {
      background-color: rgba(var(--color-error-rgb), 0.1);
      color: var(--color-error);
      border: 1px solid var(--color-error);
    }

    .badge-warning {
      display: block;
      font-size: 0.8rem;
      margin-top: 0.25rem;
    }
  }

  .info-box {
    padding: 1rem;
    margin: 2rem 0;
    background-color: var(--color-info-light);
    color: var(--color-info-dark);
    border-radius: 0.25rem;
    font-size: 0.95rem;
  }

  .conversion-actions {
    display: flex;
    gap: 1rem;
    margin: 2rem 0;

    .btn {
      padding: 0.75rem 1.5rem;
      font-size: 1rem;
      font-weight: 600;
      border: none;
      border-radius: 0.25rem;
      cursor: pointer;
      transition: all 0.2s;

      &.btn-primary {
        background-color: var(--color-success);
        color: white;

        &:hover:not(:disabled) {
          background-color: var(--color-success-dark);
        }

        &:disabled {
          opacity: 0.6;
          cursor: not-allowed;
        }
      }
    }
  }

  .technical-details {
    margin-top: 2rem;
    padding-top: 2rem;
    border-top: 1px solid var(--color-border);

    summary {
      cursor: pointer;
      font-size: 0.9rem;
      color: var(--color-text-secondary);
      user-select: none;

      &:hover {
        color: var(--color-text-primary);
      }
    }

    .details-content {
      padding: 1rem 0;
      font-size: 0.85rem;
      color: var(--color-text-secondary);

      p {
        margin: 0.5rem 0;

        strong {
          color: var(--color-text-primary);
        }
      }
    }
  }
}
```

**Orientações de implementação:**
- ✅ Exibir resultado em grid responsivo (3 colunas em desktop, 1 em mobile)
- ✅ Formatação de moeda (BRL)
- ✅ Cálculo de porcentagem de taxa
- ✅ Botão de conversão desabilitado se expirado ou já convertido
- ✅ Badge de status "Válida até HH:mm" com alerta se < 10 min
- ✅ Details colapser opcional com código de simulação + creatorId
- ✅ Reutilizar vars CSS do projeto (cores, espaçamento)

---

### **ALTERAÇÃO A7: Diálogo de Confirmação (Opcional)**

Se desejar um diálogo mais sofisticado que `confirm()` nativo:

**Arquivo:** `frontend/src/app/features/anticipation/components/confirm-conversion-dialog/confirm-conversion-dialog.component.ts`

Ou usar **Angular Material Dialog** se já disponível no projeto:

```typescript
import { MatDialog } from '@angular/material/dialog';

// Na página:
constructor(private dialog: MatDialog) {}

async onConvertClick(): Promise<void> {
  const dialogRef = this.dialog.open(ConfirmConversionDialogComponent, {
    data: { simulationResult: this.simulationResult() }
  });

  const result = await dialogRef.afterClosed().toPromise();
  if (result) {
    await this.facade.convertToRealRequest();
  }
}
```

**Alternativa simples (sem dependência):**

```typescript
// Usar confirm() nativo (como código-base em A4)
const confirmed = confirm('Deseja realmente criar esta solicitação?');
if (confirmed) {
  await this.facade.convertToRealRequest();
}
```

---

### **ALTERAÇÃO A8: Rota em app.routes.ts**

**Arquivo:** `frontend/src/app/app.routes.ts`

**O quê implementar:**

```typescript
// Dentro do array de rotas, adicionar:

{
  path: 'anticipation/simulation',
  canActivate: [AuthGuard],
  data: { requiredRoles: ['Creator', 'Admin', 'Analista'] },
  loadComponent: () => import(
    './features/anticipation/pages/simulation/anticipation-simulation-page.component'
  ).then(m => m.AnticipationSimulationPageComponent)
}

// Ou, se rotas aninhadas:
{
  path: 'anticipation',
  canActivate: [AuthGuard],
  children: [
    { path: 'my-requests', ... },
    { path: 'my-requests/new', ... },
    { path: 'list', ... },
    {
      path: 'simulation',
      data: { requiredRoles: ['Creator', 'Admin', 'Analista'] },
      loadComponent: () => import('...').then(m => m.AnticipationSimulationPageComponent)
    }
  ]
}
```

**Orientações de implementação:**
- ✅ Reutilizar padrão de `canActivate: [AuthGuard]` existente
- ✅ `data.requiredRoles` para controle de acesso
- ✅ `loadComponent()` para lazy loading
- ✅ Alinhar com padrão de rotas do projeto (aninhadas ou flat)

---

### **ALTERAÇÃO A9: Exports em Index Modules**

**Arquivo 1:** `frontend/src/app/domain/index.ts`

Adicionar:
```typescript
export { AnticipationSimulation, SimulationResult, ConversionResult, SimulateAnticipationPayload, SimulationConversionError } from './anticipation';
```

**Arquivo 2:** `frontend/src/app/application/anticipation/index.ts`

Adicionar:
```typescript
export { AnticipationSimulationFacade } from './anticipation-simulation.facade';
```

---

### **ALTERAÇÃO A10: Link "Simular" em RF-1 (Opcional, UX)**

**Arquivo:** `frontend/src/app/features/anticipation/pages/my-requests/anticipation-my-requests-page.component.html`

Adicionar botão/link:

```html
<button (click)="router.navigate(['anticipation', 'simulation'])" class="btn btn-secondary">
  Simular Antecipação
</button>
```

---

### **ALTERAÇÃO A11: Ação "Simular em Nome de" em Admin List (Opcional, UX)**

**Arquivo:** `frontend/src/app/features/anticipation/pages/admin-list/...component.html`

Adicionar ação (ex.: em menu de linha da tabela):

```html
<button (click)="simulateOnBehalf(creator.id)">
  Simular em nome de...
</button>
```

Implementar em TypeScript:

```typescript
simulateOnBehalf(creatorId: string): void {
  this.router.navigate(
    ['anticipation', 'simulation'],
    { queryParams: { creatorId } }
  );
}
```

Na página de simulação (A4), ler `queryParams`:

```typescript
ngOnInit(): void {
  const creatorId = this.activatedRoute.snapshot.queryParams['creatorId'];
  if (creatorId) {
    this.facade.setCreatorIdForSimulation(creatorId);
  }
}
```

---

## § 6. CHECKLIST DE IMPLEMENTAÇÃO

### Fase 1: Domain + Infrastructure (M1)

- [ ] **A1** — Tipos adicionados a `domain/anticipation.ts`
- [ ] **A2** — Métodos HTTP adicionados a `infrastructure/anticipation-requests.http.service.ts`
- [ ] Porta `AnticipationRequestsPort` estendida com `simulateAnticipation()` e `convertSimulationToReal()`

### Fase 2: Application (M1)

- [ ] **A3** — Façade completa (`anticipation-simulation.facade.ts`): Signals, métodos, tratamento de erro

### Fase 3: Features — Estrutura (M2)

- [ ] **A4** — Página container (`anticipation-simulation-page.component.*`)
- [ ] **A5** — Form component (`anticipation-simulation-form.component.*`)
- [ ] **A6** — Result panel component (`anticipation-simulation-result-panel.component.*`)

### Fase 4: Integração (M2-M3)

- [ ] **A8** — Rota adicionada a `app.routes.ts`
- [ ] **A9** — Exports adicionados a `domain/index.ts` e `application/index.ts`
- [ ] **A7** — Diálogo (opcional, se usar Material Dialog)

### Fase 5: UX Enhancements (M3)

- [ ] **A10** — Link "Simular" adicionado a RF-1 (opcional)
- [ ] **A11** — Ação "Simular em nome de" em Admin List (opcional)

### Validação

- [ ] Rotas funcionam (navegação de/para `/anticipation/simulation`)
- [ ] Formulário submete payload correto
- [ ] HTTP calls com sucesso (mock/backend)
- [ ] Resultado exibido corretamente
- [ ] Conversão possível quando válido
- [ ] Mensagens de erro claras
- [ ] AuthGuard + roles funcionam

---

## § 7. TESTES & COBERTURA

### Parceiros de Teste

**Arquivo:** `.cursor/plans/rf-3-testes.md` (referência; não alterar durante implementação)

Os testes já estão escritos. Durante esta fase:

1. **Executar testes**: `npm run test` ou `ng test`
2. **Testes devem guiar implementação** (TDD: red → green)
3. **NÃO alterar testes** a menos que absolutamente necessário
4. **Cobertura esperada**: >80% de lógica, 100% de componentes

### Validação de Integração

- ✅ Teste E2E: Creator acessa `/anticipation/simulation`
- ✅ Teste E2E: Preenche formulário, clica "Simular"
- ✅ Teste E2E: Resultado exibido
- ✅ Teste E2E: Clica "Criar solicitação real", confirmação, redirecionado para RF-1
- ✅ Teste E2E: Conversão falha (expirada, pendente, etc.), mensagem clara

---

## § 8. REGRAS FINAIS (OBRIGATÓRIAS)

### Princípios de Mercenário

1. ✅ **Código mínimo necessário**: Sem extras, sem "nice-to-haves"
2. ✅ **Reutilizar padrões existentes**: Signals, Facades, HttpClient, i18n
3. ✅ **Testes guiam implementação**: Não criar; deixar falhar primeiro
4. ✅ **Funcionalidade > Qualidade**: No escopo, qualidade virá depois (outro command)
5. ✅ **Comentários apenas onde necessário**: Regras de negócio sutil; não por óbvio

### Proibições Estritas

- 🚫 Criar/alterar testes (exceto se deixar código sem cobertura)
- 🚫 Refatorar código existente
- 🚫 Mudar nomes de variáveis por estilo
- 🚫 Adicionar dependências
- 🚫 Implementar "best practices" não solicitadas

### Obrigações

- ✅ Traduzir cada CA (CA-RF3-1 a CA-RF3-6) em código
- ✅ Reutilizar estruturas do projeto (nomes, patterns, cores CSS)
- ✅ Implementar em ordem de precedência (A1 → A11)
- ✅ Validar contra árvore de testes (não alterar, apenas usar como spec)
- ✅ Documentar ambiguidades se encontrar (em comentário no código)

---

## § 9. PRÓXIMOS PASSOS

1. **Implementação**: Começar com **A1** (tipos domain); fim com **A11** (UX)
2. **Testes**: Executar suite para cada fase; garantir cobertura
3. **Integração**: Testar fluxo fim-a-fim (formulário → resultado → conversão → RF-1)
4. **Validação**: Alinhar com testes E2E; marcar RF-3 como "Implementado"
5. **Qualidade**: Usar command `refactor-angular` para melhorias (fase posterior)

---

## § 10. RASTREABILIDADE

| Documento Origem | Seção | Status |
|------------------|-------|--------|
| `demandas/RF-3-simulacao-solicitacao-antecipacao.md` | User stories, CAs | Spec principal ✅ |
| `.cursor/plans/rf-3-simulacao.md` | § 3 (Maestro Report) | Fonte de rferência ✅ |
| `.cursor/plans/rf-3-testes.md` | Testes unitários + E2E | Cobertura (não alterar) ✅ |
| `docs/tracability.md` | Rastreabilidade | Atualizar pós-implementação |

---

**Documento**: RF-3 Simulação — Plano de Implementação  
**Status**: ✅ Pronto para Implementação (Mercenário)  
**Última atualização**: 6 de março de 2026

