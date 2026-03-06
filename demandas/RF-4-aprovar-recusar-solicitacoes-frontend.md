## [RF-4] Aprovar e recusar solicitações no frontend (Analistas e Admins)

### Contexto e objetivo de negócio

A lista global de solicitações de antecipação (RF-2) e o detalhe da solicitação já permitem a Admin e Analista **visualizar** todas as solicitações e abrir o detalhe de qualquer uma. O backend expõe, desde RA-3, os endpoints **aprovar** (`POST /api/v1/anticipations/{id}/approve`) e **recusar** (`POST /api/v1/anticipations/{id}/reject`). Falta, porém, que o frontend ofereça **ações de aprovar e recusar** na interface, para que Analistas e Admins possam decidir sobre solicitações em estado "Em análise" (Pending) sem depender de ferramentas externas à aplicação.

O objetivo desta demanda é adicionar, no frontend Angular, a capacidade de **aprovar** (com observação opcional) e **recusar** (com motivo obrigatório) solicitações de antecipação a partir da tela de lista global e do componente de detalhe, restrita aos papéis Analista e Admin, com feedback claro e atualização imediata do estado na UI.

### User stories de frontend

- **US-AP1 – Aprovar solicitação em análise**
  - Como **Analista** ou **Admin**, quero **aprovar uma solicitação de antecipação que está em análise**, para **concluir o fluxo de forma positiva e registrar a decisão**.

- **US-AP2 – Recusar solicitação com motivo**
  - Como **Analista** ou **Admin**, quero **recusar uma solicitação em análise informando um motivo obrigatório**, para **comunicar ao creator o motivo da recusa e manter rastreabilidade**.

- **US-AP3 – Ver apenas ações quando aplicável**
  - Como **Analista** ou **Admin**, quero **ver os botões Aprovar e Recusar apenas quando a solicitação está em análise (Pending)** e **não vê-los em solicitações já aprovadas, recusadas ou canceladas**, para **evitar confusão e erros de uso**.

- **US-AP4 – Confirmação e feedback**
  - Como **Analista** ou **Admin**, quero **confirmar a ação antes de enviar** e **receber feedback claro de sucesso ou erro** (incluindo mensagem do backend quando houver), para **ter certeza do resultado e poder corrigir em caso de falha**.

### Personas / papéis

- **Admin (persona principal)**
  - Usa a lista global (RF-2) e o detalhe; deve poder aprovar e recusar solicitações em análise.
- **Analista (persona principal)**
  - Mesmo fluxo que Admin: lista global e detalhe com ações aprovar/recusar.
- **Creator (impacto indireto)**
  - Não vê nem usa as ações de aprovar/recusar; beneficia-se das decisões registradas e do motivo em caso de recusa.

### Telas, módulos, relatórios e navegação

- **Módulo lógico de frontend**
  - Área/módulo de **Antecipação** (Admin) — mesma área de RF-2.

- **Telas impactadas**
  - **Lista global de solicitações (Admin)** (RF-2): sem alteração de layout principal; o detalhe exibido (lateral ou inline) passa a incluir as ações Aprovar e Recusar quando aplicável.
  - **Componente de detalhe da solicitação**: reutilizado na lista global (Admin) e, quando no contexto Admin/Analista, exibe os botões "Aprovar" e "Recusar" apenas se `status === Pending` (Em análise). Recusar exige motivo (campo de texto ou diálogo modal).

- **Navegação**
  - Admin/Analista → Lista global → clica em uma solicitação → detalhe abre (com ou sem painel lateral) → vê Aprovar/Recusar se status = Em análise → escolhe ação → (se Recusar) preenche motivo → confirma → feedback e atualização do estado/detalhe e lista.

### Permissões e segurança

- **Roles `Analista` e `Admin`**
  - Podem **ver** e **usar** as ações Aprovar e Recusar quando a solicitação está em estado Pending.
  - O backend já restringe `POST .../approve` e `POST .../reject` a essas roles; o frontend deve esconder os botões para usuários que não tenham essas roles (ex.: Creator).

- **Role `Creator`**
  - Não deve ver os botões Aprovar e Recusar em nenhum contexto (nem na lista global, à qual não tem acesso conforme RF-2).

- **Comportamento de rede**
  - Chamadas a approve/reject devem usar o token de autenticação existente; em caso de 403, exibir mensagem de permissão insuficiente.

