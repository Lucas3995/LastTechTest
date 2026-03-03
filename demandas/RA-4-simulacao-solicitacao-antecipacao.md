## [RA-4] Simulação de solicitação de antecipação (fake, sem persistência)

### Contexto e objetivo de negócio

Os creators precisam entender o impacto financeiro de uma antecipação **antes** de efetivamente registrar uma solicitação real no sistema. Hoje, a única forma de obter esse entendimento seria criando uma solicitação "de verdade", o que gera registros persistidos, trilhas de auditoria e possíveis impactos operacionais, mesmo quando o usuário só queria "experimentar cenários".  
Este card tem como objetivo **expor via API uma funcionalidade de simulação de antecipação** que utilize exatamente as mesmas regras de cálculo e validação das solicitações reais (RA-1, regras existentes e `InstrucoesProjeto` somadas), mas **sem qualquer efeito persistente** na chamada de simulação. A simulação deve permitir que o creator explore cenários (valores, prazos, taxas, etc.) com rapidez e segurança; opcionalmente, o usuário pode **transformar a última simulação em cache em uma solicitação real**, desde que as regras de negócio permitam e que os valores da real sejam idênticos aos da simulação.

### User story principal

- Como **Creator**, quero **simular uma solicitação de antecipação com os mesmos critérios de análise da solicitação real**, para **entender o valor líquido esperado, taxas e impactos antes de decidir se realmente envio a solicitação para análise**.

### User stories adicionais

- Como **Creator**, quero **poder variar parâmetros principais da simulação (valor, datas, parcelas, etc.) em múltiplas tentativas rápidas**, para **comparar cenários sem precisar criar ou cancelar solicitações reais**.  
- Como **Analista**, quero **poder simular uma solicitação de antecipação em nome de qualquer creator**, para **apoiar o atendimento e esclarecer dúvidas de usuários, sem gerar registros reais indevidos**.

> O usuário **Admin** continua sendo um **superusuário** com acesso a **todas as ações** de todas as roles no escopo (inclusive as ações descritas para `Creator` e `Analista`), mas sem ganhar um comportamento de negócio diferente dos demais neste card.

### Personas / papéis afetados

- **Creator**: usuário que deseja antecipar recebíveis e precisa testar cenários antes de enviar uma solicitação real; pode converter sua última simulação em cache em solicitação real, desde que não exista outra solicitação em aberto e que as regras de negócio sejam satisfeitas.  
- **Analista**: papel ligado a atividades de análise/apoio operacional, que aqui pode **simular em nome de qualquer creator** para suporte e esclarecimento de dúvidas; não converte simulações em solicitações reais.  
- **Admin**: superusuário administrativo com acesso a **todas as ações** de todas as roles; neste card, pode executar qualquer operação disponível a `Creator` e `Analista`, inclusive converter simulações em reais em nome de qualquer creator.

### Telas, módulos, relatórios e navegação

- **Escopo deste card**:
  - Não haverá telas/front-end implementados aqui.
  - Este card foca exclusivamente na **exposição de endpoints de API** para simulação, cache e conversão em real, e na modelagem de domínio associada.
- **Módulo lógico**:
  - Domínio de **antecipação de recebíveis** / **solicitações de antecipação**, reutilizando o mesmo agregado/modelo de RA-1, mas em modo de simulação (sem persistência até a conversão).
- **Integrações futuras previstas (não implementadas neste card, mas a considerar na modelagem)**:
  - Tela de simulação para `Creator` (formulário de parâmetros + apresentação de resultado simulado).
  - Possível integração da simulação como passo anterior ao fluxo de criação real de solicitação (RA-1) – ex.: "Simular → Confirmar → Criar solicitação real".

### Permissões e segurança

- **Role `Creator`**:
  - Pode **executar simulações** para si próprio, com base em suas próprias condições de negócio (limites, contratos, regras aplicáveis). Pode criar quantas simulações quiser; apenas a **última** permanece em cache (a anterior é substituída).
  - Pode **converter uma simulação fake sua em uma solicitação real**, usando o código da simulação, desde que: (a) não exista outra solicitação de antecipação em aberto para esse creator (seja vinda de simulação ou de criação direta); (b) todas as regras de negócio de criação (RA-1 + regras existentes + `InstrucoesProjeto`) estejam satisfeitas no momento da conversão; (c) os valores da solicitação real sejam idênticos aos da simulação em cache.
  - Não pode simular explicitamente em nome de outros creators (identidade vem do token/autenticação).
