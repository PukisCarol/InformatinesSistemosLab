using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using InformacinesSistemos.Models;
using Microsoft.Extensions.Configuration;
using Npgsql;

namespace InformacinesSistemos.Services
{
    public class ReviewService : IReviewService
    {
        private readonly string _connString;

        public ReviewService(IConfiguration cfg)
        {
            _connString = cfg.GetConnectionString("DefaultConnection")
                         ?? throw new InvalidOperationException("Nerasta 'DefaultConnection' jungtis.");
        }

        // Pagalbinis metodas – atidaryti jungtį
        private NpgsqlConnection CreateConnection()
            => new NpgsqlConnection(_connString);

        // ======================
        // VISI ATSILIEPIMAI
        // ======================
        public async Task<List<Atsiliepimas>> GetAllAsync()
        {
            var list = new List<Atsiliepimas>();

            await using var conn = CreateConnection();
            await conn.OpenAsync();

            // prisitaikyk prie tikrų lentelės ir stulpelių pavadinimų, jei reikia
            var sql = @"
                SELECT atsiliepimoid,
                       data,
                       tekstas,
                       ivertinimas,
                       fk_naudotojasasmenskodas,
                       fk_zaidimaszaidimoid
                FROM ""Atsiliepimas""
            ";

            await using var cmd = new NpgsqlCommand(sql, conn);
            await using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                var review = new Atsiliepimas
                {
                    AtsiliepimoId = reader.GetInt32(0),
                    Data = reader.GetDateTime(1),
                    AtsiliepimoTekstas = reader.GetString(2),
                    Ivertinimas = reader.GetInt32(3),
                    NaudotojasId = reader.GetInt32(4),
                    ZaidimoId = reader.GetInt32(5)
                };

                list.Add(review);
            }

            return list;
        }

        // ======================
        // ATSILIEPIMAI KONKREČIAM ŽAIDIMUI
        // ======================
        public async Task<List<Atsiliepimas>> GetForGameAsync(int gameId)
        {
            // paprastai užteks filtruoti jau gautą sąrašą
            var all = await GetAllAsync();
            return all.Where(r => r.ZaidimoId == gameId).ToList();
        }

        // ======================
        // PRIDĖTI ATSILIEPIMĄ
        // ======================
        public async Task AddReviewAsync(Atsiliepimas review)
        {
            await using var conn = new NpgsqlConnection(_connString);
            await conn.OpenAsync();

            const string sql = @"
        INSERT INTO atsiliepimas
            (data, atsiliepimotekstas, ivertinimas,
             fk_zaidimaszaidimoid, fk_naudotojasasmenskodas)
        VALUES
            (@data, @tekstas, @ivertinimas, @zaidimasId, @naudotojasId);";

            await using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("data", review.Data);
            cmd.Parameters.AddWithValue("tekstas", review.AtsiliepimoTekstas);
            cmd.Parameters.AddWithValue("ivertinimas", review.Ivertinimas);
            cmd.Parameters.AddWithValue("zaidimasId", review.ZaidimoId);
            cmd.Parameters.AddWithValue("naudotojasId", review.NaudotojasId);

            await cmd.ExecuteNonQueryAsync();
        }


        // ======================
        // PATVIRTINTI ATSILIEPIMĄ (jei nenaudoji – tiesiog no-op)
        // ======================
        public Task AcceptReviewAsync(int id)
        {
            // Jei neturi stulpelio „patvirtintas“ – kol kas nedarom nieko.
            // Galima vėliau išplėsti.
            return Task.CompletedTask;
        }

        // ======================
        // IŠTRINTI ATSILIEPIMĄ
        // ======================
        public async Task DeleteReviewAsync(int id)
        {
            await using var conn = CreateConnection();
            await conn.OpenAsync();

            var sql = @"DELETE FROM ""Atsiliepimas"" WHERE atsiliepimoid = @id";
            await using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("id", id);

            await cmd.ExecuteNonQueryAsync();
        }
    }
}
