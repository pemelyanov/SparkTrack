namespace SparkTrack.WebAPI.Extensions;

using Core.Exceptions;
using Microsoft.AspNetCore.Mvc;

public static class ControllerExtensions
{
    public static Task<ActionResult<TData>> OkWithDomainExceptionsHandling<TData>(
        this Controller controller,
        Func<Task<TData>> action
    )
    {
        return controller.HandleDomainExceptionsInternal<ActionResult<TData>>(
            async () =>
            {
                var result = await action();

                if (result is null) return controller.NotFound();

                return controller.Ok(result);
            }
        );
    }

    public static Task<ActionResult> OkWithDomainExceptionsHandling(this Controller controller, Func<Task> action)
    {
        return controller.HandleDomainExceptionsInternal<ActionResult>(
            async () =>
            {
                await action();

                return controller.Ok();
            }
        );
    }

    public static Task<ActionResult<TData>> CreatedWithDomainExceptionsHandling<TData>(
        this Controller controller,
        Func<Task<TData>> action
    )
    {
        return controller.HandleDomainExceptionsInternal<ActionResult<TData>>(
            async () =>
            {
                TData id = await action();

                return controller.Created((string?)null, id);
            }
        );
    }

    public static Task<ActionResult> CreatedWithDomainExceptionsHandling(this Controller controller, Func<Task> action)
    {
        return controller.HandleDomainExceptionsInternal<ActionResult>(
            async () =>
            {
                await action();

                return controller.Created();
            }
        );
    }

    private static async Task<TResult> HandleDomainExceptionsInternal<TResult>(
        this Controller controller,
        Func<Task<TResult>> action
    ) where TResult : class
    {
        try
        {
            return await action();
        }
        catch (UnauthorizedException)
        {
            return (controller.Unauthorized() as TResult)!;
        }
        catch (ForbiddenException)
        {
            return (controller.Forbid() as TResult)!;
        }
        catch (ConflictException)
        {
            return (controller.Conflict() as TResult)!;
        }
    }
}