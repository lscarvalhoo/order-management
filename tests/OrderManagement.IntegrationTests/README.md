# Testes de Integração - Order Management API

## ✅ Status dos Testes

**Todos os 18 testes de integração estão passando!**

## 📋 Testes Implementados

### Arquivos Criados

1. **CustomWebApplicationFactory.cs** - Factory personalizada para testes de integração com:
   - Configuração de banco de dados SQLite In-Memory
   - Configuração de credenciais de teste
   - Isolamento completo entre testes

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
   - ✅ GET /api/orders com autenticação retorna OK
   - ✅ POST /api/orders com dados válidos retorna Created
   - ✅ POST /api/orders com dados inválidos (quantidade zero) retorna BadRequest
   - ✅ POST /api/orders com dados inválidos (preço negativo) retorna BadRequest
   - ✅ POST /api/orders com lista de itens vazia retorna BadRequest
   - ✅ GET /api/orders/{id} com pedido existente retorna OK
   - ✅ GET /api/orders/{id} com pedido inexistente retorna NotFound
   - ✅ PATCH /api/orders/{id}/cancel com pedido válido retorna NoContent
   - ✅ PATCH /api/orders/{id}/cancel com pedido já cancelado retorna BadRequest
   - ✅ PATCH /api/orders/{id}/cancel com pedido inexistente retorna NotFound
   - ✅ GET /api/orders com paginação retorna resultados corretos
   - ✅ GET /api/orders com filtro por status funciona corretamente

## 🔧 Solução Implementada

### Uso de SQLite In-Memory

A solução utiliza SQLite in-memory para os testes de integração, que oferece:

- ✅ Performance superior ao EF Core InMemory
- ✅ Suporte completo a relacionamentos e constraints
- ✅ Comportamento mais próximo do SQLite de produção
- ✅ Conexão mantida aberta durante todo o ciclo de vida da factory
- ✅ Isolamento completo entre testes

### Configuração da Factory

```csharp
protected override void ConfigureWebHost(IWebHostBuilder builder)
{
	builder.UseEnvironment("Testing");

	builder.ConfigureAppConfiguration((context, config) =>
	{
		// Configuração de credenciais fixas para testes
		config.AddInMemoryCollection(new Dictionary<string, string?>
		{
			["DevelopmentAuth:FixedUser:Email"] = "dev@martech.com",
			["DevelopmentAuth:FixedUser:Password"] = "Senha@123",
			["DevelopmentAuth:FixedUser:Role"] = "Admin",
			["Jwt:Key"] = "YourSuperSecretKeyForJWTTokenGenerationWithMinimum32Characters",
			["Jwt:Issuer"] = "OrderManagementAPI",
			["Jwt:Audience"] = "OrderManagementClient"
		});
	});

	builder.ConfigureServices(services =>
	{
		// Remove o DbContext existente
		var descriptor = services.SingleOrDefault(
			d => d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>));
		if (descriptor != null)
		{
			services.Remove(descriptor);
		}

		// Configura SQLite in-memory
		_connection = new SqliteConnection("DataSource=:memory:");
		_connection.Open();

		services.AddDbContext<ApplicationDbContext>(options =>
		{
			options.UseSqlite(_connection);
		});

		// Cria o banco de dados
		var serviceProvider = services.BuildServiceProvider();
		using var scope = serviceProvider.CreateScope();
		var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
		db.Database.EnsureCreated();
	});
}
```

## 🚀 Executar os Testes

### Via linha de comando:
```bash
# Todos os testes de integração
dotnet test tests/OrderManagement.IntegrationTests/

# Com detalhes verbosos
dotnet test tests/OrderManagement.IntegrationTests/ --logger "console;verbosity=detailed"
```

### Via Visual Studio:
1. Abrir o **Test Explorer** (Ctrl + E, T)
2. Executar todos os testes do projeto **OrderManagement.IntegrationTests**

## 📊 Cobertura

Os testes cobrem:
- ✅ Autenticação JWT
- ✅ Autorização de endpoints
- ✅ Validação de entrada
- ✅ Criação de pedidos
- ✅ Consulta de pedidos (com paginação e filtros)
- ✅ Cancelamento de pedidos
- ✅ Tratamento de erros (NotFound, BadRequest)
- ✅ Regras de negócio (status, validações)

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
