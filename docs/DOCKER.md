# Docker Setup - Order Management API

Esta documentação descreve como executar a aplicação Order Management usando Docker e Docker Compose.

## 📋 Pré-requisitos

- [Docker](https://docs.docker.com/get-docker/) (20.10 ou superior)
- [Docker Compose](https://docs.docker.com/compose/install/) (v2.0 ou superior)

### Verificar Instalação

```bash
docker --version
docker-compose --version
```

## 🚀 Quick Start

### Opção 1: Usando Scripts Helper

**No Windows (PowerShell):**
```powershell
.\docker.ps1 build
.\docker.ps1 up
```

**No Linux/macOS:**
```bash
chmod +x docker.sh
./docker.sh build
./docker.sh up
```

### Opção 2: Usando Docker Compose Diretamente

```bash
# Build da imagem
docker-compose build

# Iniciar os serviços
docker-compose up -d

# Ver logs
docker-compose logs -f
```

## 📁 Estrutura de Arquivos Docker

```
order-management/
├── Dockerfile                          # Multi-stage build para a API
├── .dockerignore                       # Arquivos ignorados no build
├── docker-compose.yml                  # Orquestração dos serviços
├── .env.example                        # Exemplo de variáveis de ambiente
├── docker.sh                           # Helper script (Linux/macOS)
├── docker.ps1                          # Helper script (Windows)
├── data/                               # Volume para banco de dados SQLite
└── logs/                               # Volume para logs da aplicação
```

## 🐳 Dockerfile

O Dockerfile utiliza multi-stage build para otimizar o tamanho da imagem:

### Stage 1: Build
- Usa a imagem `mcr.microsoft.com/dotnet/sdk:10.0`
- Restaura dependências
- Compila o projeto em modo Release

### Stage 2: Publish
- Publica o projeto sem AppHost

### Stage 3: Runtime
- Usa a imagem `mcr.microsoft.com/dotnet/aspnet:10.0` (mais leve)
- Copia apenas os arquivos publicados
- Expõe as portas 8080 e 8081
- Configura health check

## 🔧 Configuração

### Variáveis de Ambiente

As seguintes variáveis podem ser configuradas no `docker-compose.yml` ou em um arquivo `.env`:

| Variável | Descrição | Padrão |
|----------|-----------|--------|
| `ASPNETCORE_ENVIRONMENT` | Ambiente de execução | Production |
| `ASPNETCORE_URLS` | URLs que a aplicação escuta | http://+:8080 |
| `ConnectionStrings__DefaultConnection` | String de conexão do banco | Data Source=/app/data/ordermanagement.db |
| `Jwt__Key` | Chave secreta JWT | (definida no compose) |
| `Jwt__Issuer` | Emissor do token JWT | OrderManagementAPI |
| `Jwt__Audience` | Audiência do token JWT | OrderManagementClient |
| `OpenTelemetry__ServiceName` | Nome do serviço para telemetria | OrderManagement.API |
| `OpenTelemetry__ServiceVersion` | Versão do serviço | 1.0.0 |

### Portas

- **5000**: Porta HTTP da API (mapeada para 8080 no container)

### Volumes

- `./data:/app/data`: Persiste o banco de dados SQLite
- `./logs:/app/logs`: Persiste os logs da aplicação

## 📝 Comandos Úteis

### Scripts Helper

#### Windows (PowerShell)

```powershell
# Build da imagem
.\docker.ps1 build

# Iniciar serviços
.\docker.ps1 up

# Parar serviços
.\docker.ps1 down

# Reiniciar serviços
.\docker.ps1 restart

# Ver logs de todos os serviços
.\docker.ps1 logs

# Ver logs apenas da API
.\docker.ps1 logs-api

# Verificar saúde da API
.\docker.ps1 health

# Limpar volumes e dados
.\docker.ps1 clean

# Rebuild completo (clean + build + up)
.\docker.ps1 rebuild

# Abrir shell no container
.\docker.ps1 shell

# Ajuda
.\docker.ps1 help
```

#### Linux/macOS

```bash
# Build da imagem
./docker.sh build

# Iniciar serviços
./docker.sh up

# Parar serviços
./docker.sh down

# Reiniciar serviços
./docker.sh restart

# Ver logs de todos os serviços
./docker.sh logs

# Ver logs apenas da API
./docker.sh logs-api

# Verificar saúde da API
./docker.sh health

# Limpar volumes e dados
./docker.sh clean

# Rebuild completo (clean + build + up)
./docker.sh rebuild

# Abrir shell no container
./docker.sh shell

# Ajuda
./docker.sh help
```

### Docker Compose Direto

```bash
# Build
docker-compose build
docker-compose build --no-cache  # Build sem cache

# Start/Stop
docker-compose up -d              # Iniciar em background
docker-compose down               # Parar e remover containers
docker-compose down -v            # Parar e remover containers + volumes

# Logs
docker-compose logs               # Ver todos os logs
docker-compose logs -f            # Seguir logs em tempo real
docker-compose logs -f api        # Logs apenas da API

# Status
docker-compose ps                 # Ver status dos containers
docker-compose top                # Ver processos em execução

# Restart
docker-compose restart            # Reiniciar todos os serviços
docker-compose restart api        # Reiniciar apenas a API

# Execute comandos
docker-compose exec api bash      # Abrir shell no container da API
docker-compose exec api dotnet --version  # Ver versão do .NET
```

### Docker Direto

```bash
# Ver containers em execução
docker ps

# Ver logs
docker logs ordermanagement-api
docker logs -f ordermanagement-api

# Entrar no container
docker exec -it ordermanagement-api bash

# Inspecionar container
docker inspect ordermanagement-api

# Ver uso de recursos
docker stats ordermanagement-api

# Parar/Iniciar
docker stop ordermanagement-api
docker start ordermanagement-api
docker restart ordermanagement-api

# Remover container
docker rm ordermanagement-api
docker rm -f ordermanagement-api  # Forçar remoção
```

## 🏥 Health Check

A aplicação expõe um endpoint de health check:

```bash
# Via curl
curl http://localhost:5000/health

# Via PowerShell
Invoke-WebRequest -Uri http://localhost:5000/health

# Usando o script helper
.\docker.ps1 health    # Windows
./docker.sh health     # Linux/macOS
```

O Docker também realiza health checks automaticamente:
- **Intervalo**: 30 segundos
- **Timeout**: 3 segundos
- **Retries**: 3 tentativas
- **Start Period**: 10 segundos

## 🌐 Acessando a Aplicação

Após iniciar os serviços:

- **API Base URL**: http://localhost:5000
- **Swagger UI**: http://localhost:5000/swagger
- **Health Check**: http://localhost:5000/health

### Exemplo de Requisição

```bash
# Login
curl -X POST http://localhost:5000/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{
	"email": "admin@ordermanagement.com",
	"password": "Admin@123"
  }'

# Criar Order (com token)
curl -X POST http://localhost:5000/api/orders \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer YOUR_TOKEN_HERE" \
  -d '{
	"customerId": "550e8400-e29b-41d4-a716-446655440000",
	"items": [
	  {
		"productName": "Product A",
		"quantity": 2,
		"unitPrice": 49.90
	  }
	]
  }'
```

## 🔍 Troubleshooting

### Container não inicia

```bash
# Ver logs detalhados
docker-compose logs api

# Verificar se a porta está disponível
netstat -an | grep 5000    # Linux/macOS
netstat -an | findstr 5000 # Windows
```

### Rebuild da imagem

```bash
# Reconstruir sem cache
docker-compose build --no-cache

# Ou usando helper script
.\docker.ps1 rebuild    # Windows
./docker.sh rebuild     # Linux/macOS
```

### Limpar tudo

```bash
# Parar tudo e remover volumes
docker-compose down -v

# Remover imagens órfãs
docker image prune -f

# Remover volumes não utilizados
docker volume prune -f
```

### Erro de permissão (Linux)

```bash
# Dar permissão de execução aos scripts
chmod +x docker.sh

# Executar comandos docker sem sudo (adicionar usuário ao grupo docker)
sudo usermod -aG docker $USER
newgrp docker
```

### Verificar logs de build

```bash
# Build com output detalhado
docker-compose build --progress=plain
```

## 📊 Monitoramento

### Ver Logs em Tempo Real

```bash
docker-compose logs -f api
```

### Ver Uso de Recursos

```bash
docker stats ordermanagement-api
```

### Inspecionar Container

```bash
docker inspect ordermanagement-api
```

## 🔐 Segurança

### Boas Práticas

1. **Nunca commite o arquivo `.env`** com credenciais reais
2. **Use secrets** para ambientes de produção:
   ```yaml
   secrets:
	 jwt_key:
	   file: ./secrets/jwt_key.txt
   ```
3. **Atualize regularmente** as imagens base
4. **Escaneie vulnerabilidades**:
   ```bash
   docker scan ordermanagement-api
   ```

### Mudando a Chave JWT em Produção

Edite o `docker-compose.yml` ou crie um arquivo `.env`:

```env
Jwt__Key=SEU_NOVO_SECRET_AQUI_COM_MINIMO_32_CARACTERES
```

## 🚀 Deploy em Produção

### Considerações

1. **Use um banco de dados externo** (PostgreSQL, SQL Server)
2. **Configure um reverse proxy** (Nginx, Traefik)
3. **Use HTTPS** com certificados válidos
4. **Configure logging externo** (ELK, Seq, Application Insights)
5. **Use orquestração** (Kubernetes, Docker Swarm)
6. **Configure backup** dos volumes de dados
7. **Monitore** com ferramentas como Prometheus/Grafana

### Exemplo com Banco de Dados Externo

```yaml
environment:
  - ConnectionStrings__DefaultConnection=Server=db-server;Database=ordermanagement;User=sa;Password=YourPassword
```

## 📚 Referências

- [Docker Documentation](https://docs.docker.com/)
- [Docker Compose Documentation](https://docs.docker.com/compose/)
- [.NET Docker Images](https://hub.docker.com/_/microsoft-dotnet)
- [Best Practices for .NET Docker Images](https://docs.microsoft.com/en-us/dotnet/core/docker/build-container)
