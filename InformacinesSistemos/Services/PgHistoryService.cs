using InformacinesSistemos.Models;
using Npgsql;

namespace InformacinesSistemos.Services
{
    public class PgHistoryService : IHistoryService
    {
        private readonly string _connString;

        public PgHistoryService(IConfiguration cfg)
        {
            _connString = cfg.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("Nerasta ConnectionStrings:DefaultConnection.");
        }

        public async Task AddEntryAsync(int naudotojasId, int zaidimoId, string tipas, DateTime paskutinisMokejimas)
        {
            await using var conn = new NpgsqlConnection(_connString);
            await conn.OpenAsync();

            var sql = """
                INSERT INTO VartotojoSandoris
                    (fk_NaudotojasAsmensKodas, fk_ZaidimasZaidimoId, Tipas, Busena, PaskutinisMokejimas)
                VALUES
                    (@nk, @zk, @tipas, 'Aktyvi', @pm);
                """;

            await using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@nk", naudotojasId);
            cmd.Parameters.AddWithValue("@zk", zaidimoId);
            cmd.Parameters.AddWithValue("@tipas", tipas);
            cmd.Parameters.AddWithValue("@pm", paskutinisMokejimas);

            await cmd.ExecuteNonQueryAsync();
        }

        public async Task<List<VartotojoSandoris>> GetForUserAsync(int naudotojasId)
        {
            var list = new List<VartotojoSandoris>();

            await using var conn = new NpgsqlConnection(_connString);
            await conn.OpenAsync();

            var sql = """
                SELECT
                    s.id,
                    s.fk_NaudotojasAsmensKodas,
                    s.fk_ZaidimasZaidimoId,
                    s.tipas,
                    s.busena,
                    s.paskutinisMokejimas,
                    z.aprasymas AS pavadinimas
                FROM VartotojoSandoris s
                JOIN Zaidimas z ON z.zaidimoid = s.fk_ZaidimasZaidimoId
                WHERE s.fk_NaudotojasAsmensKodas = @nk
                ORDER BY s.paskutinisMokejimas DESC, s.id DESC;
                """;

            await using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@nk", naudotojasId);

            await using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                list.Add(new VartotojoSandoris
                {
                    Id = reader.GetInt32(reader.GetOrdinal("id")),
                    NaudotojasId = reader.GetInt32(reader.GetOrdinal("fk_NaudotojasAsmensKodas")),
                    ZaidimoId = reader.GetInt32(reader.GetOrdinal("fk_ZaidimasZaidimoId")),
                    Tipas = reader.GetString(reader.GetOrdinal("tipas")),
                    Busena = reader.GetString(reader.GetOrdinal("busena")),
                    PaskutinisMokejimas = reader.GetDateTime(reader.GetOrdinal("paskutinisMokejimas")),
                    ZaidimoPavadinimas = reader.GetString(reader.GetOrdinal("pavadinimas"))
                });
            }

            return list;
        }

        public async Task CancelRentalAsync(int sandorioId)
        {
            await using var conn = new NpgsqlConnection(_connString);
            await conn.OpenAsync();

            var sql = """
                UPDATE VartotojoSandoris
                SET Busena = 'Atšaukta'
                WHERE Id = @id AND Tipas = 'Nuoma';
                """;

            await using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@id", sandorioId);

            await cmd.ExecuteNonQueryAsync();
        }
    }
}
