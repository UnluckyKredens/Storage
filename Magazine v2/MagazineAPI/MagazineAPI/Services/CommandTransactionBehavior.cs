using MagazineAPIApplication.Modules.Authentication;
using MagazineAPIDomain.Repositories;
using Mediator;

namespace MagazineAPI.Services;

// The entire handler (including its initial checks and outbox writes) is retried.
public sealed class CommandTransactionBehavior<TMessage, TResponse>(IUnitOfWork unitOfWork)
    : IPipelineBehavior<TMessage, TResponse> where TMessage : IMessage
{
    public async ValueTask<TResponse> Handle(TMessage message, MessageHandlerDelegate<TMessage, TResponse> next,
        CancellationToken cancellationToken)
    {
        if (message is not IBaseCommand || message is LoginUserCommand)
            return await next(message, cancellationToken);

        TResponse result = default!;
        await unitOfWork.ExecuteInTransactionAsync(async ct => result = await next(message, ct), cancellationToken);
        return result;
    }
}
