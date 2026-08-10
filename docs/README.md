# Documentação do Order Management API

Esta pasta contém toda a documentação técnica do projeto Order Management API.

## Índice de Documentação

### Docker

- **[DOCKER.md](DOCKER.md)** - Documentação completa do Docker
  - Pré-requisitos e instalação
  - Estrutura de arquivos
  - Configuração detalhada
  - Comandos úteis
  - Troubleshooting
  - Guia de produção

- **[DOCKER_QUICKSTART.md](DOCKER_QUICKSTART.md)** - Guia rápido de início
  - Comandos essenciais (Windows/Linux)
  - URLs de acesso
  - Troubleshooting comum

- **[DOCKER_EXAMPLES.md](DOCKER_EXAMPLES.md)** - Exemplos práticos
  - 12 cenários de uso
  - Comandos completos
  - Monitoramento e debugging
  - Deploy e backup

- **[DOCKER_CHECKLIST.md](DOCKER_CHECKLIST.md)** - Checklist de implementação
  - Lista de arquivos criados
  - Recursos implementados
  - Validação de funcionalidades

- **[DOCKER_IMPLEMENTATION_SUMMARY.md](DOCKER_IMPLEMENTATION_SUMMARY.md)** - Sumário completo
  - Resumo da implementação
  - Estatísticas
  - Status final

### Autenticação e Segurança

- **[AUTH.md](AUTH.md)** - Documentação de autenticação
  - JWT Bearer Token
  - Endpoints de autenticação
  - Exemplos de uso

- **[AUTH_IMPLEMENTATION.md](AUTH_IMPLEMENTATION.md)** - Detalhes de implementação
  - Configuração JWT
  - Validação de tokens
  - Renovação de tokens

- **[SECURITY.md](SECURITY.md)** - Guia de segurança
  - Boas práticas
  - Políticas de segurança
  - Vulnerabilidades conhecidas

### Observabilidade

- **[OPENTELEMETRY.md](OPENTELEMETRY.md)** - Documentação do OpenTelemetry
  - Instrumentação
  - Configuração
  - Spans customizados
  - Console export

- **[OPENTELEMETRY_OUTPUT.md](OPENTELEMETRY_OUTPUT.md)** - Exemplos de output
  - Traces de criação de pedidos
  - Traces de cancelamento
  - Hierarquia de spans

### Análise de Código

- **[SONARQUBE.md](SONARQUBE.md)** - SonarQube Configuration
  - Instalação e configuração
  - Comandos helper (sonar.sh/sonar.ps1)
  - Execução de análises
  - Métricas e relatórios
  - Integração CI/CD
  - Troubleshooting

###  Outros

- **[REFACTORING_SUMMARY.md](REFACTORING_SUMMARY.md)** - Histórico de refatorações
  - Mudanças realizadas
  - Melhorias implementadas
  - Lições aprendidas

## 🚀 Quick Links

### Para Começar
1. [Docker Quick Start](DOCKER_QUICKSTART.md) - Subir a aplicação em 30 segundos
2. [Autenticação](AUTH.md) - Fazer login e obter token
3. [README Principal](../README.md) - Visão geral do projeto

### Para Desenvolvedores
1. [Exemplos Docker](DOCKER_EXAMPLES.md) - Cenários práticos
2. [OpenTelemetry](OPENTELEMETRY.md) - Rastreamento e observabilidade
3. [Segurança](SECURITY.md) - Práticas de segurança

### Para DevOps
1. [Guia Docker Completo](DOCKER.md) - Deploy e configuração
2. [Checklist Docker](DOCKER_CHECKLIST.md) - Validação
3. [Implementação Docker](DOCKER_IMPLEMENTATION_SUMMARY.md) - Sumário técnico

## Estrutura da Documentação

```
docs/
├── README.md                              (este arquivo)
├── DOCKER.md                              (5000+ palavras)
├── DOCKER_QUICKSTART.md                   (guia rápido)
├── DOCKER_EXAMPLES.md                     (12 cenários)
├── DOCKER_CHECKLIST.md                    (validação)
├── DOCKER_IMPLEMENTATION_SUMMARY.md       (sumário)
├── AUTH.md                                (autenticação)
├── AUTH_IMPLEMENTATION.md                 (implementação auth)
├── SECURITY.md                            (segurança)
├── OPENTELEMETRY.md                       (observabilidade)
├── OPENTELEMETRY_OUTPUT.md                (exemplos traces)
└── REFACTORING_SUMMARY.md                 (histórico)
```

## Navegação Rápida por Tarefa

### Quero executar a aplicação
→ [DOCKER_QUICKSTART.md](DOCKER_QUICKSTART.md)

### Quero entender como funciona o Docker
→ [DOCKER.md](DOCKER.md)

### Quero ver exemplos práticos
→ [DOCKER_EXAMPLES.md](DOCKER_EXAMPLES.md)

### Quero fazer autenticação
→ [AUTH.md](AUTH.md)

### Quero entender o OpenTelemetry
→ [OPENTELEMETRY.md](OPENTELEMETRY.md)

### Quero validar minha implementação
→ [DOCKER_CHECKLIST.md](DOCKER_CHECKLIST.md)

### Quero configurações de segurança
→ [SECURITY.md](SECURITY.md)

## Contribuindo com a Documentação

Ao adicionar nova documentação:
1. Coloque arquivos `.md` nesta pasta `docs/`
2. Atualize este `README.md` com o novo arquivo
3. Mantenha o padrão de nomenclatura
4. Use emojis para facilitar identificação
5. Inclua links entre documentos relacionados

## Links Úteis

- [README Principal](../README.md)
- [Código Fonte](../src/)
- [Testes](../tests/)
- [Scripts](../scripts/) (docker.sh, docker.ps1, sonar.sh, sonar.ps1)
- [Configurações Build](../build/) (Docker Compose, Dockerfiles, SonarQube)

---

**Última atualização:** Janeiro 2024  
**Versão da API:** 1.0.0  
**Framework:** .NET 10
