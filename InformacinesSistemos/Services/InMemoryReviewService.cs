using System;
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
            _reviews = new List<Atsiliepimas>
            {
                new Atsiliepimas
                {
                    AtsiliepimoId = 1,
                    Data = DateTime.Today.AddDays(-30),
                    AtsiliepimoTekstas = "Patiko labai",
                    Ivertinimas = 5,
                    NaudotojasId = 1,
                    ZaidimoId = 2001
                },
                new Atsiliepimas
                {
                    AtsiliepimoId = 2,
                    Data = DateTime.Today.AddDays(-10),
                    AtsiliepimoTekstas = "Galėtų būti geriau",
                    Ivertinimas = 3,
                    NaudotojasId = 2,
                    ZaidimoId = 2001
                }
            };
        }

        public Task<List<Atsiliepimas>> GetAllAsync()
            => Task.FromResult(_reviews.ToList());

        public Task<List<Atsiliepimas>> GetForGameAsync(int gameId)
            => Task.FromResult(_reviews.Where(r => r.ZaidimoId == gameId).ToList());

        public Task AddReviewAsync(Atsiliepimas review)
        {
            if (review == null) throw new ArgumentNullException(nameof(review));

            if (review.AtsiliepimoId == 0)
                review.AtsiliepimoId = _reviews.Count == 0
                    ? 1
                    : _reviews.Max(r => r.AtsiliepimoId) + 1;

            if (review.Data == default)
                review.Data = DateTime.Today;

            _reviews.Add(review);
            return Task.CompletedTask;
        }

        public Task AcceptReviewAsync(int id)
        {
            // in-memory – nieko nedarom
            return Task.CompletedTask;
        }

        public Task DeleteReviewAsync(int id)
        {
            _reviews.RemoveAll(r => r.AtsiliepimoId == id);
            return Task.CompletedTask;
        }
    }
}
