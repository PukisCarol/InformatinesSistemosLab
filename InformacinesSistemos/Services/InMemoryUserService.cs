using InformacinesSistemos.Models;

namespace InformacinesSistemos.Services;

public class InMemoryUserService : IUserService
{
    private readonly List<Naudotojas> _users;

    public InMemoryUserService()
    {
        _users = new List<Naudotojas>
        {
            new Naudotojas
            {
                NaudotojasId = 1,
                Vardas = "Jonas",
                Pavarde = "Žaidėjas",
                ElPastas = "jonas@example.com",
                Slaptazodis = "****",
                Role = "User",
                RegistracijosData = DateTime.Today.AddDays(-30),
                PaskirosBusena = "Aktyvi",
                TelefonoNumeris = "+37060000001",
                AsmensKodas = 0
            },
            new Naudotojas
            {
                NaudotojasId = 2,
                Vardas = "Asta",
                Pavarde = "Moderatorė",
                ElPastas = "moderator@example.com",
                Slaptazodis = "****",
                Role = "Moderator",
                RegistracijosData = DateTime.Today.AddDays(-60),
                PaskirosBusena = "Aktyvi",
                TelefonoNumeris = "+37060000002",
                AsmensKodas = 0
            },
            new Naudotojas
            {
                NaudotojasId = 3,
                Vardas = "Darius",
                Pavarde = "Administratorius",
                ElPastas = "admin@example.com",
                Slaptazodis = "****",
                Role = "Admin",
                RegistracijosData = DateTime.Today.AddDays(-90),
                PaskirosBusena = "Aktyvi",
                TelefonoNumeris = "+37060000003",
                AsmensKodas = 0
            },
            new Naudotojas
            {
                NaudotojasId = 4,
                Vardas = "Milda",
                Pavarde = "Blokuota",
                ElPastas = "blocked@example.com",
                Slaptazodis = "****",
                Role = "User",
                RegistracijosData = DateTime.Today.AddDays(-10),
                PaskirosBusena = "Blokuota",
                TelefonoNumeris = "+37060000004",
                AsmensKodas = 0
            }
        };
    }

    public Task<List<Naudotojas>> GetAllAsync()
    {
        return Task.FromResult(_users
            .OrderBy(u => u.PaskirosBusena)
            .ThenBy(u => u.Role)
            .ThenBy(u => u.Vardas)
            .ToList());
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
}
