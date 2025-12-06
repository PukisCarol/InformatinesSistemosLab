
namespace InformacinesSistemos.Services
{
    public interface ILoginService
    {
        Task<LoginResult> AuthenticateAsync(string email, string password);
    }
}
