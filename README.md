# MyServer - Phonebook API

A simple phonebook REST API built with ASP.NET Core 10, Dapper, and PostgreSQL.

## Stack

- **ASP.NET Core 10** — Minimal API
- **Dapper** — SQL micro-ORM
- **Npgsql** — PostgreSQL driver
- **DbUp** — SQL migration runner
- **PostgreSQL 17** — via Docker

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [Docker](https://www.docker.com/)

## Getting Started

Start the database:

```bash
docker compose up -d
```

Run the app (migrations run automatically on startup):

```bash
dotnet run
```

## API

All endpoints accept and return JSON.

| Method | Path | Description |
|--------|------|-------------|
| GET | `/contacts` | List all contacts |
| GET | `/contacts/{id}` | Get a contact by ID |
| POST | `/contacts` | Create a new contact |
| PUT | `/contacts/{id}` | Update a contact |
| DELETE | `/contacts/{id}` | Delete a contact |

### Request body (POST / PUT)

```json
{
  "firstName": "John",
  "lastName": "Doe",
  "phoneNumber": "+1-555-0100"
}
```

### Example

```bash
# Create
curl -X POST http://localhost:5000/contacts \
  -H "Content-Type: application/json" \
  -d '{"firstName":"John","lastName":"Doe","phoneNumber":"+1-555-0100"}'

# List
curl http://localhost:5000/contacts

# Update
curl -X PUT http://localhost:5000/contacts/1 \
  -H "Content-Type: application/json" \
  -d '{"firstName":"Jane","lastName":"Doe","phoneNumber":"+1-555-0199"}'

# Delete
curl -X DELETE http://localhost:5000/contacts/1
```

## Migrations

SQL migration scripts live in `Migrations/`. DbUp runs them in filename order at startup, skipping scripts that have already been applied. To add a new migration, create a file like `002_add_email.sql`.

## Configuration

The connection string is in `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "Postgres": "Host=localhost;Port=5432;Database=phonebook;Username=postgres;Password=postgres"
  }
}
```
