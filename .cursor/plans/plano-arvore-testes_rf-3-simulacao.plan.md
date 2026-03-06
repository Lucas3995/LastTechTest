# PLANO / FONTE DE VERDADE — Árvore de Testes RF-3: Simulação de Solicitação de Antecipação (Frontend)

**Versão:** 1.0
**Data:** 6 de março de 2026
**Autor:** GitHub Copilot (agente especializado)
**Status:** Pronto para Implementação de Testes
**Plano Global Referenciado:** `.cursor/plans/rf-3-simulacao.md`

---

## § 1. RESUMO EXECUTIVO

### Objetivo
Produzir uma **árvore de testes completa** para a implementação da funcionalidade RF-3 (Simulação de Antecipação), seguindo a arquitetura Angular do projeto e os padrões estabelecidos nas skills `quadro-de-recompensas` e `mestre-freire-angular`.

### Escopo de Testes
- **Unitários**: Componentes isolados, facades, services HTTP, domain types
- **Integração**: Interação componente-facade, facade-service, service-HTTP
- **E2E**: Fluxos completos de usuário (Playwright)
- **Cobertura Target**: >80% para lógica de negócio, 100% para componentes novos

### Estrutura de Testes Planejada
- **11 arquivos .spec.ts** (unit/integration)
- **1 suíte E2E** (6 cenários principais)
- **Mapeamento direto** dos 6 CA-RF3 para casos de teste
- **Padrão**: `describe/it` com nomes descritivos, mocks adequados

---

## § 2. ESCOPO DO FRONTEND (Mapeamento Arquitetural)

### 2.1 Estrutura de Camadas (Angular Frontend)

| Camada | Componentes | Padrão Arquitetural |
|--------|-------------|-------------------|
| **Domain** | `AnticipationSimulation`, `SimulationResult`, `ConversionResult`, `SimulationError` | Tipos/interfaces TypeScript strong-typed |
| **Infrastructure** | `AnticipationRequestsHttpService` (métodos `simulateAnticipation`, `convertSimulationToReal`) | HTTP service implementando porta |
| **Application** | `AnticipationSimulationFacade` (Signals, estado, orquestração) | Façade com injeção de dependências |
| **Presentation** | Páginas + Componentes (Form, ResultPanel, Dialog) | Standalone components, reactive forms |

### 2.2 Arquivos de Teste Planejados

#### Unitários (Isolados)
- `domain/anticipation.spec.ts` — Tipos e validações
- `infrastructure/anticipation-requests.http.service.spec.ts` — Métodos HTTP
- `application/anticipation/anticipation-simulation.facade.spec.ts` — Lógica de negócio
- `features/anticipation/components/anticipation-simulation-form/anticipation-simulation-form.component.spec.ts` — Formulário
- `features/anticipation/components/anticipation-simulation-result-panel/anticipation-simulation-result-panel.component.spec.ts` — Painel resultado
- `features/anticipation/components/confirm-conversion-dialog/confirm-conversion-dialog.component.spec.ts` — Diálogo (opcional)
- `features/anticipation/pages/simulation/anticipation-simulation-page.component.spec.ts` — Página container

#### Integração
- `application/anticipation/anticipation-simulation.facade.integration.spec.ts` — Facade + HTTP service
- `features/anticipation/pages/simulation/anticipation-simulation-page.integration.spec.ts` — Página + Facade

#### E2E
- `e2e/anticipation-simulation.e2e-spec.ts` — Fluxos completos (Playwright)

---

## § 3. MAPEAMENTO — Plano Global → Casos de Teste

### 3.1 Mapeamento por CA-RF3

