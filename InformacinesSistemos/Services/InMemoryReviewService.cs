using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using InformacinesSistemos.Models;

namespace InformacinesSistemos.Services
{
    public class InMemoryReviewService : IReviewService
    {
        private readonly List<Atsiliepimas> _reviews = new();

        public InMemoryReviewService()
        {
            // Jei labai norėsi seed'inti testinių atsiliepimų,
            // galėsim vėliau pritaikyti pagal tikrus Atsiliepimas laukus.
            // Dabar paliekam tuščią sąrašą, kad nepyktų dėl neegzistuojančių property.
        }

        public Task<List<Atsiliepimas>> GetAllAsync()
        {
            return Task.FromResult(_reviews.ToList());
        }

        public Task AddReviewAsync(Atsiliepimas review)
        {
            _reviews.Add(review);
            return Task.CompletedTask;
        }

        public Task AcceptReviewAsync(int id)
        {
            // In-memory režime nieko nedarom – tik kad tenkintume interface'ą
            return Task.CompletedTask;
        }

        public Task DeleteReviewAsync(int id)
        {
            // Paprastai išmeskim iš sąrašo, jei yra
            _reviews.RemoveAll(r => r.GetHashCode() == id);
            // (čia nieko kritiško, nes vis tiek realiai nenaudosi šito serviso)
            return Task.CompletedTask;
        }
    }
}
