# Referência — Engenharia de Requisitos (para relatório de alterações)

Resumo de conceitos úteis para redigir o relatório de alterações de forma alinhada à Engenharia de Requisitos. Fonte: [ENGENHARIA_DE_REQUISITOS.md](../../../ENGENHARIA_DE_REQUISITOS.md) (norma IEEE/ISO/IEC 29148).

---

## Conceitos centrais

| Conceito | Definição |
|----------|-----------|
| **Requisito funcional** | O que o sistema deve fazer; comportamentos, funções e capacidades esperadas. |
| **Requisito não funcional** | Como o sistema deve se comportar; desempenho, segurança, usabilidade, disponibilidade. |
| **Requisito de negócio** | Objetivos estratégicos que motivam a existência da funcionalidade; valor entregue. |
| **Requisito de sistema/software** | Especificação em nível de sistema ou software; pode derivar de requisitos de negócio. |

Ao descrever a demanda e as alterações, usar estes termos quando ajudar a evitar ambiguidade (ex.: "Requisito funcional: o utilizador deve poder X"; "Requisito não funcional: resposta em menos de 2s").

---

## Atributos de requisito (qualidade documental)

Requisitos bem escritos são: **completo**, **consistente**, **não ambíguo**, **verificável**, **rastreável**, **relevante**. No relatório, cada alteração pode ser ligada à parte da demanda que atende (rastreabilidade demanda → alteração).

---

## Atividades relevantes para o Maestro

- **Revisão das demandas**: Analisar a demanda do utilizador; identificar o que está claro e o que é ambíguo.
- **Esclarecimento das demandas**: Refinar e esclarecer; remover ambiguidades na medida do possível; assinalar premissas quando não for possível.
- **Distribuição das tarefas**: Atribuir alterações a componentes do sistema (onde cada mudança deve ocorrer) — isto é o núcleo do relatório do Maestro.

Não é papel do Maestro: elicitação com stakeholders, validação formal, baseline ou gestão de mudanças em projeto.

---

## Boas práticas para o texto do relatório

- **Não ambíguo**: Uma única interpretação possível; evitar "melhorar", "tornar mais rápido" sem critério objectivo.
- **Verificável**: Cada alteração deve permitir verificar se foi feita (ex.: "Criar endpoint POST /api/foo" em vez de "adicionar suporte a foo").
- **Rastreável**: Ligar cada item de alteração à parte da demanda que atende (campo "Requisito atendido" no template).
