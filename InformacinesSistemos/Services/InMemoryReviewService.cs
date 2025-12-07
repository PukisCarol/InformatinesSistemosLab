
using InformacinesSistemos.Components.Pages;
using InformacinesSistemos.Models;

namespace InformacinesSistemos.Services
{
    public class InMemoryReviewService : IReviewService
    {
        private readonly List<Atsiliepimas> _reviews;

        public InMemoryReviewService()
        {
            _reviews = new List<Atsiliepimas>
        {
            new Atsiliepimas
            {
                AtsiliepimoId = 1,
                Data = DateTime.Today.AddDays(-30),
                AtsiliepimoTekstas = "Patiko labai",
                Ivertinimas = 5
            },
            new Atsiliepimas
            {
                AtsiliepimoId = 2,
                Data = DateTime.Today.AddDays(-10),
                AtsiliepimoTekstas = "Nepatiko",
                Ivertinimas = 2
            }
        };
        }
        public Task AcceptReviewAsync(int reviewId)
        {
            var review = _reviews.FirstOrDefault(u => u.AtsiliepimoId == reviewId);
           

            return Task.CompletedTask;
        }

        public Task AddReviewAsync(Atsiliepimas review)
        {
            throw new NotImplementedException();
        }

        public Task DeleteReviewAsync(int reviewId)
        {
            var review = _reviews.FirstOrDefault(u => u.AtsiliepimoId == reviewId);
            

            return Task.CompletedTask;
        }

        public Task<List<Atsiliepimas>> GetAllAsync()
        {
            return Task.FromResult(_reviews
            .OrderBy(u => u.Ivertinimas)
            .ThenBy(u => u.AtsiliepimoTekstas)
            .ToList());
        }
    }
}
