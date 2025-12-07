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

    // ======================================================
    // Bendri metodai
    // ======================================================

    public async Task<List<Naudotojas>> GetAllAsync()
    {
        var users = new List<Naudotojas>();

        await using var conn = new NpgsqlConnection(_connString);
        await conn.OpenAsync();

        var sql = """
            SELECT vardas,
                   pavarde,
                   slaptažodis,
                   asmenskodas,
                   telefononumeris,
                   epastas,
                   role,
                   registracijosdata,
                   paskyrosbusena,
                   fk_teisesteisesid
            FROM naudotojas;
            """;

        await using var cmd = new NpgsqlCommand(sql, conn);
        await using var reader = await cmd.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            users.Add(MapNaudotojas(reader));
        }

        return users;
    }
    public async Task UpdatePasswordAsync(int naudotojasId, string newPassword)
    {
        await using var conn = new NpgsqlConnection(_connString);
        await conn.OpenAsync();

        var sql = """
        UPDATE naudotojas
        SET slaptažodis = @pwd
        WHERE asmenskodas = @id;
        """;

        await using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@pwd", newPassword);   // <-- ČIA JAU ATEINA HASH
        cmd.Parameters.AddWithValue("@id", naudotojasId);

        await cmd.ExecuteNonQueryAsync();
    }


    public async Task UpdateStatusAsync(int naudotojasId, string paskyrosBusena)
    {
        await using var conn = new NpgsqlConnection(_connString);
        await conn.OpenAsync();

        var sql = """
            UPDATE naudotojas
            SET paskyrosbusena = @busena
            WHERE asmenskodas = @id;
            """;

        await using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@busena", paskyrosBusena);
        cmd.Parameters.AddWithValue("@id", naudotojasId);

        await cmd.ExecuteNonQueryAsync();
    }

    public async Task UpdateRoleAsync(int naudotojasId, string role)
    {
        await using var conn = new NpgsqlConnection(_connString);
        await conn.OpenAsync();

        var sql = """
            UPDATE naudotojas
            SET role = @role
            WHERE asmenskodas = @id;
            """;

        await using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@role", role);
        cmd.Parameters.AddWithValue("@id", naudotojasId);

        await cmd.ExecuteNonQueryAsync();
    }

    public async Task<Naudotojas?> GetByEmailAsync(string email)
    {
        await using var conn = new NpgsqlConnection(_connString);
        await conn.OpenAsync();

        var sql = """
            SELECT vardas,
                   pavarde,
                   slaptažodis,
                   asmenskodas,
                   telefononumeris,
                   epastas,
                   role,
                   registracijosdata,
                   paskyrosbusena,
                   fk_teisesteisesid
            FROM naudotojas
            WHERE epastas = @email
            LIMIT 1;
            """;

        await using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@email", email);

        await using var reader = await cmd.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            return MapNaudotojas(reader);
        }

        return null;
    }

    // ======================================================
    // Profilio redagavimas / paskyros šalinimas
    // ======================================================

    public async Task UpdateProfileAsync(Naudotojas user)
    {
        await using var conn = new NpgsqlConnection(_connString);
        await conn.OpenAsync();

        var sql = """
            UPDATE naudotojas
            SET vardas = @vardas,
                pavarde = @pavarde,
                telefononumeris = @tel,
                epastas = @email
            WHERE asmenskodas = @id;
            """;

        await using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@vardas", user.Vardas ?? string.Empty);
        cmd.Parameters.AddWithValue("@pavarde", user.Pavarde ?? string.Empty);
        cmd.Parameters.AddWithValue("@tel", user.TelefonoNumeris ?? string.Empty);
        cmd.Parameters.AddWithValue("@email", user.ElPastas ?? string.Empty);
        cmd.Parameters.AddWithValue("@id", user.NaudotojasId);

        await cmd.ExecuteNonQueryAsync();
    }

    /// <summary>
    /// „Ištrina“ paskyrą – pakeičia jos būseną į „Ištrinta“.
    /// (soft delete, kad nesulūžtų užsakymai ir t.t.)
    /// </summary>
    public async Task DeleteAsync(int naudotojasId)
    {
        await using var conn = new NpgsqlConnection(_connString);
        await conn.OpenAsync();

        var sql = """
            UPDATE naudotojas
            SET paskyrosbusena = 'Ištrinta'
            WHERE asmenskodas = @id;
            """;

        await using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@id", naudotojasId);

        await cmd.ExecuteNonQueryAsync();
    }

    // ======================================================
    // Pagalbinis mapperis
    // ======================================================

    private static Naudotojas MapNaudotojas(NpgsqlDataReader r)
    {
        return new Naudotojas
        {
            NaudotojasId = r.GetInt32(r.GetOrdinal("asmenskodas")),
            Vardas = r.GetString(r.GetOrdinal("vardas")),
            Pavarde = r.GetString(r.GetOrdinal("pavarde")),
            Slaptazodis = r.GetString(r.GetOrdinal("slaptažodis")),
            TelefonoNumeris = r.GetString(r.GetOrdinal("telefononumeris")),
            ElPastas = r.GetString(r.GetOrdinal("epastas")),
            Role = r.GetString(r.GetOrdinal("role")),
            RegistracijosData = r.IsDBNull(r.GetOrdinal("registracijosdata"))
                ? DateTime.MinValue
                : r.GetDateTime(r.GetOrdinal("registracijosdata")),
            PaskirosBusena = r.GetString(r.GetOrdinal("paskyrosbusena")),
            fk_TeisesTeisesId = r.GetInt32(r.GetOrdinal("fk_teisesteisesid"))
        };
    }
}
