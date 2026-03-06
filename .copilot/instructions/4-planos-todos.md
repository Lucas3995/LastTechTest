---
description: Ao gerar ou atualizar planos (.plan.md ou CreatePlan), preencher sempre a seção todos do frontmatter com itens acionáveis.
alwaysApply: true
---

# Planos — Seção de Todos

Ao **gerar** ou **atualizar** um plano (ficheiro `.plan.md` ou uso da ferramenta CreatePlan):

1. **Preencher sempre a secção `todos`** no frontmatter do plano; não deixar `todos: []` vazio quando o plano descrever alterações ou passos implementáveis.

2. **Formato dos todos:** cada item deve ter:
   - **id:** identificador único e estável (ex.: `backend-enum-json`, `frontend-preview-404`).
   - **content:** descrição curta e acionável do que fazer (suficiente para quem for executar).

3. **Origem dos todos:** derivar os itens das alterações ou passos já descritos no corpo do plano (ex.: um item de “Alterações necessárias” ou “Resumo executivo” vira um todo com id e content correspondentes). Itens opcionais podem ser marcados no content, ex.: "(Opcional) ...".

4. **Quantidade:** incluir pelo menos um todo por alteração ou fase relevante; agrupar só quando for um único passo muito pequeno.

Com isso, a secção de todos fica sempre preenchida e utilizável para acompanhamento e execução do plano.
