# DocumentaÃ§Ã£o do Order Management API

Esta pasta contÃ©m toda a documentaÃ§Ã£o tÃ©cnica do projeto Order Management API.

## Ãndice de DocumentaÃ§Ã£o

### Docker

- **[DOCKER.md](DOCKER.md)** - DocumentaÃ§Ã£o completa do Docker
  - PrÃ©-requisitos e instalaÃ§Ã£o
  - Estrutura de arquivos
  - ConfiguraÃ§Ã£o detalhada
  - Comandos Ãºteis
  - Troubleshooting
  - Guia de produÃ§Ã£o

- **[DOCKER_QUICKSTART.md](DOCKER_QUICKSTART.md)** - Guia rÃ¡pido de inÃ­cio
  - Comandos essenciais (Windows/Linux)
  - URLs de acesso
  - Troubleshooting comum

- **[DOCKER_EXAMPLES.md](DOCKER_EXAMPLES.md)** - Exemplos prÃ¡ticos
  - 12 cenÃ¡rios de uso
  - Comandos completos
  - Monitoramento e debugging
  - Deploy e backup

- **[DOCKER_CHECKLIST.md](DOCKER_CHECKLIST.md)** - Checklist de implementaÃ§Ã£o
  - Lista de arquivos criados
  - Recursos implementados
  - ValidaÃ§Ã£o de funcionalidades

- **[DOCKER_IMPLEMENTATION_SUMMARY.md](DOCKER_IMPLEMENTATION_SUMMARY.md)** - SumÃ¡rio completo
  - Resumo da implementaÃ§Ã£o
  - EstatÃ­sticas
  - Status final

### AutenticaÃ§Ã£o e SeguranÃ§a

- **[AUTH.md](AUTH.md)** - DocumentaÃ§Ã£o de autenticaÃ§Ã£o
  - JWT Bearer Token
  - Endpoints de autenticaÃ§Ã£o
  - Exemplos de uso

- **[AUTH_IMPLEMENTATION.md](AUTH_IMPLEMENTATION.md)** - Detalhes de implementaÃ§Ã£o
  - ConfiguraÃ§Ã£o JWT
  - ValidaÃ§Ã£o de tokens
  - RenovaÃ§Ã£o de tokens

- **[SECURITY.md](SECURITY.md)** - Guia de seguranÃ§a
  - Boas prÃ¡ticas
  - PolÃ­ticas de seguranÃ§a
  - Vulnerabilidades conhecidas

### Observabilidade

- **[OPENTELEMETRY.md](OPENTELEMETRY.md)** - DocumentaÃ§Ã£o do OpenTelemetry
  - InstrumentaÃ§Ã£o
  - ConfiguraÃ§Ã£o
  - Spans customizados
  - Console export

- **[OPENTELEMETRY_OUTPUT.md](OPENTELEMETRY_OUTPUT.md)** - Exemplos de output
  - Traces de criaÃ§Ã£o de pedidos
  - Traces de cancelamento
  - Hierarquia de spans

### AnÃ¡lise de CÃ³digo

- **[SONARQUBE.md](SONARQUBE.md)** - SonarQube Configuration
  - InstalaÃ§Ã£o e configuraÃ§Ã£o
  - Comandos helper (sonar.sh/sonar.ps1)
  - ExecuÃ§Ã£o de anÃ¡lises
  - MÃ©tricas e relatÃ³rios
  - IntegraÃ§Ã£o CI/CD
  - Troubleshooting

###  Outros

- **[REFACTORING_SUMMARY.md](REFACTORING_SUMMARY.md)** - HistÃ³rico de refatoraÃ§Ãµes
  - MudanÃ§as realizadas
  - Melhorias implementadas
  - LiÃ§Ãµes aprendidas

## ðŸš€ Quick Links

### Para ComeÃ§ar
1. [Docker Quick Start](DOCKER_QUICKSTART.md) - Subir a aplicaÃ§Ã£o em 30 segundos
2. [AutenticaÃ§Ã£o](AUTH.md) - Fazer login e obter token
3. [README Principal](../README.md) - VisÃ£o geral do projeto

