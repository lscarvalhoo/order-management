# Documentação do Order Management API

Esta pasta contém a documentação técnica do projeto.

## Índice

### Docker

- **[DOCKER.md](DOCKER.md)** - Documentação completa do Docker
  - Pré-requisitos e instalação, estrutura de arquivos, configuração, comandos, troubleshooting e guia de produção

- **[DOCKER_QUICKSTART.md](DOCKER_QUICKSTART.md)** - Guia rápido
  - Comandos essenciais (Windows/Linux), URLs de acesso, troubleshooting comum

- **[DOCKER_EXAMPLES.md](DOCKER_EXAMPLES.md)** - Exemplos práticos
  - 12 cenários de uso, monitoramento, debugging, deploy e backup

### Autenticação e Segurança

- **[AUTH.md](AUTH.md)** - Documentação de autenticação
  - JWT Bearer Token, endpoints de autenticação, exemplos de uso

- **[SECURITY.md](SECURITY.md)** - Guia de segurança
  - Boas práticas, políticas de segurança

### Observabilidade

- **[OPENTELEMETRY.md](OPENTELEMETRY.md)** - Documentação do OpenTelemetry
  - Instrumentação, configuração, spans customizados, console export

### Análise de Código

- **[SONARQUBE.md](SONARQUBE.md)** - SonarQube
  - Instalação e configuração, comandos helper, execução de análises, métricas, CI/CD

## Quick Links

### Para Começar
1. [Docker Quick Start](DOCKER_QUICKSTART.md) - Subir a aplicação rapidamente
2. [Autenticação](AUTH.md) - Fazer login e obter token
3. [README Principal](../README.md) - Visão geral do projeto

### Para Desenvolvedores
1. [Exemplos Docker](DOCKER_EXAMPLES.md) - Cenários práticos
2. [OpenTelemetry](OPENTELEMETRY.md) - Rastreamento e observabilidade
3. [Segurança](SECURITY.md) - Práticas de segurança

### Para DevOps
1. [Guia Docker Completo](DOCKER.md) - Deploy e configuração
2. [SonarQube](SONARQUBE.md) - Análise de qualidade

## Estrutura

```
docs/
├── README.md
├── DOCKER.md
├── DOCKER_QUICKSTART.md
├── DOCKER_EXAMPLES.md
├── AUTH.md
├── auth-requests.http
├── SECURITY.md
├── OPENTELEMETRY.md
└── SONARQUBE.md
```

## Links Úteis

- [README Principal](../README.md)
- [Código Fonte](../src/)
- [Testes](../tests/)
- [Scripts](../scripts/)
- [Configurações Build](../build/)