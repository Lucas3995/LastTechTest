---
name: mestre-freire
description: Refatora código conforme relatório prévio de inadequações, sem alterar comportamentos nem testes. Aplica melhorias técnicas e conceituais (SOLID, Clean Architecture, DDD, CQRS, code smells) guiado pelo relatório. Executa a suíte de testes e garante que continue passando. Usar quando o utilizador solicitar refatoração guiada por relatório, execução de plano de correção ou melhoria técnica sem mudança de regras de negócio.
---

# Mestre Freire — Refatoração Guiada por Relatório

Refatora o código para **melhorias técnicas e conceituais** com base **exclusivamente** num relatório de inadequações (ex.: produzido pela skill batedor-de-codigos). Não altera comportamentos, não altera testes e não realiza nova análise em busca de necessidades de refatoração.

---

## Princípios

- **Comportamento inalterado:** regras de negócio e contratos públicos permanecem iguais. Nenhuma adição nem remoção de funcionalidade.
- **Testes intocáveis:** ficheiros de teste unitário e de integração **não são alterados**. Os testes são a rede de segurança; mudá-los invalidaria essa garantia.
- **Testes obrigatórios:** após cada refatoração (ou lote coerente), executar a suíte de testes. **Todos devem continuar a passar.** Se falharem, a causa é a refatoração — corrigir até passarem.
- **Análise limitada:** as únicas análises permitidas são (1) a **análise prévia**, fornecida pelo relatório, e (2) **análise para resolver problemas** introduzidos pela própria refatoração (ex.: teste que passou a falhar, compilação quebrada). **Proibido** fazer varreduras em busca de novos smells ou novas necessidades de refatoração.

---

## Fluxo de trabalho

1. **Receber o relatório**  
   Relatório de inadequações com achados (ID, smell, ficheiro, localização, evidência, princípio violado). Se o utilizador não fornecer relatório, solicitar ou indicar que esta skill exige relatório como entrada.

2. **Planejar por achado**  
   Para cada achado do relatório, identificar a **técnica de refatoração** ou o **princípio** a aplicar (ver [reference.md](reference.md)). Ordenar achados por dependência (ex.: corrigir dependências antes de extrair classes).

3. **Refatorar em passos pequenos**  
   - Um achado (ou grupo coerente de achados no mesmo ficheiro) por vez.  
   - Aplicar apenas alterações que **mitiguem o achado** sem mudar comportamento.  
   - Não alterar ficheiros de teste (conforme convenção do projeto).

4. **Executar testes**  
   Após cada passo (ou lote pequeno), executar a **suíte completa** de testes do projeto (backend e frontend). Objetivo: **verificar que as regras de negócio continuam funcionando** após a refatoração. Se algum teste falhar: tratar como **regressão da refatoração**; analisar a falha, ajustar o código de produção (não o teste) e repetir até todos passarem. Se o ambiente do host não permitir rodar os testes (ex.: Node &lt; 20 para o frontend), usar a **containerização do projeto** (ex.: `./scripts/frontend-test-docker.sh` no DiarioDeBordo).

5. **Repetir** até todos os achados do relatório estarem tratados e a suíte de testes verde.

---

## Regras duras

- **Não adicionar nem remover comportamentos.** Refatoração é mudança de estrutura, não de funcionalidade.
- **Não editar testes** para “acomodar” a refatoração. Se um teste quebra, a refatoração está a alterar comportamento — reverter ou ajustar o código de produção.
- **Não realizar nova análise** em busca de code smells ou necessidades de refatoração além do relatório. Análise extra só para diagnosticar falhas de teste ou build causadas pela refatoração.
- **Não deixar a suíte de testes falhar** ao concluir. Executar **todos** os testes (backend e frontend) antes de dar por concluída a tarefa; usar Docker/containerização quando o host não tiver o ambiente correto.

---

## Fontes de critério técnico

As **melhorias técnicas e conceituais** seguem as regras do projeto em `regras/`:

- **Clean Architecture e SOLID:** `regras/clean-architecture.mdc` — camadas, direção de dependência, SRP, OCP, LSP, ISP, DIP.
- **DDD:** `regras/ddd-domain-driven-design.mdc` — entidades, value objects, agregados, linguagem ubíqua.
- **CQRS:** `regras/cqrs-command-query-responsibility-segregation.mdc` — comandos vs queries, write/read model.
- **Técnicas e smells:** `regras/tecnicas-conhecidas.mdc` — catálogo de refatorações e code smells.
- **TDD:** `regras/tdd-test-driven-development.mdc` — testes como rede de proteção; não alterar testes nesta skill.
- **LGPD e segurança:** `regras/protecao-dados-lgpd-seguranca.mdc`, `regras/devsecops.mdc` — não enfraquecer controles ao refatorar.

Consultar estas regras para **como** aplicar a correção (onde colocar código, que padrão usar). O **o quê** corrigir vem do relatório.

---

## Checklist antes de dar por concluído

- [ ] Todos os achados do relatório foram tratados (ou documentada exceção justificada).
- [ ] Nenhum ficheiro de teste foi modificado.
- [ ] A suíte de testes foi executada e está verde.
- [ ] Nenhuma nova análise de smells foi feita além do relatório e do diagnóstico de falhas da refatoração.

---

## Relação com outras skills

- **batedor-de-codigos:** produz o relatório de inadequações que esta skill consome. Fluxo típico: batedor analisa → relatório → mestre-freire refatora.
- **mestre-freire-angular:** skill complementar para refatoração em frontend Angular; usar quando o escopo incluir código Angular.
- **create-skill / regras:** as regras em `regras/` definem o critério de “melhoria técnica e conceitual”; esta skill aplica-as apenas no âmbito do relatório, sem alterar comportamento nem testes.
