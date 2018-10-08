using System.Threading.Tasks;
using GruntiMaps.Interfaces;

namespace GruntiMaps.Models
{
    public class LocalQueue: IQueue
    {
        public LocalQueue(Options options)
        {

        }

        public Task AddMessage(string queueName, string message)
        {
            throw new System.NotImplementedException();
        }

        public Task<Message> GetMessage(string queueName)
        {
            throw new System.NotImplementedException();
        }

        public Task DeleteMessage(string queueName, Message message)
        {
            throw new System.NotImplementedException();
        }
    }
}