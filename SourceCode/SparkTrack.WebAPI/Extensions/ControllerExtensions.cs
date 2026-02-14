namespace SparkTrack.WebAPI.Extensions;

using Core.Exceptions;
using Microsoft.AspNetCore.Mvc;

public static class ControllerExtensions
{
    extension(Controller controller)
    {
        public Task<ActionResult<TData>> OkWithDomainExceptionsHandling<TData>(
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

        public Task<ActionResult> OkWithDomainExceptionsHandling(Func<Task> action)
        {
            return controller.HandleDomainExceptionsInternal<ActionResult>(
                async () =>
                {
                    await action();

                    return controller.Ok();
                }
            );
        }

        public Task<ActionResult<TData>> CreatedWithDomainExceptionsHandling<TData>(
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

        public Task<ActionResult> CreatedWithDomainExceptionsHandling(Func<Task> action)
        {
            return controller.HandleDomainExceptionsInternal<ActionResult>(
                async () =>
                {
                    await action();

                    return controller.Created();
                }
            );
        }

        private async Task<TResult> HandleDomainExceptionsInternal<TResult>(
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
            catch (NotFoundException)
            {
                return (controller.NotFound() as TResult)!;
            }
        }
    }
}