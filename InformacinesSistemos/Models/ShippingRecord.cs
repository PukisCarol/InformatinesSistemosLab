
using System;

namespace InformacinesSistemos.Models
{
    public class ShippingRecord
    {
        public string Adresas { get; set; } = string.Empty;
        public decimal SiuntimoKaina { get; set; }
        public string Busena { get; set; } = "Paruoštas";
        public DateOnly? IsSiuntimoData { get; set; } = DateOnly.FromDateTime(DateTime.Today);

        public int? FkNaudotojasAsmensKodas { get; set; } // nebenaudojamas, pildomas NULL
        public int? FkApmokejimasSekaltosId { get; set; }
        public int? FkUzsakymasSuUzsakymoId { get; set; }
    }
}