| CA-RF3 | Requisito | Testes Unitários | Testes Integração | Testes E2E |
|--------|-----------|------------------|-------------------|------------|
| **CA-RF3-1** | Simulação válida com resultado | ✅ Form validation, HTTP success, Facade state update, ResultPanel display | ✅ Form → Facade → HTTP → ResultPanel | ✅ Creator preenche form, submete, vê resultado correto |
| **CA-RF3-2** | Mensagens erro localizadas | ✅ Form validators, HTTP error mapping, Facade error handling | ✅ Form errors → Facade → Error display | ✅ Creator submete inválido, vê mensagens campo-a-campo |
| **CA-RF3-3** | Sem efeitos persistentes | ✅ HTTP mock (não persiste), Facade não armazena além Signals | ✅ Múltiplas simulações não interferem | ✅ Múltiplas simulações, RF-1 sem solicitações novas |
| **CA-RF3-4** | Conversão bem-sucedida | ✅ HTTP convert success, Facade state clear, ResultPanel hide convert button | ✅ Facade convert → HTTP → Success feedback | ✅ Creator simula, converte, vê em RF-1, botão desabilita |
| **CA-RF3-5** | Rejeição conversão (5 cenários) | ✅ HTTP error codes (422), Facade error mapping (expirada/pendente/regras) | ✅ Convert fail → Error display específico | ✅ Tentativas conversão inválida, mensagens corretas |
| **CA-RF3-6** | Admin/Analista simula em nome | ✅ Form creatorId field, HTTP payload include creatorId, Facade creatorIdForSimulation | ✅ Admin form → Facade → HTTP with creatorId | ✅ Analista seleciona creator, simula, vê resultado sem botão conversão |

### 3.2 Mapeamento por Alteração (A1-A11)

| Alteração | Tipo Teste | Arquivo .spec.ts | Cenários Principais |
|-----------|------------|------------------|-------------------|
| **A1** (Domain types) | Unit | `domain/anticipation.spec.ts` | Type guards, interface compliance, enum validation |
| **A2** (HTTP service) | Unit + Int | `infrastructure/anticipation-requests.http.service.spec.ts` | HTTP calls, payload mapping, error handling |
| **A3** (Facade) | Unit + Int | `application/anticipation/anticipation-simulation.facade.spec.ts` | Signals updates, async methods, error states |
| **A4** (Página) | Unit + Int | `features/anticipation/pages/simulation/anticipation-simulation-page.component.spec.ts` | Component composition, event handling |
| **A5** (Form component) | Unit | `features/anticipation/components/anticipation-simulation-form/anticipation-simulation-form.component.spec.ts` | Form validation, emit events |
| **A6** (Result panel) | Unit | `features/anticipation/components/anticipation-simulation-result-panel/anticipation-simulation-result-panel.component.spec.ts` | Input display, conditional buttons |
| **A7** (Dialog) | Unit | `features/anticipation/components/confirm-conversion-dialog/confirm-conversion-dialog.component.spec.ts` | Dialog interaction, confirm/cancel |
| **A8** (Routes) | E2E | `e2e/anticipation-simulation.e2e-spec.ts` | Route navigation, auth guards |
| **A9** (Exports) | - | - | Cobertos por imports nos testes acima |
| **A10** (Link RF-1) | E2E | `e2e/anticipation-simulation.e2e-spec.ts` | Navigation from RF-1 to simulation |
| **A11** (Link admin) | E2E | `e2e/anticipation-simulation.e2e-spec.ts` | Admin simulate on behalf of creator |

---

## § 4. ESTRUTURA DA ÁRVORE DE TESTES

### 4.1 Testes Unitários (Isolados)

#### `domain/anticipation.spec.ts`
```typescript
describe('Domain Types - Anticipation Simulation', () => {
  describe('AnticipationSimulation interface', () => {
    it('should validate required fields', () => { /* ... */ });
    it('should accept optional fields', () => { /* ... */ });
  });
  
  describe('SimulationResult type', () => {
    it('should extend AnticipationSimulation', () => { /* ... */ });
    it('should include simulationCode and validUntil', () => { /* ... */ });
  });
  
  describe('ConversionResult type', () => {
    it('should include request id and status', () => { /* ... */ });
  });
  
  describe('SimulationError enum', () => {
    it('should define error codes', () => { /* ... */ });
  });
});
```

