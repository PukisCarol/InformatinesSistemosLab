using InformacinesSistemos.Models;

namespace InformacinesSistemos.Services
{
    public interface IHistoryService
    {
        Task AddEntryAsync(int naudotojasId, int zaidimoId, string tipas, DateTime paskutinisMokejimas);
        Task<List<VartotojoSandoris>> GetForUserAsync(int naudotojasId);
        Task CancelRentalAsync(int sandorioId);
    }
}
