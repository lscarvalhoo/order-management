# Testes de Integração - Order Management API

## Status dos Testes

Todos os testes de integração estão passando: **18/18** 

## Objetivo

Este documento resume a abordagem, a configuração e como executar os testes de integração do projeto.

## Implementação dos Testes

Principais arquivos e responsabilidades:

- `CustomWebApplicationFactory.cs` — Factory personalizada que inicializa o host em ambiente `Testing` e configura um banco SQLite in-memory com conexão mantida aberta durante o ciclo de vida dos testes.
- `IntegrationTestBase.cs` — Classe base que fornece helper para autenticação (obter token) e configuração do `HttpClient` com o header `Authorization`.
- `Controllers/AuthControllerTests.cs` — Casos de teste para o endpoint de autenticação (`/api/auth/login`).
- `Controllers/OrdersControllerTests.cs` — Casos de teste para os endpoints de pedidos (`/api/orders`, `/api/orders/{id}`, `/api/orders/{id}/cancel`).

### Principais cenários cobertos

- Autenticação JWT (login válido/inválido)
- Autorização de endpoints que exigem token
- Validação de entrada (regras do `CreateOrderCommand`)
- Criação de pedidos e retorno do `Created` com Location
- Consulta por ID, paginação e filtros
- Cancelamento de pedido (regras de negócio)
- Tratamento de erros (400, 401, 404)

## Arquitetura dos Testes

### Uso de SQLite in-memory

A suite usa SQLite in-memory em vez do provider `InMemory` do EF Core para:

- Ter comportamento mais próximo do ambiente real (constraints, SQL)
- Permitir verificação de migrations e queries reais
- Melhor desempenho e previsibilidade nos testes

A factory mantém a conexão aberta para preservar o banco durante todo o ciclo de vida da aplicação de testes.

Exemplo (trecho do `CustomWebApplicationFactory`):

```csharp
protected override void ConfigureWebHost(IWebHostBuilder builder)
{
	builder.UseEnvironment("Testing");

	builder.ConfigureAppConfiguration((context, config) =>
	{
		config.AddInMemoryCollection(new Dictionary<string, string?>
		{
			["DevelopmentAuth:FixedUser:Email"] = "dev@martech.com",
			["DevelopmentAuth:FixedUser:Password"] = "Senha@123",
			["DevelopmentAuth:FixedUser:Role"] = "Admin",
			["Jwt:Issuer"] = "OrderManagementAPI",
			["Jwt:Audience"] = "OrderManagementClient"
		});
	});

	builder.ConfigureServices(services =>
	{
		var descriptor = services.SingleOrDefault(
			d => d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>));
		if (descriptor != null) services.Remove(descriptor);

		_connection = new SqliteConnection("DataSource=:memory:");
		_connection.Open();

		services.AddDbContext<ApplicationDbContext>(options =>
		{
			options.UseSqlite(_connection);
		});

		var serviceProvider = services.BuildServiceProvider();
		using var scope = serviceProvider.CreateScope();
		var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
		db.Database.EnsureCreated();
	});
}
```

## Executar os Testes

### Linha de comando

- Executar todos os testes do projeto de integração:

```powershell
dotnet test tests/OrderManagement.IntegrationTests/OrderManagement.IntegrationTests.csproj
```

- Executar com saída detalhada:

```powershell
dotnet test tests/OrderManagement.IntegrationTests/ --logger "console;verbosity=detailed"
```

- Executar testes filtrados (ex.: apenas testes de autenticação):

```powershell
dotnet test --filter "FullyQualifiedName~AuthControllerTests"
```

### Visual Studio

1. Abrir *Test Explorer* (Test > Test Explorer)
2. Selecionar o projeto `OrderManagement.IntegrationTests`
3. Executar todos ou casos específicos

## Credenciais de Teste

Valores em `appsettings.Development.json` usados nos testes:

- Email: `dev@martech.com`
- Senha: `Senha@123`
- Role: `Admin`

## Observações

- A autenticação, validação e comportamentos de negócio são validados pela suite.
- No ambiente `Testing`, a aplicação gera uma chave JWT efêmera em memória quando `Jwt:Key` não é informada. Isso evita versionar segredos no repositório e mantém os testes independentes de credenciais reais.
- Se algum teste falhar localmente, verifique se outro processo não está bloqueando a criação/abertura da conexão SQLite in-memory.
- Para debugging, abra o console de saída do test runner ou execute o teste individual no Visual Studio para ter logs detalhados.
