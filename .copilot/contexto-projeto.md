Desafio Técnico – LastLink
🎯 Objetivo
Criar uma API simples para gestão de solicitações de antecipação de valores,
com foco em clareza de código, boas práticas de engenharia, testes
automatizados e estrutura pensada para evoluir.
📚 Contexto
A LastLink permite que criadores recebam suas receitas por meio da
plataforma. Para ajudar no fluxo de caixa, disponibilizamos a opção de
antecipação: o criador pode solicitar que parte de seus recebíveis futuros seja
liberada antes do prazo, mediante uma taxa.
Você foi convidado a construir um serviço de antecipação de valores. Esse
serviço será consumido por um sistema interno e precisa expor uma API REST
para gerenciar as solicitações.
🧱 Requisitos
📌 Funcionalidades da API
Criar uma solicitação de antecipação
Informar: creator_id, valor_solicitado, data_solicitacao
Aplicar taxa de 5% sobre o valor solicitado
Calcular e retornar: valor_liquido, status (default = “pendente”)
Listar solicitações por creator_id
Aprovar ou recusar uma solicitação
Atualizar status: “aprovada” ou “recusada”
(Opcional) Expor endpoint para simulação sem criar solicitação (GET com
query params)
🔎 Regras de negócio
Valor solicitado deve ser maior que R$ 100,00
Um creator não pode ter mais de uma solicitação pendente ao mesmo tempo
A taxa de antecipação é fixa: 5% sobre o valor bruto
Desafio Técnico – LastLink 1
Toda solicitação inicia com status pendentef
🧪 O que esperamos ver
Código claro, organizado, coeso
Testes automatizados (unitários ou de integração)
Modelagem bem pensada (domínio separado de controller, por exemplo)
Uso de versionamento de API (v1)
Um README explicando como rodar localmente (sem dor)
(Opcional) 🌐 Frontend
Se quiser mostrar skills fullstack, você pode entregar uma tela simples em
Angular ou React que:
Mostre uma lista de solicitações existentes
Permita criar uma nova solicitação com formulário
Permita aprovar/reprovar
🧰 Stack sugerida
Backend: C# / .NET Core (ou stack à sua escolha)
Banco de dados: em memória (ex: SQLite ou mock em memória)
Testes: framework da sua stack preferida
🚀 Entrega
Suba em um repositório no GitHub/GitLab ou envie um .zip
README com instruções de execução e testes
Caso use Postman, inclua uma collection
Prazo sugerido: 5 dias úteis
Desafio Técnico – LastLink 2acessibilidade - é comum que ao criar sites e sistemas em spa as pessoas esqueçam da semantica de elementos em tela e otimizar eles para acessibilidade e inclusive para ferramentas de leitura de tela ou para pessoas com deficiencia visual. isso é um erro, uma UI rebusta deve considerar esse aspecto bem como a excelencia em usabilidade, UX/UI e design
Diretivas allow developers to extend HTML by attaching custom behavior or transforming elements 
Signals are best for synchronous, fine-grained state management in the UI, offering better performance and simpler syntax
RxJS/Observables remain essential for handling complex asynchronous data streams, such as API calls, WebSockets, and event handling, using a rich set of operators
ciclo de vida de componentes, diretivas e services é uma preocupação com a qual precisamos sempre prestar atenção quando arquitetamos e criamos
typed forms são um modo muito interessante de assegurar a funcionalidade de formularios e deixar os formularios de forma tipada

estrutura modular: uma aplicação angular é feita de areas, modulos, paginas, componentes, diretivas e serviços
uma diretiva serve para extender o html adicionando comportamentos customizados ou transformando elementos
componentes são partes de código possiveis de serem utilizadas para montar as paginas, podem ser conjuntos de elementos, elementos unicos com comportamentos extendidos e/ou abstrações reutilizaveis
paginas são conjuntos de componentes agrupados para permitir um usuario ter acesso a uma ação, conjunto de ações, informação ou conjunto de informações. uma pagina é quase que um story telling em formato de UX/UI, guiando o usuario de maneira assertiva, informativa e orientadora no processo em que ele esta atuando nela. uma pagina tem um conjunto de componentes que podem não ser exclusivamente dele assim como pode ter elementos html "padroes"
modulos são são o menor unidade de software implantavel e desenvolvivel de forma independente. um módulo permite ao um usuario ter acesso ao conjunto coeso e interligado de ações para que ele possa as encontrar de forma pratica e centralizda e informativa sobre elas. um modulo tem um conjunto de paginas que podem não ser exclusivamente dele assim como pode ter paginas proprias e exclusivas que mais nenhum outro modulo tem
areas estão intimamente ligadas a parte de permissao de roles de usuario. são conjuntos de modulos, que geram uma visão de sistema e podem por vezes ter seu visual/UI completamente diferente uma das outras

um modulo frontend deve funcionar de forma independente do backend exceto nos momentos onde precisar fazer solicitações a ele.
isso cria necessidade de todo frontend robusto ter um módulo "central de ações de servidor" que deve gerenciar todas as requisições feitas para os backends que eventualmente ele use.
isso significa que se voce solitar um processo de backend, isso sera tratado como um design pattern command e sera incluido em uma central acessivel do menu superior, onde o usuario podera ver o processo solicitado e quando ele finalizar é ali onde ele poderá continuar com o feedback dele
um frontend nao deve ter suas paginas quebradas por falta de comunicação com o backend, nesses casos o que deve acontecer é o usuario ser avisado sobre o ocorrido e ter sua requisição salva em formato command para que ele possa tentar novamente mais tarde.
isso é um padrao que tem em sua intrincidade a assincronicidade e desacoplamento entre front e backend bem como trata requisições ao backend como algo não garantido e com efeitos provenientes de um ambiente ao qual o frontend nao tem controle e deve ser tratado como tal