- **Role `Analista`**:
  - Pode **executar simulações em nome de qualquer creator**, quando o fluxo de atendimento exigir esse suporte.
  - Não converte simulações em solicitações reais (a decisão de formalizar a solicitação é do creator; o Admin pode atuar em exceções).
  - Deve respeitar as mesmas regras de negócio e validações que seriam aplicadas se o próprio creator estivesse simulando.
- **Role `Admin`**:
  - Tem acesso a **todas as ações** descritas para `Creator` e `Analista` neste card.
  - Pode **converter simulações em solicitações reais em nome de qualquer creator**, para cenários excepcionais/operacionais, sempre respeitando as mesmas regras de negócio (incluindo ausência de solicitação em aberto e valores idênticos à simulação).
- **Segurança geral**:
  - Todos os endpoints devem respeitar o mecanismo de autenticação/autorização já estabelecido (JWT, roles, etc.).
  - A simulação **não deve expor dados sensíveis** além do necessário para o resultado financeiro e informações de contexto aprovadas pelo negócio.

### Fluxos de uso e regras de negócio

#### Fluxo principal – simulação de uma solicitação

1. O usuário (role `Creator` autenticado, `Analista` ou `Admin`) chama o endpoint de **simulação de antecipação** fornecendo:
   - Identificadores/parametrizações relevantes (ex.: contratos, recebíveis, parcelas, data desejada, valor a antecipar, etc.), conforme regras de RA-1 e `InstrucoesProjeto`.
   - Se for `Analista` ou `Admin`, opcionalmente o identificador do creator em nome de quem a simulação está sendo feita (conforme contrato de API).
2. O sistema:
   - **Reutiliza as mesmas regras de cálculo e validação** previstas para criação de uma solicitação real (RA-1 + regras existentes + `InstrucoesProjeto` como adição), incluindo restrições de valores, limites contratuais, datas válidas, prazos, taxas aplicáveis, etc.
   - Executa o cálculo **em memória/camada de domínio** sem criar nenhuma entidade persistida de solicitação de antecipação.
   - Avalia inconsistências ou impossibilidades, retornando erros de validação adequados quando a simulação não for viável.
3. Se a simulação for válida:
   - O sistema **armazena essa simulação em cache** (apenas a última por creator: uma nova simulação do mesmo creator **substitui** a anterior).
   - Cache com **validade real de 2 horas**; a data de validade **retornada ao usuário** deve ser **20 minutos antes** da expiração real (ex.: criada às 10:00, expira às 12:00, o usuário vê validade até 11:40).
   - O sistema retorna um **código da simulação** (`simulationCode`) e a **data de validade exposta**, além do resultado financeiro (valor bruto, taxas, valor líquido, datas, etc.).

#### Regras de negócio – cache e quantidade de simulações

- Um usuário pode **criar quantas simulações quiser**; **apenas a última fica em cache** por creator. Quando o usuário cria uma nova simulação, se já existia uma para ele, ela é **substituída** pela nova.
- Ter uma **solicitação em aberto** (seja vinda de simulação ou de requisição direta) **não impede** o creator de **criar simulações**; só impede de **transformar** uma simulação em solicitação real.
- Um creator **não** é impedido de criar simulações por existência ou não de simulações anteriores (apenas a última permanece em cache; as demais são substituídas).

#### Fluxo – conversão da simulação em solicitação real

