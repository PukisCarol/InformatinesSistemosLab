using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using InformacinesSistemos.Models;

namespace InformacinesSistemos.Services
{
    public class InMemoryReviewService : IReviewService
    {
        private readonly List<Atsiliepimas> _reviews;

        public InMemoryReviewService()
        {
            // Paprasti seed'iniai atsiliepimai testavimui
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

        public Task<List<Atsiliepimas>> GetAllAsync()
        {
            return Task.FromResult(_reviews.ToList());
        }

        public Task AddReviewAsync(Atsiliepimas review)
        {
            if (review == null) throw new ArgumentNullException(nameof(review));

            // jei ID nenustatytas – suteikiam naują
            if (review.AtsiliepimoId == 0)
            {
                var nextId = _reviews.Count == 0
                    ? 1
                    : _reviews.Max(r => r.AtsiliepimoId) + 1;

                review.AtsiliepimoId = nextId;
            }

            _reviews.Add(review);
            return Task.CompletedTask;
        }

        public Task AcceptReviewAsync(int reviewId)
        {
            var review = _reviews.FirstOrDefault(r => r.AtsiliepimoId == reviewId);
            if (review != null)
            {
                // Jei tavo Atsiliepimas turi kokį lauką "Patvirtintas" / "Busena" – gali čia nustatyti
                // review.Patvirtintas = true;
            }

            return Task.CompletedTask;
        }

        public Task AddReviewAsync(Atsiliepimas review)
        {
            throw new NotImplementedException();
        }

        public Task DeleteReviewAsync(int reviewId)
        {
            _reviews.RemoveAll(r => r.AtsiliepimoId == reviewId);
            return Task.CompletedTask;
        }
    }
}
