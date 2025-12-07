using System.Threading.Tasks;

namespace InformacinesSistemos.Services
{
    public interface IEmailService
    {
        Task SendComplaintEmailAsync(string fromEmail, string fromName, string subject, string body);
    }
}