- Existe um endpoint específico (ex.: `POST /api/v1/anticipations/simulations/{simulationCode}/confirm`) para:
  - **Buscar a simulação no cache** a partir do `simulationCode`.
  - **Verificar** que não existe outra **solicitação de antecipação em aberto** para o creator; se existir, a conversão é **rejeitada** (mensagem clara).
  - **Reverificar** que as regras de negócio de criação (RA-1 + existentes + `InstrucoesProjeto`) ainda permitem aquela operação no momento da conversão (ex.: saldo/limite, outras restrições). Se alguma regra impedir, a conversão é rejeitada com mensagem de validação.
  - Caso todas as condições sejam satisfeitas:
    - Criar uma **solicitação real de antecipação** usando **exatamente os valores e parâmetros da simulação armazenada em cache** (sem recálculo que altere valores).
    - Persistir essa solicitação seguindo o fluxo normal de RA-1 (estado inicial `ANALISE_PENDENTE`, auditoria).
    - Marcar a simulação como **utilizada** (não pode ser convertida novamente).
- **Valores da solicitação real = valores da simulação**: ao converter, os valores financeiros e parâmetros da solicitação real criada devem ser **idênticos** aos calculados e retornados na simulação (e armazenados no cache). O usuário validou esses valores na simulação; não deve haver recálculo nem alteração na conversão.

#### Regras de negócio – reuso e ausência de efeitos persistentes (simulação)

- **Reuso de regras**: as regras usadas na simulação devem ser as **mesmas** da criação real (RA-1); as de `InstrucoesProjeto` **se somam** às já existentes.
- **Ausência de efeitos persistentes na chamada de simulação**: a chamada de simulação **não** cria registros de solicitação no banco, não altera estados de solicitações existentes e não gera efeitos colaterais duradouros. É aceitável registrar logs técnicos ou auditoria leve de uso da simulação.
- **Expiração e idempotência da conversão**: se o `simulationCode` não existir no cache, já tiver expirado (após 2 horas reais) ou já tiver sido convertido, o endpoint de conversão não cria solicitação real e retorna mensagem explícita (ex.: simulação expirada ou já utilizada).

### Critérios de aceitação (testáveis)

- **CA1 – Simulação válida para Creator usando as mesmas regras da solicitação real**  
  - Dado que existe um creator autenticado com condições válidas para realizar uma antecipação conforme regras (já existentes + `InstrucoesProjeto`),  
  - Quando ele chamar o endpoint de **simulação de antecipação** informando parâmetros válidos,  
  - Então o sistema deve validar os parâmetros usando as mesmas regras de negócio da criação real (RA-1), calcular valores simulados (taxas, valor líquido, datas, etc.), retornar resultado de simulação com dados financeiros, **código da simulação** e **data de validade exposta** (20 minutos antes da expiração real), com status HTTP adequado (ex.: 200).

- **CA2 – Simulação rejeitada por violar regras de negócio**  
  - Dado que um creator autenticado informa parâmetros que **violam alguma regra de negócio**,  
  - Quando ele chamar o endpoint de simulação,  
  - Então o sistema deve detectar a violação (idêntico à criação real), não gerar entidade persistida, retornar erro de validação claro (ex.: 400/422) com os motivos.

- **CA3 – Ausência de efeitos persistentes na chamada de simulação**  
  - Dado que o usuário (`Creator`, `Analista` ou `Admin`) realiza uma ou várias chamadas ao endpoint de simulação,  
  - Então o sistema não deve criar registros de solicitação de antecipação nem alterar estados de solicitações existentes na operação de simulação; no máximo registrar logs/auditoria leve de uso.

- **CA4 – Simulação por Analista (ou Admin) em nome de outro usuário**  
  - Dado que existe um usuário com role `Analista` autenticado e um creator válido no sistema,  
  - Quando o `Analista` chamar o endpoint de simulação **em nome desse creator** (conforme contrato da API),  
  - Então o sistema deve aplicar as regras de negócio como se a solicitação fosse daquele creator, retornar o resultado de simulação normalmente e não criar nenhuma solicitação persistida.  
  - O mesmo vale para `Admin` quando realizar a mesma operação.

- **CA5 – Desempenho adequado para múltiplas simulações sequenciais**  
  - Dado que um creator autenticado realiza diversas chamadas de simulação em sequência variando parâmetros,  
  - Então o sistema deve responder dentro do tempo de resposta aceitável definido pelo projeto e não degradar de forma significativa o desempenho de RA-1 (criação) e RA-2 (consulta).

