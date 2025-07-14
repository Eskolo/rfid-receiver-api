using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace rfid_receiver_api.Middleware;

public class ApiKeyAttribute : Attribute, IAsyncActionFilter
{
    private const string ApiKeyHeaderName = "x-api-key";
    private const string ValidApiKey = "arsch";

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        if (!context.HttpContext.Request.Headers.TryGetValue(ApiKeyHeaderName, out var key) || key != ValidApiKey)
        {
            context.Result = new UnauthorizedResult();
            return;
        }

        await next(); // call the controller action
    }
}