using InformacinesSistemos.Models;

namespace InformacinesSistemos.Services;

public interface IGameService
{
    Task<List<Zaidimas>> GetAllAsync();
    Task<Zaidimas?> GetByIdAsync(int id);

    // Nauji metodai
    Task<Zaidimas> AddAsync(Zaidimas game);
    Task UpdateAsync(Zaidimas game);
}
