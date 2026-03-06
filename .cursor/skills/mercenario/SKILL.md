---
name: mercenario
description: Transforma regras de negócio em código de software; lê especificações, interpreta condições/restrições/fluxos do domínio e expressa-os no código. Não altera código de teste salvo pedido expresso do utilizador; não realiza análise nem alterações de performance, arquitetura ou qualidade de código. Usar quando o utilizador pedir implementação de regras de negócio, tradução de especificação para código ou implementação de lógica de domínio.
---

# Mercenário — Construção de Software

Foco na **tradução de regras de negócio para código** (implementação da regra). Não cobre arquitetura, testes nem qualidade de código em geral.

---

## Restrições obrigatórias

- **Testes:** É **proibido** alterar ficheiros, módulos ou classes de teste, exceto quando o utilizador solicitar explicitamente.
- **Qualidade, performance e arquitetura:** É **proibido** propor ou realizar análise ou alterações relativas a performance, arquitetura ou qualidade de código (refatoração por qualidade, code smells, adequação a padrões, etc. ficam fora do escopo desta skill).

---

## Fluxo de trabalho

1. **Ler e interpretar a especificação** — Requisitos funcionais, regras explícitas, restrições; entidades, operações, cenários (fluxo feliz e exceções de negócio).
2. **Identificar entidades, valores e operações** — Substantivos → tipos; verbos/ações → métodos ou comandos; onde cada regra “vive”.
3. **Mapear condições para código** — “Se X então Y” em condicionais, guard clauses ou early returns; cobrir todas as ramificações.
4. **Expressar cálculos e fórmulas** — Fórmulas de negócio em expressões e funções; constantes nomeadas para parâmetros.
5. **Modelar estados e transições** — Estados (enum, constantes, tipo) e transições permitidas quando a regra depende de estado.
6. **Tratar exceções de negócio** — Retorno (resultado, optional, Either), exceção de negócio ou valor de erro; consistência no tratamento.
7. **Revisar aderência** — Cada regra documentada tem um lugar claro no código; não há comportamento extra não especificado.

O detalhe de *como* expressar cada tipo de regra (conceitos, lógica, técnicas, padrões, normas) está em [reference.md](reference.md).

---

## Índice rápido (reference.md)

| Precisa de… | Secção no reference |
|-------------|----------------------|
| Que conceito aplicar | §1 Conceitos centrais |
| Como expressar regra X no código | §2 Lógica, §4 Técnicas, §8 Normas |
| Que fazer ao implementar | §3 Atividades |
| Que padrão usar | §5 Padrões |
| Princípios na escrita | §6 Princípios |
| Estruturas de dados e algoritmos | §7 Estruturas e algoritmos |
| Busca por termos | §9 Índice de termos |

---

## Princípios na escrita

- **DRY:** Não duplicar a mesma regra; extrair para função/método ou tipo quando a condição ou cálculo aparece em mais de um ponto.
- **KISS:** Preferir a solução mais simples que implemente corretamente a regra; não introduzir abstrações ou padrões “por precaução”.
- **YAGNI:** Implementar só o que a demanda e a especificação pedem; não codificar regras ou cenários “para o futuro” sem requisito.
- **Um lugar por regra:** Cada regra de negócio tem um único ponto no código onde está implementada.
- **Nomes do domínio:** Tipos, métodos, variáveis e constantes refletem vocabulário do negócio.

Detalhe completo em [reference.md](reference.md).
