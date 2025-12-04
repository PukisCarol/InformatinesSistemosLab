namespace InformacinesSistemos.Models;

public class CartItem
{
    public int ZaidimoId { get; set; }
    public Zaidimas? Zaidimas { get; set; }
    public int Quantity { get; set; }
}
