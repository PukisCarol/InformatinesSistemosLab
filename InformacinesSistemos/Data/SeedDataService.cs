using System.Collections.Generic;
using InformacinesSistemos.Models;

namespace InformacinesSistemos.Data
{
    // Simple in-memory seed data service to simulate DB
    public class IGameServices
    {
        public List<Game> Games { get; } = new List<Game>();
        public List<User> Users { get; } = new List<User>();
        public List<Game> Cart { get; } = new List<Game>();

        public IGameServices()
        {
            Users.Add(new User { Id = 1, Name = "Admin", Email = "admin@example.com", Role = "Admin" });
            Users.Add(new User { Id = 2, Name = "Jonas", Email = "jonas@example.com", Role = "User" });

            Games.Add(new Game { Id = 1, Title = "Monopolis", Type = "Stalo", Price = 10m, Description = "Klasikinis stalo žaidimas" });
            Games.Add(new Game { Id = 2, Title = "Space Battle", Type = "Kompiuterinis", Price = 15m, Description = "Veiksmo žaidimas" });
        }
    }
}