### Para Desenvolvedores
1. [Exemplos Docker](DOCKER_EXAMPLES.md) - CenÃ¡rios prÃ¡ticos
2. [OpenTelemetry](OPENTELEMETRY.md) - Rastreamento e observabilidade
3. [SeguranÃ§a](SECURITY.md) - PrÃ¡ticas de seguranÃ§a

### Para DevOps
1. [Guia Docker Completo](DOCKER.md) - Deploy e configuraÃ§Ã£o
2. [Checklist Docker](DOCKER_CHECKLIST.md) - ValidaÃ§Ã£o
3. [ImplementaÃ§Ã£o Docker](DOCKER_IMPLEMENTATION_SUMMARY.md) - SumÃ¡rio tÃ©cnico

## Estrutura da DocumentaÃ§Ã£o

```
docs/
â”œâ”€â”€ README.md                              (este arquivo)
â”œâ”€â”€ DOCKER.md                              (5000+ palavras)
â”œâ”€â”€ DOCKER_QUICKSTART.md                   (guia rÃ¡pido)
â”œâ”€â”€ DOCKER_EXAMPLES.md                     (12 cenÃ¡rios)
â”œâ”€â”€ DOCKER_CHECKLIST.md                    (validaÃ§Ã£o)
â”œâ”€â”€ DOCKER_IMPLEMENTATION_SUMMARY.md       (sumÃ¡rio)
â”œâ”€â”€ AUTH.md                                (autenticaÃ§Ã£o)
â”œâ”€â”€ AUTH_IMPLEMENTATION.md                 (implementaÃ§Ã£o auth)
â”œâ”€â”€ SECURITY.md                            (seguranÃ§a)
â”œâ”€â”€ OPENTELEMETRY.md                       (observabilidade)
â”œâ”€â”€ OPENTELEMETRY_OUTPUT.md                (exemplos traces)
â””â”€â”€ REFACTORING_SUMMARY.md                 (histÃ³rico)
```

## NavegaÃ§Ã£o RÃ¡pida por Tarefa

### Quero executar a aplicaÃ§Ã£o
â†’ [DOCKER_QUICKSTART.md](DOCKER_QUICKSTART.md)

### Quero entender como funciona o Docker
â†’ [DOCKER.md](DOCKER.md)

### Quero ver exemplos prÃ¡ticos
â†’ [DOCKER_EXAMPLES.md](DOCKER_EXAMPLES.md)

### Quero fazer autenticaÃ§Ã£o
â†’ [AUTH.md](AUTH.md)

### Quero entender o OpenTelemetry
â†’ [OPENTELEMETRY.md](OPENTELEMETRY.md)

### Quero validar minha implementaÃ§Ã£o
â†’ [DOCKER_CHECKLIST.md](DOCKER_CHECKLIST.md)

### Quero configuraÃ§Ãµes de seguranÃ§a
â†’ [SECURITY.md](SECURITY.md)

## Contribuindo com a DocumentaÃ§Ã£o

Ao adicionar nova documentaÃ§Ã£o:
1. Coloque arquivos `.md` nesta pasta `docs/`
2. Atualize este `README.md` com o novo arquivo
3. Mantenha o padrÃ£o de nomenclatura
4. Use emojis para facilitar identificaÃ§Ã£o
5. Inclua links entre documentos relacionados

## Links Ãšteis

- [README Principal](../README.md)
- [CÃ³digo Fonte](../src/)
- [Testes](../tests/)
- [Scripts](../scripts/) (docker.sh, docker.ps1, sonar.sh, sonar.ps1)
- [ConfiguraÃ§Ãµes Build](../build/) (Docker Compose, Dockerfiles, SonarQube)

---

**Ãšltima atualizaÃ§Ã£o:** Janeiro 2024  
**VersÃ£o da API:** 1.0.0  
**Framework:** .NET 10


