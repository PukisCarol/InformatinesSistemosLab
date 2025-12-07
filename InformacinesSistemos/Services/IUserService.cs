using InformacinesSistemos.Models;

namespace InformacinesSistemos.Services
{
    public interface IUserService
    {
        Task<List<Naudotojas>> GetAllAsync();
        Task UpdateStatusAsync(int naudotojasId, string paskyrosBusena);
        Task UpdateRoleAsync(int naudotojasId, string role);

        Task<Naudotojas?> GetByEmailAsync(string email);

        Task UpdateProfileAsync(Naudotojas user);
        Task DeleteAsync(int naudotojasId);

        Task UpdatePasswordAsync(int naudotojasId, string newPassword);
    }
}
