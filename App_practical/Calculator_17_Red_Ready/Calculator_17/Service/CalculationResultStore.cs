
using System.Collections.Concurrent;
using Calculator_17.Models;

namespace Calculator_17.Services;

public class CalculationResultStore
{
    private readonly ConcurrentDictionary<Guid, TaskCompletionSource<CalculationMessage>> _pending = new();

    public Task<CalculationMessage> WaitForResultAsync(Guid requestId, CancellationToken cancellationToken)
    {
        var source = new TaskCompletionSource<CalculationMessage>(
            TaskCreationOptions.RunContinuationsAsynchronously);

        if (!_pending.TryAdd(requestId, source))
            throw new InvalidOperationException("Такой запрос уже существует.");

        cancellationToken.Register(() =>
        {
            if (_pending.TryRemove(requestId, out var pending))
                pending.TrySetCanceled(cancellationToken);
        });

        return source.Task;
    }

    public void Complete(CalculationMessage message)
    {
        if (_pending.TryRemove(message.RequestId, out var source))
            source.TrySetResult(message);
    }
}
