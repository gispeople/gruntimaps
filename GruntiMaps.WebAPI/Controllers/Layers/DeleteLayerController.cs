using System;
using System.Threading.Tasks;
using GruntiMaps.Api.DataContracts.V2;
using GruntiMaps.WebAPI.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace GruntiMaps.WebAPI.Controllers.Layers
{
    public class DeleteLayerController : ApiControllerBase
    {
        private readonly IMapData _mapData;

        public DeleteLayerController(IMapData mapData)
        {
            _mapData = mapData;
        }

        [HttpPost(Resources.Layers + "/{id}")]
        public async Task<IActionResult> Invoke(string id)
        {
            // todo
            throw new NotImplementedException();
        }
    }
}
