
// Services/ReportsDtos.cs
namespace InformacinesSistemos.Services
{
    public class YearlyStatDto
    {
        public int Metai { get; set; }
        public int ZaidimuKiekis { get; set; }
        public double VidutineKaina { get; set; }
        public int AtsiliepimuKiekis { get; set; }
    }

    public class GenreCountDto
    {
        public int ZanroId { get; set; }
        public string Pavadinimas { get; set; } = "";
        public int ZaidimuKiekis { get; set; }
    }

    public class ReportsVm
    {
        public List<YearlyStatDto> MetuStatistika { get; set; } = new();
        public List<GenreCountDto> ZaidimaiPagalZanra { get; set; } = new();
    }
}
