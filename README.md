# JobProcessor

**Um serviço robusto de processamento de tarefas em segundo plano** desenvolvido em **C#** com **ASP.NET Core**, **RabbitMQ** e **MongoDB**, totalmente containerizado com **Docker**. Este projeto foi criado para o **Desafio de Programação para Desenvolvedor C# / ASP.NET**, demonstrando uma implementação escalável e eficiente de uma API REST para gerenciamento de tarefas, processamento assíncrono com workers, controle de concorrência, retentativas e orquestração de serviços.

---

## 📋 Descrição

O **JobProcessor** é um sistema para processamento de tarefas assíncronas, projetado para atender aos seguintes requisitos:

- **Recebimento de Tarefas**: API REST para criar tarefas com ID (GUID), tipo (ex.: "EnviarEmail", "GerarRelatorio") e payload (JSON), armazenadas no MongoDB e publicadas em uma fila RabbitMQ.
- **Processamento Assíncrono**: Múltiplos workers consomem tarefas da fila RabbitMQ, com suporte a retentativas (máximo de 3 tentativas) e controle de concorrência via `BasicQos`.
- **Consulta de Status**: API para consultar o status das tarefas (Pendente, EmProcessamento, Concluído, Erro).
- **Escalabilidade**: Suporte a múltiplos workers (3 réplicas por padrão) e uso de RabbitMQ para distribuição eficiente de tarefas.
- **Containerização**: Deployment simplificado com Docker e orquestração via Docker Compose.

---

## 🛠 Tecnologias Utilizadas

- **Linguagem**: C# com ASP.NET Core
- **Banco de Dados**: MongoDB (NoSQL)
- **Fila**: RabbitMQ
- **Containerização**: Docker e Docker Compose
- **Outras Ferramentas**:
  - ILogger para logging estruturado
  - Injeção de dependência
  - Tratamento de erros com FluentValidation

---

## 📦 Pré-requisitos

- **Docker** (com Docker Compose incluído)
- **Git**
- *(Opcional)*: .NET SDK 8.0 para desenvolvimento local sem Docker

---

## 🚀 Como Executar

Siga os passos abaixo para rodar o projeto localmente usando **Docker Compose**:

### 1. Clone o Repositório

```bash
git clone https://github.com/matheusbatista1/JobProcessor.git
cd JobProcessor
```

### 2. Configure as Variáveis de Ambiente

Copie o arquivo `.env.example` para `.env`:

```bash
cp .env.example .env
```

Edite o arquivo `.env` com as variáveis de ambiente necessárias:

```env
RABBITMQ_HOST=rabbitmq
RABBITMQ_USERNAME=guest
RABBITMQ_PASSWORD=guest
RABBITMQ_QUEUE=jobs_queue
MONGO_CONNECTION_STRING=mongodb://host.docker.internal:27017
MONGO_DATABASE=jobprocessor
```

**Descrição das Variáveis**:
- `RABBITMQ_HOST`: Nome do serviço RabbitMQ (use `rabbitmq` para Docker Compose).
- `RABBITMQ_USERNAME` / `RABBITMQ_PASSWORD`: Credenciais para autenticação no RabbitMQ (padrão: `guest`).
- `RABBITMQ_QUEUE`: Nome da fila para tarefas (padrão: `jobs_queue`).
- `MONGO_CONNECTION_STRING`: String de conexão com o MongoDB (use `mongodb://host.docker.internal:27017` para Docker Compose).
- `MONGO_DATABASE`: Nome do banco de dados MongoDB (padrão: `jobprocessor`).

### 3. Inicie os Serviços

Construa e inicie os contêineres (RabbitMQ, MongoDB, API e workers):

```bash
docker-compose up --build -d
```

Verifique se os contêineres estão rodando:

```bash
docker ps
```

**Saída esperada**:
- `jobprocessor-rabbitmq-1`
- `jobprocessor-mongo-1`
- `jobprocessor-jobprocessor-api-1`
- Três réplicas do worker: `jobprocessor-jobprocessor-worker-1`, `-2`, `-3`

### 4. Acesse os Serviços

- **API**: Disponível em `http://localhost:5000/api/jobs`
- **RabbitMQ Management**: Acesse em `http://localhost:15672` (usuário: `guest`, senha: `guest`)
- **MongoDB**: Conecte-se via `mongodb://localhost:27017` (use ferramentas como MongoDB Compass, se desejar)

### 5. Teste a API

**Criar uma Tarefa**:

Envie uma solicitação POST para `http://localhost:5000/api/jobs`:

ℹ️ **Notas sobre Enums**

- **`Type`** é um enum enviado como número no `POST` e retornado como texto no `GET`:

| Valor | Nome           |
|-------|----------------|
| `1`   | EnviarEmail    |
| `2`   | GerarRelatorio |

🔁 **Exemplo de envio via POST**:
```json
{
  "Type": 1,
  "Payload": "{\"to\":\"example@email.com\",\"subject\":\"Teste\"}"
}
```

```bash
curl -X POST http://localhost:5000/api/jobs -H "Content-Type: application/json" -d '{"Type": 1,"Payload":"{\"to\":\"example@email.com\",\"subject\":\"Teste\"}"}'
```