#### `infrastructure/anticipation-requests.http.service.spec.ts`
```typescript
describe('AnticipationRequestsHttpService', () => {
  describe('simulateAnticipation()', () => {
    it('should POST to correct endpoint', () => { /* ... */ });
    it('should map response to SimulationResult', () => { /* ... */ });
    it('should handle validation errors (400)', () => { /* ... */ });
    it('should handle server errors (500)', () => { /* ... */ });
  });
  
  describe('convertSimulationToReal()', () => {
    it('should POST to confirm endpoint', () => { /* ... */ });
    it('should include simulationCode in URL', () => { /* ... */ });
    it('should handle conversion success', () => { /* ... */ });
    it('should handle expired simulation (422)', () => { /* ... */ });
    it('should handle pending request exists (422)', () => { /* ... */ });
    it('should handle rules violation (422)', () => { /* ... */ });
    it('should handle already used (422)', () => { /* ... */ });
  });
});
```

#### `application/anticipation/anticipation-simulation.facade.spec.ts`
```typescript
describe('AnticipationSimulationFacade', () => {
  describe('Signals state', () => {
    it('should initialize with null simulationResult', () => { /* ... */ });
    it('should initialize with false loading', () => { /* ... */ });
    it('should initialize with null errorMessage', () => { /* ... */ });
  });
  
  describe('simulate()', () => {
    it('should call httpService.simulateAnticipation', () => { /* ... */ });
    it('should update simulationResult on success', () => { /* ... */ });
    it('should set loading during request', () => { /* ... */ });
    it('should handle errors via ERROR_PRESENTATION_BUILDER', () => { /* ... */ });
    it('should set creatorId in payload for admin', () => { /* ... */ });
  });
  
  describe('convertToRealRequest()', () => {
    it('should call httpService.convertSimulationToReal', () => { /* ... */ });
    it('should clear simulationResult on success', () => { /* ... */ });
    it('should navigate to my-requests on success', () => { /* ... */ });
    it('should handle specific error messages', () => { /* ... */ });
  });
  
  describe('canConvert signal', () => {
    it('should be true for Creator with valid simulation', () => { /* ... */ });
    it('should be true for Admin with valid simulation', () => { /* ... */ });
    it('should be false for Analista', () => { /* ... */ });
    it('should be false when simulation expired', () => { /* ... */ });
  });
});
```

#### `features/anticipation/components/anticipation-simulation-form/anticipation-simulation-form.component.spec.ts`
```typescript
describe('AnticipationSimulationFormComponent', () => {
  describe('Form validation', () => {
    it('should require requestedAmount', () => { /* ... */ });
    it('should validate minimum amount (100)', () => { /* ... */ });
    it('should show creatorId field for Admin/Analista', () => { /* ... */ });
  });
  
  describe('Submit behavior', () => {
    it('should emit submit event with payload', () => { /* ... */ });
    it('should disable submit when invalid', () => { /* ... */ });
    it('should disable submit when isSubmitting', () => { /* ... */ });
  });
  
  describe('Creator selection (Admin/Analista)', () => {
    it('should emit creatorSelected on change', () => { /* ... */ });
    it('should pre-populate creatorId if provided', () => { /* ... */ });
  });
});
```

#### `features/anticipation/components/anticipation-simulation-result-panel/anticipation-simulation-result-panel.component.spec.ts`
```typescript
describe('AnticipationSimulationResultPanelComponent', () => {
  describe('Result display', () => {
    it('should display grossAmount, fees, netAmount', () => { /* ... */ });
    it('should show validUntil badge', () => { /* ... */ });
    it('should highlight validUntil if expiring soon', () => { /* ... */ });
  });
  
  describe('Convert button', () => {
    it('should show for Creator with valid simulation', () => { /* ... */ });
    it('should show for Admin with valid simulation', () => { /* ... */ });
    it('should hide for Analista', () => { /* ... */ });
    it('should disable when simulation expired', () => { /* ... */ });
    it('should emit convertClick on button press', () => { /* ... */ });
  });
});
```

#### `features/anticipation/pages/simulation/anticipation-simulation-page.component.spec.ts`
```typescript
describe('AnticipationSimulationPageComponent', () => {
  describe('Component composition', () => {
    it('should render form component', () => { /* ... */ });
    it('should render result panel when simulationResult exists', () => { /* ... */ });
    it('should show convert section for Creator/Admin', () => { /* ... */ });
  });
  
  describe('Event handling', () => {
    it('should call facade.simulate on form submit', () => { /* ... */ });
    it('should call facade.convertToRealRequest on convert click', () => { /* ... */ });
    it('should reset facade on init', () => { /* ... */ });
  });
  
  describe('Error display', () => {
    it('should show error message from facade', () => { /* ... */ });
    it('should show error support id', () => { /* ... */ });
  });
});
```

