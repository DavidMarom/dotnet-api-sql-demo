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

## Database Objects

### Search Function — `search_contacts` (Migration 002)

A PostgreSQL **function** that performs a case-insensitive full-text search across first name, last name, and full name:

```sql
SELECT * FROM search_contacts('john');
```

Called via `SELECT` and returns a result set directly. Used by `GET /contacts/search?q=`.

### Stored Procedure — `upsert_contact` (Migration 003)

A PostgreSQL **stored procedure** written in `plpgsql` that inserts or updates a contact:

```sql
-- Insert (no ID supplied)
CALL upsert_contact('Jane', 'Doe', '+1-555-0100', NULL);

-- Update (existing ID supplied — falls back to insert if not found)
CALL upsert_contact('Jane', 'Doe', '+1-555-0199', 5);
```

**How it differs from a function:**

| | Function (`search_contacts`) | Procedure (`upsert_contact`) |
|---|---|---|
| Created with | `CREATE FUNCTION` | `CREATE PROCEDURE` |
| Called with | `SELECT` | `CALL` |
| Returns data | `RETURNS TABLE` | `INOUT` parameter |
| Language | `sql` | `plpgsql` |
| Can manage transactions | No | Yes |

**Logic:** tries to `UPDATE` by the supplied ID first. If no row is matched (or no ID was given), falls through to `INSERT` and returns the new ID via the `INOUT p_id` parameter.

**In .NET:** Dapper doesn't support `INOUT` parameters for procedures, so `UpsertAsync` uses `NpgsqlCommand` directly with `ParameterDirection.InputOutput`. The returned ID is read back from the parameter after `ExecuteNonQueryAsync`.

Used by `POST /contacts/upsert`:

```bash
# Insert (no ?id)
curl -X POST http://localhost:5000/contacts/upsert \
  -H "Content-Type: application/json" \
  -d '{"firstName":"Jane","lastName":"Doe","phoneNumber":"+1-555-0100"}'

# Update contact ID 5 (falls back to insert if ID 5 doesn't exist)
curl -X POST "http://localhost:5000/contacts/upsert?id=5" \
  -H "Content-Type: application/json" \
  -d '{"firstName":"Jane","lastName":"Doe","phoneNumber":"+1-555-0199"}'
```

## Migrations

SQL migration scripts live in `Migrations/`. DbUp runs them in filename order at startup, skipping scripts that have already been applied. To add a new migration, create a file like `004_add_email.sql`.

## Configuration

The connection string is in `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "Postgres": "Host=localhost;Port=5432;Database=phonebook;Username=postgres;Password=postgres"
  }
}
```
