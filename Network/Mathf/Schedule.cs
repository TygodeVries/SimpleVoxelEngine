using System.Collections.Concurrent;

namespace Shared.Mathf;

public static class Schedule
{
    private static readonly ConcurrentQueue<Action> Actions = new();

    public static void Tick()
    {
        while (Actions.TryDequeue(out var action))
            action();
    }

    public static void Run(Action action)
    {
        ArgumentNullException.ThrowIfNull(action);
        Actions.Enqueue(action);
    }

    public static CancellationTokenSource RunLater(Action action, TimeSpan delay, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(action);

        var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

        _ = Task.Run(async () =>
        {
            try
            {
                await Task.Delay(delay, cts.Token);
                if (!cts.IsCancellationRequested)
                    Run(action);
            }
            catch (OperationCanceledException)
            {
            }
            finally
            {
                cts.Dispose();
            }
        });

        return cts;
    }

    public static CancellationTokenSource RunRepeated(Action action, TimeSpan interval, int? amount = null, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(action);

        var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

        _ = Task.Run(async () =>
        {
            try
            {
                for (int i = 0; amount is null || i < amount.Value; i++)
                {
                    await Task.Delay(interval, cts.Token);
                    Run(action);
                }
            }
            catch (OperationCanceledException)
            {
            }
            finally
            {
                cts.Dispose();
            }
        });

        return cts;
    }
}
