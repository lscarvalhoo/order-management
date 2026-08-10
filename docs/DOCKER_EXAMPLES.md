# 🎯 Exemplos Práticos de Uso com Docker

Este arquivo contém exemplos práticos e cenários comuns de uso da aplicação rodando em Docker.

## 📦 Cenário 1: Primeira Execução

```powershell
# Windows
.\docker.ps1 build
.\docker.ps1 up

# Aguardar ~30 segundos para inicialização
.\docker.ps1 health

# Acessar Swagger
start http://localhost:5000/swagger
```

```bash
# Linux/macOS
chmod +x docker.sh
./docker.sh build
./docker.sh up

# Aguardar ~30 segundos para inicialização
./docker.sh health

# Acessar Swagger
open http://localhost:5000/swagger  # macOS
xdg-open http://localhost:5000/swagger  # Linux
```

## 🔐 Cenário 2: Autenticação e Primeiro Pedido

### 1. Login

```bash
curl -X POST http://localhost:5000/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{
	"email": "admin@ordermanagement.com",
	"password": "Admin@123"
  }'
```

**Resposta:**
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "expiresAt": "2024-01-15T20:00:00Z"
}
```

### 2. Criar Pedido

```bash
# Salve o token em uma variável
TOKEN="eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."

curl -X POST http://localhost:5000/api/orders \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer $TOKEN" \
  -d '{
	"customerId": "550e8400-e29b-41d4-a716-446655440000",
	"items": [
	  {
		"productName": "Notebook Dell",
		"quantity": 2,
		"unitPrice": 2500.00
	  },
	  {
		"productName": "Mouse Logitech",
		"quantity": 2,
		"unitPrice": 120.50
	  }
	]
  }'
```

### 3. Listar Pedidos

```bash
curl -X GET "http://localhost:5000/api/orders?page=1&pageSize=10" \
  -H "Authorization: Bearer $TOKEN"
```

### 4. Buscar Pedido Específico

```bash
# Substitua {orderId} pelo ID retornado na criação
curl -X GET "http://localhost:5000/api/orders/{orderId}" \
  -H "Authorization: Bearer $TOKEN"
```

### 5. Cancelar Pedido

```bash
curl -X PATCH "http://localhost:5000/api/orders/{orderId}/cancel" \
  -H "Authorization: Bearer $TOKEN"
```

## 📊 Cenário 3: Monitoramento e Logs

### Ver Logs em Tempo Real

```powershell
# Windows - Logs da API
.\docker.ps1 logs-api
```

```bash
# Linux/macOS - Logs da API
./docker.sh logs-api
```

### Ver Logs Específicos

```bash
# Ver apenas erros
docker-compose logs api | grep -i error

# Ver logs das últimas 100 linhas
docker-compose logs --tail=100 api

# Ver logs de um período específico
docker-compose logs --since="2024-01-15T10:00:00" api
```

### OpenTelemetry Traces

Os traces aparecem automaticamente no console. Exemplo de output:

```
Activity.TraceId:            a1b2c3d4e5f6g7h8i9j0k1l2m3n4o5p6
Activity.SpanId:             1a2b3c4d5e6f7a8b
Activity.DisplayName:        POST /api/orders
Activity.Duration:           00:00:00.2456789
Activity.Tags:
	order.customer_id: 550e8400-e29b-41d4-a716-446655440000
	order.items_count: 2
	order.total_amount: 5241.00
```

## 🔄 Cenário 4: Desenvolvimento e Hot-Reload

### Usando docker-compose.dev.yml

```bash
# Iniciar em modo desenvolvimento
docker-compose -f docker-compose.dev.yml up -d

# Os arquivos serão montados e o dotnet watch fará hot-reload
docker-compose -f docker-compose.dev.yml logs -f api
```

### Fazer Alteração no Código

1. Edite um arquivo em `src/`
2. Observe o log do container
3. A aplicação será recompilada automaticamente
4. Teste a mudança imediatamente

## 🛠️ Cenário 5: Troubleshooting

### Container não inicia

```bash
# Ver logs completos
docker-compose logs api

# Ver últimas 50 linhas
docker-compose logs --tail=50 api

# Inspecionar container
docker inspect ordermanagement-api

# Verificar status
docker-compose ps
```

### Porta ocupada

```bash
# Verificar o que está usando a porta 5000
netstat -ano | findstr :5000  # Windows
lsof -i :5000                 # Linux/macOS

# Ou mude a porta no docker-compose.yml:
# ports:
#   - "5001:8080"
```

### Banco corrompido

```bash
# Parar containers
docker-compose down

# Deletar banco
rm -rf data/

# Reiniciar
docker-compose up -d

# O banco será recriado automaticamente
```

### Rebuild completo

```powershell
# Windows
.\docker.ps1 rebuild
```

```bash
# Linux/macOS
./docker.sh rebuild
```

## 🔍 Cenário 6: Debugging

### Entrar no Container

```bash
# Abrir shell interativo
docker-compose exec api /bin/bash

# Ver variáveis de ambiente
docker-compose exec api env

# Ver versão do .NET
docker-compose exec api dotnet --version

# Listar arquivos
docker-compose exec api ls -la /app

# Ver conteúdo do banco
docker-compose exec api ls -lh /app/data/
```

### Inspecionar Arquivos de Log

```bash
# Ver logs do arquivo (dentro do container)
docker-compose exec api tail -f /app/logs/log-20240115.txt

