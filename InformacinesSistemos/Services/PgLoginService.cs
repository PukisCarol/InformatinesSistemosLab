
using Npgsql;
using System.Security.Cryptography;

namespace InformacinesSistemos.Services
{
    public class PgLoginService : ILoginService
    {
        private readonly string _connString;

        public PgLoginService(IConfiguration cfg)
        {
            _connString = cfg.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("Nerasta ConnectionStrings:DefaultConnection konfigūracija.");
        }

        public async Task<LoginResult> AuthenticateAsync(string email, string password)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
                    return new LoginResult { Success = false, Error = "El. paštas ir slaptažodis privalomi." };

                await using var conn = new NpgsqlConnection(_connString);
                await conn.OpenAsync();

                // Paimame naudotoją, slaptažodžio hash ir rolę (iš teises.tipas arba naudotojas.role)
                await using var cmd = new NpgsqlCommand(@"
                    SELECT 
                        n.epastas,
                        n.""slaptažodis"" AS pwd,  -- diakritika cituojama kabutėmis
                        COALESCE(t.tipas, n.role) AS role,
                        n.paskyrosbusena,
                        t.galiojimodata
                    FROM public.naudotojas n
                    LEFT JOIN public.teises t ON t.teisesid = n.fk_teisesteisesid
                    WHERE n.epastas = @ep
                    LIMIT 1;", conn);

                cmd.Parameters.AddWithValue("@ep", email);

                await using var reader = await cmd.ExecuteReaderAsync();
                if (!await reader.ReadAsync())
                {
                    return new LoginResult { Success = false, Error = "Naudotojas su tokiu el. paštu nerastas." }; // 404
                }

                var dbPassword = reader["pwd"] as string;
                var role = reader["role"] as string;
                var status = reader["paskyrosbusena"] as string;
                var galiojimoObj = reader["galiojimodata"];

                // Paskyros būsena
                if (!string.IsNullOrWhiteSpace(status) && !status.Equals("Aktyvi", StringComparison.OrdinalIgnoreCase))
                {
                    return new LoginResult { Success = false, Error = "Paskyra neaktyvi." }; // 403
                }

                // Teisės galiojimas, jei nurodytas
                if (galiojimoObj is DateTime galiojimodata && galiojimodata.Date < DateTime.UtcNow.Date)
                {
                    return new LoginResult { Success = false, Error = "Naudotojo teisės nebegalioja." }; // 403
                }

                // Slaptažodžio tikrinimas
                if (string.IsNullOrEmpty(dbPassword) || !VerifyPassword(password, dbPassword))
                {
                    return new LoginResult { Success = false, Error = "Neteisingas slaptažodis." }; // 401
                }

                // Rolė
                if (string.IsNullOrWhiteSpace(role))
                    role = "User";

                return new LoginResult { Success = true, Role = role };
            }
            catch (PostgresException pgex)
            {
                return new LoginResult { Success = false, Error = $"DB klaida: {pgex.SqlState} {pgex.MessageText}" };
            }
            catch (Exception ex)
            {
                return new LoginResult { Success = false, Error = $"Išimtis: {ex.Message}" };
            }
        }

        // PBKDF2 slaptažodžio patikra: tikisi formatą "salt:hash" (Base64), kaip registracijos metu
        private static bool VerifyPassword(string password, string stored)
        {
            var parts = stored.Split(':');
            if (parts.Length != 2) return false;

            var salt = Convert.FromBase64String(parts[0]);
            var expectedHash = Convert.FromBase64String(parts[1]);

            using var rfc2898 = new Rfc2898DeriveBytes(password, salt, 100_000, HashAlgorithmName.SHA256);
            var computed = rfc2898.GetBytes(expectedHash.Length);

            // Pastaba: saugumo sumetimais naudokite constant-time palyginimą
            return FixedTimeEquals(computed, expectedHash);
        }

        private static bool FixedTimeEquals(ReadOnlySpan<byte> a, ReadOnlySpan<byte> b)
        {
            if (a.Length != b.Length) return false;
            int diff = 0;
            for (int i = 0; i < a.Length; i++)
                diff |= a[i] ^ b[i];
            return diff == 0;
        }
    }
}
