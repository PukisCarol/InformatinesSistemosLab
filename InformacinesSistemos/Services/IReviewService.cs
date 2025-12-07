using InformacinesSistemos.Models;
namespace InformacinesSistemos.Services
{
    public interface IReviewService
    {
        Task<List<Atsiliepimas>> GetAllAsync();
        Task AcceptReviewAsync(int userId);
        Task DeleteReviewAsync(int userId);
        Task AddReviewAsync(Atsiliepimas review);
    }
}
