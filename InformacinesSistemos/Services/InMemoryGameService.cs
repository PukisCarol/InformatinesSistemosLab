using InformacinesSistemos.Models;

namespace InformacinesSistemos.Services;

public class InMemoryGameService : IGameService
{
    private readonly List<Zaidimas> _games;
    private int _nextId = 4;

    public InMemoryGameService()
    {
        // Žanrai
        var strategy = new Zanras { ZanroId = 1, Pavadinimas = "Strategija" };
        var rpg = new Zanras { ZanroId = 2, Pavadinimas = "RPG" };
        var party = new Zanras { ZanroId = 3, Pavadinimas = "Vakarėlių žaidimas" };

        var owner = new Naudotojas
        {
            NaudotojasId = 1,
            Vardas = "Jonas",
            Pavarde = "Žaidėjas",
            ElPastas = "jonas@example.com",
            Slaptazodis = "****",
            Role = "User",
            RegistracijosData = DateTime.Today.AddDays(-30),
            PaskirosBusena = "Aktyvi",
            TelefonoNumeris = "+37060000000",
            AsmensKodas = 0
        };

        _games = new List<Zaidimas>
        {
            new Zaidimas
            {
                ZaidimoId = 1,
                Kaina = 29.99,
                Reitingas = 4.6,
                AmziausCenzas = 10,
                Kurejas = "Kosmos",
                ZaidejuSkaicius = "3–4",
                Aprasymas = "Stalo žaidimas „Catan“ – statyk gyvenvietes ir prekiauk ištekliais.",
                ZanroId = strategy.ZanroId,
                Zanras = strategy,
                SavininkoId = owner.NaudotojasId,
                Savininkas = owner,
                Stalo = new StaloZaidimas
                {
                    StaloZaidimoId = 1,
                    ZaidimoId = 1,
                    Ilgis = 30,
                    Plotis = 30,
                    Aukstis = 8,
                    Trukme = 60,
                    Svoris = 1.2
                }
            },
            new Zaidimas
            {
                ZaidimoId = 2,
                Kaina = 49.99,
                Reitingas = 4.8,
                AmziausCenzas = 16,
                Kurejas = "CD Projekt RED",
                ZaidejuSkaicius = "1",
                Aprasymas = "Kompiuterinis RPG „The Witcher 3: Wild Hunt“.",
                ZanroId = rpg.ZanroId,
                Zanras = rpg,
                SavininkoId = owner.NaudotojasId,
                Savininkas = owner,
                Kompiuterinis = new KompiuterinisZaidimas
                {
                    KompiuterinioZaidimoId = 2,
                    ZaidimoId = 2,
                    UzimamaVietaDiske = 50,
                    SisteminiaiReikalavimai = "16GB RAM, GTX 1060",
                    OS = "Windows",
                    Platforma = "PC"
                }
            },
            new Zaidimas
            {
                ZaidimoId = 3,
                Kaina = 19.99,
                Reitingas = 4.2,
                AmziausCenzas = 12,
                Kurejas = "Party Games Inc.",
                ZaidejuSkaicius = "4–10",
                Aprasymas = "Linksmų užduočių vakarėlių žaidimas.",
                ZanroId = party.ZanroId,
                Zanras = party,
                SavininkoId = owner.NaudotojasId,
                Savininkas = owner,
                Stalo = new StaloZaidimas
                {
                    StaloZaidimoId = 3,
                    ZaidimoId = 3,
                    Ilgis = 25,
                    Plotis = 25,
                    Aukstis = 6,
                    Trukme = 30,
                    Svoris = 0.8
                }
            }
        };
    }

    public Task<List<Zaidimas>> GetAllAsync()
    {
        return Task.FromResult(_games.ToList());
    }

    public Task<Zaidimas?> GetByIdAsync(int id)
    {
        var g = _games.FirstOrDefault(x => x.ZaidimoId == id);
        return Task.FromResult(g);
    }

    public Task<Zaidimas> AddAsync(Zaidimas game)
    {
        // priskiriam naują ID
        game.ZaidimoId = _nextId++;
        _games.Add(game);
        return Task.FromResult(game);
    }

    public Task UpdateAsync(Zaidimas game)
    {
        var existing = _games.FirstOrDefault(g => g.ZaidimoId == game.ZaidimoId);
        if (existing != null)
        {
            // labai paprastas update – realiai perrašom pagrindinius laukus
            existing.Kaina = game.Kaina;
            existing.Reitingas = game.Reitingas;
            existing.AmziausCenzas = game.AmziausCenzas;
            existing.Kurejas = game.Kurejas;
            existing.ZaidejuSkaicius = game.ZaidejuSkaicius;
            existing.Aprasymas = game.Aprasymas;
            existing.ZanroId = game.ZanroId;
            existing.Zanras = game.Zanras;
            existing.SavininkoId = game.SavininkoId;
            existing.Savininkas = game.Savininkas;
            existing.Kompiuterinis = game.Kompiuterinis;
            existing.Stalo = game.Stalo;
        }

        return Task.CompletedTask;
    }

    public Task<List<Zanras>> GetAllGenresAsync()
    {
        throw new NotImplementedException();
    }

    public Task AddGenreToGameAsync(int gameId, int genreId)
    {
        throw new NotImplementedException();
    }
}
