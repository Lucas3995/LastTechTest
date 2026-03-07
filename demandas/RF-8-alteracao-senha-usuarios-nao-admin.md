## [RF-8] Tela de alteração de senha para usuários não-admin

### Contexto e objetivo de negócio

O backend já disponibiliza `POST /auth/change-password` para usuário autenticado e já possui testes E2E para o fluxo de troca de senha. Porém, no frontend não existe tela/rota para que usuários finais (`Creator` e `Analista`) alterem a própria senha de forma autônoma.

O objetivo desta demanda é implementar a experiência de alteração de senha no frontend para usuários não-admin, reduzindo dependência de reset operacional e aumentando segurança de acesso no uso diário da plataforma.

Escopo desta demanda:

- Criar tela/fluxo de alteração de senha para `Creator` e `Analista`.
- Reaproveitar endpoint existente (`POST /auth/change-password`) sem alterar contrato.
- Manter endpoint aberto para usuário autenticado no backend, mas sem entrada de UI para `Admin`.

### User stories de frontend

- **US-PW1 – Alterar senha com autonomia**
  - Como **Creator** ou **Analista**, quero **alterar minha própria senha na área logada**, para **manter minha conta segura sem depender de intervenção administrativa**.
- **US-PW2 – Validar nova senha antes de enviar**
  - Como **Creator** ou **Analista**, quero **ver validações claras no formulário** (campos obrigatórios, confirmação, critérios mínimos), para **evitar erro de submissão**.
- **US-PW3 – Receber feedback claro de sucesso/erro**
  - Como **Creator** ou **Analista**, quero **receber confirmação quando a senha for alterada** e **mensagem útil em caso de falha**, para **saber como proceder**.
- **US-PW4 – Não expor fluxo de troca para Admin via menu**
  - Como **produto**, quero **não exibir essa operação no menu do Admin** no escopo atual, para **manter aderência à política “UI para não-admin” definida para este card**.

### Personas / papéis

- **Creator (persona principal)**
  - Usuário recorrente do módulo de antecipação, precisa trocar senha de modo simples e seguro.
- **Analista (persona principal)**
  - Usuário interno de operação, também necessita autonomia para atualização de credenciais.
- **Admin (fora de escopo de UI desta demanda)**
  - Continua com fluxos de gestão/reset já existentes (RF-6), sem tela de troca própria neste card.

### Telas, módulos, relatórios e navegação

- **Módulo lógico de frontend**
  - Área autenticada (`shell`) com novo ponto de navegação para `Creator` e `Analista`.
- **Tela principal deste card**
  - `Tela Alteração de senha`:
    - Formulário com:
      - Senha atual.
      - Nova senha.
      - Confirmar nova senha.
    - Ações:
      - `Salvar nova senha`.
      - `Cancelar/voltar`.
- **Navegação**
  - Usuário logado (`Creator`/`Analista`) acessa a operação a partir de item no menu de conta/perfil.
  - `Admin` não vê item de menu/atalho para essa tela no escopo deste card.

### Permissões e segurança

- **Role `Creator` e `Analista`**
  - Podem acessar a rota de alteração de senha no frontend.
  - Podem executar `POST /auth/change-password` para a própria conta autenticada.
- **Role `Admin`**
  - Endpoint backend permanece tecnicamente acessível por autenticação.
  - UI de troca não é exibida no menu/atalhos de Admin neste card.
- **Requisitos de segurança**
  - Nunca logar senha em frontend.
  - Campos de senha com máscara por padrão.
  - Em erro, exibir apenas mensagem segura do backend + código/ID de suporte quando disponível.

### Fluxos de uso e regras de negócio (UI)

#### Fluxo 1 – Troca de senha bem-sucedida

1. Creator/Analista acessa a tela de alteração de senha.
2. Informa senha atual, nova senha e confirmação.
3. Frontend valida:
  - Campos obrigatórios.
  - Nova senha e confirmação idênticas.
  - Regras mínimas de senha (alinhadas ao backend).
4. Frontend envia `POST /auth/change-password` com `{ CurrentPassword, NewPassword }`.
5. Em sucesso:
  - Exibe mensagem “Senha alterada com sucesso”.
  - Permite seguir no app (ou direciona para novo login, conforme política de sessão definida na implementação).

#### Fluxo 2 – Senha atual inválida

1. Usuário preenche senha atual incorreta e envia.
2. Backend retorna erro de domínio (ex.: senha atual inválida).
3. Frontend exibe mensagem clara com contexto da operação e suporte id quando disponível.
4. Formulário permanece aberto para correção.

#### Fluxo 3 – Validação de confirmação

1. Usuário informa nova senha e confirmação diferentes.
2. Frontend bloqueia envio e apresenta validação inline.

### Critérios de aceitação (testáveis)

- **CA-RF8-1 – Acesso da tela para não-admin**
  - Dado que estou autenticado como `Creator` ou `Analista`,
  - Quando navego pelo menu de conta/perfil,
  - Então devo encontrar acesso à tela de alteração de senha.
