
namespace InformacinesSistemos.Services
{
    public class RegisterDto
    {
        public string Vardas { get; set; } = default!;
        public string Pavarde { get; set; } = default!;
        public int AsmensKodas { get; set; }
        public string Telefonas { get; set; } = default!;
        public string ElPastas { get; set; } = default!;
        public string SlaptazodisPlain { get; set; } = default!;
        public string Role { get; set; } = "User";
        public string? PaskyrosBusena { get; set; } = "Aktyvi";
        public int? FkTeisesTeisId { get; set; } = 1;
    }

    public class RegisterResult
    {
        public bool Success { get; set; }
        public string? Error { get; set; }
    }


    public interface IRegistrationService
    {
        Task<RegisterResult> RegisterAsync(RegisterDto dto);
    }

}
