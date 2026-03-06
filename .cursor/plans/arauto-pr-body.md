## O que foi feito

- Autenticação: login com JWT (POST /api/auth/login), comando de reset de senha por admin e testes (E2E e unitários).
- Frontend: fluxo de login, guards e interceptor de token; página "Minhas solicitações de antecipação" (RF1) com listagem, filtros, detalhe e empty state.
- Configuração: proxy, variáveis de ambiente, Dockerfile e nginx para desenvolvimento e produção.
- Atualização das demandas (RA-1, RF-1 a RF-3) e documentação (contrato de auth, planos e árvore de testes).

## Impacto funcional

- Utilizadores podem autenticar-se e aceder a rotas protegidas.
- Administradores podem redefinir a senha de utilizadores.
- Utilizadores podem ver e filtrar as suas solicitações de antecipação numa página dedicada.

## Impacto técnico

- Backend: AuthController, Program.cs, CQRS AdminResetUserPassword (command, handler, validator); testes unitários e E2E.
- Frontend: camadas core (auth), application (anticipation facade), infrastructure (HTTP), features (auth login, anticipation my-requests); app.config, routes, shell.
- Infra: frontend Dockerfile, angular.json, nginx.conf, proxy.conf.json, environments.

## Como testar

1. Subir backend e frontend (ex.: Docker Compose ou local).
2. Aceder à aplicação e fazer login com credenciais válidas.
3. Navegar para "Minhas solicitações" e validar listagem, filtros e detalhe.
4. (Opcional) Executar testes backend e frontend e E2E.
