# Documentação Técnica — Order Management API

---

## Sumário

- [Autenticação](#autenticação)
- [Segurança](#segurança)
- [Docker](#docker)
- [Observabilidade](#observabilidade)
- [Análise de Código (SonarQube)](#análise-de-código-sonarqube)

---

## Autenticação

### Endpoint de Login

**POST** `/api/auth/login` — retorna um JWT válido por 8 horas.

**Request:**
```bash
curl -X POST "http://localhost:5180/api/auth/login" \
  -H "Content-Type: application/json" \
  -d '{"username": "dev@martech.com", "password": "Senha@123"}'
```

**Response 200:**
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "expiresAt": "2024-01-15T18:30:00Z"
}
```

**Response 401:**
```json
{ "message": "Invalid username or password" }
```

### Credenciais de Desenvolvimento

Definidas em `appsettings.Development.json`:

```json
{
  "DevelopmentAuth": {
    "FixedUser": {
      "Email": "dev@martech.com",
      "Password": "Senha@123",
      "Role": "Admin"
    }
  }
}
```

Para alterar, edite o arquivo e reinicie a aplicação.

### Usando o Token

Inclua o token no header `Authorization` de todas as requisições protegidas:

```bash
curl -X GET "http://localhost:5180/api/orders" \
  -H "Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
```

### Testando no Swagger

1. Acesse `/swagger`
2. Execute **POST /api/auth/login** com as credenciais acima
3. Copie o token retornado
4. Clique em **Authorize** (cadeado) e cole: `Bearer {token}`
5. Todos os endpoints protegidos estarão disponíveis

### Claims do Token

| Claim | Valor |
|---|---|
| `email` / `sub` | dev@martech.com |
| `role` | Admin |
| `jti` | ID único do token |

---

## Segurança

### Arquitetura de Autenticação

As credenciais de desenvolvimento saíram do código-fonte para configuração fortemente tipada:

```
appsettings.Development.json
  └── DevelopmentAuthOptions      (classe tipada)
        └── IAuthenticationService (interface)
              └── DevelopmentAuthenticationService (implementação)
                    └── AuthController (apenas consome)
```

**Benefícios:** sem hardcode no fonte, fácil de trocar por ambiente, testável via mock.

### Fornecer `Jwt__Key` em Produção

Fora de `Development`/`Testing`, a ausência de `Jwt__Key` interrompe a inicialização.

Opções:

1. **Variável de ambiente** (recomendado):
   ```bash
   export Jwt__Key=valor-forte-com-minimo-32-bytes
   ```

2. **Arquivo `build/.env`** (Docker Compose local):
   ```env
   Jwt__Key=gere-um-valor-forte-e-unico-com-pelo-menos-32-bytes
   ```
   Copie `build/.env.example` para `build/.env` e não versione o arquivo.

3. **Azure Key Vault** (produção):
   ```csharp
   builder.Configuration.AddAzureKeyVault(
       new Uri("https://your-keyvault.vault.azure.net/"),
       new DefaultAzureCredential());
   ```

4. **User Secrets** (desenvolvimento local alternativo):
   ```bash
   dotnet user-secrets set "Jwt:Key" "valor-local-seguro"
   ```

### Checklist de Segurança

- [x] Credenciais fora do código-fonte
- [x] Configuração por ambiente (Development / Production)
- [x] Serviço de autenticação injetável e testável
- [x] `Jwt__Key` não versionada no repositório
- [x] Dockerfiles com execução non-root (`appuser`)
- [x] `.dockerignore` bloqueando `.env`, `*.pem`, `*.key`, `*.pfx`, `*.snk`

---

## Docker

### Pré-requisitos

- Docker 20.10+
- Docker Compose v2.0+

### Execução Rápida

**Build e start (scripts helper):**

```powershell
# Windows
.\scripts\docker.ps1 build
.\scripts\docker.ps1 up
```

```bash
# Linux/macOS
chmod +x scripts/docker.sh
./scripts/docker.sh build
./scripts/docker.sh up
```

**Ou direto com compose (a partir da raiz do repositório):**

```bash
docker compose -f build/docker-compose.yml up --build
docker compose -f build/docker-compose.yml down
```

**Ou apenas a API em container isolado:**

```bash
docker build -f build/Dockerfile -t ordermanagement:local .
docker run --rm -p 5180:8080 --env-file build/.env --name ordermanagement ordermanagement:local
```

### URLs de Acesso

| Serviço | URL |
|---|---|
| API | http://localhost:5180 |
| Swagger | http://localhost:5180/swagger |
| Health Check | http://localhost:5180/health |

> Em containers com a configuração padrão do compose, a porta pode ser `5000`.

### Variáveis de Ambiente

| Variável | Descrição | Padrão |
|---|---|---|
| `ASPNETCORE_ENVIRONMENT` | Ambiente de execução | `Production` |
| `ASPNETCORE_URLS` | Porta interna | `http://+:8080` |
| `ConnectionStrings__DefaultConnection` | SQLite path | `/app/data/ordermanagement.db` |
| `Jwt__Key` | Chave secreta JWT (obrigatória fora de Dev/Test) | — |
| `Jwt__Issuer` | Emissor do token | `OrderManagementAPI` |
| `Jwt__Audience` | Audiência do token | `OrderManagementClient` |
| `OpenTelemetry__ServiceName` | Nome para telemetria | `OrderManagement.API` |

### Comandos Úteis

```powershell
# Windows (scripts/docker.ps1)
.\scripts\docker.ps1 up          # Iniciar
.\scripts\docker.ps1 down        # Parar
.\scripts\docker.ps1 restart     # Reiniciar
.\scripts\docker.ps1 logs-api    # Logs da API
.\scripts\docker.ps1 health      # Health check
.\scripts\docker.ps1 rebuild     # Clean + build + up
.\scripts\docker.ps1 shell       # Shell no container
.\scripts\docker.ps1 clean       # Remover volumes e dados
```

```bash
# Linux/macOS (scripts/docker.sh) — mesmos comandos com ./scripts/docker.sh
```

```bash
# Docker Compose direto
docker compose logs -f api
docker compose exec api bash
docker compose down -v            # para e remove volumes
docker compose build --no-cache   # rebuild sem cache
```

### Volumes

| Volume | Conteúdo |
|---|---|
| `./data` | Banco de dados SQLite |
| `./logs` | Logs da aplicação |

### Troubleshooting

**Container não inicia:**
```bash
docker compose logs api
docker compose ps
```

**Porta em uso:**
```bash
netstat -ano | findstr :5180   # Windows
lsof -i :5180                  # Linux/macOS
```

**Rebuild completo:**
```bash
docker compose down -v
docker compose build --no-cache
docker compose up -d
```

**Permissão negada (Linux):**
```bash
sudo usermod -aG docker $USER && newgrp docker
```

### Modo Desenvolvimento (Hot Reload)

```bash
docker compose -f build/docker-compose.dev.yml up -d
docker compose -f build/docker-compose.dev.yml logs -f api
```

Alterações em `src/` são detectadas automaticamente pelo `dotnet watch`.

---

## Observabilidade

### Instrumentação Automática

O projeto usa OpenTelemetry com exportação para console, cobrindo:

- **ASP.NET Core** — tracing de requisições HTTP com enriquecimento de tags e filtragem de rotas irrelevantes (`/swagger/*`, `/_framework/*`)
- **HTTP Client** — chamadas HTTP de saída
- **SQL Client** — queries de banco com captura de SQL
- **Custom spans** — operações de negócio nos handlers

### Configuração

`appsettings.json`:
```json
{
  "OpenTelemetry": {
    "ServiceName": "OrderManagement.API",
    "ServiceVersion": "1.0.0"
  }
}
```

Registro em `Program.cs`:
```csharp
builder.Services.AddOpenTelemetryConfiguration(builder.Configuration);
```

### Tracing Customizado nos Handlers

```csharp
// CreateOrderCommandHandler
using var activity = ApplicationActivitySource.StartActivity("CreateOrder");
activity?.SetTag("order.customer_id", request.CustomerId);
activity?.SetTag("order.items_count", request.Items.Count);
activity?.SetTag("order.total_amount", order.TotalAmount);

// CancelOrderCommandHandler
using var activity = ApplicationActivitySource.StartActivity("CancelOrder");
activity?.SetTag("order.id", request.OrderId);
activity?.SetTag("order.current_status", order.Status.ToString());
```

**Convenção de tags:** lowercase com ponto como separador (`order.customer_id`). Nunca incluir PII.

### Exemplo de Output no Console

```
Activity.TraceId:      8a1d2c3e4f5a6b7c8d9e0f1a2b3c4d5e
Activity.SpanId:       1a2b3c4d5e6f7a8b
Activity.DisplayName:  CreateOrder
Activity.Duration:     00:00:00.0234567
Activity.Tags:
    order.customer_id: 12345678-1234-1234-1234-123456789012
    order.items_count: 2
    order.total_amount: 150.00
```

### Atributos de Recurso

Adicionados automaticamente a todos os traces:

| Atributo | Valor |
|---|---|
| `service.name` | OrderManagement.API |
| `service.version` | 1.0.0 |
| `deployment.environment` | Development / Production |
| `host.name` | Nome da máquina |

### Exportadores Disponíveis

A implementação atual usa `AddConsoleExporter()`. Para trocar, edite `OpenTelemetryExtensions.cs`:

| Exportador | Uso |
|---|---|
| Console | Desenvolvimento local (atual) |
| Jaeger / Zipkin | Visualização de traces distribuídos |
| Azure Monitor | Aplicações hospedadas no Azure |
| OTLP | Protocolo agnóstico de vendor |

---

## Análise de Código (SonarQube)

### Pré-requisitos

- Docker 20.10+ e Docker Compose v2.0+
- 4 GB de RAM disponível
- Token gerado após iniciar o SonarQube

### Início Rápido

```powershell
# 1. Iniciar SonarQube (aguarde ~60s)
.\scripts\sonar.ps1 start

# 2. Verificar status
.\scripts\sonar.ps1 status

# 3. Executar análise padrão
$env:SONAR_TOKEN="seu_token"
.\scripts\sonar.ps1 analyze

# 3b. Executar análise apontando para ambiente de desenvolvimento
# (sobe a API com build/docker-compose.dev.yml e roda scanner)
$env:SONAR_TOKEN="seu_token"
.\scripts\sonar.ps1 analyze-dev

# 4. Ver resultados
start http://localhost:9000/dashboard?id=order-management

# 5. Parar
.\scripts\sonar.ps1 stop
```

```bash
# Linux/macOS — mesmos passos com ./scripts/sonar.sh
SONAR_TOKEN=seu_token ./scripts/sonar.sh analyze
```

**Análise local sem Docker** (mais rápida):
```powershell
dotnet tool install --global dotnet-sonarscanner
$env:SONAR_TOKEN="seu_token"
.\scripts\analyze-local.ps1
```

### Gerando o Token

1. Acesse http://localhost:9000 (login: `admin` / `admin` — troque na primeira vez)
2. Vá em **My Account → Security → Generate Tokens**
3. Nome: `scanner`, Type: **User Token**
4. Copie o token gerado (não será exibido novamente)

**Via API:**
```bash
curl -u admin:sua_senha \
  -X POST "http://localhost:9000/api/user_tokens/generate?name=scanner"
```

### Comandos dos Scripts

| Comando | Descrição |
|---|---|
| `start` | Inicia SonarQube + PostgreSQL |
| `stop` | Para os serviços |
| `analyze` | Executa análise de código |
| `analyze-dev` | Sobe a API de desenvolvimento (`docker-compose.dev.yml`) e executa a análise |
| `status` | Verifica se o SonarQube está pronto |
| `logs` | Exibe logs em tempo real |
| `token` | Instruções para gerar token |
| `clean` | Remove volumes e dados |

### Configuração de Exclusões (`sonar-project.properties`)

```properties
sonar.projectKey=order-management
sonar.sources=src
sonar.tests=tests
sonar.exclusions=**/Migrations/**,**/obj/**,**/bin/**
sonar.coverage.exclusions=**/Program.cs,**/Migrations/**,**/*Tests/**
sonar.cs.opencover.reportsPaths=**/coverage.opencover.xml
```

### Métricas Monitoradas

| Categoria | Métricas |
|---|---|
| Qualidade | Bugs, Vulnerabilities, Code Smells, Security Hotspots |
| Cobertura | Line Coverage, Branch Coverage |
| Duplicação | Duplicated Lines, Duplicated Blocks |
| Complexidade | Cyclomatic, Cognitive |

### CI/CD — GitHub Actions

```yaml
name: SonarQube Analysis
on:
  push:
    branches: [main]
  pull_request:
    branches: [main]

jobs:
  sonarqube:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v3
        with:
          fetch-depth: 0

      - uses: actions/setup-dotnet@v3
        with:
          dotnet-version: '10.0.x'

      - name: Install SonarScanner
        run: dotnet tool install --global dotnet-sonarscanner

      - name: Begin analysis
        run: |
          dotnet sonarscanner begin \
            /k:"order-management" \
            /d:sonar.host.url="${{ secrets.SONAR_HOST_URL }}" \
            /d:sonar.token="${{ secrets.SONAR_TOKEN }}" \
            /d:sonar.cs.opencover.reportsPaths="**/coverage.opencover.xml"

      - run: dotnet build --no-restore

      - name: Run tests with coverage
        run: |
          dotnet test --no-build \
            --collect:"XPlat Code Coverage" \
            -- DataCollectionRunSettings.DataCollectors.DataCollector.Configuration.Format=opencover

      - name: End analysis
        run: dotnet sonarscanner end /d:sonar.token="${{ secrets.SONAR_TOKEN }}"
        env:
          GITHUB_TOKEN: ${{ secrets.GITHUB_TOKEN }}
```

Secrets necessários: `SONAR_HOST_URL` e `SONAR_TOKEN`.

### Troubleshooting

**SonarQube não inicia:**
```bash
.\scripts\sonar.ps1 logs
docker stats ordermanagement-sonarqube   # verificar memória
```

**Erro `vm.max_map_count` (Linux):**
```bash
sudo sysctl -w vm.max_map_count=262144
```

**Token inválido:**
```bash
echo $env:SONAR_TOKEN   # confirmar se está definido
```

**Porta 9000 em uso:** edite `build/docker-compose.sonar.yml` e troque `9000:9000` por `9001:9000`.

---

*Framework: .NET 10 | SonarQube: 10 Community*
