using System.Threading.Tasks;
using GruntiMaps.Common.Enums;

namespace GruntiMaps.ResourceAccess.Table
{
    public interface IStatusTable
    {
        Task<LayerStatus?> GetStatus(string id);
        Task UpdateStatus(string id, LayerStatus status);
    }
}
