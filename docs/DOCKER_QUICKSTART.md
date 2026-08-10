# ðŸ³ Docker Quick Start

Este guia oferece um resumo rÃ¡pido para subir a aplicaÃ§Ã£o usando Docker.

## PrÃ©-requisitos

- Docker 20.10+
- Docker Compose v2.0+

## ðŸš€ Comandos RÃ¡pidos

### Windows (PowerShell)

```powershell
# Build e Start
.\docker.ps1 build
.\docker.ps1 up

# Ver logs
.\docker.ps1 logs-api

# Verificar saÃºde
.\docker.ps1 health

# Parar
.\docker.ps1 down
```

### Linux/macOS

```bash
# Dar permissÃ£o de execuÃ§Ã£o (primeira vez)
chmod +x docker.sh

# Build e Start
./docker.sh build
./docker.sh up

# Ver logs
./docker.sh logs-api

# Verificar saÃºde
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

## ðŸŒ URLs

ApÃ³s iniciar:

- **API**: http://localhost:5000
- **Swagger**: http://localhost:5000/swagger
- **Health**: http://localhost:5000/health

## ðŸ“‚ Volumes

- `./data` - Banco de dados SQLite
- `./logs` - Logs da aplicaÃ§Ã£o

## ðŸ”§ CustomizaÃ§Ã£o

Copie `.env.example` para `.env` e ajuste as variÃ¡veis:

```bash
cp .env.example .env
```

## ðŸ“– DocumentaÃ§Ã£o Completa

Para documentaÃ§Ã£o completa, veja [DOCKER.md](DOCKER.md)

## ðŸ” Troubleshooting

### Porta 5000 em uso

Edite `docker-compose.yml` e mude:
```yaml
ports:
  - "5001:8080"  # Usar porta 5001 ao invÃ©s de 5000
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

## ðŸŽ¯ Testando a API

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