### Fluxos de uso e regras de negócio (UI)

#### Fluxo 1 – Aprovar solicitação

1. Admin ou Analista está na lista global e abre o detalhe de uma solicitação em estado "Em análise" (Pending).
2. O componente de detalhe exibe os botões "Aprovar" e "Recusar".
3. O usuário clica em "Aprovar".
4. (Opcional) O sistema pode exibir um diálogo de confirmação e campo opcional "Observação"; ou enviar direto com observação vazia.
5. O frontend chama `POST /api/v1/anticipations/{id}/approve` com body `{ "observation": "..." }` (opcional).
6. Em sucesso: a UI atualiza o estado da solicitação para "Aprovada", esconde os botões Aprovar/Recusar, exibe mensagem de sucesso e atualiza a lista se estiver visível.
7. Em erro (400, 403, 404, 422, 501): exibir mensagem retornada pelo backend e, quando disponível, código/ID para suporte; não alterar o estado na UI até nova carga.

#### Fluxo 2 – Recusar solicitação (motivo obrigatório)

1. Admin ou Analista abre o detalhe de uma solicitação em estado Pending.
2. Clica em "Recusar".
3. O sistema exibe um campo (inline ou em diálogo) para **motivo da recusa**, obrigatório.
4. O usuário preenche o motivo e confirma.
5. Se o motivo estiver vazio ou só espaços, o frontend não envia a requisição e exibe validação (ex.: "Informe o motivo da recusa").
6. O frontend chama `POST /api/v1/anticipations/{id}/reject` com body `{ "reason": "..." }`.
7. Em sucesso: a UI atualiza o estado para "Recusada", esconde os botões, exibe mensagem de sucesso e atualiza a lista/detalhe.
8. Em erro: mesmo tratamento do fluxo de aprovar.

#### Regras de negócio (UI)

- Botões "Aprovar" e "Recusar" visíveis **apenas** quando:
  - O usuário logado tem role Analista ou Admin, **e**
  - O status da solicitação é Pending (Em análise).
- Recusa sem motivo não deve submeter; validação no frontend antes de chamar a API.
- Após transição bem-sucedida, o detalhe e a lista devem refletir o novo estado (recarregar detalhe ou usar resposta da API para atualizar estado local).

### Critérios de aceitação (testáveis – foco frontend)

- **CA-RF4-1 – Botões Aprovar e Recusar visíveis apenas para Analista/Admin e status Pending**
  - Dado que estou autenticado como Admin (ou Analista) e a solicitação em detalhe está com status Pending,  
  - Quando visualizo o detalhe dessa solicitação na lista global,  
  - Então devo ver os botões "Aprovar" e "Recusar".

- **CA-RF4-2 – Botões ocultos para Creator**
  - Dado que estou autenticado como Creator,  
  - Quando visualizo o detalhe de qualquer solicitação (em contexto onde o detalhe seja acessível),  
  - Então não devo ver os botões Aprovar e Recusar.

- **CA-RF4-3 – Botões ocultos quando status não é Pending**
  - Dado que estou autenticado como Admin e a solicitação em detalhe está Aprovada, Recusada ou Cancelada,  
  - Quando visualizo o detalhe,  
  - Então não devo ver os botões Aprovar e Recusar.

- **CA-RF4-4 – Aprovar envia requisição e atualiza estado**
  - Dado que estou autenticado como Admin, a solicitação está Pending e clico em Aprovar (e confirmo, se houver confirmação),  
  - Quando a API retorna sucesso,  
  - Então o estado exibido deve passar a "Aprovada" e os botões Aprovar/Recusar devem desaparecer; mensagem de sucesso deve ser exibida.

- **CA-RF4-5 – Recusar exige motivo**
  - Dado que estou autenticado como Admin e clico em Recusar,  
  - Quando não preencho o motivo (ou deixo em branco),  
  - Então a requisição não deve ser enviada e deve ser exibida mensagem de validação (ex.: motivo obrigatório).

- **CA-RF4-6 – Recusar com motivo envia e atualiza estado**
  - Dado que estou autenticado como Admin, a solicitação está Pending, preencho o motivo e confirmo,  
  - Quando a API retorna sucesso,  
  - Então o estado exibido deve passar a "Recusada", os botões devem desaparecer e mensagem de sucesso deve ser exibida.

