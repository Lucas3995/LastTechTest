---
name: mestre-freire-angular
description: Criar frontend Angular do zero (última versão), refatorar guiado por relatório e evoluir com demandas. Camadas (domain, application, infrastructure, core, shared, features), conceitos fundantes (áreas, módulos, páginas, componentes, directives, serviços) com mapeamento para estrutura de ficheiros e pastas, convenções (templateUrl, styleUrl), a11y, Signals/RxJS, lifecycle, typed forms, padrão command para backend. Spec antes de código. Usar quando criar, refatorar ou evoluir frontend Angular.
---

# Mestre Freire — Angular

Skill **complementar** à [mestre-freire](../mestre-freire/SKILL.md). Aplica critério e convenções específicos do frontend Angular em três modos: **Greenfield** (criar app do zero), **Refatoração** (relatório → refatorar) e **Evolução** (demandas → implementar e refatorar).

Usar **em conjunto** com mestre-freire quando o relatório de inadequações incluir ficheiros Angular. Usar **sozinha** (ou com maestro/quadro-de-recompensas) para Greenfield e Evolução.

---

## Modos de uso

| Modo | Entrada obrigatória | Fluxo | Comando de testes |
|------|--------------------|-------|-------------------|
| **Greenfield** | Spec/contrato de API (ex.: OpenAPI) e estrutura desejada (áreas, módulos) | Ler spec → planejar pastas e camadas → implementar (regra + reference) → validar | `npm run test` ou `ng test` |
| **Refatoração** | Relatório de inadequações (ex.: batedor-de-codigos) | Ler relatório → planejar por achado → refatorar em passos (mestre-freire + esta skill) → executar testes | `npm run test` ou `ng test` |
| **Evolução** | Demanda com critérios verificáveis e/ou relatório do maestro; contrato de API se aplicável | Ler spec/demanda → planejar (maestro quando aplicável) → implementar respeitando regra e reference → validar | `npm run test` ou `ng test` |

- **Spec antes de código:** Nunca gerar ou alterar código Angular sem artefato de spec (contrato, relatório, demanda com critérios).
- **Validar:** Após cada passo ou lote coerente, executar a suíte de testes; todos devem passar.

---

## O que esta skill acrescenta

- **Regra de critério técnico:** [.github/instructions/angular-frontend.instructions.md](.github/instructions/angular-frontend.instructions.md) — camadas, conceitos (áreas, módulos, páginas, componentes, directives, serviços) e mapeamento para estrutura de ficheiros e pastas, convenções, DIP, a11y, Signals/RxJS, lifecycle, typed forms, central de ações (Command).
- **Modo Greenfield:** criar app com estrutura de áreas/módulos/páginas, camadas e central de ações conforme regra e reference.
- **Modo Evolução:** novas features/páginas/módulos respeitando as mesmas regras e estrutura.
- **Ficheiros de teste:** em Angular não alterar `*.spec.ts`. Mantê-los intocados; se um teste falhar após alteração, corrigir o código de produção.
- **Comando de testes:** `npm run test` ou `ng test`. Executar após cada passo de refatoração ou implementação.
- **Convenções:** componentes com `templateUrl` e `styleUrl`; lógica de domínio em `application/` ou `domain/`; componentes de página não injetam `HttpClient` diretamente; dependências de interfaces do domain, implementações em infrastructure.

---

## Fontes de critério (Angular)

- **Regra:** `.github/instructions/angular-frontend.instructions.md` (path relativo à raiz do projeto) — camadas, estrutura modular (conceitos → ficheiros/pastas), convenções, serviços por camada, regras duras, a11y, Signals/RxJS, lifecycle, typed forms, Command.
- **reference.md:** [reference.md](reference.md) — fluxo spec-first, DIP e camadas, estrutura modular, testes, a11y, Signals/RxJS, lifecycle, typed forms, central de ações.
- Para categorias de smell e princípios gerais (SRP, OCP, etc.), consultar [reference da mestre-freire](../mestre-freire/reference.md) quando necessário.

Consultar a regra e o reference para **onde** colocar código (domain, application, infrastructure, features; áreas, módulos, páginas, componentes, directives) e **como** respeitar as convenções.

---

## Checklist por modo (para agente IA)

### Greenfield

- [ ] Spec/contrato de API e estrutura desejada (áreas, módulos) disponíveis.
- [ ] Pastas e camadas planeadas conforme regra (domain, application, infrastructure, core, shared, features; áreas/módulos/páginas).
- [ ] Scaffolding segue convenções (templateUrl, styleUrl, standalone, lazy loading).
- [ ] Central de ações de servidor prevista para requisições ao backend.
- [ ] `npm run test` ou `ng test` executado e verde.

### Refatoração

- [ ] Relatório de inadequações recebido; achados ordenados por dependência.
- [ ] Cada achado tratado com técnica da mestre-freire e critério Angular (regra + reference).
- [ ] Nenhum ficheiro `*.spec.ts` alterado.
- [ ] `npm run test` ou `ng test` executado após passos; suíte verde.

### Evolução

- [ ] Demanda com critérios verificáveis (e/ou relatório maestro); contrato de API se aplicável.
- [ ] Novos artefatos na camada e pasta corretas (regra: conceitos → ficheiros/pastas).
- [ ] Convenções e regras duras respeitadas (a11y, Signals/RxJS, lifecycle, typed forms, Command).
- [ ] `npm run test` ou `ng test` executado e verde.

---

## Relação com mestre-freire

- **mestre-freire** define o fluxo de refatoração (relatório → planejar → refatorar em passos → executar testes) e as regras gerais (comportamento inalterado, testes intocáveis, análise limitada ao relatório).
- **mestre-freire-angular** adiciona a regra angular-frontend, os três modos (Greenfield, Refatoração, Evolução), a convenção `*.spec.ts` e o comando `npm run test` / `ng test` para o frontend Angular.

Ao refatorar código Angular, seguir primeiro a mestre-freire e aplicar em seguida os critérios desta skill para cada achado. Ao criar ou evoluir, usar esta skill com a regra e o reference como fonte única de critério de estrutura e convenções.

---

## Relação com outras skills

- **maestro:** produz relatório de alterações; usar em Evolução para planejar o que implementar.
- **quadro-de-recompensas:** cria testes a partir do relatório; usar em Evolução quando a demanda exigir novos testes.
- **batedor-de-codigos:** produz relatório de inadequações; usar em Refatoração como entrada.
- **clean-architecture-analysis:** relatório de conformidade com Clean Architecture; quando a refatoração for guiada por esse relatório, aplicar os critérios Angular desta skill.
- **software-engineering-practice:** UX/UI, a11y, qualidade, spec-driven; alinhar critérios de usabilidade e acessibilidade com a regra angular-frontend.
