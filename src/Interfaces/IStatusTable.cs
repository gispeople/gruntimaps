using System.Threading.Tasks;
using GruntiMaps.Models;

namespace GruntiMaps.Interfaces
{
    public interface IStatusTable
    {
        Task AddStatus(string queueId, string jobId);
        Task<JobStatus?> GetStatus(string jobId);
        Task UpdateStatus(string queueId, JobStatus status);
    }
}
