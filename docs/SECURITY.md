# Arquitetura de SeguranÃ§a - AutenticaÃ§Ã£o

## Estrutura de Credenciais

As credenciais de desenvolvimento foram movidas do cÃ³digo hardcoded para uma estrutura mais segura e configurÃ¡vel.

### Antes
```csharp
// AuthController.cs - Hardcoded
private const string FixedEmail = "dev@martech.com";
private const string FixedPassword = "Senha@123";
```

### Agora
```
Hierarquia de SeguranÃ§a:
â”œâ”€â”€ appsettings.Development.json (ConfiguraÃ§Ã£o)
â”œâ”€â”€ DevelopmentAuthOptions (Classe fortemente tipada)
â”œâ”€â”€ IAuthenticationService (AbstraÃ§Ã£o)
â”œâ”€â”€ DevelopmentAuthenticationService (ImplementaÃ§Ã£o)
â””â”€â”€ AuthController (Apenas usa o serviÃ§o)
```

## Componentes

### 1. **ConfiguraÃ§Ã£o** (`appsettings.Development.json`)
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

**BenefÃ­cios:**
- NÃ£o estÃ¡ no cÃ³digo-fonte
- FÃ¡cil de alterar sem recompilar
- Separado por ambiente (Development, Production)
- Pode ser excluÃ­do do Git

### 2. **Classe de ConfiguraÃ§Ã£o** (`DevelopmentAuthOptions.cs`)
```csharp
public class DevelopmentAuthOptions
{
	public const string SectionName = "DevelopmentAuth";
	public FixedUserCredentials FixedUser { get; set; } = new();
}
```

**BenefÃ­cios:**
- Fortemente tipado (type-safe)
- IntelliSense no Visual Studio
- ValidaÃ§Ã£o em tempo de compilaÃ§Ã£o

### 3. **ServiÃ§o de AutenticaÃ§Ã£o** (`AuthenticationService.cs`)
```csharp
public interface IAuthenticationService
{
	bool ValidateCredentials(string email, string password);
	string GetUserRole(string email);
}
```

**BenefÃ­cios:**
- Separa lÃ³gica de autenticaÃ§Ã£o do controller
- FÃ¡cil de testar (mockable)
- Responsabilidade Ãºnica
- Pode ser substituÃ­do por outras implementaÃ§Ãµes

### 4. **Controller** (`AuthController.cs`)
```csharp
public AuthController(
	IAuthenticationService authenticationService,
	IJwtTokenService jwtTokenService)
{
	// Apenas injeta dependÃªncias
}
```

**BenefÃ­cios:**
- CÃ³digo limpo, sem lÃ³gica de validaÃ§Ã£o
- TestÃ¡vel
- NÃ£o conhece detalhes de implementaÃ§Ã£o

## NÃ­veis de SeguranÃ§a

### Desenvolvimento (Atual)
`appsettings.Development.json`
- Credenciais em arquivo de configuraÃ§Ã£o
- NÃ£o vai para produÃ§Ã£o
- FÃ¡cil de trocar

### ProduÃ§Ã£o (Recomendado)
**OpÃ§Ãµes:**

1. **VariÃ¡veis de Ambiente**
```bash
export DevelopmentAuth__FixedUser__Email=admin@prod.com
export DevelopmentAuth__FixedUser__Password=StrongP@ssw0rd
```

2. **Azure Key Vault**
```csharp
builder.Configuration.AddAzureKeyVault(
	new Uri("https://your-keyvault.vault.azure.net/"),
	new DefaultAzureCredential());
```

3. **User Secrets** (Desenvolvimento Local)
```bash
dotnet user-secrets set "DevelopmentAuth:FixedUser:Email" "dev@local.com"
dotnet user-secrets set "DevelopmentAuth:FixedUser:Password" "LocalP@ss"
```

4. **Banco de Dados** (ProduÃ§Ã£o Real)
```csharp
// Implementar UserService que busca no banco
public class DatabaseAuthenticationService : IAuthenticationService
{
	// ValidaÃ§Ã£o contra banco de dados
}
```

## Como Testar

### Alterar Credenciais

Edite `appsettings.Development.json`:
```json
{
  "DevelopmentAuth": {
	"FixedUser": {
	  "Email": "novo@email.com",
	  "Password": "NovaSenha@123",
	  "Role": "User"
	}
  }
}
```

Reinicie a aplicaÃ§Ã£o. Novas credenciais jÃ¡ estarÃ£o ativas!

### MÃºltiplos UsuÃ¡rios

Para adicionar suporte a mÃºltiplos usuÃ¡rios, altere a configuraÃ§Ã£o:

```json
{
  "DevelopmentAuth": {
	"Users": [
	  {
		"Email": "admin@martech.com",
		"Password": "Admin@123",
		"Role": "Admin"
	  },
	  {
		"Email": "user@martech.com",
		"Password": "User@123",
		"Role": "User"
	  }
	]
  }
}
```

E atualize o serviÃ§o:
```csharp
public class DevelopmentAuthenticationService : IAuthenticationService
{
	private readonly List<FixedUserCredentials> _users;

	public bool ValidateCredentials(string email, string password)
	{
		return _users.Any(u => 
			u.Email == email && u.Password == password);
	}
}
```

## Checklist de SeguranÃ§a

- Credenciais fora do cÃ³digo-fonte
- ConfiguraÃ§Ã£o por ambiente
- ServiÃ§o injetÃ¡vel e testÃ¡vel
- Logging de tentativas de login
- ValidaÃ§Ã£o fortemente tipada
- **TODO**: Criptografia de senha (BCrypt, Argon2)
- **TODO**: Rate limiting para evitar brute force
- **TODO**: Auditoria de logins no banco de dados

## EvoluÃ§Ã£o Futura

### Fase 1 (Atual)
- Credenciais em configuraÃ§Ã£o
- ServiÃ§o de autenticaÃ§Ã£o dedicado
- JWT Token service

### Fase 2 (PrÃ³ximos passos)
- [ ] Hash de senhas com BCrypt
- [ ] MÃºltiplos usuÃ¡rios em configuraÃ§Ã£o
- [ ] Rate limiting (10 tentativas/minuto)

### Fase 3 (ProduÃ§Ã£o)
- [ ] IntegraÃ§Ã£o com banco de dados
- [ ] ASP.NET Core Identity
- [ ] OAuth 2.0 / OpenID Connect
- [ ] Refresh Tokens
- [ ] Two-Factor Authentication (2FA)

## Arquivos Relacionados

- `src/API/OrderManagement.API/appsettings.Development.json`
- `src/API/OrderManagement.API/Configuration/DevelopmentAuthOptions.cs`
- `src/API/OrderManagement.API/Services/AuthenticationService.cs`
- `src/API/OrderManagement.API/Controllers/AuthController.cs`
- `src/API/OrderManagement.API/Extensions/AuthenticationServiceExtensions.cs`

