using System.Data;
using Dapper;
using MyServer.Models;
using Npgsql;
using NpgsqlTypes;

namespace MyServer.Repositories;

public interface IContactRepository
{
    Task<IEnumerable<Contact>> GetAllAsync();
    Task<Contact?> GetByIdAsync(int id);
    Task<IEnumerable<Contact>> SearchAsync(string query);
    Task<Contact> CreateAsync(ContactRequest request);
    Task<Contact?> UpdateAsync(int id, ContactRequest request);
    Task<bool> DeleteAsync(int id);
    Task<Contact> UpsertAsync(int? id, ContactRequest request);
}

public class ContactRepository(string connectionString) : IContactRepository
{
    public async Task<IEnumerable<Contact>> GetAllAsync()
    {
        await using var conn = new NpgsqlConnection(connectionString);
        var contactDict = new Dictionary<int, Contact>();
        await conn.QueryAsync<Contact, Company, Contact>(
            @"SELECT c.id, c.first_name AS FirstName, c.last_name AS LastName, c.phone_number AS PhoneNumber,
                     co.id, co.name AS Name
              FROM contacts c
              LEFT JOIN contact_companies cc ON cc.contact_id = c.id
              LEFT JOIN companies co ON co.id = cc.company_id
              ORDER BY c.last_name, c.first_name",
            (contact, company) =>
            {
                if (!contactDict.TryGetValue(contact.Id, out var existing))
                {
                    existing = contact;
                    contactDict[contact.Id] = existing;
                }
                if (company is not null && company.Id > 0)
                    existing.Companies.Add(company);
                return existing;
            },
            splitOn: "id");
        return contactDict.Values;
    }

    public async Task<Contact?> GetByIdAsync(int id)
    {
        await using var conn = new NpgsqlConnection(connectionString);
        var contactDict = new Dictionary<int, Contact>();
        await conn.QueryAsync<Contact, Company, Contact>(
            @"SELECT c.id, c.first_name AS FirstName, c.last_name AS LastName, c.phone_number AS PhoneNumber,
                     co.id, co.name AS Name
              FROM contacts c
              LEFT JOIN contact_companies cc ON cc.contact_id = c.id
              LEFT JOIN companies co ON co.id = cc.company_id
              WHERE c.id = @id",
            (contact, company) =>
            {
                if (!contactDict.TryGetValue(contact.Id, out var existing))
                {
                    existing = contact;
                    contactDict[contact.Id] = existing;
                }
                if (company is not null && company.Id > 0)
                    existing.Companies.Add(company);
                return existing;
            },
            new { id },
            splitOn: "id");
        return contactDict.Values.FirstOrDefault();
    }

    public async Task<IEnumerable<Contact>> SearchAsync(string query)
    {
        await using var conn = new NpgsqlConnection(connectionString);
        var contactDict = new Dictionary<int, Contact>();
        await conn.QueryAsync<Contact, Company, Contact>(
            @"SELECT s.id, s.first_name AS FirstName, s.last_name AS LastName, s.phone_number AS PhoneNumber,
                     co.id, co.name AS Name
              FROM search_contacts(@query) s
              LEFT JOIN contact_companies cc ON cc.contact_id = s.id
              LEFT JOIN companies co ON co.id = cc.company_id",
            (contact, company) =>
            {
                if (!contactDict.TryGetValue(contact.Id, out var existing))
                {
                    existing = contact;
                    contactDict[contact.Id] = existing;
                }
                if (company is not null && company.Id > 0)
                    existing.Companies.Add(company);
                return existing;
            },
            new { query },
            splitOn: "id");
        return contactDict.Values;
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

    // Demonstrates calling a PostgreSQL stored procedure (CALL syntax, INOUT parameter)
    public async Task<Contact> UpsertAsync(int? id, ContactRequest request)
    {
        await using var conn = new NpgsqlConnection(connectionString);
        await conn.OpenAsync();

        await using var cmd = new NpgsqlCommand(
            "CALL upsert_contact(@p_first_name, @p_last_name, @p_phone_number, @p_id)", conn);

        cmd.Parameters.AddWithValue("p_first_name", request.FirstName);
        cmd.Parameters.AddWithValue("p_last_name", request.LastName);
        cmd.Parameters.AddWithValue("p_phone_number", request.PhoneNumber);

        var idParam = new NpgsqlParameter("p_id", NpgsqlDbType.Integer)
        {
            Direction = ParameterDirection.InputOutput,
            Value = id.HasValue ? (object)id.Value : DBNull.Value
        };
        cmd.Parameters.Add(idParam);

        await cmd.ExecuteNonQueryAsync();

        var resultId = (int)idParam.Value!;
        return (await GetByIdAsync(resultId))!;
    }
}
