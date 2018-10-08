using System.Threading.Tasks;
using GruntiMaps.Models;

namespace GruntiMaps.Interfaces
{
    public interface IQueue
    {
        Task AddMessage(string queueName, string message);
        Task<Message> GetMessage(string queueName);
        Task DeleteMessage(string queueName, Message message);
    }
}