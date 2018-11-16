using System.Threading.Tasks;
using GruntiMaps.Common.Enums;

namespace GruntiMaps.WebAPI.Interfaces
{
    public interface IStatusTable
    {
        Task<LayerStatus?> GetStatus(string id);
        Task UpdateStatus(string id, LayerStatus status);
    }
}
