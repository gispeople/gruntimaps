using System.Threading.Tasks;

namespace GruntiMaps.ResourceAccess.Queue
{
    public interface IQueue
    {
        Task<string> AddMessage(string message);
        Task<Message> GetMessage();
        Task DeleteMessage(Message message);
    }
}
