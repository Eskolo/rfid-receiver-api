using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace rfid_receiver_api.Middleware;

public class ApiKeyAttribute : Attribute, IAsyncActionFilter
{
    private const string ApiKeyHeaderName = "x-api-key";

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var configuration = context.HttpContext.RequestServices.GetService(typeof(IConfiguration)) as IConfiguration;
        var validApiKey = configuration?["ApiKeys:RFID"] ?? throw new InvalidOperationException("API key configuration is missing.");

        if (!context.HttpContext.Request.Headers.TryGetValue(ApiKeyHeaderName, out var key) || key != validApiKey)
        {
            context.Result = new UnauthorizedResult();
            return;
        }

        await next(); // call the controller action
    }
}