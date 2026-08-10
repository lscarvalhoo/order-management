# OpenTelemetry Console Output Example

## Exemplo de Output ao Criar um Pedido

Quando você faz uma requisição `POST /api/orders`, verá uma saída similar a esta no console:

```
Activity.TraceId:            a1b2c3d4e5f6g7h8i9j0k1l2m3n4o5p6
Activity.SpanId:             1a2b3c4d5e6f7a8b
Activity.TraceFlags:         Recorded
Activity.ActivitySourceName: OpenTelemetry.Instrumentation.AspNetCore
Activity.DisplayName:        POST /api/orders
Activity.Kind:               Server
Activity.StartTime:          2024-01-15T14:30:00.1234567Z
Activity.Duration:           00:00:00.2456789
Activity.Tags:
	http.request.method: POST
	http.request.path: /api/orders
	http.response.status_code: 201
	net.host.name: localhost
	net.host.port: 5000
Resource associated with Activity:
	service.name: OrderManagement.API
	service.version: 1.0.0
	deployment.environment: Development
	host.name: YOUR-MACHINE-NAME
	telemetry.sdk.name: opentelemetry
	telemetry.sdk.language: dotnet
	telemetry.sdk.version: 1.10.0

Activity.TraceId:            a1b2c3d4e5f6g7h8i9j0k1l2m3n4o5p6
Activity.SpanId:             9b8a7c6d5e4f3a2b
Activity.TraceFlags:         Recorded
Activity.ParentSpanId:       1a2b3c4d5e6f7a8b
Activity.ActivitySourceName: OrderManagement.Application
Activity.DisplayName:        CreateOrder
Activity.Kind:               Internal
Activity.StartTime:          2024-01-15T14:30:00.1456789Z
Activity.Duration:           00:00:00.1234567
Activity.Tags:
	order.customer_id: 550e8400-e29b-41d4-a716-446655440000
	order.items_count: 3
	order.id: 660e8400-e29b-41d4-a716-446655440001
	order.total_amount: 275.50
Resource associated with Activity:
	service.name: OrderManagement.API
	service.version: 1.0.0
	deployment.environment: Development

Activity.TraceId:            a1b2c3d4e5f6g7h8i9j0k1l2m3n4o5p6
Activity.SpanId:             8c7b6a5d4e3f2a1b
Activity.TraceFlags:         Recorded
Activity.ParentSpanId:       9b8a7c6d5e4f3a2b
Activity.ActivitySourceName: OpenTelemetry.Instrumentation.SqlClient
Activity.DisplayName:        INSERT Orders
Activity.Kind:               Client
Activity.StartTime:          2024-01-15T14:30:00.1789012Z
Activity.Duration:           00:00:00.0891234
Activity.Tags:
	db.system: sqlite
	db.name: ordermanagement.db
	db.statement: INSERT INTO Orders (Id, CustomerId, Status, CreatedAt) VALUES (?, ?, ?, ?)
	db.connection_string: Data Source=ordermanagement.db
Resource associated with Activity:
	service.name: OrderManagement.API
	service.version: 1.0.0
```

## Exemplo de Output ao Cancelar um Pedido

Para uma requisição `PATCH /api/orders/{id}/cancel`:

```
Activity.TraceId:            b2c3d4e5f6g7h8i9j0k1l2m3n4o5p6q7
Activity.SpanId:             2b3c4d5e6f7a8b9c
Activity.TraceFlags:         Recorded
Activity.ActivitySourceName: OpenTelemetry.Instrumentation.AspNetCore
Activity.DisplayName:        PATCH /api/orders/{id}/cancel
Activity.Kind:               Server
Activity.StartTime:          2024-01-15T14:35:00.1234567Z
Activity.Duration:           00:00:00.1567890
Activity.Tags:
	http.request.method: PATCH
	http.request.path: /api/orders/660e8400-e29b-41d4-a716-446655440001/cancel
	http.response.status_code: 200
Resource associated with Activity:
	service.name: OrderManagement.API
	service.version: 1.0.0

Activity.TraceId:            b2c3d4e5f6g7h8i9j0k1l2m3n4o5p6q7
Activity.SpanId:             3c4d5e6f7a8b9c0d
Activity.TraceFlags:         Recorded
Activity.ParentSpanId:       2b3c4d5e6f7a8b9c
Activity.ActivitySourceName: OrderManagement.Application
Activity.DisplayName:        CancelOrder
Activity.Kind:               Internal
Activity.StartTime:          2024-01-15T14:35:00.1456789Z
Activity.Duration:           00:00:00.1234567
Activity.Tags:
	order.id: 660e8400-e29b-41d4-a716-446655440001
	order.found: True
	order.current_status: Pending
	order.cancellation_allowed: True
	order.new_status: Cancelled
Resource associated with Activity:
	service.name: OrderManagement.API
	service.version: 1.0.0
```

## Entendendo os Campos

- **TraceId**: Identificador único para toda a requisição (mesmo para todos os spans)
- **SpanId**: Identificador único deste span específico
- **ParentSpanId**: ID do span pai (mostra a hierarquia)
- **ActivitySourceName**: Qual instrumentação gerou este span
- **DisplayName**: Nome da operação
- **Kind**: Server (requisição HTTP), Client (chamada externa), Internal (operação interna)
- **Duration**: Tempo de execução
- **Tags**: Metadados customizados sobre a operação

## Hierarquia de Spans

```
┌─────────────────────────────────────────────┐
│  POST /api/orders (Server)                  │  ← HTTP Request
│  Duration: 00:00:00.2456789                 │
│                                             │
│  ┌───────────────────────────────────────┐ │
│  │ CreateOrder (Internal)                │ │  ← Handler
│  │ Duration: 00:00:00.1234567            │ │
│  │                                       │ │
│  │  ┌─────────────────────────────────┐ │ │
│  │  │ INSERT Orders (Client)          │ │ │  ← Database
│  │  │ Duration: 00:00:00.0891234      │ │ │
│  │  └─────────────────────────────────┘ │ │
│  └───────────────────────────────────────┘ │
└─────────────────────────────────────────────┘
```

Cada nível mostra onde o tempo foi gasto, facilitando a identificação de gargalos!
