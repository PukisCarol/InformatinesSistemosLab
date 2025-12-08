
using System.Threading.Tasks;
using InformacinesSistemos.Models;

namespace InformacinesSistemos.Services
{
    public interface IBankPaymentService
    {
        Task<(bool ok, string message)> TryChargeAsync(CardPaymentForm card, decimal amount);
        Task<int?> InsertShippingAsync(ShippingRecord shipping);
    }
}
