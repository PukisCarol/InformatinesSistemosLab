namespace InformacinesSistemos.Models;

// =====================
// Naudotojas
// =====================
public class Naudotojas
{
    // Primary key
    public int AsmensKodas { get; set; }

    // Optional: keep NaudotojasId as an alias if you want
    public int NaudotojasId
    {
        get => AsmensKodas;
        set => AsmensKodas = value;
    }

    public string Vardas { get; set; } = null!;
    public string Pavarde { get; set; } = null!;
    public string Slaptazodis { get; set; } = null!;
    public string TelefonoNumeris { get; set; } = null!;
    public string ElPastas { get; set; } = null!;
    public string Role { get; set; } = null!;
    public DateTime RegistracijosData { get; set; }
    public string PaskirosBusena { get; set; } = null!;

    public int fk_TeisesTeisesId { get; set; }

    // Navigacija
    public ICollection<Zaidimas> ParduodamiNuomojamiZaidimai { get; set; } = new List<Zaidimas>();
    public ICollection<Uzsakymas> Uzsakymai { get; set; } = new List<Uzsakymas>();
    public ICollection<Apmokejimas> Apmokejimai { get; set; } = new List<Apmokejimas>();
    public ICollection<Teises> Teises { get; set; } = new List<Teises>();
    public ICollection<Atsiliepimas> Atsiliepimai { get; set; } = new List<Atsiliepimas>();
    public ICollection<Nusiskundimas> Nusiskundimai { get; set; } = new List<Nusiskundimas>();
}

// =====================
// Žaidimas + žanras
// =====================
public class Zaidimas
{
    public int ZaidimoId { get; set; }
    public DateTime IsleidimoData { get; set; }

    public double Kaina { get; set; }
    public double Reitingas { get; set; }
    public int AmziausCenzas { get; set; }
    public string Kurejas { get; set; } = null!;
    public string ZaidejuSkaicius { get; set; } = null!;
    public string Aprasymas { get; set; } = null!;

    // FK į žanrą
    public int ZanroId { get; set; }
    public Zanras? Zanras { get; set; }

    // FK į savininką (kas parduoda/nuomoja kitiems)
    public int SavininkoId { get; set; }
    public Naudotojas? Savininkas { get; set; }

    // 1:1 detalės
    public KompiuterinisZaidimas? Kompiuterinis { get; set; }
    public StaloZaidimas? Stalo { get; set; }

    // Navigacija
    public ICollection<Atsiliepimas> Atsiliepimai { get; set; } = new List<Atsiliepimas>();
    public ICollection<Uzsakymas> Uzsakymai { get; set; } = new List<Uzsakymas>();
}

public class Zanras
{
    public int ZanroId { get; set; }
    public string Pavadinimas { get; set; } = null!;

    public ICollection<Zaidimas> Zaidimai { get; set; } = new List<Zaidimas>();
}
public class ZanrasPriklauso
{
    public int fk_ZaidimasZaidimoId { get; set; }
    public int fk_ZanrasZanroId {get; set;}
}
// =====================
// Kompiuteriniai ir stalo žaidimai
// =====================
public class KompiuterinisZaidimas
{
    public int KompiuterinioZaidimoId { get; set; } // = ZaidimoId (1:1)
    public double UzimamaVietaDiske { get; set; }
    public string SisteminiaiReikalavimai { get; set; } = null!;
    public string OS { get; set; } = null!;
    public string Platforma { get; set; } = null!;

    public int ZaidimoId { get; set; }
    public Zaidimas? Zaidimas { get; set; }
}

public class StaloZaidimas
{
    public int StaloZaidimoId { get; set; } // = ZaidimoId (1:1)
    public int AmziausCenzas { get; set; }
    public string ZaidejuSkaicius { get; set; } = null!;
    public double Ilgis { get; set; }
    public double Plotis { get; set; }
    public double Aukstis { get; set; }
    public int Trukme { get; set; }
    public double Svoris { get; set; }

    public int ZaidimoId { get; set; }
}

// =====================
// Užsakymas, apmokėjimas, siuntimas
// =====================
public class Uzsakymas
{
    public int UzsakymoId { get; set; }
    public double Suma { get; set; }
    public string Busena { get; set; } = null!;
    public string PristatymoBudas { get; set; } = null!;
    public DateTime MokėjimoTerminasIki { get; set; }
    public DateTime UzsakymoData { get; set; }

    // Kas užsako
    public int NaudotojasId { get; set; }
    public Naudotojas? Naudotojas { get; set; }

    // Koks žaidimas (pagal diagramą – vienas žaidimas vienam užsakymui)
    public int ZaidimoId { get; set; }
    public Zaidimas? Zaidimas { get; set; }

    // Siuntimas / apmokėjimas (0..1)
    public int? SiuntimoId { get; set; }
    public Siuntimas? Siuntimas { get; set; }

    public int? ApmokejimoId { get; set; }
    public Apmokejimas? Apmokejimas { get; set; }
}

public class Apmokejimas
{
    public int ApmokejimoId { get; set; }
    public DateTime Data { get; set; }
    public int SaskaitosId { get; set; }
    public string ApmokejimoTipas { get; set; } = null!;
    public string Statusas { get; set; } = null!;

    // Mokėtojas
    public int MoketojasId { get; set; }
    public Naudotojas? Moketojas { get; set; }

    // Kurį užsakymą apmoka
    public int UzsakymoId { get; set; }
    public Uzsakymas? Uzsakymas { get; set; }
}

public class Siuntimas
{
    public int SiuntimoId { get; set; }
    public string Adresas { get; set; } = null!;
    public double SiuntimoDydis { get; set; }
    public double SiuntimoKaina { get; set; }
    public string Busena { get; set; } = null!;
    public DateTime IssiuntimoData { get; set; }

    public int UzsakymoId { get; set; }
    public Uzsakymas? Uzsakymas { get; set; }
}

// =====================
// Teisės (apribojimai / rolės dokumentai)
// =====================
public class Teises
{
    public int TeisesId { get; set; }
    public string Dokumentas { get; set; } = null!;
    public DateTime GaliojimoData { get; set; }
    public DateTime SuteikimoData { get; set; }
    public string Tipas { get; set; } = null!;

    public int NaudotojasId { get; set; }
    public Naudotojas? Naudotojas { get; set; }
}

// =====================
// Atsiliepimai ir nusiskundimai
// =====================
public class Atsiliepimas
{
    public int AtsiliepimoId { get; set; }
    public DateTime Data { get; set; }
    public string AtsiliepimoTekstas { get; set; } = null!;
    public int Ivertinimas { get; set; }

    public int NaudotojasId { get; set; }
    public Naudotojas? Naudotojas { get; set; }

    public int ZaidimoId { get; set; }
    public Zaidimas? Zaidimas { get; set; }
}

public class Nusiskundimas
{
    public int NusiskundimoId { get; set; }
    public DateTime Data { get; set; }
    public string NusiskundimoTekstas { get; set; } = null!;
    public string Statusas { get; set; } = "Naujas";
    public string Tema { get; set; } = null!;

    public int NaudotojasId { get; set; }
    public Naudotojas? Naudotojas { get; set; }
}
