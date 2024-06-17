using St.HolyChain.Core.Exceptions;

namespace St.HolyChain.Core.Helpers;

internal static class TaskHelper
{
    internal static async Task WhenAll(params Task[] tasks)
    {
        ArgumentNullException.ThrowIfNull(tasks);

        if (tasks.Length == 0)
        {
            return;
        }

        if (tasks.Length == 1)
        {
            await tasks[0];
        }

        try
        {
            await Task.WhenAll(tasks);
        }
        catch
        {
            var exceptions = new List<Exception>();
            foreach (var item in tasks)
            {
                if (item.IsCanceled)
                {
                    exceptions.Add(new HandlerCancelledException());

                }
                else if (item.IsFaulted)
                {
                    var aggregateExceptions = item.Exception?.InnerExceptions;
                    if (aggregateExceptions is null)
                    {
                        continue;
                    }

                    exceptions.AddRange(aggregateExceptions);
                }
            }

            throw new AggregateException(exceptions);
        }
    }
}
