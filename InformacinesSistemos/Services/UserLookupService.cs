
// Services/UserLookupService.cs
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Npgsql;

namespace InformacinesSistemos.Services
{
    public class UserLookupService : IUserLookupService
    {
        private readonly string _conn;

        public UserLookupService(IConfiguration cfg)
        {
            _conn = cfg.GetConnectionString("DefaultConnection")!;
        }

        public async Task<int?> GetAsmensKodasByEmailAsync(string email)
        {
            await using var conn = new NpgsqlConnection(_conn);
            await conn.OpenAsync();

            const string sql = @"
                SELECT asmenskodas
                FROM public.naudotojas
                WHERE lower(epastas) = lower(@e)
                LIMIT 1";

            await using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@e", email);

            var obj = await cmd.ExecuteScalarAsync();
            return obj == null ? (int?)null : (int)obj;
        }
    }
}
