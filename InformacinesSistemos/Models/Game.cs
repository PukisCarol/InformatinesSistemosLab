namespace InformacinesSistemos.Models
{
    public class Game
    {
        public int Id { get; set; }
        public string? Title { get; set; }
        public string? Type { get; set; } // Stalo / Kompiuterinis
        public decimal Price { get; set; }
        public string? Description { get; set; }
    }
}
