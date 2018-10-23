using System.Threading.Tasks;
using GruntiMaps.WebAPI.Models;

namespace GruntiMaps.WebAPI.Interfaces
{
    public interface IQueue
    {
        Task<string> AddMessage(string message);
        Task<Message> GetMessage();
        Task DeleteMessage(Message message);
    }
}