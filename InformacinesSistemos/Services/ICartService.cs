using InformacinesSistemos.Models;

namespace InformacinesSistemos.Services;

public interface ICartService
{
    event Action? CartChanged;

    IReadOnlyList<CartItem> Items { get; }

    void Add(Zaidimas game, int quantity = 1);
    void Remove(int gameId);
    void Clear();
    double GetTotal();
}
