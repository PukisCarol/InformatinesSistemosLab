using InformacinesSistemos.Models;
using Npgsql;

namespace InformacinesSistemos.Services;

public class PgUserService : IUserService
{
    private readonly string _connString;

    public PgUserService(IConfiguration cfg)
    {
        _connString = cfg.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Nerasta ConnectionStrings:DefaultConnection konfigūracija.");
    }

    // Get all users
    public async Task<List<Naudotojas>> GetAllAsync()
    {
        var users = new List<Naudotojas>();

        await using var conn = new NpgsqlConnection(_connString);
        await conn.OpenAsync();

        var sql = @"
            SELECT *
            FROM naudotojas
            ORDER BY paskyrosbusena, role, vardas;
        ";

        await using var cmd = new NpgsqlCommand(sql, conn);
        await using var reader = await cmd.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            users.Add(MapNaudotojas(reader));
        }

        return users;
    }

    // Get user by email
    public async Task<Naudotojas?> GetByEmailAsync(string email)
    {
        await using var conn = new NpgsqlConnection(_connString);
        await conn.OpenAsync();

        var sql = "SELECT * FROM naudotojas WHERE epastas = @email LIMIT 1;";
        await using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@email", email);

        await using var reader = await cmd.ExecuteReaderAsync();
        if (await reader.ReadAsync())
            return MapNaudotojas(reader);

        return null;
    }

    // Update user status
    public async Task UpdateStatusAsync(int naudotojasId, string paskyrosBusena)
    {
        await using var conn = new NpgsqlConnection(_connString);
        await conn.OpenAsync();

        var sql = "UPDATE naudotojas SET paskyrosbusena = @status WHERE asmenskodas = @id;";
        await using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@status", paskyrosBusena);
        cmd.Parameters.AddWithValue("@id", naudotojasId);

        await cmd.ExecuteNonQueryAsync();
    }

    // Update user role
    public async Task UpdateRoleAsync(int naudotojasId, string role)
    {
        await using var conn = new NpgsqlConnection(_connString);
        await conn.OpenAsync();

        var sql = "UPDATE naudotojas SET role = @role WHERE asmenskodas = @id;";
        await using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@role", role);
        cmd.Parameters.AddWithValue("@id", naudotojasId);

        await cmd.ExecuteNonQueryAsync();
    }

    // Map database row to Naudotojas object
    private static Naudotojas MapNaudotojas(NpgsqlDataReader r)
    {
        return new Naudotojas
        {
            NaudotojasId = r.GetInt32(r.GetOrdinal("asmenskodas")), // assuming asmenskodas is PK
            Vardas = r.GetString(r.GetOrdinal("vardas")),
            Pavarde = r.GetString(r.GetOrdinal("pavarde")),
            Slaptazodis = r.GetString(r.GetOrdinal("slaptažodis")),
            Role = r.GetString(r.GetOrdinal("role")),
            RegistracijosData = r.IsDBNull(r.GetOrdinal("registracijosData")) ? DateTime.MinValue : r.GetDateTime(r.GetOrdinal("registracijosData")),
            PaskirosBusena = r.GetString(r.GetOrdinal("paskyrosbusena")),
            TelefonoNumeris = r.GetString(r.GetOrdinal("telefononumeris")),
            ElPastas = r.GetString(r.GetOrdinal("epastas")),
            fk_TeisesTeisesId = r.GetInt32(r.GetOrdinal("fk_teisesteisesid"))
        };
    }
}