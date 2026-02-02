using System.Threading.Tasks;
using Kemet_api.DTOs;

namespace Kemet_api.Services
{
    public interface IDashboardService
    {
        Task<DashboardDataDto> GetDashboardDataAsync();
        Task TrackEventAsync(string eventType, string? category, Guid? userId = null);
    }
}
