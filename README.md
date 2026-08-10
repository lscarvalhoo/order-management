# OrderManagement

API REST para gerenciamento de pedidos, construída com .NET 10 seguindo os princípios de Clean Architecture, CQRS e boas práticas de engenharia de software.

---

## Sumário

- [Stack](#stack)
- [Arquitetura](#arquitetura)
- [Domínio](#domínio)
- [Como Executar](#como-executar)
- [Banco de Dados](#banco-de-dados)
- [Testes](#testes)
- [Observabilidade](#observabilidade)
- [Análise de Código](#análise-de-código)
- [Documentação da API](#documentação-da-api)

---

## Stack

| Camada | Tecnologia |
|---|---|
| Runtime | .NET 10 |
| API | ASP.NET Core Controllers |
| ORM | Entity Framework Core + SQLite |
| CQRS | MediatR |
| Validação | FluentValidation |
| Autenticação | JWT Bearer |
| Logging | Serilog |
| Rastreamento | OpenTelemetry |
| Testes | xUnit + Moq + FluentAssertions |
| Containers | Docker + Docker Compose |
| Qualidade | SonarQube |

---

## Arquitetura

A solução segue **Clean Architecture** com dependências sempre apontando para camadas internas.

```
OrderManagement.sln
├── src/
│   ├── Domain/           ← entidades, enums, interfaces (sem dependências externas)
│   ├── Application/      ← commands, queries, DTOs, behaviors (depende de Domain)
│   ├── Infrastructure/   ← EF Core, repositórios, migrações (depende de Application)
│   └── API/              ← controllers, middleware, DI (depende de Application e Infrastructure)
└── tests/
    ├── OrderManagement.UnitTests/         ← handlers e validators (xUnit)
    └── OrderManagement.IntegrationTests/  ← endpoints (WebApplicationFactory)
```

### Por que Controllers e não Minimal API?

- Melhor organização para múltiplos endpoints relacionados (`AuthController`, `OrdersController`)
- Alinhamento natural com Clean Architecture e CQRS
- Integração nativa do Swagger com `[Authorize]` e `[ProducesResponseType]`
- Testabilidade direta com injeção de dependências e mocks

---

## Domínio

### Order

| Campo | Tipo | Descrição |
|---|---|---|
| `Id` | `Guid` | Identificador único |
| `CustomerId` | `Guid` | Identificador do cliente |
| `Status` | `enum` | `Pending`, `Confirmed`, `Cancelled` |
| `CreatedAt` | `DateTime` | Data de criação (UTC) |
| `Items` | `List<OrderItem>` | Itens do pedido |
| `TotalAmount` | `decimal` | Calculado no domínio: ∑ UnitPrice × Quantity |

### OrderItem

| Campo | Tipo | Descrição |
|---|---|---|
| `Id` | `Guid` | Identificador único |
| `OrderId` | `Guid` | Referência ao pedido |
| `ProductName` | `string` | Nome do produto |
| `Quantity` | `int` | Quantidade (> 0) |
| `UnitPrice` | `decimal` | Preço unitário (> 0) |

### Regras de Negócio

- Um pedido deve ter **pelo menos 1 item**.
- `UnitPrice` e `Quantity` devem ser **maiores que zero**.
- Apenas pedidos com status **`Pending`** podem ser cancelados.
- `TotalAmount` é calculado na entidade de domínio, não na camada de aplicação.

---

## Como Executar

### Pré-requisitos

- [.NET 10 SDK](https://dotnet.microsoft.com/download)

---

### **Execução Local**

#### **Usando o script auxiliar:**

> **Primeira vez?** Habilite a execução de scripts:
> ```powershell
> Set-ExecutionPolicy -ExecutionPolicy RemoteSigned -Scope CurrentUser
> ```

**Windows:**
```powershell
# Execute a partir da raiz do projeto (order-management/)
cd C:\caminho\para\order-management

# Executar API
.\scripts\run-local.ps1

# Executar com hot reload (auto-restart)
.\scripts\run-local.ps1 watch

# Build da solução
.\scripts\run-local.ps1 build

# Executar todos os testes
.\scripts\run-local.ps1 test

# Ver ajuda
.\scripts\run-local.ps1 help
```

**Linux/macOS:**
```bash
# Execute a partir da raiz do projeto (order-management/)
cd /caminho/para/order-management

# Tornar executável (apenas primeira vez)
chmod +x scripts/run-local.sh

# Executar API
./scripts/run-local.sh

# Hot reload
./scripts/run-local.sh watch

# Build e testes
./scripts/run-local.sh build
./scripts/run-local.sh test
```

#### **Manualmente (tradicional):**
```bash
# 1. Clone o repositório
git clone https://github.com/lscarvalhoo/order-management.git
cd order-management

# 2. Restaure as dependências
dotnet restore

# 3. Execute a API
cd src/API/OrderManagement.API
dotnet run
```

#### **Acesso:**
| Serviço | URL |
|---------|-----|
| **API** | http://localhost:5180 |
| **Swagger UI** | http://localhost:5180/swagger |
| **Health Check** | http://localhost:5180/health |

**Recursos:**
- Migrations aplicadas automaticamente
- Banco de dados SQLite local
- Hot reload habilitado (modo `watch`)
- Logs estruturados com Serilog
- OpenTelemetry configurado
- Health checks integrados

---

## Banco de Dados

O projeto usa **SQLite** com migrations aplicadas automaticamente na inicialização.

| Ambiente | Localização |
|---|---|
| Local | `src/API/OrderManagement.API/ordermanagement.db` |
| Docker | `./data/ordermanagement.db` (volume montado) |

### Schema

```sql
CREATE TABLE Orders (
    Id         TEXT    PRIMARY KEY,
    CustomerId TEXT    NOT NULL,
    Status     INTEGER NOT NULL,
    CreatedAt  TEXT    NOT NULL
);

CREATE TABLE OrderItems (
    Id          TEXT    PRIMARY KEY,
    OrderId     TEXT    NOT NULL,
    ProductName TEXT    NOT NULL,
    Quantity    INTEGER NOT NULL,
    UnitPrice   REAL    NOT NULL,
    FOREIGN KEY (OrderId) REFERENCES Orders(Id) ON DELETE CASCADE
);
```

### Ferramentas para acesso ao banco

- **CLI:** `sqlite3 src/API/OrderManagement.API/ordermanagement.db`
- **GUI:** [DB Browser for SQLite](https://sqlitebrowser.org/dl/)
- **VS Code:** extensão *SQLite Viewer*

---

## Testes

```bash
# Unitários
dotnet test tests/OrderManagement.UnitTests/

# Integração
dotnet test tests/OrderManagement.IntegrationTests/

# Todos
dotnet test
```

---

## Observabilidade

### Serilog

- Logs estruturados em console e arquivo (`logs/log-YYYYMMDD.txt`)
- Pipeline behavior que registra request/response e tempo de execução de cada command/query

### OpenTelemetry

- Instrumentação automática de requisições HTTP
- Rastreamento de operações de banco de dados (SQLite)
- Spans customizados para operações de negócio
- Exportação para console

---

## Análise de Código

O projeto inclui configuração do **SonarQube** para análise de qualidade, cobertura de testes e detecção de vulnerabilidades.

### Iniciar SonarQube

**Windows:**
```powershell
.\scripts\sonar.ps1 start
.\scripts\sonar.ps1 status
```

**Linux/macOS:**
```bash
chmod +x scripts/sonar.sh
./scripts/sonar.sh start
./scripts/sonar.sh status
```

### Executar Análise

1. Acesse http://localhost:9000 (credenciais padrão: `admin` / `admin`)
2. Gere um token em **My Account → Security → Generate Tokens**
3. Execute:

**Windows:**
```powershell
$env:SONAR_TOKEN="seu_token"
.\scripts\sonar.ps1 analyze
```

**Linux/macOS:**
```bash
SONAR_TOKEN=seu_token ./scripts/sonar.sh analyze
```

4. Resultados: http://localhost:9000/dashboard?id=order-management

---

## Documentação da API

### Visão Geral

| Item | Valor |
|---|---|
| Estilo | REST |
| Autenticação | JWT Bearer Token |
| Formato | `application/json` |
| Base path | `/api` |

### Fluxo de Consumo

1. Autenticar via `POST /api/auth/login` para obter o JWT.
2. Incluir o token no header `Authorization: Bearer <token>` em todas as demais requisições.
3. Operar sobre pedidos com os endpoints abaixo.

---

### Endpoints

| Método | Rota | Auth | Descrição |
|---|---|---|---|
| `POST` | `/api/auth/login` | — | Autenticação, retorna JWT |
| `GET` | `/api/orders` | ✓ | Lista pedidos com paginação |
| `GET` | `/api/orders/{id}` | ✓ | Busca pedido por ID |
| `POST` | `/api/orders` | ✓ | Cria um novo pedido |
| `PATCH` | `/api/orders/{id}/cancel` | ✓ | Cancela um pedido |

---

### POST `/api/auth/login`

Autentica o usuário e retorna um token JWT válido por 8 horas.

**Request body**
```json
{
  "username": "dev@martech.com",
  "password": "Senha@123"
}
```

**Responses**

| Status | Descrição |
|---|---|
| `200 OK` | Login realizado com sucesso |
| `400 Bad Request` | Dados inválidos (e-mail malformado, senha vazia, etc.) |
| `401 Unauthorized` | Credenciais incorretas |

**200 – Body**
```json
{
  "token": "eyJhbGci...",
  "expiresAt": "2026-08-10T04:00:00Z"
}
```

---

### GET `/api/orders`

Lista pedidos com paginação.

**Query parameters**

| Parâmetro | Tipo | Obrigatório | Padrão | Descrição |
|---|---|---|---|---|
| `page` | `integer` | não | `1` | Número da página |
| `pageSize` | `integer` | não | `10` | Itens por página |

**Responses**

| Status | Descrição |
|---|---|
| `200 OK` | Lista paginada retornada |
| `401 Unauthorized` | Token ausente ou inválido |

**200 – Body**
```json
{
  "items": [
    {
      "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
      "customerId": "9b3f1c2e-4d5a-4e6b-8c7d-0e1f2a3b4c5d",
      "status": "Pending",
      "createdAt": "2026-08-10T12:00:00Z",
      "totalAmount": 150.00,
      "items": [
        {
          "productName": "Produto A",
          "quantity": 2,
          "unitPrice": 50.00
        },
        {
          "productName": "Produto B",
          "quantity": 1,
          "unitPrice": 50.00
        }
      ]
    }
  ],
  "page": 1,
  "pageSize": 10,
  "totalCount": 1
}
```

---

### GET `/api/orders/{id}`

Retorna os detalhes de um pedido específico.

**Path parameters**

| Parâmetro | Tipo | Descrição |
|---|---|---|
| `id` | `uuid` | Identificador do pedido |

**Responses**

| Status | Descrição |
|---|---|
| `200 OK` | Pedido encontrado |
| `401 Unauthorized` | Token ausente ou inválido |
| `404 Not Found` | Pedido não encontrado |

**200 – Body**
```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "customerId": "9b3f1c2e-4d5a-4e6b-8c7d-0e1f2a3b4c5d",
  "status": "Pending",
  "createdAt": "2026-08-10T12:00:00Z",
  "totalAmount": 100.00,
  "items": [
    {
      "productName": "Produto A",
      "quantity": 2,
      "unitPrice": 50.00
    }
  ]
}
```

---

### POST `/api/orders`

Cria um novo pedido com status inicial `Pending`.

**Request body**
```json
{
  "customerId": "9b3f1c2e-4d5a-4e6b-8c7d-0e1f2a3b4c5d",
  "items": [
    {
      "productName": "Produto A",
      "quantity": 2,
      "unitPrice": 50.00
    }
  ]
}
```

**Responses**

| Status | Descrição |
|---|---|
| `201 Created` | Pedido criado; header `Location` aponta para o recurso |
| `400 Bad Request` | Dados inválidos (sem itens, preço/quantidade ≤ 0, etc.) |
| `401 Unauthorized` | Token ausente ou inválido |

---

### PATCH `/api/orders/{id}/cancel`

Cancela um pedido. Só é permitido para pedidos com status `Pending`.

**Path parameters**

| Parâmetro | Tipo | Descrição |
|---|---|---|
| `id` | `uuid` | Identificador do pedido |

**Responses**

| Status | Descrição |
|---|---|
| `204 No Content` | Pedido cancelado com sucesso |
| `400 Bad Request` | Status atual não permite cancelamento |
| `401 Unauthorized` | Token ausente ou inválido |
| `404 Not Found` | Pedido não encontrado |

---

### Padrão de Erros

Todas as respostas de erro retornam JSON no formato:

```json
{
  "message": "Descrição do erro"
}
```
