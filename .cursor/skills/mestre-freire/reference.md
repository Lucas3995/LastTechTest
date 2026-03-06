# Referência — Mapeamento Achado → Técnica e Regra

Consulta rápida para aplicar refatoração a cada tipo de achado do relatório. O **princípio/ref violada** no achado indica qual regra e técnica usar.

---

## Por categoria de smell (relatório)

| Categoria (relatório) | Smells típicos | Regra principal | Técnicas (tecnicas-conhecidas) |
|----------------------|----------------|------------------|---------------------------------|
| **Bloaters** | Long Method, Large Class, Long Parameter List, Primitive Obsession, Data Clumps | clean-architecture (SRP), ddd (Value Objects) | Extract Method, Extract Class, Replace Data Value with Object, Introduce Parameter Object, Replace Magic Number with Symbolic Constant |
| **OO Abusers** | Switch Statements, Temporary Field, Refused Bequest, Alternative Classes, Feature Envy | clean-architecture (OCP, LSP, ISP), tecnicas-conhecidas | Replace Conditional with Polymorphism, Replace Type Code with State/Strategy, Move Method, Move Field, Extract Interface |
| **Change Preventers** | Divergent Change, Shotgun Surgery, Parallel Inheritance | clean-architecture (SRP, CCP) | Extract Class, Extract Superclass, Move Method/Field |
| **Dispensables** | Duplicate Code, Lazy Class, Data Class, Dead Code, Speculative Generality | ddd (entidades ricas), tecnicas-conhecidas | Extract Method, Inline Class, Move Method to domain, Remove Dead Code |
| **Couplers** | Feature Envy, Inappropriate Intimacy, Message Chains, Middle Man | clean-architecture (DIP), tecnicas-conhecidas | Move Method, Hide Delegate, Remove Middle Man, Introduce Parameter Object |
| **Arch and Struct** | Circular Dependency, God Object, Hard-Coded Dependencies | clean-architecture (camadas, DIP) | Extract Class, Introduce interfaces in domain/application, inject dependencies at composition root |
| **Test Smells** | (Relatório pode listar; mestre-freire **não altera testes**) | — | Nenhuma alteração em ficheiros de teste |

---

## Por princípio violado (campo do achado)

| Princípio/Ref | Ação de refatoração | Onde ver detalhes |
|---------------|---------------------|--------------------|
| **SRP** | Separar responsabilidades em classes/módulos distintos; Extract Class, Extract Method | clean-architecture §3.1, tecnicas-conhecidas §5.2 |
| **OCP** | Introduzir abstrações (interfaces/estratégias); evitar modificar código estável | clean-architecture §3.2, tecnicas-conhecidas §5.4 (Replace Conditional with Polymorphism) |
| **LSP** | Corrigir hierarquias para que subclasses sejam substituíveis; Replace Inheritance with Delegation se Refused Bequest | clean-architecture §3.3, tecnicas-conhecidas §5.6 |
| **ISP** | Segregar interfaces; Extract Interface com métodos usados pelo cliente | clean-architecture §3.4, tecnicas-conhecidas §5.6 |
| **DIP** | Depender de abstrações; interfaces em camadas internas, implementações na borda | clean-architecture §3.5, §5 |
| **Clean Architecture** | Respeitar direção de dependência; mover código para a camada correta (Entidades, Casos de Uso, Adaptadores, Drivers) | clean-architecture §2, §5, §7.2 |
| **DDD** | Value Objects para primitivos agrupados; regras em entidades/agregados; linguagem ubíqua | ddd-domain-driven-design § building blocks, § refatorar |
| **CQRS** | Separar comandos (alteram estado) de queries (só leem); handlers e DTOs distintos | cqrs-command-query-responsibility-segregation §2, §6 |

---

## Ordem sugerida ao tratar achados

1. **Dependências e arquitetura** (Circular Dependency, Hard-Coded Dependencies, Framework Coupling) — estabilizar camadas e injeção.
2. **God Object / Large Class** — extrair classes e mover para camadas corretas.
3. **Long Method / Duplicate Code** — Extract Method, Substitute Algorithm.
4. **Feature Envy / Data Class** — Move Method, enriquecer domínio (DDD).
5. **Primitive Obsession / Data Clumps** — Value Objects, Parameter Object.
6. **Switch Statements / Type Code** — State/Strategy ou polimorfismo.
7. **Dispensables** (Dead Code, Lazy Class, Middle Man) — remover ou consolidar.

---

## Comando de testes

Executar o comando de testes do projeto (ex.: frontend `npm run test`, backend `dotnet test`). Não alterar ficheiros de teste. Se o projeto usar outros comandos, seguir `package.json`, solution ou raiz do repositório.
