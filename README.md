# Audit Trace

Sistema de gerenciamento e processamento de produtos utilizando Clean Architecture, com API RESTful e Workers para processamento assíncrono.

## 📋 Sobre o Projeto

O Audit Trace é uma aplicação que demonstra a implementação de Clean Architecture em .NET, separando responsabilidades entre API (interface REST) e Workers (processamento em background). O sistema gerencia produtos com controle de status e processamento assíncrono.

### Funcionalidades

**API REST:**
- Cadastro, consulta, atualização e exclusão de produtos
- Filtro de produtos por status
- Documentação interativa via Swagger

**Worker (Background Service):**
- Processamento automático de produtos criados
- Atualização de status de `Created` para `Registered`
- Execução periódica e configurável

**Status de Produtos:**
- `Created` (0) - Produto recém-cadastrado pela API
- `Registered` (1) - Produto processado pelo Worker
- `Inactive` (2) - Produto inativo

## 🏗️ Arquitetura

```
audit-trace/
├── src/
│   ├── Domain/           # Entidades, Enums e Interfaces de negócio
│   ├── Application/      # Casos de uso, Services e DTOs
│   ├── Infra/            # Implementação de repositórios e DbContext
│   ├── Api/              # Controllers REST e configuração da API
│   └── Jobs/             # Background Workers
└── tests/
    └── Tests/            # Testes unitários
```

### Camadas

- **Domain**: Núcleo da aplicação, sem dependências externas
- **Application**: Lógica de aplicação e orquestração
- **Infra**: Acesso a dados e recursos externos
- **Api**: Interface HTTP/REST
- **Jobs**: Processamento assíncrono e agendado

## 🛠️ Tecnologias

- **.NET 8.0** - Framework principal
- **Entity Framework Core** - ORM
- **PostgreSQL** - Banco de dados
- **Docker & Docker Compose** - Containerização
- **Swagger/OpenAPI** - Documentação da API
- **xUnit & Moq** - Testes

## 🚀 Como Usar

### Pré-requisitos
- .NET 8.0 SDK
- Docker e Docker Compose

### 1. Iniciar o banco de dados

```bash
docker-compose up -d
```

### 2. Executar a API

```bash
cd src/Api
dotnet run
```

Acesse: https://localhost:5001/swagger

### 3. Executar o Worker

```bash
cd src/Jobs
dotnet run
```

📖 **Documentação completa**: Ver [USAGE.md](USAGE.md) para guia detalhado

## 📡 Endpoints Principais

| Método | Endpoint | Descrição |
|--------|----------|-----------|
| GET | `/api/products` | Lista todos os produtos |
| GET | `/api/products/status/{status}` | Filtra por status (0, 1 ou 2) |
| GET | `/api/products/{id}` | Busca produto por ID |
| POST | `/api/products` | Cria novo produto |
| PUT | `/api/products/{id}` | Atualiza produto |
| DELETE | `/api/products/{id}` | Remove produto |

## 🔄 Fluxo de Processamento

1. **API** cadastra produto → Status: `Created` (0)
2. **Worker** detecta produtos com status `Created`
3. **Worker** processa e atualiza → Status: `Registered` (1)

## 🧪 Testes

```bash
cd tests/Tests
dotnet test
```

## 📦 Banco de Dados

**PostgreSQL via Docker:**
- Host: localhost
- Port: 5432
- Database: audittrace
- User/Password: postgres/postgres

**PgAdmin**: http://localhost:5050
- Email: admin@audittrace.com
- Password: admin

## ⚙️ Configuração

**Worker Interval** (`src/Jobs/appsettings.json`):
```json
{
  "WorkerSettings": {
    "ProductRegistrationInterval": 30000
  }
}
```

**Conexão do Banco** (`src/Api/appsettings.json` e `src/Jobs/appsettings.json`):
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=audittrace;Username=postgres;Password=postgres"
  }
}
```

## 🎯 Princípios Aplicados

- **Clean Architecture**: Separação de responsabilidades e independência de frameworks
- **SOLID**: Código orientado a princípios de design
- **Repository Pattern**: Abstração do acesso a dados
- **Dependency Injection**: Inversão de controle
- **Separation of Concerns**: API e Workers independentes