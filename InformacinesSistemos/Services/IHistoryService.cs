using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using InformacinesSistemos.Models;

namespace InformacinesSistemos.Services
{
    public interface IHistoryService
    {
        // Grąžina tik to naudotojo įrašus
        Task<List<VartotojoSandoris>> GetForUserAsync(int naudotojasId);

        // Prideda naują pirkimo/nuomos įrašą
        Task AddEntryAsync(int naudotojasId, int zaidimoId, string tipas, DateTime paskutinisMokejimas);

        // Atšaukia nuomą
        Task CancelRentalAsync(int sandorisId);
    }
}
