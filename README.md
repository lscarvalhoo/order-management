# OrderManagement

Backend para gerenciamento de pedidos, com foco em organização, manutenibilidade e testabilidade.

## Visão da Arquitetura

A solução segue os princípios da **Clean Architecture**, em que as dependências sempre apontam para as camadas mais internas da aplicação.

- `Domain` não depende de nenhuma outra camada.
- `Application` depende apenas de `Domain`.
- `Infrastructure` depende de `Application` e `Domain`.
- `API` depende de `Application` e `Infrastructure`, sendo a última utilizada para configuração da aplicação (injeção de dependências, banco de dados, logging, etc.).

## Credenciais

```
Email: dev@martech.com
Senha: Senha@123
```

**Endpoint de autenticação**: `POST /api/auth/login`

## Estrutura da Solução

```text
OrderManagement.sln
├── src/
│   ├── API/
│   │   └── OrderManagement.API/                 (ASP.NET Core Web API - .NET 10)
│   ├── Application/
│   │   └── OrderManagement.Application/         (Class Library - .NET 10)
│   ├── Domain/
│   │   └── OrderManagement.Domain/              (Class Library - .NET 10)
│   └── Infrastructure/
│       └── OrderManagement.Infrastructure/      (Class Library - .NET 10)
└── tests/
    ├── OrderManagement.UnitTests/               (xUnit)
    └── OrderManagement.IntegrationTests/        (xUnit + WebApplicationFactory)
```
## Por que Controllers e não Minimal API?

Apesar do .NET 10 oferecer suporte para **Minimal APIs**, este projeto utiliza **Controllers** porque:

- **Organização**: Melhor para projetos com múltiplos endpoints relacionados (AuthController, OrdersController)
- **Clean Architecture**: Alinha perfeitamente com a separação em camadas e CQRS
- **Manutenibilidade**: Código mais explícito, facilita onboarding de novos desenvolvedores
- **Testabilidade**: Injeção de dependências tradicional e mock direto
- **Swagger**: Integração nativa com attributes `[Authorize]`, `[ProducesResponseType]`, etc.
- **Boas práticas**: Estrutura profissional preparada para crescimento futuro

## Projetos

- `OrderManagement.API`
- `OrderManagement.Application`
- `OrderManagement.Domain`
- `OrderManagement.Infrastructure`
- `OrderManagement.UnitTests`
- `OrderManagement.IntegrationTests`
