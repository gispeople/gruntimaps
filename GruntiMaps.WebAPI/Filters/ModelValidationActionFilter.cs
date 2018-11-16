using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace GruntiMaps.WebAPI.Filters
{
    /// <summary>
    /// Middleware which ensures the Model in the request is valid. If it is not valid a BadRequest result
    /// with the validation errors is returned.
    /// </summary>
    public class ModelValidationActionFilter : IActionFilter
    {
        public void OnActionExecuting(ActionExecutingContext context)
        {
            if (!context.ModelState.IsValid)
            {
                context.Result = new BadRequestObjectResult(context.ModelState);
            }
        }

        public void OnActionExecuted(ActionExecutedContext context)
        {
        }
    }
}
