# SonarQube Configuration - Order Management API

Esta documentação descreve como configurar e usar o SonarQube para análise de código do projeto Order Management API.

## 📋 Pré-requisitos

- Docker 20.10+
- Docker Compose v2.0+
- 4GB de RAM disponível para o SonarQube
- SonarQube Token (gerado após iniciar o SonarQube)

## 🚀 Quick Start

### 1. Iniciar o SonarQube

**Windows:**
```powershell
.\sonar.ps1 start
```

**Linux/macOS:**
```bash
chmod +x sonar.sh
./sonar.sh start
```

Aguarde ~60 segundos para o SonarQube inicializar completamente.

### 2. Verificar Status

```bash
.\sonar.ps1 status    # Windows
./sonar.sh status     # Linux
```

### 3. Criar Token

```bash
.\sonar.ps1 token     # Windows - Ver instruções
./sonar.sh token      # Linux - Ver instruções
```

Acesse http://localhost:9000 e siga as instruções para criar o token.

### 4. Executar Análise

**Windows:**
```powershell
$env:SONAR_TOKEN="seu_token_aqui"
.\sonar.ps1 analyze
```

**Linux/macOS:**
```bash
SONAR_TOKEN=seu_token_aqui ./sonar.sh analyze
```

## 📁 Estrutura de Arquivos

```
order-management/
├── docker-compose.sonar.yml        # Configuração SonarQube
├── Dockerfile.scanner              # Scanner .NET com coverage
├── sonar.sh                        # Helper Linux/macOS
├── sonar.ps1                       # Helper Windows
├── sonarqube.properties           # Configurações do projeto
└── docs/
	└── SONARQUBE.md               # Esta documentação
```

## 🐳 Componentes Docker

### SonarQube Server
- **Imagem**: `sonarqube:10-community`
- **Porta**: 9000
- **Database**: PostgreSQL
- **Volumes**: data, extensions, logs

### PostgreSQL Database
- **Imagem**: `postgres:16-alpine`
- **User**: sonar
- **Database**: sonar
- **Volume**: postgresql_data

### Scanner Service
- **Base**: `mcr.microsoft.com/dotnet/sdk:10.0`
- **Ferramentas**: 
  - dotnet-sonarscanner
  - Java 17 (requisito do scanner)
  - dotnet-reportgenerator-globaltool
- **Profile**: analysis (executado sob demanda)

## 🔧 Configuração

### Variáveis de Ambiente

No `docker-compose.sonar.yml`:

```yaml
environment:
  - SONAR_HOST_URL=http://sonarqube:9000
  - SONAR_TOKEN=${SONAR_TOKEN}
  - SONAR_PROJECT_KEY=order-management
  - SONAR_PROJECT_NAME=Order Management API
  - SONAR_PROJECT_VERSION=1.0.0
```

### Exclusões e Cobertura

O scanner está configurado com:

```bash
# Exclusões de análise
/d:sonar.exclusions="**/Migrations/**,**/obj/**,**/bin/**"

# Exclusões de cobertura
/d:sonar.coverage.exclusions="**/Program.cs,**/Migrations/**,**/*Tests/**"

# Relatório de cobertura
/d:sonar.cs.opencover.reportsPaths="**/coverage.opencover.xml"
```

## 📝 Comandos Helper

### Scripts sonar.sh / sonar.ps1

| Comando | Descrição |
|---------|-----------|
| `start` | Inicia SonarQube e PostgreSQL |
| `stop` | Para os serviços |
| `restart` | Reinicia os serviços |
| `logs` | Exibe logs em tempo real |
| `analyze` | Executa análise de código |
| `status` | Verifica status do SonarQube |
| `token` | Mostra instruções para criar token |
| `clean` | Remove volumes e dados |
| `help` | Exibe ajuda |

## 🔐 Gerando o Token

### Via Interface Web

1. Acesse http://localhost:9000
2. Login: `admin` / `admin`
3. Você será forçado a alterar a senha
4. Vá em: **My Account** → **Security** → **Generate Tokens**
5. Nome: `scanner` (ou qualquer nome)
6. Type: **User Token**
7. Clique em **Generate**
8. Copie o token (você não verá novamente!)

### Via API

