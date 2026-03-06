# [RF-6] Gestão de Usuários Administradora (Cadastro, Listagem e Reset de Senha)

### Contexto e objetivo de negócio

Atualmente, o provisionamento de novos usuários e a resolução de bloqueios por esquecimento de senha no LastTechTest dependem de intervenção técnica manual via banco de dados ou execução de comandos de infraestrutura. Isso cria um ponto único de falha: a operação para quando um Analista ou Creator não consegue logar e precisa esperar o time de TI para destravar seu acesso.

O objetivo desta demanda é transformar a área logada do **Admin** em um centro de autonomia operacional através de uma interface de **Gestão de Identidade de Baixa Fricção**. O backend já possui suporte maduro para criação e reset (comandos `AdminCreateUser` e `AdminResetUserPassword`), mas a funcionalidade de **Listagem de Usuários** precisa ser implementada de ponta a ponta (Fullstack). 

Com esta entrega, usuarios **Admin** poderão:
1.  **Visualizar** o ecossistema de usuários ativos e seus papéis.
2.  **Provisionar** novos acessos instantaneamente.
3.  **Resgatar** acessos bloqueados em menos de 10 segundos, eliminando chamados de suporte.

### User stories principais

- **US-G1 – Visualizar ecossistema de usuários**: Como **Admin**, quero **ver uma lista clara de todos os usuários do sistema**, para **entender quem tem acesso à plataforma**.
- **US-G2 – Provisionamento autônomo**: Como **Admin**, quero **cadastrar novos Analistas, Creators ou Admins**, para que **a operação escale sem depender da engenharia**.
- **US-G3 – Resgate de acesso (Reset)**: Como **Admin**, quero **gerar uma nova senha temporária para um usuário**, para **resolver um bloqueio por esquecimento de forma imediata**.
- **US-G4 – Restrição de privilégios**: Como **Usuário Básico**, quero que **minha área de gestão seja restrita**, para **garantir que apenas administradores possam alterar permissões**.

### Personas / papéis afetados

- **Admin**: Precisa de controle total. Ele valoriza interfaces limpas, botões de ação óbvios e feedback visual imediato (Toasts/Snackbars). Seu objetivo principal é "destravar" pessoas.
- **Analistas e Creator**: Os beneficiários do resgate. Precisam voltar a operar rápido para não perder prazos de antecipação.

### Telas, módulos, relatórios e navegação

- **Módulo Superior**: "Administração" (Acessível apenas via Guard de Role Admin).
- **Tela de Gestão (`/admin/users`)**: 
    - **Header**: Título "Gestão de Usuários" + Botão "+ Novo Usuário".
    - **Filtros**: Campo de pesquisa por E-mail (com ícone de busca e debounce).
    - **Tabela (MatTable)**: Colunas `E-mail`, `Papel (Badge Colorida)` e `Ações`.
- **Interações de UX (Storytelling)**:
    - **Cadastro**: Abre um **Drawer Lateral** (MatSidenav) para preenchimento de E-mail e Seleção de Papel (Select). Ao salvar, o usuário é adicionado à lista e uma mensagem de sucesso aparece.
    - **Reset**: Ao clicar no ícone de "Cadeado" na linha, abre um **Diálogo de Confirmação**. Após confirmar, o sistema exibe a senha temporária (**Trocar@123**) em um componente de destaque com botão "Copiar".

### Permissões e segurança

- **Role `Admin`**: Única role com acesso visual ao menu e acesso técnico aos endpoints `/auth/admin/*`.
- **Impedimento de Auto-Reset**: O sistema deve desabilitar o botão de reset na linha do próprio Admin logado para evitar incidentes de lockout.
- **Contrato de API**: Todas as chamadas devem trafegar o JWT no header `Authorization`.

### Fluxos de uso e regras de negócio

#### Fluxo 1: Auditoria e Busca (Listagem)
1.  Admin acessa "Gestão de Usuários".
2.  O sistema executa `GET /auth/admin/users`.
3.  A tabela exibe todos os usuários do Identity integrados aos dados do domínio.

