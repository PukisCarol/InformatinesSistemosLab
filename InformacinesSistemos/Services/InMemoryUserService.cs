using InformacinesSistemos.Models;

namespace InformacinesSistemos.Services
{
    /// <summary>
    /// Paprastas in-memory vartotojų servisukas testams.
    /// Duomenys laikomi tik atmintyje, į DB nerašo.
    /// </summary>
    public class InMemoryUserService : IUserService
    {
        private readonly List<Naudotojas> _users = new();

        public InMemoryUserService()
        {
            // jeigu nori, gali čia susidėti kokių testinių vartotojų
            _users.Add(new Naudotojas
            {
                NaudotojasId = 1,
                Vardas = "Test",
                Pavarde = "User",
                Slaptazodis = "test123",
                ElPastas = "test@example.com",
                TelefonoNumeris = "860000000",
                Role = "Klientas",
                RegistracijosData = DateTime.Now,
                PaskirosBusena = "Aktyvi",
                fk_TeisesTeisesId = 1
            });
        }

        // ----------------------------------------------------
        // Bendri metodai
        // ----------------------------------------------------

        public Task<List<Naudotojas>> GetAllAsync()
        {
            return Task.FromResult(_users.ToList());
        }

        public Task UpdateStatusAsync(int naudotojasId, string paskyrosBusena)
        {
            var user = _users.FirstOrDefault(u => u.NaudotojasId == naudotojasId);
            if (user != null)
            {
                user.PaskirosBusena = paskyrosBusena;
            }
            return Task.CompletedTask;
        }

        public Task UpdateRoleAsync(int naudotojasId, string role)
        {
            var user = _users.FirstOrDefault(u => u.NaudotojasId == naudotojasId);
            if (user != null)
            {
                user.Role = role;
            }
            return Task.CompletedTask;
        }

        public Task<Naudotojas?> GetByEmailAsync(string email)
        {
            var user = _users.FirstOrDefault(u =>
                string.Equals(u.ElPastas, email, StringComparison.OrdinalIgnoreCase));

            return Task.FromResult(user);
        }

        // ----------------------------------------------------
        // Profilio redagavimas
        // ----------------------------------------------------

        public Task UpdateProfileAsync(Naudotojas user)
        {
            var existing = _users.FirstOrDefault(u => u.NaudotojasId == user.NaudotojasId);
            if (existing != null)
            {
                existing.Vardas = user.Vardas;
                existing.Pavarde = user.Pavarde;
                existing.ElPastas = user.ElPastas;
                existing.TelefonoNumeris = user.TelefonoNumeris;
            }

            return Task.CompletedTask;
        }

        public Task DeleteAsync(int naudotojasId)
        {
            _users.RemoveAll(u => u.NaudotojasId == naudotojasId);
            return Task.CompletedTask;
        }


        public Task UpdatePasswordAsync(int naudotojasId, string newPassword)
        {
            var existing = _users.FirstOrDefault(u => u.NaudotojasId == naudotojasId);
            if (existing != null)
            {
                existing.Slaptazodis = newPassword;
            }

            return Task.CompletedTask;
        }
    }
}
