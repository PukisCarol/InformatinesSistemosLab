
// Services/IUserLookupService.cs
using System.Threading.Tasks;

namespace InformacinesSistemos.Services
{
    public interface IUserLookupService
    {
        Task<int?> GetAsmensKodasByEmailAsync(string email);
    }
}
