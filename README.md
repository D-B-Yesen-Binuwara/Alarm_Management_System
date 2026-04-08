# Alarm_Management_System

Web-based system to model and simulate fault detection, alarm handling, and impact analysis in a telecom network hierarchy.

## Requirements

- **.NET 9.0 SDK** ([download](https://dotnet.microsoft.com/download/dotnet/9.0))
- **Docker & Docker Compose** ([install](https://docs.docker.com/get-docker/))
- **SQL Server 2022** (runs in Docker container, no local install needed)

## Quick Start

Run from the project root.

1. Start API + SQL Server:

```bash
make rebuild
```

2. Seed schema and sample data:

```bash
make seed
```

3. Open API docs:

- Swagger: http://localhost:5289/swagger

4. Run automated impact-analysis demo checks:

```bash
make demo
```

## Available Commands

- `make up`: start containers
- `make down`: stop containers
- `make rebuild`: rebuild and start containers
- `make seed`: load `Backend/DB_Schema.sql` into SQL Server container
- `make demo`: run impact-analysis test script

## Manual API Tests

```bash
curl -X POST http://localhost:5289/api/impact-analysis/analyze/1
curl http://localhost:5289/api/impact-analysis/result/1
curl -X POST http://localhost:5289/api/impact-analysis/clear/1
```

## Demo Resources

- Automated script: `scripts/run-impact-tests.sh`
- Speaking notes: `scripts/demo-speaking-notes.md`
