namespace SparkTrack.WebAPI.Middlewares;

using Core.Services.Authorization;

public class AuthorizationServiceMiddleware(IAuthorizationService authorizationService) : IMiddleware
{
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        // TODO: Доработать после добавления авторизации
        var stringId = context.User.FindFirst("Id")?.Value;
        if (stringId is null || !Guid.TryParse(stringId, out Guid id))
        {
            await next(context);
            return;
        }
        
        await authorizationService.AuthorizeAsync(id);
        await next(context);
    }
}