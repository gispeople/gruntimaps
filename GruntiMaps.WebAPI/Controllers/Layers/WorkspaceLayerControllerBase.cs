using GruntiMaps.Api.Common.Extensions;
using GruntiMaps.Api.DataContracts.V2;
using Microsoft.AspNetCore.Mvc;

namespace GruntiMaps.WebAPI.Controllers.Layers
{
    [Route(Resources.Workspaces + "/{workspaceId}/" + Resources.Layers + "/{layerId}")]
    public class WorkspaceLayerControllerBase : WorkspaceControllerBase
    {
        protected string LayerId => ControllerContext.RouteData.GetLayerId();
    }
}
