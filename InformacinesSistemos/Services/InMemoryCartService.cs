using InformacinesSistemos.Models;

namespace InformacinesSistemos.Services;

public class InMemoryCartService : ICartService
{
    private readonly List<CartItem> _items = new();

    public event Action? CartChanged;

    public IReadOnlyList<CartItem> Items => _items;

    public void Add(Zaidimas game, int quantity = 1)
    {
        if (game == null) throw new ArgumentNullException(nameof(game));

        var existing = _items.FirstOrDefault(i => i.ZaidimoId == game.ZaidimoId);
        var q = quantity <= 0 ? 1 : quantity;

        if (existing == null)
        {
            _items.Add(new CartItem
            {
                ZaidimoId = game.ZaidimoId,
                Zaidimas = game,
                Quantity = q
            });
        }
        else
        {
            existing.Quantity += q;
        }

        CartChanged?.Invoke();
    }

    public void Remove(int gameId)
    {
        var item = _items.FirstOrDefault(i => i.ZaidimoId == gameId);
        if (item != null)
        {
            _items.Remove(item);
            CartChanged?.Invoke();
        }
    }

    public void Clear()
    {
        if (_items.Count == 0) return;
        _items.Clear();
        CartChanged?.Invoke();
    }

    public double GetTotal()
    {
        return _items.Sum(i => (i.Zaidimas?.Kaina ?? 0) * i.Quantity);
    }
}
