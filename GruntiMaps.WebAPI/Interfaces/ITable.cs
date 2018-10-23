using System.Threading.Tasks;
using GruntiMaps.Models;

namespace GruntiMaps.Interfaces
{
    public interface ITable
    {
        Task AddQueue(string queueId, string jobId);
        Task<JobStatus?> GetJobStatus(string jobId);
        Task UpdateQueueStatus(string queueId, JobStatus status);
    }
}
