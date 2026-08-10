# 🔒 Arquitetura de Segurança - Autenticação

## 📊 Estrutura de Credenciais

As credenciais de desenvolvimento foram movidas do código hardcoded para uma estrutura mais segura e configurável.

### Antes ❌
```csharp
// AuthController.cs - Hardcoded
private const string FixedEmail = "dev@martech.com";
private const string FixedPassword = "Senha@123";
```

### Agora ✅
```
📁 Hierarquia de Segurança:
├── appsettings.Development.json (Configuração)
├── DevelopmentAuthOptions (Classe fortemente tipada)
├── IAuthenticationService (Abstração)
├── DevelopmentAuthenticationService (Implementação)
└── AuthController (Apenas usa o serviço)
```

## 🏗️ Componentes

### 1. **Configuração** (`appsettings.Development.json`)
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

**Benefícios:**
- Não está no código-fonte
- Fácil de alterar sem recompilar
- Separado por ambiente (Development, Production)
- Pode ser excluído do Git

### 2. **Classe de Configuração** (`DevelopmentAuthOptions.cs`)
```csharp
public class DevelopmentAuthOptions
{
	public const string SectionName = "DevelopmentAuth";
	public FixedUserCredentials FixedUser { get; set; } = new();
}
```

**Benefícios:**
- Fortemente tipado (type-safe)
- IntelliSense no Visual Studio
- Validação em tempo de compilação

### 3. **Serviço de Autenticação** (`AuthenticationService.cs`)
```csharp
public interface IAuthenticationService
{
	bool ValidateCredentials(string email, string password);
	string GetUserRole(string email);
}
```

**Benefícios:**
- Separa lógica de autenticação do controller
- Fácil de testar (mockable)
- Responsabilidade única
- Pode ser substituído por outras implementações

### 4. **Controller** (`AuthController.cs`)
```csharp
public AuthController(
	IAuthenticationService authenticationService,
	IJwtTokenService jwtTokenService)
{
	// Apenas injeta dependências
}
```

**Benefícios:**
- Código limpo, sem lógica de validação
- Testável
- Não conhece detalhes de implementação

## 🔐 Níveis de Segurança

### Desenvolvimento (Atual)
✅ `appsettings.Development.json`
- Credenciais em arquivo de configuração
- Não vai para produção
- Fácil de trocar

### Produção (Recomendado)
🚀 **Opções:**

1. **Variáveis de Ambiente**
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

4. **Banco de Dados** (Produção Real)
```csharp
// Implementar UserService que busca no banco
public class DatabaseAuthenticationService : IAuthenticationService
{
	// Validação contra banco de dados
}
```

## 🧪 Como Testar

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

Reinicie a aplicação. Novas credenciais já estarão ativas!

### Múltiplos Usuários

Para adicionar suporte a múltiplos usuários, altere a configuração:

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

E atualize o serviço:
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

## 📋 Checklist de Segurança

- ✅ Credenciais fora do código-fonte
- ✅ Configuração por ambiente
- ✅ Serviço injetável e testável
- ✅ Logging de tentativas de login
- ✅ Validação fortemente tipada
- ⚠️ **TODO**: Criptografia de senha (BCrypt, Argon2)
- ⚠️ **TODO**: Rate limiting para evitar brute force
- ⚠️ **TODO**: Auditoria de logins no banco de dados

## 🚀 Evolução Futura

### Fase 1 (Atual) ✅
- Credenciais em configuração
- Serviço de autenticação dedicado
- JWT Token service

### Fase 2 (Próximos passos)
- [ ] Hash de senhas com BCrypt
- [ ] Múltiplos usuários em configuração
- [ ] Rate limiting (10 tentativas/minuto)

### Fase 3 (Produção)
- [ ] Integração com banco de dados
- [ ] ASP.NET Core Identity
- [ ] OAuth 2.0 / OpenID Connect
- [ ] Refresh Tokens
- [ ] Two-Factor Authentication (2FA)

## 🔗 Arquivos Relacionados

- `src/API/OrderManagement.API/appsettings.Development.json`
- `src/API/OrderManagement.API/Configuration/DevelopmentAuthOptions.cs`
- `src/API/OrderManagement.API/Services/AuthenticationService.cs`
- `src/API/OrderManagement.API/Controllers/AuthController.cs`
- `src/API/OrderManagement.API/Extensions/AuthenticationServiceExtensions.cs`
