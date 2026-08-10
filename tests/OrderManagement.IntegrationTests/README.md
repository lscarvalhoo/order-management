# Testes de Integração - Order Management API

## ✅ Testes Implementados

### Arquivos Criados

1. **CustomWebApplicationFactory.cs** - Factory personalizada para testes de integração com:
   - Configuração de banco de dados In-Memory
   - Configuração de credenciais de teste
   - Isolamento por teste com nomes únicos de banco de dados

2. **IntegrationTestBase.cs** - Classe base para testes com:
   - Helper para obter token de autenticação
   - Helper para definir header de autorização
   - Configuração do HttpClient

3. **Controllers/AuthControllerTests.cs** - Testes do endpoint de autenticação:
   - ✅ Login com credenciais válidas retorna OK com token
   - ✅ Login com credenciais inválidas retorna Unauthorized
   - ✅ Login com username vazio retorna BadRequest
   - ✅ Login com password vazio retorna BadRequest

4. **Controllers/OrdersControllerTests.cs** - Testes dos endpoints de pedidos:
   - ✅ GET /api/orders sem autenticação retorna Unauthorized
   - ✅ POST /api/orders com dados inválidos (quantidade zero) retorna BadRequest
   - ✅ POST /api/orders com dados inválidos (preço negativo) retorna BadRequest
   - ✅ POST /api/orders com lista de itens vazia retorna BadRequest
   - ⚠️ GET /api/orders com autenticação retorna OK (falhando)
   - ⚠️ POST /api/orders com dados válidos retorna Created (falhando)
   - ⚠️ GET /api/orders/{id} com pedido existente retorna OK (falhando)
   - ⚠️ GET /api/orders/{id} com pedido inexistente retorna NotFound (falhando)
   - ⚠️ PATCH /api/orders/{id}/cancel com pedido válido retorna NoContent (falhando)
   - ⚠️ Outros testes de cenários de pedidos (falhando)

## ⚠️ Problema Identificado

Alguns testes estão falhando com o erro:
```
System.InvalidOperationException: Services for database providers 'Microsoft.EntityFrameworkCore.Sqlite', 
'Microsoft.EntityFrameworkCore.InMemory' have been registered in the service provider.
```

### Causa
O Entity Framework Core está detectando que ambos os provedores de banco de dados (SQLite e InMemory) estão registrados no mesmo service provider, o que não é permitido.

### Tentativas de Correção
1. ✅ Remoção do registro do DbContext usando `RemoveAll`
2. ✅ Uso de nomes únicos para bancos de dados In-Memory
3. ✅ Configuração do ambiente como "Testing"
4. ⚠️ O problema persiste para testes que fazem queries ao banco

## 🔧 Próximos Passos para Correção

### Opção 1: Refatorar InfrastructureServiceExtensions
Modificar `InfrastructureServiceExtensions.cs` para aceitar um delegate que configure o provider:

```csharp
public static IServiceCollection AddInfrastructureServices(
	this IServiceCollection services,
	IConfiguration configuration,
	Action<DbContextOptionsBuilder>? configureDbContext = null)
{
	if (configureDbContext != null)
	{
		services.AddDbContext<ApplicationDbContext>(configureDbContext);
	}
	else
	{
		services.AddDb Context<ApplicationDbContext>(options =>
			options.UseSqlite(configuration.GetConnectionString("DefaultConnection")));
	}

	services.AddScoped<IOrderRepository, OrderRepository>();
	return services;
}
```

### Opção 2: Usar SQLite In-Memory
Mudar para SQLite em modo In-Memory em vez de EF Core InMemory:

```csharp
services.AddDbContext<ApplicationDbContext>(options =>
{
	var connection = new SqliteConnection("DataSource=:memory:");
	connection.Open();
	options.UseSqlite(connection);
});
```

### Opção 3: Isolar Completamente o DbContext nos Testes
Remover completamente o registro do Infrastructure e registrar apenas o necessário para os testes.

## 📊 Resultados Atuais

- **Total de Testes**: 18
- **Passando**: 10 (55%)
- **Falhando**: 8 (45%)

### Testes Passando
- ✅ Todos os testes de autenticação (4/4)
- ✅ Testes de validação de pedidos (6/10)

### Testes Falhando
- ⚠️ Testes que interagem com o banco de dados (8/10)

## 🚀 Como Executar os Testes

```powershell
# Executar todos os testes de integração
dotnet test tests/OrderManagement.IntegrationTests/OrderManagement.IntegrationTests.csproj

# Executar apenas testes de autenticação
dotnet test --filter "FullyQualifiedName~AuthControllerTests"

# Executar apenas testes de pedidos
dotnet test --filter "FullyQualifiedName~OrdersControllerTests"
```

## 📝 Observações

1. O WebApplicationFactory está configurado corretamente e iniciando a aplicação
2. A autenticação JWT está funcionando perfeitamente nos testes
3. O ValidationBehavior do MediatR está sendo executado corretamente
4. O LoggingBehavior do Serilog está registrando todas as requisições

## 🔍 Recomendação

Recomendo implementar a **Opção 2 (SQLite In-Memory)** pois:
- Mantém compatibilidade com o provedor SQLite já usado no projeto
- Não requer mudanças na camada de Infrastructure
- Permite testar migrations e queries SQL reais
- É mais próximo do comportamento de produção que o EF Core InMemory
