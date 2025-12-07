using InformacinesSistemos.Models;

namespace InformacinesSistemos.Services;

public interface IUserService
{
    Task<List<Naudotojas>> GetAllAsync();
    Task UpdateStatusAsync(int naudotojasId, string paskyrosBusena);
    Task UpdateRoleAsync(int naudotojasId, string role);
    // New method to get a user by email
    Task<Naudotojas?> GetByEmailAsync(string email);
}
