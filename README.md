# OrderManagement

API para gerenciamento de pedidos, com foco em organização, manutenibilidade e testabilidade.

## Documentacao da API

Esta documentacao descreve o fluxo recomendado para consumir a API, os endpoints disponiveis e os principais contratos de entrada e saida.

## Visao Geral

- Estilo: REST
- Autenticacao: JWT Bearer Token
- Formato de dados: JSON
- Base path: /api

## Fluxo Recomendado de Consumo

1. Autenticar com credenciais validas em POST /api/auth/login.
2. Receber token JWT e data de expiracao.
3. Enviar o token no header Authorization para os endpoints protegidos.
4. Criar pedidos, consultar pedidos e cancelar pedidos conforme regra de negocio.

## Autenticacao

### Endpoint

- Metodo: POST
- Rota: /api/auth/login

### Requisicao

Body JSON esperado:

{
    "username": "dev@martech.com",
    "password": "Senha@123"
}

### Resposta de sucesso (200)

{
    "token": "jwt-token",
    "expiresAt": "2026-08-09T20:00:00Z"
}

### Possiveis erros

- 400 Bad Request: dados invalidos (formato de email, senha vazia, etc.)
- 401 Unauthorized: credenciais invalidas

### Uso do token

Enviar em todos os endpoints protegidos:

Authorization: Bearer SEU_TOKEN_JWT

## Pedidos (Orders)

Todos os endpoints abaixo exigem autenticacao.

### 1) Listar pedidos

- Metodo: GET
- Rota: /api/orders
- Query params:
    - page (opcional, padrao 1)
    - pageSize (opcional, padrao 10)

Resposta de sucesso (200):

{
    "items": [
        {
            "id": "00000000-0000-0000-0000-000000000001",
            "customerId": "00000000-0000-0000-0000-000000000010",
            "status": "Pending",
            "createdAt": "2026-08-09T12:00:00Z",
            "totalAmount": 150.0,
            "items": [
                {
                    "productName": "Produto A",
                    "quantity": 2,
                    "unitPrice": 50.0
                }
            ]
        }
    ],
    "page": 1,
    "pageSize": 10,
    "totalCount": 1
}

### 2) Buscar pedido por ID

- Metodo: GET
- Rota: /api/orders/{id}

Resposta de sucesso (200):

{
    "id": "00000000-0000-0000-0000-000000000001",
    "customerId": "00000000-0000-0000-0000-000000000010",
    "status": "Pending",
    "createdAt": "2026-08-09T12:00:00Z",
    "totalAmount": 150.0,
    "items": [
        {
            "productName": "Produto A",
            "quantity": 2,
            "unitPrice": 50.0
        }
    ]
}

Possiveis erros:

- 404 Not Found: pedido nao encontrado

### 3) Criar pedido

- Metodo: POST
- Rota: /api/orders

Body JSON esperado:

{
    "customerId": "00000000-0000-0000-0000-000000000010",
    "items": [
        {
            "productName": "Produto A",
            "quantity": 2,
            "unitPrice": 50.0
        }
    ]
}

Resposta de sucesso (201 Created):

{
    "id": "00000000-0000-0000-0000-000000000001",
    "customerId": "00000000-0000-0000-0000-000000000010",
    "status": "Pending",
    "createdAt": "2026-08-09T12:00:00Z",
    "totalAmount": 100.0,
    "items": [
        {
            "productName": "Produto A",
            "quantity": 2,
            "unitPrice": 50.0
        }
    ]
}

### 4) Cancelar pedido

- Metodo: PATCH
- Rota: /api/orders/{id}/cancel

Regra de negocio:

- Apenas pedidos com status Pending podem ser cancelados.

Resposta de sucesso:

- 204 No Content

Possiveis erros:

- 400 Bad Request: pedido em status que nao permite cancelamento
- 404 Not Found: pedido nao encontrado

## Padrao de Erros

As respostas de erro seguem formato JSON com mensagem descritiva. Exemplo:

{
    "message": "Order with ID ... not found"
}

## Visão da Arquitetura

A solução segue os princípios da **Clean Architecture**, em que as dependências sempre apontam para as camadas mais internas da aplicação.

- `Domain` não depende de nenhuma outra camada.
- `Application` depende apenas de `Domain`.
- `Infrastructure` depende de `Application` e `Domain`.
- `API` depende de `Application` e `Infrastructure`, sendo a última utilizada para configuração da aplicação (injeção de dependências, banco de dados, logging, etc.).

## Estrutura da Solução

```text
OrderManagement.sln
├── src/
│   ├── API/
│   │   └── OrderManagement.API/                 (ASP.NET Core Web API - .NET 10)
│   ├── Application/
│   │   └── OrderManagement.Application/         (Class Library - .NET 10)
│   ├── Domain/
│   │   └── OrderManagement.Domain/              (Class Library - .NET 10)
│   └── Infrastructure/
│       └── OrderManagement.Infrastructure/      (Class Library - .NET 10)
└── tests/
    ├── OrderManagement.UnitTests/               (xUnit)
    └── OrderManagement.IntegrationTests/        (xUnit + WebApplicationFactory)
```

## Observabilidade

### OpenTelemetry

O projeto implementa **OpenTelemetry** para rastreamento distribuído com export para console:

- Instrumentação automática de requisições HTTP
- Rastreamento de operações de banco de dados (SQLite)
- Spans customizados para operações de negócio
- Tags contextuais para cada operação
- Captura de exceções

### Logging com Serilog

- Logs estruturados
- Output para console e arquivo
- Request/response logging com pipeline behavior
- Tempo de execução de cada comando/query

## Docker

O projeto pode ser executado completamente em containers Docker, facilitando o deployment e garantindo consistência entre ambientes.

### Quick Start

**Windows (PowerShell):**
```powershell
.\docker.ps1 build
.\docker.ps1 up
```

**Linux/macOS:**
```bash
chmod +x docker.sh
./docker.sh build
./docker.sh up
```

### Recursos Docker

- Multi-stage build otimizado
- Health check automático
- Volumes persistentes para dados e logs
- Scripts helper para Windows e Linux
- Configuração via variáveis de ambiente
- Imagens baseadas em .NET 10

### Acesso

Após iniciar os containers:
- **API**: http://localhost:5000
- **Swagger**: http://localhost:5000/swagger
- **Health Check**: http://localhost:5000/health


## Por que Controllers e não Minimal API?

Apesar do .NET 10 oferecer suporte para **Minimal APIs**, este projeto utiliza **Controllers** porque:

- **Organização**: Melhor para projetos com múltiplos endpoints relacionados (AuthController, OrdersController)
- **Clean Architecture**: Alinha perfeitamente com a separação em camadas e CQRS
- **Manutenibilidade**: Código mais explícito, facilita onboarding de novos desenvolvedores
- **Testabilidade**: Injeção de dependências tradicional e mock direto
- **Swagger**: Integração nativa com attributes `[Authorize]`, `[ProducesResponseType]`, etc.
- **Boas práticas**: Estrutura profissional preparada para crescimento futuro

## Projetos

- `OrderManagement.API`
- `OrderManagement.Application`
- `OrderManagement.Domain`
- `OrderManagement.Infrastructure`
- `OrderManagement.UnitTests`
- `OrderManagement.IntegrationTests`
