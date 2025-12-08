using System.Collections.Generic;
using System.Threading.Tasks;
using InformacinesSistemos.Models;

namespace InformacinesSistemos.Services
{
    public interface IReviewService
    {
        /// <summary>Visų atsiliepimų sąrašas (demo / adminams ir pan.).</summary>
        Task<List<Atsiliepimas>> GetAllAsync();

        /// <summary>Konkretaus žaidimo atsiliepimai.</summary>
        Task<List<Atsiliepimas>> GetForGameAsync(int gameId);

        /// <summary>Pridėti naują atsiliepimą.</summary>
        Task AddReviewAsync(Atsiliepimas review);

        /// <summary>Patvirtinti atsiliepimą (jei reikia moderavimo).</summary>
        Task AcceptReviewAsync(int id);

        /// <summary>Ištrinti atsiliepimą.</summary>
        Task DeleteReviewAsync(int id);
    }
}
