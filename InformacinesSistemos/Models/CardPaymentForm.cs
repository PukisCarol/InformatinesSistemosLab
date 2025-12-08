
namespace InformacinesSistemos.Models
{
    public class CardPaymentForm
    {
        public int KortelesID { get; set; }
        public string Vardas { get; set; } = string.Empty;
        public string Pavarde { get; set; } = string.Empty;
        public string PinKodas { get; set; } = string.Empty; // 4 skaitmenys
    }
}
