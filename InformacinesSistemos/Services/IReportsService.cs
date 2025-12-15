
// Services/IReportsService.cs
using System.Threading.Tasks;

namespace InformacinesSistemos.Services
{
    public interface IReportsService
    {
        Task<ReportsVm> GetReportAsync();
    }
}
