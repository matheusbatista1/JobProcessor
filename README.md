# JobProcessor

A background task-processing service built in C# and ASP.NET Core. It exposes a small REST API for submitting jobs, stores them in MongoDB, and queues them on RabbitMQ so a pool of worker instances can run them asynchronously. Each job is tracked through a status lifecycle, retried automatically on failure, and the whole stack scales horizontally just by adding more workers.

## What it does

- **Submit jobs over HTTP** — `POST /api/jobs` with a job type and a JSON payload. Each job is created with a GUID, persisted to MongoDB, and published to the queue.
- **Async processing** — the API publishes work to a RabbitMQ queue; workers pick it up and run it independently of the request, processing one message at a time per worker (`BasicQos(prefetchCount=1)`).
- **Status tracking** — every job moves through a defined lifecycle (`Pendente` → `EmProcessamento` → `Concluido` / `Erro`) and can be queried by id at any time.
- **Automatic retries** — failed jobs are retried up to 3 times before being marked as errored. RabbitMQ connections are also retried on startup.
- **Horizontal scaling** — multiple worker replicas consume from the same queue concurrently (3 by default), with RabbitMQ distributing work between them.

## Tech stack

| Layer         | Technology                         |
| ------------- | ---------------------------------- |
| Language      | C# (ASP.NET Core)                  |
| API           | ASP.NET Core REST API              |
| Messaging     | RabbitMQ (`rabbitmq:3-management`) |
| Storage       | MongoDB (`mongo`)                  |
| Validation    | FluentValidation                   |
| Logging       | Structured logging via `ILogger`   |
| Orchestration | Docker Compose                     |

## Architecture

The solution follows a layered design with clear separation of concerns and dependency injection throughout:

- **`JobProcessor.Domain`** — core entities, enums, and domain contracts (job, status, type).
- **`JobProcessor.Application`** — business logic, services, and validation.
- **`JobProcessor.Infra`** — infrastructure integrations (MongoDB persistence, RabbitMQ messaging).
- **`JobProcessor.API`** — REST endpoints for submitting and querying jobs.
- **`JobProcessor.JobsWorker`** — background worker that consumes the queue and executes jobs.

At runtime, the API persists each job to MongoDB and enqueues it on RabbitMQ; the workers consume the queue and update job state and results back in MongoDB. The API and workers stay decoupled, so either side can scale on its own. A health check is configured for RabbitMQ, and the API and workers wait for the broker to become healthy before starting.

```
Client → API (POST /api/jobs) → MongoDB + RabbitMQ queue → Worker(s) → MongoDB
                  ↑                                                      │
                  └──────────── GET /api/jobs/{id} ─────────────────────┘
```

## Getting started

### Prerequisites

- [Docker](https://www.docker.com/) and Docker Compose
- Git
- *(Optional)* .NET SDK 8.0, for local development without Docker

### Run with Docker Compose

```bash
# 1. Clone the repository
git clone https://github.com/matheusbatista1/JobProcessor.git
cd JobProcessor

# 2. Create your .env from the example and adjust values if needed
cp .env.example .env

# 3. Build and start the whole stack (API, workers, RabbitMQ, MongoDB)
docker-compose up --build -d

# 4. Check the containers are running
docker ps
```

Once everything is up:

| Service             | URL / Port                                          |
| ------------------- | --------------------------------------------------- |
| API                 | http://localhost:5000/api/jobs (port 8080 internally) |
| RabbitMQ management | http://localhost:15672 (user/pass: `guest`/`guest`) |
| RabbitMQ (AMQP)     | localhost:5672                                      |
| MongoDB             | mongodb://localhost:27017                           |

You can follow a worker's logs to watch jobs being processed:

```bash
docker logs jobprocessor-jobprocessor-worker-1
```

## Configuration

Configuration is supplied via a `.env` file (see `.env.example`). These values are read by the API and the workers, and the RabbitMQ container uses them for its initial credentials.

| Variable                  | Example value                          | Description                          |
| ------------------------- | -------------------------------------- | ------------------------------------ |
| `MONGO_CONNECTION_STRING` | `mongodb://host.docker.internal:27017` | MongoDB connection string            |
| `MONGO_DATABASE`          | `JobProcessor`                         | Database name                        |
| `RABBITMQ_HOST`           | `rabbitmq`                             | RabbitMQ host                        |
| `RABBITMQ_USERNAME`       | `guest`                                | RabbitMQ username                    |
| `RABBITMQ_PASSWORD`       | `guest`                                | RabbitMQ password                    |
| `RABBITMQ_QUEUE`          | `jobs_queue`                           | Queue jobs are published to/consumed |

> `.env` is excluded from version control — keep your real credentials out of the repo.

## API

### Create a job

`POST /api/jobs`

`Type` is sent as a number, and `Payload` is a JSON string describing the work to perform.

```json
{
  "Type": 1,
  "Payload": "{\"to\":\"example@email.com\",\"subject\":\"Test\"}"
}
```

```bash
curl -X POST http://localhost:5000/api/jobs \
  -H "Content-Type: application/json" \
  -d '{"Type": 1,"Payload":"{\"to\":\"example@email.com\",\"subject\":\"Test\"}"}'
```

The response returns the id of the created job.

**Job types**

| Value | Type             | Meaning           |
| ----- | ---------------- | ----------------- |
| `1`   | `EnviarEmail`    | Send an email     |
| `2`   | `GerarRelatorio` | Generate a report |

> `Type` is submitted as a number on `POST` and returned as text on `GET`.

### Query a job's status

`GET /api/jobs/{id}`

```bash
curl http://localhost:5000/api/jobs/{id}
```

**Example response**

```json
{
  "Id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "Type": "EnviarEmail",
  "Payload": "{\"to\":\"example@email.com\",\"subject\":\"Test\"}",
  "Status": "Concluido",
  "RetryCount": 0
}
```

**Job status**

| Value | Status            | Meaning              |
| ----- | ----------------- | -------------------- |
| `1`   | `Pendente`        | Pending (queued)     |
| `2`   | `EmProcessamento` | Currently processing |
| `3`   | `Concluido`       | Completed            |
| `4`   | `Erro`            | Failed after retries |

## Scaling the workers

The worker service runs with **3 replicas** by default (`deploy.replicas` in `docker-compose.yml`). To run more (or fewer) workers, adjust that value and restart the stack:

```yaml
deploy:
  replicas: 5
```

## License

Released under the [MIT License](LICENSE).

## Contact

Matheus Batista — matheus.sbatista@outlook.com — or open an issue on the repository.