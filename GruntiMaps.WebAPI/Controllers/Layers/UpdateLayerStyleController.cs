using System.Threading.Tasks;
using GruntiMaps.Api.DataContracts.V2;
using GruntiMaps.Api.DataContracts.V2.Layers;
using GruntiMaps.WebAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GruntiMaps.WebAPI.Controllers.Layers
{
    [Authorize]
    public class UpdateLayerStyleController : WorkspaceLayerControllerBase
    {
        private readonly ILayerStyleService _styleService;

        public UpdateLayerStyleController(ILayerStyleService styleService)
        {
            _styleService = styleService;
        }

        [HttpPatch(Resources.StyleSubResource)]
        public async Task<ActionResult> Invoke([FromBody] UpdateLayerStyleDto dto)
        {
            await _styleService.Update(WorkspaceId, LayerId, dto.Styles);

            return NoContent();
        }
    }
}