**PowerShell:**
```powershell
$cred = [Convert]::ToBase64String([Text.Encoding]::ASCII.GetBytes("admin:sua_senha"))
$headers = @{Authorization="Basic $cred"}
Invoke-RestMethod -Uri "http://localhost:9000/api/user_tokens/generate?name=scanner" `
	-Method Post `
	-Headers $headers
```

**Linux/Bash:**
```bash
curl -u admin:sua_senha \
  -X POST "http://localhost:9000/api/user_tokens/generate?name=scanner"
```

## 📊 Executando Análise

### Análise Completa

```bash
# Windows
$env:SONAR_TOKEN="seu_token"
.\sonar.ps1 analyze

# Linux
SONAR_TOKEN=seu_token ./sonar.sh analyze
```

O processo irá:
1. Iniciar o scanner
2. Restaurar dependências
3. Build do projeto
4. Executar testes com cobertura (OpenCover)
5. Enviar resultados para o SonarQube
6. Gerar relatório

### Apenas Build Local

Se você quiser apenas build e testes sem enviar ao SonarQube:

```bash
docker-compose -f docker-compose.sonar.yml build scanner
```

## 📈 Visualizando Resultados

1. Acesse: http://localhost:9000
2. Faça login
3. Selecione o projeto **Order Management API**
4. Visualize:
   - **Overview**: Métricas gerais
   - **Issues**: Bugs, vulnerabilidades, code smells
   - **Measures**: Métricas detalhadas
   - **Code**: Código anotado
   - **Activity**: Histórico de análises

## 🎯 Métricas Monitoradas

O SonarQube irá analisar:

### Qualidade de Código
- ✅ **Bugs**: Erros que causam comportamento incorreto
- ✅ **Vulnerabilities**: Falhas de segurança
- ✅ **Code Smells**: Manutenibilidade e boas práticas
- ✅ **Security Hotspots**: Pontos sensíveis de segurança

### Cobertura de Testes
- ✅ **Line Coverage**: Linhas de código cobertas
- ✅ **Branch Coverage**: Branches cobertos
- ✅ **Condition Coverage**: Condições testadas

### Duplicação
- ✅ **Duplicated Lines**: Linhas duplicadas
- ✅ **Duplicated Blocks**: Blocos duplicados

### Complexidade
- ✅ **Complexity**: Complexidade ciclomática
- ✅ **Cognitive Complexity**: Complexidade cognitiva

## 🔍 Troubleshooting

### SonarQube não inicia

```bash
# Ver logs
.\sonar.ps1 logs    # Windows
./sonar.sh logs     # Linux

# Verificar memória
docker stats ordermanagement-sonarqube
```

### Erro de memória (vm.max_map_count)

**Linux:**
```bash
sudo sysctl -w vm.max_map_count=262144
echo "vm.max_map_count=262144" | sudo tee -a /etc/sysctl.conf
```

### Token inválido

```bash
# Verificar se o token está definido
echo $env:SONAR_TOKEN    # Windows
echo $SONAR_TOKEN        # Linux

# Gerar novo token
.\sonar.ps1 token
```

### Análise falha

```bash
# Ver logs do scanner
docker-compose -f docker-compose.sonar.yml logs scanner

# Rebuild do scanner
docker-compose -f docker-compose.sonar.yml build --no-cache scanner
```

### Porta 9000 em uso

Edite `docker-compose.sonar.yml`:
```yaml
ports:
  - "9001:9000"  # Use porta 9001 ao invés de 9000
```

Atualize também `SONAR_HOST_URL` no scanner.

## 🚀 Integração com CI/CD

### GitHub Actions

Crie `.github/workflows/sonar.yml`:

```yaml
name: SonarQube Analysis

on:
  push:
	branches: [ main, develop ]
  pull_request:
	branches: [ main ]

jobs:
  sonarqube:
	runs-on: ubuntu-latest
	steps:
	  - uses: actions/checkout@v3
		with:
		  fetch-depth: 0

	  - name: Setup .NET
		uses: actions/setup-dotnet@v3
		with:
		  dotnet-version: '10.0.x'

	  - name: Restore dependencies
		run: dotnet restore

	  - name: SonarScanner begin
		run: |
		  dotnet tool install --global dotnet-sonarscanner
		  dotnet sonarscanner begin \
			/k:"order-management" \
			/d:sonar.host.url="${{ secrets.SONAR_HOST_URL }}" \
			/d:sonar.token="${{ secrets.SONAR_TOKEN }}" \
			/d:sonar.cs.opencover.reportsPaths="**/coverage.opencover.xml"

	  - name: Build
		run: dotnet build --no-restore

	  - name: Test with coverage
		run: |
		  dotnet test --no-build \
			--collect:"XPlat Code Coverage" \
			-- DataCollectionRunSettings.DataCollectors.DataCollector.Configuration.Format=opencover

	  - name: SonarScanner end
		run: dotnet sonarscanner end /d:sonar.token="${{ secrets.SONAR_TOKEN }}"
		env:
		  GITHUB_TOKEN: ${{ secrets.GITHUB_TOKEN }}
```

Configure os secrets no GitHub:
- `SONAR_HOST_URL`: URL do SonarQube
- `SONAR_TOKEN`: Token gerado

### Azure DevOps

Adicione ao `azure-pipelines.yml`:

```yaml
- task: SonarQubePrepare@5
  inputs:
	SonarQube: 'SonarQube Connection'
	scannerMode: 'MSBuild'
	projectKey: 'order-management'
	projectName: 'Order Management API'
	extraProperties: |
	  sonar.cs.opencover.reportsPaths=**/coverage.opencover.xml

- task: DotNetCoreCLI@2
  inputs:
	command: 'build'

- task: DotNetCoreCLI@2
  inputs:
	command: 'test'
	arguments: '--collect:"XPlat Code Coverage"'

- task: SonarQubeAnalyze@5

- task: SonarQubePublish@5
  inputs:
	pollingTimeoutSec: '300'
```

## 📚 Configurações Avançadas

### Arquivo sonar-project.properties

Crie na raiz do projeto:

```properties
sonar.projectKey=order-management
sonar.projectName=Order Management API
sonar.projectVersion=1.0.0

# Source directories
sonar.sources=src
sonar.tests=tests

# Exclusions
sonar.exclusions=**/Migrations/**,**/obj/**,**/bin/**
sonar.coverage.exclusions=**/Program.cs,**/Migrations/**,**/*Tests/**

# Coverage
sonar.cs.opencover.reportsPaths=**/coverage.opencover.xml

# Language
sonar.language=cs
```

### Quality Gates Customizados

1. Acesse **Quality Gates** no SonarQube
2. Crie um novo gate ou edite o padrão
3. Configure condições:
   - Coverage > 80%
   - Duplications < 3%
   - Maintainability Rating = A
   - Reliability Rating = A
   - Security Rating = A

## 🎓 Boas Práticas

1. **Execute análise regularmente**
   - Em cada PR
   - No mínimo diário no branch principal

2. **Resolva issues críticas imediatamente**
   - Bugs: Prioridade máxima
   - Vulnerabilities: Correção urgente
   - Code Smells: Refatorar gradualmente

3. **Mantenha alta cobertura de testes**
   - Meta: >80% de cobertura
   - Foque em código crítico

4. **Monitore tendências**
   - Use o gráfico de atividade
   - Rastreie débito técnico

5. **Configure notificações**
   - Email para failed quality gates
   - Webhooks para integração com Slack/Teams

## 🔗 Links Úteis

- [SonarQube Documentation](https://docs.sonarqube.org/)
- [SonarScanner for .NET](https://docs.sonarqube.org/latest/analysis/scan/sonarscanner-for-msbuild/)
- [Coverage with OpenCover](https://github.com/coverlet-coverage/coverlet)
- [Quality Gates](https://docs.sonarqube.org/latest/user-guide/quality-gates/)

## 📊 Comandos de Referência Rápida

```bash
# Iniciar
.\sonar.ps1 start

# Aguardar e verificar
Start-Sleep -Seconds 60
.\sonar.ps1 status

# Criar projeto e token (interface web)
start http://localhost:9000

# Executar análise
$env:SONAR_TOKEN="seu_token"
.\sonar.ps1 analyze

# Ver resultados
start http://localhost:9000/dashboard?id=order-management

# Parar
.\sonar.ps1 stop
```

---

**Criado por:** GitHub Copilot  
**Versão SonarQube:** 10 Community  
**Compatibilidade:** .NET 10