A resposta conterá o ID da tarefa criada.

**Consultar o Status**:

🔹 `Status` **(Status da Tarefa)**

- Enum salvo e retornado como **número** na resposta da API.

| Valor | Status           |
|-------|------------------|
| `1`   | Pendente         |
| `2`   | EmProcessamento  |
| `3`   | Concluido        |
| `4`   | Erro             |

Use o ID retornado para consultar o status via GET:

```bash
curl http://localhost:5000/api/jobs/{id}
```

**Exemplo de resposta**:

```json
{
  "Id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "Type": "EnviarEmail",
  "Payload": "{\"to\":\"example@email.com\",\"subject\":\"Teste\"}",
  "Status": "Concluido",
  "RetryCount": 0
}
```

**Monitorar Workers**:

Verifique os logs de um worker para confirmar o processamento:

```bash
docker logs jobprocessor-jobprocessor-worker-1
```

---

## 📂 Estrutura do Projeto

- **`JobProcessor.API/`**: API REST para criação e consulta de tarefas, com Dockerfile para containerização.
- **`JobProcessor.JobsWorker/`**: Worker para processamento assíncrono de tarefas, com Dockerfile.
- **`JobProcessor.Application/`**: Lógica de negócios e serviços da aplicação.
- **`JobProcessor.Domain/`**: Entidades, enums e contratos do domínio.
- **`JobProcessor.Infra/`**: Integrações com RabbitMQ, MongoDB e outros serviços externos.
- **`docker-compose.yml`**: Configuração do Docker para orquestrar RabbitMQ, MongoDB, API e workers.
- **`.env.example`**: Modelo de configurações de ambiente.
- **`.gitignore`**: Exclui arquivos sensíveis (`.env`) e desnecessários (`.vs/`, `bin/`, `obj/`).
- **`LICENSE`**: Licença MIT.
- **`JobProcessor.sln`**: Solução do Visual Studio para o projeto.

---

## 🔧 Variáveis de Ambiente

As variáveis são carregadas do arquivo `.env` e usadas pelos serviços `jobprocessor-api` e `jobprocessor-worker`. O arquivo `.env.example` fornece um modelo seguro. As variáveis incluem:

**RabbitMQ**:
- `RABBITMQ_HOST`: Endereço do servidor RabbitMQ (padrão: `rabbitmq`).
- `RABBITMQ_USERNAME` / `RABBITMQ_PASSWORD`: Credenciais para autenticação.
- `RABBITMQ_QUEUE`: Nome da fila para tarefas.

**MongoDB**:
- `MONGO_CONNECTION_STRING`: URL de conexão com o MongoDB.
- `MONGO_DATABASE`: Nome do banco de dados.

---

## 📈 Escalabilidade

- O projeto utiliza **RabbitMQ** para distribuir tarefas entre múltiplos workers, garantindo processamento concorrente sem conflitos.
- **Três réplicas** do worker são configuradas por padrão (`deploy: replicas: 3` no `docker-compose.yml`).
- Para escalar, ajuste o número de réplicas no `docker-compose.yml`:

```yaml
deploy:
  replicas: 5
```

---

## ✅ Boas Práticas Implementadas

- **Código**: Arquitetura em camadas (Domain, Application, Infra), injeção de dependência e logging estruturado com `ILogger`.
- **Retentativas**: Sistema de retentativas para conexão com RabbitMQ (configurável via `appsettings.json`) e processamento de tarefas (máximo de 3 tentativas por job).
- **Concorrência**: Configuração de `BasicQos(prefetchCount=1)` para processar uma mensagem por vez por worker, com RabbitMQ gerenciando a distribuição.
- **Containerização**: Dockerfile para cada serviço e `docker-compose.yml` para orquestração, com healthchecks para RabbitMQ e MongoDB.
- **Segurança**: Arquivo `.env` ignorado pelo `.gitignore`, com modelo fornecido em `.env.example`.
- **Documentação**: README detalhado com instruções claras e exemplos de uso.

---

## 🤝 Como Contribuir

1. Faça um fork do repositório.
2. Crie uma branch para sua feature: `git checkout -b feature/nova-funcionalidade`
3. Commit suas mudanças: `git commit -m "Descrição da mudança"`
4. Envie para o repositório remoto: `git push origin feature/nova-funcionalidade`
5. Abra um pull request.

---

## 📜 Licença

**MIT License**

---

## 📝 Notas para Avaliadores

Este projeto foi desenvolvido para o **Desafio de Programação Desenvolvedor C# / ASP.NET** e demonstra:

- **Estrutura e Organização**: Código organizado em camadas, com separação clara entre API, worker e infraestrutura.
- **Boas Práticas**: Uso de padrões modernos de C#, logging, retentativas e containerização.
- **Domínio de Tecnologias**: Integração com ASP.NET Core, RabbitMQ (`RabbitMQ.Client`) e MongoDB (`MongoDB.Driver`).
- **Escalabilidade e Robustez**: Suporte a múltiplos workers, healthchecks e tratamento de erros.

Para dúvidas, entre em contato via **[matheus.sbatista@outlook.com]** ou abra uma issue no repositório.
