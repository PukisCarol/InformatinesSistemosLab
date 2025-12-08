using System.Collections.Generic;
using System.Threading.Tasks;
using InformacinesSistemos.Models;
using Npgsql;

namespace InformacinesSistemos.Services
{
    public class ReviewService : IReviewService
    {
        private readonly string _connString;

        public ReviewService(IConfiguration cfg)
        {
            _connString = cfg.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("Nerasta ConnectionStrings:DefaultConnection.");
        }

        public async Task<List<Atsiliepimas>> GetAllAsync()
        {
            var list = new List<Atsiliepimas>();

            await using var conn = new NpgsqlConnection(_connString);
            await conn.OpenAsync();

            var sql = """
                SELECT atsiliepimoid, data, atsiliepimotekstas, ivertinimas
                FROM Atsiliepimas
                ORDER BY data DESC;
                """;

            await using var cmd = new NpgsqlCommand(sql, conn);
            await using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                list.Add(new Atsiliepimas
                {
                    AtsiliepimoId = reader.GetInt32(reader.GetOrdinal("atsiliepimoid")),
                    Data = reader.GetDateTime(reader.GetOrdinal("data")),
                    AtsiliepimoTekstas = reader.GetString(reader.GetOrdinal("atsiliepimotekstas")),
                    Ivertinimas = reader.GetInt32(reader.GetOrdinal("ivertinimas"))
                });
            }

            return list;
        }

        public async Task AddReviewAsync(Atsiliepimas review)
        {
            await using var conn = new NpgsqlConnection(_connString);
            await conn.OpenAsync();

            var sql = """
                INSERT INTO Atsiliepimas (data, atsiliepimotekstas, ivertinimas)
                VALUES (@dt, @txt, @iv);
                """;

            await using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@dt", review.Data);
            cmd.Parameters.AddWithValue("@txt", review.AtsiliepimoTekstas ?? string.Empty);
            cmd.Parameters.AddWithValue("@iv", review.Ivertinimas);

            await cmd.ExecuteNonQueryAsync();
        }

        public async Task AcceptReviewAsync(int reviewId)
        {
            await using var conn = new NpgsqlConnection(_connString);
            await conn.OpenAsync();

            var sql = """
                UPDATE Atsiliepimas
                SET /* Pvz. Busena = 'Patvirtintas' */
                    data = data -- čia uždėk realų lauką, jei turi
                WHERE AtsiliepimoId = @id;
                """;

            await using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@id", reviewId);

            await cmd.ExecuteNonQueryAsync();
        }

        public async Task DeleteReviewAsync(int reviewId)
        {
            await using var conn = new NpgsqlConnection(_connString);
            await conn.OpenAsync();

            var sql = """
                DELETE FROM Atsiliepimas
                WHERE AtsiliepimoId = @id;
                """;

            await using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@id", reviewId);

            await cmd.ExecuteNonQueryAsync();
        }
    }
}
