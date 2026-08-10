# SonarQube Configuration - Order Management API

Esta documentaÃ§Ã£o descreve como configurar e usar o SonarQube para anÃ¡lise de cÃ³digo do projeto Order Management API.

## PrÃ©-requisitos

- Docker 20.10+
- Docker Compose v2.0+
- 4GB de RAM disponÃ­vel para o SonarQube
- SonarQube Token (gerado apÃ³s iniciar o SonarQube)

## Quick Start

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
.\sonar.ps1 token     # Windows - Ver instruÃ§Ãµes
./sonar.sh token      # Linux - Ver instruÃ§Ãµes
```

Acesse http://localhost:9000 e siga as instruÃ§Ãµes para criar o token.

### 4. Executar AnÃ¡lise

**Windows:**
```powershell
$env:SONAR_TOKEN="seu_token_aqui"
.\sonar.ps1 analyze
```

**Linux/macOS:**
```bash
SONAR_TOKEN=seu_token_aqui ./sonar.sh analyze
```

## Estrutura de Arquivos

```
order-management/
â”œâ”€â”€ docker-compose.sonar.yml        # ConfiguraÃ§Ã£o SonarQube
â”œâ”€â”€ Dockerfile.scanner              # Scanner .NET com coverage
â”œâ”€â”€ sonar.sh                        # Helper Linux/macOS
â”œâ”€â”€ sonar.ps1                       # Helper Windows
â”œâ”€â”€ sonarqube.properties           # ConfiguraÃ§Ãµes do projeto
â””â”€â”€ docs/
	â””â”€â”€ SONARQUBE.md               # Esta documentaÃ§Ã£o
```

## Componentes Docker

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

## ConfiguraÃ§Ã£o

### VariÃ¡veis de Ambiente

No `docker-compose.sonar.yml`:

```yaml
environment:
  - SONAR_HOST_URL=http://sonarqube:9000
  - SONAR_TOKEN=${SONAR_TOKEN}
  - SONAR_PROJECT_KEY=order-management
  - SONAR_PROJECT_NAME=Order Management API
  - SONAR_PROJECT_VERSION=1.0.0
```

### ExclusÃµes e Cobertura

O scanner estÃ¡ configurado com:

```bash
# ExclusÃµes de anÃ¡lise
/d:sonar.exclusions="**/Migrations/**,**/obj/**,**/bin/**"

# ExclusÃµes de cobertura
/d:sonar.coverage.exclusions="**/Program.cs,**/Migrations/**,**/*Tests/**"

# RelatÃ³rio de cobertura
/d:sonar.cs.opencover.reportsPaths="**/coverage.opencover.xml"
```

## Comandos Helper

### Scripts sonar.sh / sonar.ps1

| Comando | DescriÃ§Ã£o |
|---------|-----------|
| `start` | Inicia SonarQube e PostgreSQL |
| `stop` | Para os serviÃ§os |
| `restart` | Reinicia os serviÃ§os |
| `logs` | Exibe logs em tempo real |
| `analyze` | Executa anÃ¡lise de cÃ³digo |
| `status` | Verifica status do SonarQube |
| `token` | Mostra instruÃ§Ãµes para criar token |
| `clean` | Remove volumes e dados |
| `help` | Exibe ajuda |

## Gerando o Token

### Via Interface Web

1. Acesse http://localhost:9000
2. Login: `admin` / `admin`
3. VocÃª serÃ¡ forÃ§ado a alterar a senha
4. VÃ¡ em: **My Account** â†’ **Security** â†’ **Generate Tokens**
5. Nome: `scanner` (ou qualquer nome)
6. Type: **User Token**
7. Clique em **Generate**
8. Copie o token (vocÃª nÃ£o verÃ¡ novamente!)

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

## Executando AnÃ¡lise

### AnÃ¡lise Completa

```bash
# Windows
$env:SONAR_TOKEN="seu_token"
.\sonar.ps1 analyze