- **CA6 – Cache: apenas última simulação por creator; substituição e expiração**  
  - Dado que um creator autenticado executa uma simulação válida, quando o sistema concluir a simulação, então deve armazenar essa simulação em cache associado ao creator, com validade real de até 2 horas; retornar **código da simulação** e **data de validade exposta** 20 minutos antes da expiração real; invalidar/substituir qualquer simulação anterior desse mesmo creator (apenas a última permanece ativa).  
  - Dado que o mesmo creator executa uma **nova** simulação válida, então a simulação anterior deve ser substituída pela nova; apenas a nova fica disponível para conversão.

- **CA7 – Conversão bem-sucedida de simulação em solicitação real; valores idênticos**  
  - Dado que existe uma simulação em cache para um creator, ainda dentro do prazo de validade real e não utilizada, **e que não existe outra solicitação de antecipação em aberto** para esse creator, e que todas as regras de negócio de criação (RA-1 + existentes + `InstrucoesProjeto`) continuam satisfeitas,  
  - Quando o creator (ou o `Admin`, em cenário excepcional) chamar o endpoint de **conversão** passando o código da simulação,  
  - Então o sistema deve recuperar a simulação do cache, revalidar regras de negócio (sem recálculo que altere valores), criar uma **solicitação real de antecipação** persistida com **valores idênticos** aos da simulação (valor bruto, taxas, valor líquido, datas, parcelas, etc.), estado inicial adequado (ex.: `ANALISE_PENDENTE`), registrar auditoria normal de criação, marcar a simulação como utilizada e retornar os dados da solicitação real criada (ex.: HTTP 201/200).

- **CA8 – Conversão rejeitada (expirada, já utilizada, solicitação em aberto ou regras violadas)**  
  - **Cenário 1 – Simulação expirada ou inexistente ou já utilizada:**  
    Dado que o código de simulação não existe no cache, já expirou ou já foi convertido, quando o usuário tentar converter, então o sistema não deve criar nenhuma solicitação real, deve informar claramente se a simulação expirou ou já foi utilizada, e retornar código HTTP compatível (ex.: 404/410/422).  
  - **Cenário 2 – Já existe solicitação em aberto:**  
    Dado que existe uma solicitação de antecipação em aberto para o creator (vinda de simulação ou criação direta), quando o usuário tentar converter uma simulação em real, então o sistema deve rejeitar a conversão com mensagem clara (ex.: já existe solicitação em aberto) e não criar nova solicitação.  
  - **Cenário 3 – Regras de negócio deixaram de ser satisfeitas:**  
    Dado que a simulação ainda está no cache e não utilizada, mas alguma regra de negócio de criação passou a ser violada (ex.: saldo insuficiente, alteração de parâmetros), quando o usuário tentar converter, então o sistema deve rejeitar a criação da solicitação real e retornar erro de validação claro com os motivos.

### Requisitos técnicos/metodológicos aplicáveis

#### Reuso de lógica de domínio (simulação vs. criação real)

- A lógica de cálculo e validação usada na simulação deve ser **exatamente a mesma** da criação real da solicitação de antecipação, em componentes de domínio/aplicação reutilizáveis. Não é permitido duplicar regras (um cálculo para RA-1 e outro diferente para RA-4).

#### Cache – in-memory e preparação para distribuído

