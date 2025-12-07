using InformacinesSistemos.Models;
using Npgsql;

namespace InformacinesSistemos.Services
{
    public class PgReviewService : IReviewService
    {
        private readonly string _connString;

        public PgReviewService(IConfiguration cfg)
        {
            _connString = cfg.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("Nerasta ConnectionStrings:DefaultConnection konfigūracija.");
        }

        public async Task<List<Atsiliepimas>> GetAllAsync()
        {
            var reviews = new List<Atsiliepimas>();

            await using var conn = new NpgsqlConnection(_connString);
            await conn.OpenAsync();

            var sql = @"
                SELECT 
                    AtsiliepimoId, Data, AtsiliepimoTeksats, Ivertinimas,
                    fk_ZaidimasZaidimoId, fk_NaudotojasAsmensKodas
                FROM Atsiliepimas
                ORDER BY Ivertinimas, AtsiliepimoTeksats;
            ";

            await using var cmd = new NpgsqlCommand(sql, conn);
            await using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                reviews.Add(new Atsiliepimas
                {
                    AtsiliepimoId = reader.GetInt32(reader.GetOrdinal("AtsiliepimoId")),
                    Data = reader.GetDateTime(reader.GetOrdinal("Data")),
                    AtsiliepimoTekstas = reader.GetString(reader.GetOrdinal("AtsiliepimoTeksats")),
                    Ivertinimas = reader.GetInt32(reader.GetOrdinal("Ivertinimas")),
                    ZaidimoId = reader.GetInt32(reader.GetOrdinal("fk_ZaidimasZaidimoId")),
                    NaudotojasId = reader.GetInt32(reader.GetOrdinal("fk_NaudotojasAsmensKodas"))
                });
            }

            return reviews;
        }

        public async Task AcceptReviewAsync(int reviewId)
        {
            // Table has no "accepted" column; implement as needed.
            await Task.CompletedTask;
        }

        public async Task DeleteReviewAsync(int reviewId)
        {
            await using var conn = new NpgsqlConnection(_connString);
            await conn.OpenAsync();

            var sql = "DELETE FROM Atsiliepimas WHERE AtsiliepimoId = @id;";

            await using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@id", reviewId);

            await cmd.ExecuteNonQueryAsync();
        }
        public async Task AddReviewAsync(Atsiliepimas review)
        {
            await using var conn = new NpgsqlConnection(_connString);
            await conn.OpenAsync();

            var sql = @"
                INSERT INTO atsiliepimas
                (atsiliepimoid, data, atsiliepimotekstsats, ivertinimas, fk_zaidimaszaidimoid, fk_naudotojasasmenskodas)
                VALUES (@id, @d, @tekstas, @iv, @gid, @uid);
            ";

            await using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@id", review.AtsiliepimoId);
            cmd.Parameters.AddWithValue("@d", review.Data);
            cmd.Parameters.AddWithValue("@tekstas", review.AtsiliepimoTekstas);
            cmd.Parameters.AddWithValue("@iv", review.Ivertinimas);
            cmd.Parameters.AddWithValue("@gid", review.ZaidimoId);
            cmd.Parameters.AddWithValue("@uid", review.NaudotojasId);

            await cmd.ExecuteNonQueryAsync();
        }
    }
}
