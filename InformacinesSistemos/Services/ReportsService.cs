
// Services/ReportsService.cs
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using InformacinesSistemos.Services;
using InformacinesSistemos.Models;
using InformacinesSistemos.Data; // pritaikyk savo DbContext namespace

namespace InformacinesSistemos.Services
{
    public class ReportsService : IReportsService
    {
        private readonly AppDbContext _db;
        public ReportsService(AppDbContext db) => _db = db;

        public async Task<ReportsVm> GetReportAsync()
        {
            // 1) Metai: žaidimų kiekis, vidutinė kaina, atsiliepimų suma (pagal Atsiliepimas.ZaidimoId)
            var metuStats = await _db.Zaidimai
                .GroupJoin(
                    _db.Atsiliepimai,
                    game => game.ZaidimoId,
                    review => review.ZaidimoId,
                    (game, reviews) => new { game, reviewCount = reviews.Count() }
                )
                .GroupBy(x => x.game.IsleidimoData.Year)
                .Select(gr => new YearlyStatDto
                {
                    Metai = gr.Key,
                    ZaidimuKiekis = gr.Count(),
                    VidutineKaina = gr.Average(x => x.game.Kaina),
                    AtsiliepimuKiekis = gr.Sum(x => x.reviewCount)
                })
                .OrderBy(x => x.Metai)
                .ToListAsync();

            // 2) Žaidimų skaičius pagal žanrą (Zaidimas.ZanroId -> Zanras.Pavadinimas)
            var genreCounts = await _db.Zaidimai
                .GroupBy(g => g.ZanroId)
                .Select(gr => new GenreCountDto
                {
                    ZanroId = gr.Key,
                    Pavadinimas = _db.Zanrai
                        .Where(z => z.ZanroId == gr.Key)
                        .Select(z => z.Pavadinimas)
                        .FirstOrDefault() ?? "Nežinomas",
                    ZaidimuKiekis = gr.Count()
                })
                .OrderByDescending(x => x.ZaidimuKiekis)
                .ToListAsync();

            return new ReportsVm
            {
                MetuStatistika = metuStats,
                ZaidimaiPagalZanra = genreCounts
            };
        }
    }
}
