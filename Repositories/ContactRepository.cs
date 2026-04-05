using Dapper;
using MyServer.Models;
using Npgsql;

namespace MyServer.Repositories;

public interface IContactRepository
{
    Task<IEnumerable<Contact>> GetAllAsync();
    Task<Contact?> GetByIdAsync(int id);
    Task<Contact> CreateAsync(ContactRequest request);
    Task<Contact?> UpdateAsync(int id, ContactRequest request);
    Task<bool> DeleteAsync(int id);
}

public class ContactRepository(string connectionString) : IContactRepository
{
    public async Task<IEnumerable<Contact>> GetAllAsync()
    {
        await using var conn = new NpgsqlConnection(connectionString);
        return await conn.QueryAsync<Contact>(
            "SELECT id, first_name AS FirstName, last_name AS LastName, phone_number AS PhoneNumber FROM contacts ORDER BY last_name, first_name");
    }

    public async Task<Contact?> GetByIdAsync(int id)
    {
        await using var conn = new NpgsqlConnection(connectionString);
        return await conn.QuerySingleOrDefaultAsync<Contact>(
            "SELECT id, first_name AS FirstName, last_name AS LastName, phone_number AS PhoneNumber FROM contacts WHERE id = @id",
            new { id });
    }

    public async Task<Contact> CreateAsync(ContactRequest request)
    {
        await using var conn = new NpgsqlConnection(connectionString);
        var id = await conn.ExecuteScalarAsync<int>(
            "INSERT INTO contacts (first_name, last_name, phone_number) VALUES (@FirstName, @LastName, @PhoneNumber) RETURNING id",
            request);
        return (await GetByIdAsync(id))!;
    }

    public async Task<Contact?> UpdateAsync(int id, ContactRequest request)
    {
        await using var conn = new NpgsqlConnection(connectionString);
        var affected = await conn.ExecuteAsync(
            "UPDATE contacts SET first_name = @FirstName, last_name = @LastName, phone_number = @PhoneNumber WHERE id = @id",
            new { request.FirstName, request.LastName, request.PhoneNumber, id });
        return affected == 0 ? null : await GetByIdAsync(id);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        await using var conn = new NpgsqlConnection(connectionString);
        var affected = await conn.ExecuteAsync("DELETE FROM contacts WHERE id = @id", new { id });
        return affected > 0;
    }
}
