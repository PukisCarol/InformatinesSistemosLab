using System.Collections.Generic;
using System.Threading.Tasks;
using InformacinesSistemos.Models;
using Microsoft.Extensions.Configuration;
using Npgsql;

namespace InformacinesSistemos.Services
{
    public class PgReviewService : IReviewService
    {
        private readonly string _connString;

        public PgReviewService(IConfiguration cfg)
        {
            _connString = cfg.GetConnectionString("DefaultConnection")
                          ?? throw new InvalidOperationException("Nerasta DB jungtis 'DefaultConnection'.");
        }

        // --- Visų atsiliepimų sąrašas (pvz. admin/moderatoriams) ---
        public async Task<List<Atsiliepimas>> GetAllAsync()
        {
            var list = new List<Atsiliepimas>();

            await using var conn = new NpgsqlConnection(_connString);
            await conn.OpenAsync();

            const string sql = @"
                SELECT atsiliepimoid, data, atsiliepimotekstas, ivertinimas,
                       fk_zaidimaszaidimoid, fk_naudotojasasmenskodas
                FROM atsiliepimas";

            await using var cmd = new NpgsqlCommand(sql, conn);
            await using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                list.Add(new Atsiliepimas
                {
                    AtsiliepimoId = reader.GetInt32(0),
                    Data = reader.GetDateTime(1),
                    AtsiliepimoTekstas = reader.GetString(2),
                    Ivertinimas = reader.GetInt32(3),
                    ZaidimoId = reader.GetInt32(4),
                    NaudotojasId = reader.GetInt32(5)
                });
            }

            return list;
        }

        // --- Atsiliepimai konkrečiam žaidimui ---
        public async Task<List<Atsiliepimas>> GetForGameAsync(int zaidimoId)
        {
            var list = new List<Atsiliepimas>();

            await using var conn = new NpgsqlConnection(_connString);
            await conn.OpenAsync();

            const string sql = @"
                SELECT a.atsiliepimoid,
       a.data,
       a.atsiliepimotekstas,
       a.ivertinimas,
       a.fk_zaidimaszaidimoid,
       a.fk_naudotojasasmenskodas,
       n.vardas,     -- add this
       n.pavarde     -- add this
FROM atsiliepimas a
JOIN naudotojas n ON n.asmenskodas = a.fk_naudotojasasmenskodas
WHERE a.fk_zaidimaszaidimoid = @id;";

            await using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("id", zaidimoId);

            await using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                list.Add(new Atsiliepimas
                {
                    AtsiliepimoId = reader.GetInt32(0),
                    Data = reader.GetDateTime(1),
                    AtsiliepimoTekstas = reader.GetString(2),
                    Ivertinimas = reader.GetInt32(3),
                    ZaidimoId = reader.GetInt32(4),
                    NaudotojasId = reader.GetInt32(5),
                    Vardas = reader.GetString(6),
                    Pavarde = reader.GetString(7)
                });
            }

            return list;
        }

        // --- Pateikti naują atsiliepimą ---
        public async Task AddReviewAsync(Atsiliepimas review)
        {
            await using var conn = new NpgsqlConnection(_connString);
            await conn.OpenAsync();

            const string sql = @"
                INSERT INTO atsiliepimas
                    (data, atsiliepimotekstas, ivertinimas,
                     fk_zaidimaszaidimoid, fk_naudotojasasmenskodas)
                VALUES
                    (@data, @tekstas, @ivertinimas, @zaidimoId, @naudotojasAk);";

            await using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("data", review.Data);
            cmd.Parameters.AddWithValue("tekstas", review.AtsiliepimoTekstas);
            cmd.Parameters.AddWithValue("ivertinimas", review.Ivertinimas);
            cmd.Parameters.AddWithValue("zaidimoId", review.ZaidimoId);
            cmd.Parameters.AddWithValue("naudotojasAk", review.NaudotojasId); // čia = AsmensKodas

            await cmd.ExecuteNonQueryAsync();
        }

        // jei kol kas nereikia – galima palikti tuščius stub'us
        public Task AcceptReviewAsync(int id) => Task.CompletedTask;
        public async Task DeleteReviewAsync(int id)
        {
            await using var conn = new NpgsqlConnection(_connString);
            await conn.OpenAsync();

            const string sql = @"DELETE FROM atsiliepimas WHERE atsiliepimoid = @id";

            await using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("id", id);

            await cmd.ExecuteNonQueryAsync();
        }
    }
}