- **CA-RF8-2 – Admin sem entrada de UI**
  - Dado que estou autenticado como `Admin`,
  - Quando navego pelo menu da área logada,
  - Então não devo ver entrada de navegação para a tela de alteração de senha deste card.
- **CA-RF8-3 – Validação de campos obrigatórios e confirmação**
  - Dado que estou na tela de alteração de senha,
  - Quando envio formulário com campos vazios ou confirmação diferente da nova senha,
  - Então devo ver mensagens de validação e a requisição não deve ser enviada.
- **CA-RF8-4 – Sucesso na troca**
  - Dado que informo senha atual válida e nova senha válida,
  - Quando envio o formulário,
  - Então a API deve responder com sucesso e a UI deve exibir confirmação de operação concluída.
- **CA-RF8-5 – Erro de senha atual inválida**
  - Dado que informo senha atual inválida,
  - Quando envio o formulário,
  - Então a UI deve exibir mensagem de erro contextualizada e manter o formulário para nova tentativa.
- **CA-RF8-6 – Tratamento de erro com suporte**
  - Dado que ocorre erro retornado pela API (400/403/500),
  - Quando a UI apresenta o erro,
  - Então deve exibir mensagem segura e `code/traceId` (quando disponível) para suporte.

### Componentes de UI e comportamento

- **Página de alteração de senha**
  - Formulário reativo tipado com 3 campos de senha.
  - Botão primário desabilitado durante submissão ou com formulário inválido.
  - Área de feedback para sucesso e erro.
- **Comportamentos de UX**
  - Exibir validação inline por campo.
  - Manter foco/ordem de teclado acessível.
  - Mensagens curtas, sem detalhes técnicos sensíveis.

### Requisitos visuais, UX e acessibilidade (UX/UI)

- Campos com labels explícitos e texto de ajuda para regras mínimas.
- Alertas de erro com `role="alert"` e feedback de sucesso com `role="status"`.
- Contraste e foco visível em todos os elementos interativos.

### Requisitos técnicos/metodológicos aplicáveis (frontend + backend)

- **Frontend Angular**
  - Adicionar rota protegida por `AuthGuard` com `requiredRoles: ['Creator', 'Analista']`.
  - Criar serviço/facade para chamada de `POST /auth/change-password`.
  - Reaproveitar utilitário de apresentação de erro para `contextMessage + detailMessage + supportId`.
- **Backend .NET**
  - Sem mudança de contrato do endpoint.
  - Sem mudança de autorização do endpoint neste card (continua `[Authorize]`).
- **Metodologia**
  - Spec-driven development orientado pelos critérios CA-RF8-x.
  - TDD com cobertura unitária, integração e E2E.

### Mapeamento de impacto (maestro)

- **Frontend**
  - `app.routes.ts`: nova rota de alteração de senha para `Creator` e `Analista`.
  - `shell.component.ts`: novo item de navegação visível somente para `Creator`/`Analista`.
  - `core/application/infrastructure/features`: adição de fluxo completo (port/service/facade/page) para `change-password`.
- **Backend**
  - Reuso de `AuthController.ChangePassword` e `ChangePasswordCommandHandler` já existentes.
  - Sem alterações estruturais obrigatórias neste card.

### Diretrizes de testes

- **Unitários (frontend)**
  - Validação do formulário (obrigatórios, confirmação, estado de submit).
  - Facade/serviço para chamada do endpoint e tratamento de erro.
- **Integração (frontend)**
  - Componente + serviço HTTP para `POST /auth/change-password` com payload correto.
  - Exibição de mensagens de sucesso e erro.
- **E2E (frontend)**
  - Creator altera senha, realiza novo login com sucesso.
  - Analista altera senha, realiza novo login com sucesso.
  - Tentativa com senha atual inválida mostra erro e não conclui operação.
- **E2E (backend já existente como referência)**
  - `ChangePasswordE2ETests` cobre troca de senha e rejeição com senha atual inválida.

### Spec para agentes de IA

- **Seções-chave para implementação**
  - Critérios CA-RF8-x.
  - Mapeamento de impacto (maestro).
  - Diretrizes de testes.
- **Nomenclatura sugerida**
  - Página: `ChangePasswordPage`.
  - Facade: `ChangePasswordFacade`.
  - Serviço HTTP: `AuthChangePasswordService` (ou extensão de service auth existente).

### Dependências e riscos

- **Dependências**
  - Endpoint backend `POST /auth/change-password`.
  - Infra de autenticação já ativa no frontend.
- **Riscos**
  - Divergência entre validação frontend e política real de senha do backend.
  - Mitigação: sempre priorizar mensagem de domínio do backend em rejeições.

### Rastreabilidade

- **Backend**
  - Endpoint: `POST /auth/change-password`.
  - Caso de uso: `ChangePasswordCommand` + `ChangePasswordCommandHandler`.
  - Testes: `ChangePasswordE2ETests`.
- **Frontend**
  - Nova rota/página de alteração de senha.
  - Navegação condicional por role.
  - Testes unitários/integração/E2E para `Creator` e `Analista`.

