using System.Collections.Generic;
using System.Threading.Tasks;
using InformacinesSistemos.Models;

namespace InformacinesSistemos.Services
{
    public interface IReviewService
    {
        // Grąžina visus atsiliepimus
        Task<List<Atsiliepimas>> GetAllAsync();

        // Prideda naują atsiliepimą
        Task AddReviewAsync(Atsiliepimas review);

        // Patvirtina / priima atsiliepimą (moderavimui)
        Task AcceptReviewAsync(int reviewId);

        // Ištrina atsiliepimą
        Task DeleteReviewAsync(int reviewId);
    }
}