- O cache das simulações deve ser implementado **in-memory**, seguindo as orientações da documentação oficial da Microsoft: [Caching in .NET](https://learn.microsoft.com/en-us/dotnet/core/extensions/caching).
  - Utilizar a abstração adequada do ecossistema .NET (ex.: `IMemoryCache` do pacote `Microsoft.Extensions.Caching.Memory`) e registrar o serviço via injeção de dependência (ex.: `AddMemoryCache()`).
  - Configurar expiração absoluta (ex.: 2 horas) conforme `MemoryCacheEntryOptions` / `ICacheEntry` (ex.: `AbsoluteExpirationRelativeToNow`), garantindo TTL e substituição da simulação anterior do mesmo usuário conforme regras do card.
- A implementação deve estar **preparada para migração com esforço mínimo** para um cache distribuído no futuro (Redis, MongoDB ou outro).
  - A lógica de negócio (armazenar/recuperar/invalidar simulação) não deve depender diretamente de `IMemoryCache`; deve depender de uma **interface de abstração** (ex.: `ISimulationCache` ou `IAnticipationSimulationCache`) definida na camada de aplicação ou domínio.
  - A implementação concreta atual usa `IMemoryCache` (in-memory); futuras implementações (Redis, MongoDB, etc.) devem implementar a mesma interface, com configuração via DI, sem alterar os handlers/casos de uso que consomem o cache.
  - A troca para Redis, MongoDB ou outro provedor distribuído deve exigir apenas nova implementação da interface e registro em DI, sem refatoração da regra de negócio de simulação/conversão.

#### Arquitetura e padrões

- Respeitar a **Clean Architecture** do projeto (domínio, aplicação, infraestrutura, API), mantendo o endpoint de simulação e o de conversão desacoplados de detalhes de persistência e de implementação concreta do cache.  
- Utilizar **CQRS + MediatR** para os casos de uso de simulação e de conversão (ex.: `SimulateAnticipationRequestQuery`/`Command`, `ConvertSimulationToRealRequestCommand` + handlers).  
- Aplicar **DDD**, mantendo o agregado de solicitação/antecipação modelado de forma que o mesmo núcleo de regras atenda tanto o fluxo real (RA-1) quanto o de simulação (RA-4).  
- Seguir **SOLID** e os **3 princípios de coesão de componentes** (common closure, common reuse).  
- Seguir **boas práticas de .NET e C#**, usando **design patterns quando necessário** (Strategy/Policy, Factory, etc.).

#### Performance, estruturas de dados e recursos

- Escolher **estruturas de dados adequadas** para representar parcelas, fluxos de caixa, regras de taxas, etc., considerando a **complexidade assintótica** das operações de simulação.  
- Evitar consultas redundantes; carregar apenas o necessário (contratos, parâmetros, limites).  
- Utilizar corretamente os recursos da linguagem e do framework para **gerenciamento de processamento e memória**.

#### Metodologia de testes (TDD + pirâmide completa)

- Utilizar **estritamente TDD** para implementar este card.  
- Implementar **toda a pirâmide de testes**: **unitários** (regras de cálculo/validação, comportamento do cache, ausência de escrita na simulação); **integração** (caso de uso de simulação e de conversão com repositórios/cache, garantindo que simulação não grava solicitações e que conversão persiste com valores idênticos); **E2E** (chamadas HTTP para `Creator`, `Analista`, `Admin`, cobrindo CA1–CA8).  
- Usar as ferramentas padrão do projeto (xUnit, FluentAssertions, Coverlet, etc.).

### Rastreabilidade para código e testes (a preencher ao longo do ciclo)

- **Casos de uso / commands / queries:**  
  Ex.: `SimulateAnticipationRequestQuery`/`Command`, `ConvertSimulationToRealRequestCommand`, e respectivos handlers.
- **Endpoints/rotas HTTP:**  
  Ex.: `POST /api/v1/anticipations/simulations`, `POST /api/v1/anticipations/simulations/{simulationCode}/confirm` (ou rotas equivalentes).
- **Serviços de domínio / abstrações:**  
  Componente de cálculo/validação de antecipação reutilizado entre simulação e criação real; interface de cache de simulação (`ISimulationCache` ou equivalente).
- **Testes:**  
  Unitários (regras, cache); integração (simulação e conversão com persistência e cache); E2E (API cobrindo CA1–CA8).

### Dependências e riscos

- **Dependências:**  
  - **RA-1 – Criar solicitação de antecipação** (modelo de solicitação, regras de cálculo/validação reaproveitadas).  
  - **RA-2** e **RA-3** (consulta e estados) para alinhamento de modelo e verificação de “solicitação em aberto”.  
  - Regras de negócio de `InstrucoesProjeto`, **adicionadas** às regras já existentes.
- **Riscos:**  
  - Risco de divergência futura entre simulação e criação real se o reuso não for corretamente centralizado; mitigação: centralizar lógica e reforçar com testes.  
  - Risco de confusão na UX entre “simular” e “criar solicitação real”; mitigação ficará para cards de frontend (rótulos claros, fluxos distintos).
