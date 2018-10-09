using System.Threading.Tasks;
using GruntiMaps.Models;

namespace GruntiMaps.Interfaces
{
    public interface IQueue
    {
        Task AddMessage(string message);
        Task<Message> GetMessage();
        Task DeleteMessage(Message message);
    }
}