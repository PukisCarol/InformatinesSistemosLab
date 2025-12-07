using InformacinesSistemos.Models;
using Npgsql;

namespace InformacinesSistemos.Services
{
    public class PgGamesService : IGameService
    {
        private readonly string _connString;

        public PgGamesService(IConfiguration cfg)
        {
            _connString = cfg.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("Nerasta ConnectionStrings:DefaultConnection konfigūracija.");
        }

        // ----------------------------------------------------------
        // GET ALL GAMES
        // ----------------------------------------------------------
        public async Task<List<Zaidimas>> GetAllAsync()
        {
            var results = new List<Zaidimas>();

            await using var conn = new NpgsqlConnection(_connString);
            await conn.OpenAsync();

            var sql = @"
                SELECT 
                    z.*,
                    n.*,
                    sz.*,
                    kz.*,
                    za.zanroid AS zanroid,
                    za.pavadinimas AS zanro_pavadinimas
                FROM zaidimas z
                LEFT JOIN naudotojas n ON n.asmenskodas = z.fk_naudotojasasmenskodas
                LEFT JOIN stalozaidimai sz ON sz.zaidimoid = z.zaidimoid
                LEFT JOIN kompiuteriniaizaidimai kz ON kz.zaidimoid = z.zaidimoid
                LEFT JOIN zanraspriklauso zp ON zp.fk_zaidimaszaidimoid = z.zaidimoid
                LEFT JOIN zanras za ON za.zanroid = zp.fk_zanraszanroid;
            ";

            await using var cmd = new NpgsqlCommand(sql, conn);
            await using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                var game = MapZaidimas(reader);
                results.Add(game);
            }

            return results;
        }

        // ----------------------------------------------------------
        // GET GAME BY ID
        // ----------------------------------------------------------
        public async Task<Zaidimas?> GetByIdAsync(int id)
        {
            await using var conn = new NpgsqlConnection(_connString);
            await conn.OpenAsync();

            var sql = @"
                SELECT 
                    z.*,
                    n.*,
                    sz.*,
                    kz.*,
                    za.zanroid AS zanroid,
                    za.pavadinimas AS zanro_pavadinimas
                FROM zaidimas z
                LEFT JOIN naudotojas n ON n.asmenskodas = z.fk_naudotojasasmenskodas
                LEFT JOIN stalozaidimai sz ON sz.zaidimoid = z.zaidimoid
                LEFT JOIN kompiuteriniaizaidimai kz ON kz.zaidimoid = z.zaidimoid
                LEFT JOIN zanraspriklauso zp ON zp.fk_zaidimaszaidimoid = z.zaidimoid
                LEFT JOIN zanras za ON za.zanroid = zp.fk_zanraszanroid
                WHERE z.zaidimoid = @id;
            ";

            await using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@id", id);

            await using var reader = await cmd.ExecuteReaderAsync();
            if (await reader.ReadAsync())
                return MapZaidimas(reader);

            return null;
        }

        // ----------------------------------------------------------
        // ADD GAME
        // ----------------------------------------------------------
        public async Task<Zaidimas> AddAsync(Zaidimas game)
        {
            await using var conn = new NpgsqlConnection(_connString);
            await conn.OpenAsync();

            // Insert main game
            var sqlGame = @"
                INSERT INTO zaidimas
                (kaina, reitingas, amziauscenzas, kurejas, zaidejuskaicius, aprasymas, fk_naudotojasasmenskodas)
                VALUES (@k, @r, @a, @kr, @zs, @ap, @sid)
                RETURNING zaidimoid;
            ";

            await using (var cmd = new NpgsqlCommand(sqlGame, conn))
            {
                cmd.Parameters.AddWithValue("@k", game.Kaina);
                cmd.Parameters.AddWithValue("@r", game.Reitingas);
                cmd.Parameters.AddWithValue("@a", game.AmziausCenzas);
                cmd.Parameters.AddWithValue("@kr", game.Kurejas);
                cmd.Parameters.AddWithValue("@zs", game.ZaidejuSkaicius);
                cmd.Parameters.AddWithValue("@ap", game.Aprasymas);
                cmd.Parameters.AddWithValue("@sid", game.SavininkoId);

                game.ZaidimoId = (int)(await cmd.ExecuteScalarAsync() ?? 0);
            }

            // Insert table game
            if (game.Stalo != null)
            {
                var sqlStalo = @"
                    INSERT INTO stalozaidimai
                    (zaidimoid, ilgis, plotis, aukstis, trukme, svoris)
                    VALUES (@id, @il, @pl, @au, @tr, @sv);
                ";
                await using var cmd = new NpgsqlCommand(sqlStalo, conn);
                cmd.Parameters.AddWithValue("@id", game.ZaidimoId);
                cmd.Parameters.AddWithValue("@il", game.Stalo.Ilgis);
                cmd.Parameters.AddWithValue("@pl", game.Stalo.Plotis);
                cmd.Parameters.AddWithValue("@au", game.Stalo.Aukstis);
                cmd.Parameters.AddWithValue("@tr", game.Stalo.Trukme);
                cmd.Parameters.AddWithValue("@sv", game.Stalo.Svoris);

                await cmd.ExecuteNonQueryAsync();
            }

            // Insert computer game
            if (game.Kompiuterinis != null)
            {
                var sqlComp = @"
                    INSERT INTO kompiuteriniaizaidimai
                    (zaidimoid, uzimamavietadiske, sisteminiaireikalavimai, os, platforma)
                    VALUES (@id, @v, @sr, @os, @pf);
                ";
                await using var cmd = new NpgsqlCommand(sqlComp, conn);
                cmd.Parameters.AddWithValue("@id", game.ZaidimoId);
                cmd.Parameters.AddWithValue("@v", game.Kompiuterinis.UzimamaVietaDiske);
                cmd.Parameters.AddWithValue("@sr", game.Kompiuterinis.SisteminiaiReikalavimai);
                cmd.Parameters.AddWithValue("@os", game.Kompiuterinis.OS);
                cmd.Parameters.AddWithValue("@pf", game.Kompiuterinis.Platforma);

                await cmd.ExecuteNonQueryAsync();
            }

            return game;
        }

