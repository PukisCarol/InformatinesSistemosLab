
using Npgsql;
using System.Security.Cryptography;
using System.Text.RegularExpressions;

namespace InformacinesSistemos.Services
{
    public class PgRegistrationService : IRegistrationService
    {
        private readonly string _connString;

        public PgRegistrationService(IConfiguration cfg)
        {
            _connString = cfg.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("Nerasta ConnectionStrings:DefaultConnection konfigūracija.");
        }

        public async Task<RegisterResult> RegisterAsync(RegisterDto dto)
        {
            try
            {
                // Server-side validacijos (identinės esminėms UI taisyklėms)
                if (!IsValidServerSide(dto))
                    return new RegisterResult { Success = false, Error = "Netinkami registracijos duomenys." };

                var passwordHash = HashPassword(dto.SlaptazodisPlain);

                await using var conn = new NpgsqlConnection(_connString);
                await conn.OpenAsync();

                // Unikalumo patikra
                await using (var checkCmd = new NpgsqlCommand(
                    "SELECT 1 FROM public.naudotojas WHERE epastas = @ep OR asmenskodas = @ak LIMIT 1", conn))
                {
                    checkCmd.Parameters.AddWithValue("@ep", dto.ElPastas);
                    checkCmd.Parameters.AddWithValue("@ak", dto.AsmensKodas);
                    var exists = await checkCmd.ExecuteScalarAsync();
                    if (exists != null)
                        return new RegisterResult { Success = false, Error = "El. paštas arba asmens kodas jau naudojamas." };
                }

                // Įrašas
                await using (var cmd = new NpgsqlCommand(
                    @"INSERT INTO public.naudotojas
                      (asmenskodas, vardas, pavarde, slaptažodis, telefononumeris, epastas, role, registracijosdata, paskyrosbusena, fk_teisesteisesid)
                      VALUES
                      (@ak, @vardas, @pavarde, @pwd, @tel, @ep, @role, @reg, @busena, @fk)", conn))
                {
                    cmd.Parameters.AddWithValue("@ak", dto.AsmensKodas);
                    cmd.Parameters.AddWithValue("@vardas", dto.Vardas);
                    cmd.Parameters.AddWithValue("@pavarde", dto.Pavarde);
                    cmd.Parameters.AddWithValue("@pwd", passwordHash);
                    cmd.Parameters.AddWithValue("@tel", dto.Telefonas);
                    cmd.Parameters.AddWithValue("@ep", dto.ElPastas);
                    cmd.Parameters.AddWithValue("@role", dto.Role);
                    cmd.Parameters.AddWithValue("@reg", DateTime.UtcNow.Date);
                    cmd.Parameters.AddWithValue("@busena", (object?)dto.PaskyrosBusena ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@fk", dto.FkTeisesTeisId ?? 1);

                    var affected = await cmd.ExecuteNonQueryAsync();
                    if (affected != 1)
                        return new RegisterResult { Success = false, Error = $"Įrašyta eilučių: {affected} (tikėtasi 1)." };
                }

                return new RegisterResult { Success = true };
            }
            catch (PostgresException pgex)
            {
                // Pvz.: 42P01 (relation not found), 23505 (unique violation), 23502 (null violation), 28P01 (auth failed)
                return new RegisterResult { Success = false, Error = $"DB klaida: {pgex.SqlState} {pgex.MessageText}" };
            }
            catch (Exception ex)
            {
                return new RegisterResult { Success = false, Error = $"Išimtis: {ex.Message}" };
            }
        }

        private static bool IsValidServerSide(RegisterDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Vardas) ||
                string.IsNullOrWhiteSpace(dto.Pavarde) ||
                dto.AsmensKodas <= 0 ||
                string.IsNullOrWhiteSpace(dto.Telefonas) ||
                string.IsNullOrWhiteSpace(dto.ElPastas) ||
                string.IsNullOrWhiteSpace(dto.SlaptazodisPlain))
                return false;

            var regex = new Regex(@"^[^\s@]+@[^\s@]+\.[A-Za-z]{2,}$");
            return regex.IsMatch(dto.ElPastas);
        }

        // PBKDF2 hash (salt + 100k iteracijų), saugomas "salt:hash" (Base64)
        private static string HashPassword(string password)
        {
            using var rng = RandomNumberGenerator.Create();
            var salt = new byte[16];
            rng.GetBytes(salt);

            using var rfc2898 = new Rfc2898DeriveBytes(password, salt, 100_000, HashAlgorithmName.SHA256);
            var hash = rfc2898.GetBytes(32);

            return $"{Convert.ToBase64String(salt)}:{Convert.ToBase64String(hash)}";
        }
    }
}
