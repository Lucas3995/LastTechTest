## Contrato mínimo de autenticação – Frontend antecipação

Este documento descreve o contrato mínimo esperado entre o **frontend de antecipação** e o **backend de autenticação**, a ser usado como referência única para:

- Serviço de autenticação em `core` (`AuthService`).
- Guards de rota (proteção de `/anticipation/my-requests` e demais áreas logadas).
- Página de login (UX/storytelling).

O contrato é compatível com as demandas de backend de antecipação (especialmente `[demandas/RA-1-criar-solicitacao-antecipacao.md]`), que já assumem autenticação JWT e roles `Creator` e `Admin`.

---

### 1. Endpoint de login

- **Método**: `POST`
- **URL**: `/api/v1/auth/login`
- **Descrição**: autentica um usuário do sistema interno (Creator ou Admin) e retorna um token de acesso (e, opcionalmente, token de refresh) junto com os metadados mínimos do usuário necessários para o frontend.

#### 1.1. Request

Corpo JSON:

```json
{
  "email": "creator@example.com",
  "password": "string-secreta"
}
```

Observações:

- O campo de identificação pode ser `email` ou `username`, conforme ambiente; o frontend deve estar preparado para ambas as convenções de UI, mas o contrato técnico assume **um identificador textual único** mais `password`.
- Senhas **nunca** devem ser logadas em clientes ou servidores.

#### 1.2. Response de sucesso (200)

Corpo JSON:

```json
{
  "accessToken": "jwt-access-token",
  "refreshToken": "jwt-refresh-token-opcional",
  "user": {
    "id": "uuid-ou-id-interno",
    "role": "Creator",
    "creatorId": "uuid-do-creator-ou-null-para-nao-creators"
  }
}
```

Regras:

- `role`:
  - `"Creator"`: usuário que faz solicitações de antecipação sobre **seus próprios recebíveis**.
  - `"Admin"`: usuário administrativo com acesso a todos os endpoints de antecipação (ver RA-1).
  - Outros valores de role podem existir, mas, para o escopo de antecipação, apenas `Creator` e `Admin` são relevantes.
- `creatorId`:
  - Obrigatório (não-nulo) quando `role === "Creator"`.
  - Pode ser `null` ou ausente para roles que não sejam Creator (por exemplo, Admin).
- `accessToken`:
  - JWT assinado, usado no header `Authorization: Bearer <accessToken>` em todas as chamadas subsequentes a `/api/v1/antecipacoes` e outros recursos protegidos.
- `refreshToken`:
  - Opcional neste contrato mínimo. Quando existir, é usado por fluxos futuros de renovação de sessão; o frontend de antecipação, inicialmente, pode apenas armazená-lo sem utilizá-lo ativamente.

---

### 2. Estrutura de erro e rastreabilidade

Todas as respostas de erro de autenticação devem seguir um **payload estruturado**, alinhado com o padrão geral definido em RA-1:

```json
{
  "code": "string-curta",
  "message": "mensagem-de-dominio-segura-para-exibicao",
  "traceId": "id-de-correlaçao-para-logs"
}
```

Regras gerais:

- `code`:
  - Identificador curto e estável para o tipo de erro (ex.: `"AUTH_INVALID_CREDENTIALS"`, `"AUTH_FORBIDDEN_ROLE"`, `"AUTH_TOKEN_EXPIRED"`, `"AUTH_UNEXPECTED_ERROR"`).
  - Deve ser exibido na UI como **código para suporte**.
- `message`:
  - Mensagem de domínio segura, pensada para exibição em UI (sem detalhes sensíveis).
  - Pode ser reaproveitada diretamente pelo frontend em conjunto com o contexto da operação (por exemplo, “Não foi possível entrar agora. Motivo: credenciais inválidas.”).
- `traceId`:
  - Identificador de correlação com logs internos.
  - Exibido na UI junto com o `code` para facilitar atendimento de suporte.

---

### 3. Códigos e cenários de erro esperados

#### 3.1. Credenciais inválidas

- **Status HTTP**: `401 Unauthorized`
- **Body** (exemplo):

```json
{
  "code": "AUTH_INVALID_CREDENTIALS",
  "message": "Credenciais inválidas. Verifique seu email ou senha.",
  "traceId": "c0f4c8f0-1234-5678-9abc-def012345678"
}
```

Semântica:

- Usado quando email/username ou senha estiverem incorretos.
- O frontend deve:
  - Exibir mensagem contextual (“Não foi possível entrar com os dados informados.”).
  - Reforçar a mensagem de domínio (`message`) e exibir `code`/`traceId` como código/ID para suporte.

#### 3.2. Usuário autenticado sem role adequada

- **Status HTTP**: `403 Forbidden`
- **Body** (exemplo):

```json
{
  "code": "AUTH_FORBIDDEN_ROLE",
  "message": "Seu usuário não tem permissão para acessar o módulo de antecipação.",
  "traceId": "0f1e2d3c-4567-890a-bcde-f0123456789a"
}
```

Semântica:

- Ocorre quando as credenciais são válidas, mas o usuário **não é Creator nem Admin** para os contextos de antecipação.
- O frontend deve:
  - Exibir mensagem clara de falta de permissão, sugerindo entrar com outro usuário ou contatar o suporte.
  - Nunca tentar “forçar” acesso ignorando a role.

#### 3.3. Token expirado ou sessão inválida

- **Status HTTP**: `401 Unauthorized`
- **Body** (exemplo):

```json
{
  "code": "AUTH_TOKEN_EXPIRED",
  "message": "Sua sessão expirou. Entre novamente para continuar.",
  "traceId": "abcdef01-2345-6789-abcd-ef0123456789"
}
```

Semântica:

- Usado quando um access token enviado em `Authorization` já não é mais válido (expiração, revogação, etc.).
- O frontend deve:
  - Tratar este caso como necessidade de **login novamente**.
  - Redirecionar o usuário para `/login`, preservando `returnUrl` quando possível.

#### 3.4. Erro interno inesperado

- **Status HTTP**: `500 Internal Server Error` (ou outro 5xx adequado)
- **Body** (exemplo):

```json
{
  "code": "AUTH_UNEXPECTED_ERROR",
  "message": "Ocorreu um erro inesperado ao tentar autenticar. Tente novamente mais tarde.",
  "traceId": "12345678-90ab-cdef-1234-567890abcdef"
}
```

Semântica:

- Problemas não mapeados (falha de infraestrutura, exceções não tratadas, etc.).
- O frontend deve:
  - Manter a mensagem de contexto (“Não foi possível entrar agora.”) e exibir `code`/`traceId` como apoio ao suporte.

---

### 4. Relação com módulos de antecipação (RA-1 e seguintes)

- Para endpoints de antecipação sob `/api/v1/antecipacoes`:
  - O backend usa o `accessToken` (JWT) para:
    - Determinar `userId`, `role` e, quando `role === "Creator"`, o `creatorId`.
    - Restringir operações do Creator **ao seu próprio `creatorId`**, conforme descrito em RA-1.
  - O frontend **não envia** `creatorId` manualmente para criar/listar solicitações do próprio Creator; este dado vem do token.
- Para Admin:
  - O token com role `Admin` pode operar sobre qualquer creator conforme os contratos específicos de cada endpoint (RA-1, RA-3, RC-1).
  - A distinção de permissões finas é responsabilidade dos endpoints de antecipação, mas a autenticação segue o mesmo formato aqui descrito.

---

### 5. Uso esperado no frontend

- `AuthService` (em `core`):
  - Usa `POST /api/v1/auth/login` com payload `{ email, password }`.
  - Ao receber resposta 200:
    - Armazena `accessToken` (e `refreshToken`, se houver) em `localStorage`.
    - Mantém em memória o `user` com `id`, `role`, `creatorId`.
  - Ao receber erros 4xx/5xx:
    - Encapsula o payload `{ code, message, traceId }` em uma estrutura de apresentação (`buildErrorPresentation`), preservando:
      - Contexto da operação (ex.: "tentar entrar").
      - Motivo de negócio (campo `message`).
      - Código/ID de suporte (`code` e/ou `traceId`).
- Guards de rota:
  - Verificam `isAuthenticated` e `role` a partir do estado mantido pelo `AuthService`.
  - Responsáveis por redirecionar para `/login` com `returnUrl` quando necessário.
- Página de login:
  - Exibe mensagens de erro derivadas de `message` + contexto.
  - Sempre mostra, em algum lugar da UI de erro, um **código/ID para suporte** derivado de `code`/`traceId`.

Este contrato deve ser considerado **fonte de verdade** para qualquer evolução do fluxo de autenticação do frontend de antecipação, mantendo rastreabilidade clara entre backend, UI e suporte.