### 4.2 Testes de Integração

#### `application/anticipation/anticipation-simulation.facade.integration.spec.ts`
```typescript
describe('AnticipationSimulationFacade Integration', () => {
  describe('Full simulation flow', () => {
    it('should handle successful simulation end-to-end', () => { /* ... */ });
    it('should handle simulation error propagation', () => { /* ... */ });
    it('should handle conversion success', () => { /* ... */ });
    it('should handle conversion rejection scenarios', () => { /* ... */ });
  });
});
```

#### `features/anticipation/pages/simulation/anticipation-simulation-page.integration.spec.ts`
```typescript
describe('AnticipationSimulationPage Integration', () => {
  describe('Form to result flow', () => {
    it('should show result panel after successful simulation', () => { /* ... */ });
    it('should enable convert button for Creator', () => { /* ... */ });
    it('should handle form validation errors', () => { /* ... */ });
  });
});
```

### 4.3 Testes E2E (Playwright)

#### `e2e/anticipation-simulation.e2e-spec.ts`
```typescript
describe('Anticipation Simulation E2E', () => {
  describe('CA-RF3-1: Valid simulation', () => {
    it('Creator should simulate and see result', async () => { /* ... */ });
  });
  
  describe('CA-RF3-2: Validation errors', () => {
    it('should show field-specific error messages', async () => { /* ... */ });
  });
  
  describe('CA-RF3-3: No persistent effects', () => {
    it('multiple simulations should not create requests', async () => { /* ... */ });
  });
  
  describe('CA-RF3-4: Successful conversion', () => {
    it('Creator should convert simulation to real request', async () => { /* ... */ });
  });
  
  describe('CA-RF3-5: Conversion rejection', () => {
    it('should handle expired simulation', async () => { /* ... */ });
    it('should handle existing pending request', async () => { /* ... */ });
    it('should handle rules violation', async () => { /* ... */ });
    it('should handle already used simulation', async () => { /* ... */ });
  });
  
  describe('CA-RF3-6: Admin/Analyst simulation', () => {
    it('Analyst should simulate on behalf of creator', async () => { /* ... */ });
    it('Admin should simulate and convert', async () => { /* ... */ });
  });
  
  describe('Navigation integration', () => {
    it('should navigate from RF-1 to simulation', async () => { /* ... */ });
    it('should navigate from admin list to simulation', async () => { /* ... */ });
  });
});
```

---

## § 5. RELATÓRIO DE TAREFAS (Quadro de Recompensas)

### 5.1 Tarefas de Criação de Testes

