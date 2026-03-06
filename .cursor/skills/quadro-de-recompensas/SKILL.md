---
name: quadro-de-recompensas
description: Cria testes automáticos (unitários, integração, E2E) a partir de um relatório de tarefas; alterar apenas módulos/classes de teste; sem análise de qualidade, performance ou arquitetura. Usar quando o utilizador solicitar criação de testes com base em relatório de tarefas/demanda.
---

# Quadro de Recompensas — Criação de Testes a partir de Relatório de Tarefas

Cria ou atualiza **testes automáticos** (unitários, de integração e end-to-end) no front e no backend para refletir no código de testes as necessidades das demandas descritas num **relatório de tarefas a serem realizadas**. Não altera código de produção; não realiza análise de performance, arquitetura ou qualidade de código.

---

## Princípios

- **Apenas ficheiros de teste:** Alterar somente módulos e classes de teste (ex.: `*.spec.ts`, `*Tests.cs`, `e2e/*.spec.ts`). Código de produção permanece intocado.
- **Rastreabilidade:** Cada teste (ou grupo de testes) deve estar ligado a um item do relatório de tarefas (requisito/demanda atendida).
- **Sem análise extra:** Não avaliar performance, arquitetura, code smells ou adequação do código de produção; o escopo vem exclusivamente do relatório de tarefas.
- **Práticas de testes:** Seguir as práticas do documento de engenharia de ponte de requisitos (ver [reference.md](reference.md)): pirâmide de testes, AAA, FIRST, um conceito por teste, nomes descritivos.

---

## Fluxo de trabalho

1. **Receber o relatório de tarefas**  
   Se o utilizador não fornecer relatório, solicitar ou indicar que esta skill exige relatório de tarefas como entrada.

2. **Mapear tarefas a testes**  
   Para cada item do relatório: identificar onde impacta (front/back), tipo de teste adequado (unitário / integração / E2E) conforme pirâmide e criticidade do fluxo.

3. **Derivar casos de teste**  
   A partir dos itens do relatório: caixa preta (requisitos e critérios de aceite); quando necessário, inspecionar código apenas para localizar unidades ou APIs a testar — sem análise de qualidade ou arquitetura.

4. **Implementar testes**  
   Criar ou atualizar ficheiros de teste; garantir AAA (Arrange-Act-Assert), nomes descritivos, um conceito por teste; usar dublês (mocks/stubs) para isolar unidades.

5. **Executar a suíte**  
   Rodar os testes (ex.: `npm run test` / `ng test` no frontend, `dotnet test` no backend; E2E conforme projeto) e garantir que os novos ou alterados testes passam. Se houver falhas pré-existentes não introduzidas por esta skill, documentar.

---

## Formato de entrada esperado

Relatório com secção de **alterações/tarefas** contendo, por item:

- **Identificador** (ex.: ID sequencial)
- **Onde:** módulo, ficheiro ou camada
- **Tipo:** Criar | Alterar | Remover | Integrar
- **Descrição:** o que deve ser feito para atender à demanda
- **Requisito atendido:** referência à parte da demanda que o item atende

Formato de referência: template do [Maestro](../maestro/SKILL.md) (Relatório de Alterações para Demanda). Qualquer relatório estruturado com itens de alteração ou demanda por componente/fluxo é aceitável.

---

## Regras duras

- **Não modificar código fora de pastas/ficheiros de teste.** Nenhuma alteração em código de produção.
- **Não realizar análise de qualidade, performance ou arquitetura** do código de produção.
- **Não inventar tarefas:** criar apenas testes que cubram itens presentes no relatório de tarefas.
- **Após criar ou alterar testes,** executar a suíte e garantir que está verde; ou documentar falhas pré-existentes que não foram introduzidas por esta skill.

---

## Fontes de critério para os testes

Consultar [reference.md](reference.md) para:

- Pirâmide de testes, AAA, FIRST, caixa preta/caixa branca
- Técnicas: particionamento de equivalência, valor limite, testes baseados em requisitos
- Padrões e boas práticas: um conceito por teste, nomes descritivos, dublês
- Checklist rápido para criação de testes
- Mapeamento ao projeto: onde ficam os testes (front/back) e comandos para executar a suíte

---

## Relação com outras skills

- **maestro:** Pode produzir o “relatório de alterações” que esta skill usa como relatório de tarefas. Fluxo típico: Maestro analisa demanda → relatório → quadro-de-recompensas cria testes.
- **batedor-de-codigos / mestre-freire:** Escopos distintos. Esta skill não analisa inadequações nem refatora; apenas cria ou atualiza testes com base no relatório de tarefas.