# Copiar logs para o host
docker cp ordermanagement-api:/app/logs/. ./logs-backup/
```

### Testar Health Check Manualmente

```bash
# De dentro do container
docker-compose exec api curl http://localhost:8080/health

# Do host
curl http://localhost:5000/health

# Com verbose
curl -v http://localhost:5000/health
```

## 🚀 Cenário 7: Deploy Simples

### Exportar Imagem

```bash
# Salvar imagem em arquivo
docker save ordermanagement-api:latest -o ordermanagement-api.tar

# Comprimir
gzip ordermanagement-api.tar

# Transferir para servidor
scp ordermanagement-api.tar.gz user@server:/tmp/
```

### Importar e Executar em Outro Servidor

```bash
# No servidor de destino
gunzip ordermanagement-api.tar.gz
docker load -i ordermanagement-api.tar

# Executar
docker-compose up -d
```

## 📈 Cenário 8: Performance e Recursos

### Limitar Recursos

Adicione no `docker-compose.yml`:

```yaml
services:
  api:
	# ... outras configs
	deploy:
	  resources:
		limits:
		  cpus: '1.0'
		  memory: 512M
		reservations:
		  cpus: '0.5'
		  memory: 256M
```

### Monitorar Uso

```bash
# Ver uso em tempo real
docker stats ordermanagement-api

# Ver uso de disco
docker system df

# Ver tamanho da imagem
docker images ordermanagement-api
```

## 🔐 Cenário 9: Segurança

### Usar Secrets (Produção)

Crie um arquivo `secrets/jwt_key.txt`:
```
YourSuperSecretKeyHere32Characters
```

Atualize `docker-compose.yml`:
```yaml
services:
  api:
	secrets:
	  - jwt_key
	environment:
	  - Jwt__Key_File=/run/secrets/jwt_key

secrets:
  jwt_key:
	file: ./secrets/jwt_key.txt
```

### Scan de Vulnerabilidades

```bash
# Escanear imagem
docker scan ordermanagement-api

# Ver detalhes de CVEs
docker scan --severity high ordermanagement-api
```

## 🗂️ Cenário 10: Backup e Restore

### Backup do Banco de Dados

```bash
# Criar backup
docker-compose exec api tar czf /app/backup.tar.gz /app/data/

# Copiar para host
docker cp ordermanagement-api:/app/backup.tar.gz ./backups/backup-$(date +%Y%m%d).tar.gz

# Ou diretamente
tar czf backup-$(date +%Y%m%d).tar.gz data/
```

### Restore do Banco de Dados

```bash
# Parar aplicação
docker-compose down

# Restaurar dados
tar xzf backup-20240115.tar.gz

# Reiniciar
docker-compose up -d
```

## 🌐 Cenário 11: Integração com Outros Serviços

### Adicionar PostgreSQL

Crie `docker-compose.postgres.yml`:

```yaml
version: '3.8'

services:
  api:
	environment:
	  - ConnectionStrings__DefaultConnection=Host=postgres;Database=ordermanagement;Username=postgres;Password=postgres
	depends_on:
	  - postgres

  postgres:
	image: postgres:16
	container_name: ordermanagement-postgres
	environment:
	  - POSTGRES_DB=ordermanagement
	  - POSTGRES_USER=postgres
	  - POSTGRES_PASSWORD=postgres
	volumes:
	  - postgres-data:/var/lib/postgresql/data
	networks:
	  - ordermanagement-network

volumes:
  postgres-data:
```

Executar:
```bash
docker-compose -f docker-compose.yml -f docker-compose.postgres.yml up -d
```

## 📝 Cenário 12: Testes Automatizados

### Executar Testes no Container

```bash
# Build com layer de testes
docker build --target build -t ordermanagement-api:test .

# Executar testes
docker run --rm ordermanagement-api:test \
  dotnet test /src/tests/OrderManagement.UnitTests/OrderManagement.UnitTests.csproj
```

## 🎓 Dicas Úteis

### Aliases Úteis

Adicione ao seu `.bashrc` ou `.zshrc` (Linux/macOS):

```bash
alias dps='docker-compose ps'
alias dlogs='docker-compose logs -f api'
alias dup='docker-compose up -d'
alias ddown='docker-compose down'
alias drestart='docker-compose restart api'
alias dshell='docker-compose exec api /bin/bash'
```

Ou no PowerShell Profile (Windows):

```powershell
function dps { docker-compose ps }
function dlogs { docker-compose logs -f api }
function dup { docker-compose up -d }
function ddown { docker-compose down }
function drestart { docker-compose restart api }
function dshell { docker-compose exec api /bin/bash }
```

### Limpar Recursos Não Utilizados

```bash
# Limpar containers parados
docker container prune -f

# Limpar imagens não utilizadas
docker image prune -a -f

# Limpar volumes órfãos
docker volume prune -f

# Limpar tudo
docker system prune -a --volumes -f
```

## 📚 Comandos de Referência Rápida

```bash
# Build
docker-compose build [--no-cache]

# Start
docker-compose up [-d]

# Stop
docker-compose down [-v]

# Logs
docker-compose logs [-f] [api]

# Status
docker-compose ps

# Restart
docker-compose restart [api]

# Execute command
docker-compose exec api <command>

# Scale (se aplicável)
docker-compose up -d --scale api=3
```

---

Para mais informações, consulte:
- [DOCKER.md](DOCKER.md) - Documentação completa
- [DOCKER_QUICKSTART.md](DOCKER_QUICKSTART.md) - Guia rápido
- [DOCKER_CHECKLIST.md](DOCKER_CHECKLIST.md) - Checklist de implementação