#### Fluxo 2: Provisionamento (Novo Usuário)
1.  Admin clica em "+ Novo Usuário" no header.
2.  Preenche E-mail (validação de formato e unicidade).
3.  Seleciona Papel: `Creator`, `Analista` ou `Admin`.
4.  Sistema chama `POST /auth/admin/users` (Command existente).
5.  A senha inicial é definida como a padrão: **Trocar@123**.

#### Fluxo 3: Resgate (Reset de Senha)
1.  Admin localiza usuário na tabela e clica em "Resetar Senha" -> Confirma no Dialog.
2.  O sistema executa `POST /auth/admin/users/reset-password` (Command existente).
3.  O backend revoga todos os tokens ativos do usuário para forçar o deslogue (lógica já existente).
4.  O Admin comunica a nova senha (**Trocar@123**) ao usuário.

### Critérios de aceitação (testáveis)

- **CA1 – Restrição de Menu**
    - Dado que estou logado como `Creator`,
    - Quando olho para o menu lateral,
    - Então **não** devo visualizar o item "Gestão de Usuários".

- **CA2 – Busca Dinâmica**
    - Dado que existem 50 usuários cadastrados,
    - Quando digito "jose@" no campo de busca,
    - Então a tabela deve filtrar instantaneamente para exibir apenas registros correspondentes.

- **CA3 – Feedback de Unicidade**
    - Dado que o e-mail `admin@teste.com` já existe,
    - Quando tento criar um novo usuário com este mesmo e-mail,
    - Então o Drawer deve permanecer aberto e exibir o erro `400` vindo do backend.

- **CA4 – Fluxo de Reset com Senha Padrão**
    - Dado que confirmei o reset de um usuário,
    - Quando o sistema retorna sucesso,
    - Então deve aparecer um Snackbar: "Senha resetada com sucesso para: Trocar@123".

- **CA5 – Redirecionamento 403**
    - Dado que um não-admin tenta acessar `/admin/users` manualmente,
    - Quando a rota é carregada,
    - Então o `AuthGuard` deve redirecionar para `/home`.

### Requisitos técnicos/metodológicos aplicáveis

#### Backend (C# / MediatR)
- **Queries**: Implementar `ListUsersQuery` + Handler retornando `Id`, `Email` e `Roles`.
- **Repositório**: Adicionar `ListAsync()` ao `IUserRepository`.
- **Controllers**: Expor `GET /auth/admin/users` no `AuthController` com atributo `[Authorize(Roles = KnownRoles.Admin)]`.
- **Existente**: Integrar-se aos comandos de `AdminCreateUser` e `AdminResetUserPassword`.

#### Frontend (Angular 20)
- **Componentes**: Uso de `MatTable`, `MatSidenav`, `MatDialog` e `MatSnackBar`.
- **Arquitetura**: Implementar `AdminService` na camada de `infrastructure` e `AdminFacade` na camada de `application`.
- **Padrão**: Seguir rigorosamente o **Material Design Premium** (Vibrant codes, Glassmorphism em diálogos, etc).

#### Qualidade e Metodologia
- **TDD**: Testes de unidade para o novo `ListUsersQueryHandler`.
- **E2E**: Fluxo automatizado via Playwright: Admin entra, lista usuários, cria um novo e confirma o sucesso.
- **Traceability**: Utilizar `traceId` em todos os retornos de erro conforme padrão `RA-1`.

### Rastreabilidade para código e testes
- **Endpoints**: `GET/POST /auth/admin/users`, `POST /auth/admin/users/reset-password`.
- **Comandos/Queries**: `ListUsersQuery`, `AdminCreateUserCommand`, `AdminResetUserPasswordCommand`.
- **Testes Backend Ref**: `AdminUserManagementE2ETests.cs`, `AdminResetPasswordE2ETests.cs`.

---
> [!IMPORTANT]
> A senha padrão em ambiente de desenvolvimento e para resets operacionais é **Trocar@123**. Esta informação deve ser explícita na UI de confirmação de sucesso para o Administrador.
