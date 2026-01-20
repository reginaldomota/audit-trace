# Audit Trace - Guia de Uso

## 🚀 Iniciar o Sistema

### 1. Iniciar PostgreSQL com Docker

```bash
docker-compose up -d
```

Isso irá iniciar:
- **PostgreSQL** na porta `5432`
- **PgAdmin** na porta `5050` (http://localhost:5050)
  - Email: admin@audittrace.com
  - Senha: admin

### 2. Executar a API

```bash
cd src/Api
dotnet run
```

A API estará disponível em:
- https://localhost:5001
- Swagger: https://localhost:5001/swagger

### 3. Executar o Worker

```bash
cd src/Jobs
dotnet run
```

O worker processará produtos automaticamente a cada 30 segundos (15 segundos em desenvolvimento).

---

## 📋 Fluxo de Trabalho

### Como funciona:

1. **API cadastra produto** → Status: `Created` (0)
2. **Worker processa** → Busca produtos com status `Created`
3. **Worker atualiza** → Muda status para `Registered` (1)

### Status disponíveis:
- `Created` (0) - Produto recém-criado pela API
- `Registered` (1) - Produto processado pelo Worker
- `Inactive` (2) - Produto inativo

---

## 🧪 Testar o Sistema

### 1. Criar um produto (API)

```bash
curl -X POST https://localhost:5001/api/products \
  -H "Content-Type: application/json" \
  -d '{
    "name": "Notebook Dell",
    "description": "Notebook i7 16GB RAM",
    "price": 4500.00,
    "stock": 10
  }'
```

Resposta: Produto criado com `status: 0` (Created)

### 2. Ver produtos criados

```bash
curl https://localhost:5001/api/products/status/0
```

### 3. Aguardar o Worker processar (30 segundos)

O worker irá:
- Buscar produtos com status `Created`
- Atualizar para status `Registered`
- Registrar no log

### 4. Ver produtos registrados

```bash
curl https://localhost:5001/api/products/status/1
```

---

## 📡 Endpoints da API

| Método | Endpoint | Descrição |
|--------|----------|-----------|
| GET | `/api/products` | Lista todos os produtos |
| GET | `/api/products/status/{status}` | Lista por status (0=Created, 1=Registered, 2=Inactive) |
| GET | `/api/products/{id}` | Busca produto por ID |
| POST | `/api/products` | Cria novo produto |
| PUT | `/api/products/{id}` | Atualiza produto |
| DELETE | `/api/products/{id}` | Remove produto |

---

## 🐘 PostgreSQL

### Conexão padrão:
```
Host: localhost
Port: 5432
Database: audittrace
Username: postgres
Password: postgres
```

### Acessar PgAdmin:
1. Abra http://localhost:5050
2. Login: admin@audittrace.com / admin
3. Adicione servidor:
   - Name: AuditTrace
   - Host: postgres (ou host.docker.internal no Mac)
   - Port: 5432
   - Username: postgres
   - Password: postgres

---

## ⚙️ Configurações do Worker

Edite `src/Jobs/appsettings.json`:

```json
{
  "WorkerSettings": {
    "ProductRegistrationInterval": 30000  // 30 segundos
  }
}
```

---

## 🛑 Parar o Sistema

```bash
# Parar PostgreSQL
docker-compose down

# Parar e remover volumes
docker-compose down -v
```

---

## 🔧 Comandos Úteis

### Migrations

```bash
# Criar nova migration
cd src/Infra
dotnet ef migrations add NomeMigracao --startup-project ../Api/Api.csproj

# Aplicar migrations
dotnet ef database update --startup-project ../Api/Api.csproj

# Remover última migration
dotnet ef migrations remove --startup-project ../Api/Api.csproj
```

### Ver logs do Worker

```bash
cd src/Jobs
dotnet run
```

Logs esperados:
```
Product Registration Worker iniciado em: [timestamp]
Buscando produtos com status 'Created' em: [timestamp]
Encontrados X produtos para registrar
Produto [id] - [nome] atualizado de 'Created' para 'Registered'
```