- **CA-RF4-7 – Erro da API exibido ao usuário**
  - Dado que estou autenticado como Admin e aciono Aprovar ou Recusar,  
  - Quando a API retorna erro (400, 403, 404, 422 ou 501),  
  - Então a UI deve exibir mensagem de erro (incluindo texto retornado pelo backend quando seguro) e, se disponível, código/ID para suporte; o estado da solicitação na UI não deve mudar até nova carga.

### Componentes de UI e comportamento

- **Componente de detalhe da solicitação (Admin/Analista)**
  - Área de ações adicionais (abaixo ou ao lado dos dados da solicitação):
    - Botão "Aprovar" (estilo primário ou neutro).
    - Botão "Recusar" (estilo secundário ou destaque).
  - Exibição condicional: apenas quando `canShowApproveReject` (role Analista ou Admin **e** status Pending).
  - Recusar: ao clicar, abrir campo de texto (ou modal) para motivo; botão "Confirmar recusa" só habilitado quando motivo não vazio (trim).

- **Confirmação (opcional)**
  - Aprovar: pode ter diálogo "Confirmar aprovação?" com campo opcional "Observação".
  - Recusar: diálogo ou seção com motivo obrigatório e "Confirmar recusa" / "Cancelar".

- **Feedback**
  - Sucesso: toast ou mensagem inline "Solicitação aprovada." / "Solicitação recusada."
  - Erro: área de alerta com mensagem e código de suporte quando disponível.

### Requisitos técnicos/metodológicos aplicáveis (frontend Angular)

- **Referências de arquitetura Angular**
  - Aplicar a regra `.cursor/rules/angular-frontend.mdc` e a skill `mestre-freire-angular` para camadas e convenções.
- **Camadas**
  - **Domain**: estender `AnticipationRequestsPort` com `approveRequest(id, observation?: string)` e `rejectRequest(id, reason: string)` retornando `Observable<AnticipationRequest>` (ou tipo de resposta alinhado ao backend).
  - **Infrastructure**: implementar approve (POST `.../approve`) e reject (POST `.../reject`) no serviço HTTP que implementa o port; mapear resposta (Id, Protocol, Status) para domínio.
  - **Application**: facade da lista admin (ou do detalhe) expõe métodos `approveRequest` e `rejectRequest`; em sucesso, atualiza estado local (detalhe e/ou lista) e limpa erro; em erro, define mensagem apresentável.
  - **Features**: componente de detalhe recebe input (ex.: `canShowApproveReject: boolean` ou deriva de role + status); emite eventos ou chama facade para aprovar/recusar; formulário de motivo com validação.
- **Metodologias**
  - TDD e spec-driven development; critérios CA-RF4-x e esta seção são fonte para testes unitários, de integração e E2E.
  - Não suprimir erros: em blocos de tratamento de erro, registrar o erro original e exibir mensagem de UI com contexto, motivo da API e código de suporte quando disponível.

### Diretrizes de testes para o frontend

- **Unitários**
  - Port/facade: chamada a approve/reject repassa parâmetros e mapeia resposta/erro.
  - Componente de detalhe: botões Aprovar/Recusar visíveis apenas quando `canShowApproveReject` true; recusa com motivo vazio não emite evento/não chama facade; recusa com motivo preenchido emite/chama com reason.
- **Integração**
  - Serviço HTTP: POST approve e POST reject com body correto; mapeamento da resposta para modelo de domínio.
  - Facade + componente: após approve/reject bem-sucedido, estado do detalhe e da lista atualizados.
- **E2E (opcional)**
  - Admin abre lista global, seleciona solicitação Pending, aprova e vê estado "Aprovada".
  - Admin recusa com motivo e vê estado "Recusada".

### Rastreabilidade

- **Backend**: RA-3 (endpoints `POST /api/v1/anticipations/{id}/approve` e `.../reject`).
- **Frontend**: RF-4 mapeado em `docs/tracability.md` para port, HTTP service, facade(s), componente de detalhe e testes (unit, integração, E2E).
- **Dependências**: RF-2 (lista global e detalhe Admin); RA-3 (contratos e autorização no backend).

### Dependências e riscos

- **Dependências**
  - RF-2 (lista global de solicitações e componente de detalhe).
  - RA-3 (endpoints approve/reject e roles Analista, Admin).
- **Riscos**
  - Nenhum crítico identificado; mitigação de erros de rede já coberta por tratamento de erro e mensagem ao usuário.
