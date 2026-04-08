using DbUp;
using MyServer.Models;
using MyServer.Repositories;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

var connectionString = builder.Configuration.GetConnectionString("Postgres")
    ?? throw new InvalidOperationException("Connection string 'Postgres' not found.");

builder.Services.AddSingleton<IContactRepository>(_ => new ContactRepository(connectionString));

var app = builder.Build();

// Run migrations on startup
var upgrader = DeployChanges.To
    .PostgresqlDatabase(connectionString)
    .WithScriptsEmbeddedInAssembly(typeof(Program).Assembly)
    .WithTransactionPerScript()
    .LogToConsole()
    .Build();

var result = upgrader.PerformUpgrade();
if (!result.Successful)
    throw new Exception("Database migration failed", result.Error);

if (app.Environment.IsDevelopment())
    app.MapOpenApi();

// app.UseHttpsRedirection();

// GET /contacts
app.MapGet("/contacts", async (IContactRepository repo) =>
    Results.Ok(await repo.GetAllAsync()));

// GET /contacts/search?q=
app.MapGet("/contacts/search", async (string q, IContactRepository repo) =>
    Results.Ok(await repo.SearchAsync(q)));

// GET /contacts/{id}
app.MapGet("/contacts/{id:int}", async (int id, IContactRepository repo) =>
{
    var contact = await repo.GetByIdAsync(id);
    return contact is null ? Results.NotFound() : Results.Ok(contact);
});

// POST /contacts
app.MapPost("/contacts", async (ContactRequest request, IContactRepository repo) =>
{
    var created = await repo.CreateAsync(request);
    Console.WriteLine(value: $"Created contact with ID {created.Id}");

    return Results.Created($"/contacts/{created.Id}", created);
});

// PUT /contacts/{id}
app.MapPut("/contacts/{id:int}", async (int id, ContactRequest request, IContactRepository repo) =>
{
    var updated = await repo.UpdateAsync(id, request);
    return updated is null ? Results.NotFound() : Results.Ok(updated);
});

// DELETE /contacts/{id}
app.MapDelete("/contacts/{id:int}", async (int id, IContactRepository repo) =>
{
    var deleted = await repo.DeleteAsync(id);
    return deleted ? Results.NoContent() : Results.NotFound();
});

app.Run();
