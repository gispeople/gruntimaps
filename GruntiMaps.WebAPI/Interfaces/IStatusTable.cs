using System.Threading.Tasks;
using GruntiMaps.WebAPI.Models;

namespace GruntiMaps.WebAPI.Interfaces
{
    public interface IStatusTable
    {
        Task AddStatus(string queueId, string jobId);
        Task<JobStatus?> GetStatus(string jobId);
        Task UpdateStatus(string queueId, JobStatus status);
    }
}
