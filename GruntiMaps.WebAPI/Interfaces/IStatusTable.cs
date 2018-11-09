using System.Threading.Tasks;
using GruntiMaps.WebAPI.DataContracts;

namespace GruntiMaps.WebAPI.Interfaces
{
    public interface IStatusTable
    {
        Task<LayerStatus?> GetStatus(string id);
        Task UpdateStatus(string id, LayerStatus status);
    }
}