| ID | Onde | Tipo | Descrição | Requisito Atendido |
|----|------|------|-----------|-------------------|
| **T1** | `frontend/src/app/domain/anticipation.spec.ts` | Criar | Testes unitários para tipos domain (interfaces, enums, validações) | A1 (Domain types) |
| **T2** | `frontend/src/app/infrastructure/anticipation-requests.http.service.spec.ts` | Criar | Testes unitários para métodos HTTP simulateAnticipation e convertSimulationToReal | A2 (HTTP service) |
| **T3** | `frontend/src/app/application/anticipation/anticipation-simulation.facade.spec.ts` | Criar | Testes unitários para facade (Signals, métodos async, estado) | A3 (Facade) |
| **T4** | `frontend/src/app/features/anticipation/components/anticipation-simulation-form/anticipation-simulation-form.component.spec.ts` | Criar | Testes unitários para componente formulário (validação, eventos) | A5 (Form component) |
| **T5** | `frontend/src/app/features/anticipation/components/anticipation-simulation-result-panel/anticipation-simulation-result-panel.component.spec.ts` | Criar | Testes unitários para painel resultado (display, botões condicionais) | A6 (Result panel) |
| **T6** | `frontend/src/app/features/anticipation/components/confirm-conversion-dialog/confirm-conversion-dialog.component.spec.ts` | Criar | Testes unitários para diálogo confirmação (opcional) | A7 (Dialog) |
| **T7** | `frontend/src/app/features/anticipation/pages/simulation/anticipation-simulation-page.component.spec.ts` | Criar | Testes unitários para página container (composição, eventos) | A4 (Página) |
| **T8** | `frontend/src/app/application/anticipation/anticipation-simulation.facade.integration.spec.ts` | Criar | Testes integração facade + HTTP service | A2+A3 (HTTP + Facade) |
| **T9** | `frontend/src/app/features/anticipation/pages/simulation/anticipation-simulation-page.integration.spec.ts` | Criar | Testes integração página + facade | A4+A3 (Página + Facade) |
| **T10** | `frontend/e2e/anticipation-simulation.e2e-spec.ts` | Criar | Suíte E2E completa (6 cenários CA-RF3) | Todos CA-RF3 |
| **T11** | `frontend/e2e/anticipation-simulation.e2e-spec.ts` | Integrar | Testes navegação (RF-1 → simulação, admin → simulação) | A8+A10+A11 (Rotas + Links) |

### 5.2 Ordem Sugerida de Implementação

1. **T1** (Domain) — Base types, fácil começar
2. **T2** (HTTP Service) — Infraestrutura HTTP
3. **T3** (Facade) — Lógica de negócio core
4. **T4-T6** (Componentes) — UI isolada
5. **T7** (Página) — Composição
6. **T8-T9** (Integração) — Fluxos combinados
7. **T10-T11** (E2E) — Fluxos completos usuário

---

## § 6. CONSIDERAÇÕES TÉCNICAS

### 6.1 Padrões de Teste (mestre-freire-angular)

- **Mocks**: `jest.mock()` para HTTP calls, `TestBed` para Angular services
- **Signals**: Testar via `effect()` ou `computed()` para reatividade
- **Async**: `fakeAsync/tick` ou `waitFor` para Promises/Observables
- **Forms**: `FormBuilder` em testes, verificar `form.valid/invalid`
- **E2E**: Playwright page objects, data-testid attributes

### 6.2 Cobertura Esperada

| Tipo | Target | Justificativa |
|------|--------|--------------|
| **Unitários** | >80% statements, >70% branches | Lógica crítica (facade, forms, domain) |
| **Integração** | >90% statements | Fluxos críticos facade↔service |
| **E2E** | 100% cenários CA-RF3 | Requisitos funcionais obrigatórios |

### 6.3 Dependências de Teste

- **Angular Testing**: `TestBed`, `ComponentFixture`, `fakeAsync`
- **Jest**: Mocks, spies, assertions
- **Playwright**: E2E framework (já configurado)
- **RxJS**: `of()`, `throwError()` para observables

---

## § 7. PRÓXIMOS PASSOS

1. **Executar**: Usar skill `quadro-de-recompensas` com este plano para gerar código de testes
2. **Implementar**: Seguir ordem T1→T11, TDD (teste vermelho → código verde)
3. **Validar**: Executar `npm test` e `npm run e2e` após cada tarefa
4. **Cobertura**: Verificar targets com `npm run test:coverage`
5. **Integração**: Alinhar com implementação RF-3 (backend deve estar mockado)

---

## § 8. REFERÊNCIAS

- **Plano Global**: `.cursor/plans/rf-3-simulacao.md`
- **Skills**: `quadro-de-recompensas`, `mestre-freire-angular`
- **Estrutura**: `.cursor/rules/angular-frontend.mdc`
- **Exemplos**: Testes existentes em `frontend/src/app/features/anticipation/`

---

**Estado:** Pronto para execução com `quadro-de-recompensas`  
**Arquivo de Output:** `plano-arvore-testes_rf-3-simulacao.plan.md`  
**Última atualização:** 6 de março de 2026</content>
<parameter name="filePath">/media/belo/BeloSSD_2/Processos_Sel/LastLink/LastTechTest/.cursor/plans/plano-arvore-testes_rf-3-simulacao.plan.md