# Linux
SONAR_TOKEN=seu_token ./sonar.sh analyze
```

O processo irÃ¡:
1. Iniciar o scanner
2. Restaurar dependÃªncias
3. Build do projeto
4. Executar testes com cobertura (OpenCover)
5. Enviar resultados para o SonarQube
6. Gerar relatÃ³rio

### Apenas Build Local

Se vocÃª quiser apenas build e testes sem enviar ao SonarQube:

```bash
docker-compose -f docker-compose.sonar.yml build scanner
```

## Visualizando Resultados

1. Acesse: http://localhost:9000
2. FaÃ§a login
3. Selecione o projeto **Order Management API**
4. Visualize:
   - **Overview**: MÃ©tricas gerais
   - **Issues**: Bugs, vulnerabilidades, code smells
   - **Measures**: MÃ©tricas detalhadas
   - **Code**: CÃ³digo anotado
   - **Activity**: HistÃ³rico de anÃ¡lises

## MÃ©tricas Monitoradas

O SonarQube irÃ¡ analisar:

### Qualidade de CÃ³digo
- **Bugs**: Erros que causam comportamento incorreto
- **Vulnerabilities**: Falhas de seguranÃ§a
- **Code Smells**: Manutenibilidade e boas prÃ¡ticas
- **Security Hotspots**: Pontos sensÃ­veis de seguranÃ§a

### Cobertura de Testes
- **Line Coverage**: Linhas de cÃ³digo cobertas
- **Branch Coverage**: Branches cobertos
- **Condition Coverage**: CondiÃ§Ãµes testadas

### DuplicaÃ§Ã£o
- **Duplicated Lines**: Linhas duplicadas
- **Duplicated Blocks**: Blocos duplicados

### Complexidade
- **Complexity**: Complexidade ciclomÃ¡tica
- **Cognitive Complexity**: Complexidade cognitiva

## Troubleshooting

### SonarQube nÃ£o inicia

```bash
# Ver logs
.\sonar.ps1 logs    # Windows
./sonar.sh logs     # Linux

# Verificar memÃ³ria
docker stats ordermanagement-sonarqube
```

### Erro de memÃ³ria (vm.max_map_count)

**Linux:**
```bash
sudo sysctl -w vm.max_map_count=262144
echo "vm.max_map_count=262144" | sudo tee -a /etc/sysctl.conf
```

### Token invÃ¡lido

```bash
# Verificar se o token estÃ¡ definido
echo $env:SONAR_TOKEN    # Windows
echo $SONAR_TOKEN        # Linux

# Gerar novo token
.\sonar.ps1 token
```

### AnÃ¡lise falha

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
  - "9001:9000"  # Use porta 9001 ao invÃ©s de 9000
```

Atualize tambÃ©m `SONAR_HOST_URL` no scanner.

## IntegraÃ§Ã£o com CI/CD

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

## ðŸ“š ConfiguraÃ§Ãµes AvanÃ§adas

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
2. Crie um novo gate ou edite o padrÃ£o
3. Configure condiÃ§Ãµes:
   - Coverage > 80%
   - Duplications < 3%
   - Maintainability Rating = A
   - Reliability Rating = A
   - Security Rating = A

## ðŸŽ“ Boas PrÃ¡ticas

1. **Execute anÃ¡lise regularmente**
   - Em cada PR
   - No mÃ­nimo diÃ¡rio no branch principal

2. **Resolva issues crÃ­ticas imediatamente**
   - Bugs: Prioridade mÃ¡xima
   - Vulnerabilities: CorreÃ§Ã£o urgente
   - Code Smells: Refatorar gradualmente

3. **Mantenha alta cobertura de testes**
   - Meta: >80% de cobertura
   - Foque em cÃ³digo crÃ­tico

4. **Monitore tendÃªncias**
   - Use o grÃ¡fico de atividade
   - Rastreie dÃ©bito tÃ©cnico

5. **Configure notificaÃ§Ãµes**
   - Email para failed quality gates
   - Webhooks para integraÃ§Ã£o com Slack/Teams

## Links Ãšteis

- [SonarQube Documentation](https://docs.sonarqube.org/)
- [SonarScanner for .NET](https://docs.sonarqube.org/latest/analysis/scan/sonarscanner-for-msbuild/)
- [Coverage with OpenCover](https://github.com/coverlet-coverage/coverlet)
- [Quality Gates](https://docs.sonarqube.org/latest/user-guide/quality-gates/)

## Comandos de ReferÃªncia RÃ¡pida

```bash
# Iniciar
.\sonar.ps1 start

# Aguardar e verificar
Start-Sleep -Seconds 60
.\sonar.ps1 status

# Criar projeto e token (interface web)
start http://localhost:9000

# Executar anÃ¡lise
$env:SONAR_TOKEN="seu_token"
.\sonar.ps1 analyze

# Ver resultados
start http://localhost:9000/dashboard?id=order-management

# Parar
.\sonar.ps1 stop
```

---

**Criado por:** GitHub Copilot  
**VersÃ£o SonarQube:** 10 Community  
**Compatibilidade:** .NET 10