        // ----------------------------------------------------------
        // ROW → OBJECT MAPPER
        // ----------------------------------------------------------
        private Zaidimas MapZaidimas(NpgsqlDataReader r)
        {
            var game = new Zaidimas
            {
                ZaidimoId = r.GetInt32(r.GetOrdinal("zaidimoid")),
                Kaina = r.GetDouble(r.GetOrdinal("kaina")),
                Reitingas = r.GetDouble(r.GetOrdinal("reitingas")),
                AmziausCenzas = r.GetInt32(r.GetOrdinal("amziauscenzas")),
                Kurejas = r.GetString(r.GetOrdinal("kurejas")),
                ZaidejuSkaicius = r.GetString(r.GetOrdinal("zaidejuskaicius")),
                Aprasymas = r.GetString(r.GetOrdinal("aprasymas")),
                SavininkoId = r.GetInt32(r.GetOrdinal("fk_naudotojasasmenskodas"))
            };

            // NAUDOTOJAS
            if (!r.IsDBNull(r.GetOrdinal("asmenskodas")))
            {
                game.Savininkas = new Naudotojas
                {
                    NaudotojasId = r.GetInt32(r.GetOrdinal("asmenskodas")),
                    Vardas = r.GetString(r.GetOrdinal("vardas")),
                    Pavarde = r.GetString(r.GetOrdinal("pavarde")),
                    ElPastas = r.GetString(r.GetOrdinal("epastas"))
                };
            }

            // ZANRAS
            if (!r.IsDBNull(r.GetOrdinal("zanroid")))
            {
                game.Zanras = new Zanras
                {
                    ZanroId = r.GetInt32(r.GetOrdinal("zanroid")),
                    Pavadinimas = r.GetString(r.GetOrdinal("zanro_pavadinimas"))
                };
            }

            // STALO ZAIDIMAS
            if (!r.IsDBNull(r.GetOrdinal("ilgis")))
            {
                game.Stalo = new StaloZaidimas
                {
                    ZaidimoId = game.ZaidimoId,
                    AmziausCenzas = r.GetInt32(r.GetOrdinal("amziauscenzas")),
                    ZaidejuSkaicius = r.GetString(r.GetOrdinal("zaidejuskaicius")),
                    Ilgis = r.GetDouble(r.GetOrdinal("ilgis")),
                    Plotis = r.GetDouble(r.GetOrdinal("plotis")),
                    Aukstis = r.GetDouble(r.GetOrdinal("aukstis")),
                    Trukme = r.GetInt32(r.GetOrdinal("trukme")),
                    Svoris = r.GetDouble(r.GetOrdinal("svoris"))
                };
            }

            // KOMPIUTERINIS ZAIDIMAS
            if (!r.IsDBNull(r.GetOrdinal("uzimamavietadiske")))
            {
                game.Kompiuterinis = new KompiuterinisZaidimas
                {
                    ZaidimoId = game.ZaidimoId,
                    UzimamaVietaDiske = r.GetDouble(r.GetOrdinal("uzimamavietadiske")),
                    SisteminiaiReikalavimai = r.GetString(r.GetOrdinal("sisteminiaireikalavimai")),
                    OS = r.GetString(r.GetOrdinal("os")),
                    Platforma = r.GetString(r.GetOrdinal("platforma"))
                };
            }

            return game;
        }

        public Task UpdateAsync(Zaidimas game)
        {
            throw new NotImplementedException();
        }
    }
}
