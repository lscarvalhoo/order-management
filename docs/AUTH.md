# 🔐 Autenticação JWT - Order Management API

## Endpoint de Login

**POST** `/api/auth/login`

Retorna um token JWT válido por 8 horas.

### Credenciais de Desenvolvimento

As credenciais estão configuradas em `appsettings.Development.json`:

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

**Para alterar as credenciais**, edite o arquivo `appsettings.Development.json`.

### Request Example

```bash
curl -X POST "https://localhost:7000/api/auth/login" \
  -H "Content-Type: application/json" \
  -d '{
	"username": "dev@martech.com",
	"password": "Senha@123"
  }'
```

### Response Success (200 OK)

```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "expiresAt": "2024-01-15T18:30:00Z"
}
```

### Response Error (401 Unauthorized)

```json
{
  "message": "Invalid username or password"
}
```

## Como Usar o Token

Após obter o token, inclua-o no header `Authorization` das requisições protegidas:

```bash
curl -X GET "https://localhost:7000/api/orders" \
  -H "Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
```

## Testando no Swagger

1. Acesse `/swagger`
2. Clique em **POST /api/auth/login**
3. Use as credenciais:
   - Username: `dev@martech.com`
   - Password: `Senha@123`
4. Copie o token retornado
5. Clique no botão **Authorize** (cadeado) no topo do Swagger
6. Cole o token no formato: `Bearer {seu-token-aqui}`
7. Agora você pode testar endpoints protegidos

## Claims no Token

O token JWT contém as seguintes claims:

- **Email**: dev@martech.com
- **Name**: dev@martech.com
- **Role**: Admin
- **Jti**: ID único do token
- **Sub**: Subject (email do usuário)

## Configuração JWT (appsettings.json)

```json
{
  "Jwt": {
	"Key": "YourSuperSecretKeyForJWTTokenGenerationWithMinimum32Characters",
	"Issuer": "OrderManagementAPI",
	"Audience": "OrderManagementClient"
  }
}
```

⚠️ **IMPORTANTE**: Em produção, use variáveis de ambiente para a `Jwt:Key`!
