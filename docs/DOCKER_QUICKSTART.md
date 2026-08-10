# 🐳 Docker Quick Start

Este guia oferece um resumo rápido para subir a aplicação usando Docker.

## Pré-requisitos

- Docker 20.10+
- Docker Compose v2.0+

## 🚀 Comandos Rápidos

### Windows (PowerShell)

```powershell
# Build e Start
.\docker.ps1 build
.\docker.ps1 up

# Ver logs
.\docker.ps1 logs-api

# Verificar saúde
.\docker.ps1 health

# Parar
.\docker.ps1 down
```

### Linux/macOS

```bash
# Dar permissão de execução (primeira vez)
chmod +x docker.sh

# Build e Start
./docker.sh build
./docker.sh up

# Ver logs
./docker.sh logs-api

# Verificar saúde
./docker.sh health

# Parar
./docker.sh down
```

### Docker Compose Direto

```bash
# Build
docker-compose build

# Start (background)
docker-compose up -d

# Logs
docker-compose logs -f api

# Health check
curl http://localhost:5000/health

# Stop
docker-compose down
```

## 🌐 URLs

Após iniciar:

- **API**: http://localhost:5000
- **Swagger**: http://localhost:5000/swagger
- **Health**: http://localhost:5000/health

## 📂 Volumes

- `./data` - Banco de dados SQLite
- `./logs` - Logs da aplicação

## 🔧 Customização

Copie `.env.example` para `.env` e ajuste as variáveis:

```bash
cp .env.example .env
```

## 📖 Documentação Completa

Para documentação completa, veja [DOCKER.md](DOCKER.md)

## 🔍 Troubleshooting

### Porta 5000 em uso

Edite `docker-compose.yml` e mude:
```yaml
ports:
  - "5001:8080"  # Usar porta 5001 ao invés de 5000
```

### Ver logs de erro

```bash
docker-compose logs api
```

### Rebuild completo

```bash
.\docker.ps1 rebuild    # Windows
./docker.sh rebuild     # Linux
```

## 🎯 Testando a API

```bash
# Login
curl -X POST http://localhost:5000/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"admin@ordermanagement.com","password":"Admin@123"}'

# Criar pedido (substitua YOUR_TOKEN_HERE pelo token recebido)
curl -X POST http://localhost:5000/api/orders \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer YOUR_TOKEN_HERE" \
  -d '{"customerId":"550e8400-e29b-41d4-a716-446655440000","items":[{"productName":"Product A","quantity":2,"unitPrice":49.90}]}'
```
