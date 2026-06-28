using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace PowerUpSQLSharp.Core.Utils
{
    internal static class ThreadedExecutor
    {
        public static IReadOnlyList<T> Execute<T>(
            IEnumerable<string> items,
            int maxThreads,
            Func<string, CancellationToken, IReadOnlyList<T>> worker,
            CancellationToken cancellationToken = default)
        {
            var results = new List<T>();
            var itemList = new List<string>(items ?? Array.Empty<string>());
            if (itemList.Count == 0)
            {
                return results;
            }

            var threadCount = Math.Max(1, Math.Min(maxThreads, itemList.Count));
            using (var gate = new SemaphoreSlim(threadCount, threadCount))
            {
                var tasks = new List<Task<IReadOnlyList<T>>>(itemList.Count);

                foreach (var item in itemList)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    gate.Wait(cancellationToken);

                    tasks.Add(Task.Run(() =>
                    {
                        try
                        {
                            return worker(item, cancellationToken);
                        }
                        finally
                        {
                            gate.Release();
                        }
                    }, cancellationToken));
                }

                Task.WaitAll(tasks.ToArray(), cancellationToken);

                foreach (var task in tasks)
                {
                    if (task.IsFaulted)
                    {
                        throw task.Exception?.GetBaseException()
                            ?? new InvalidOperationException("Threaded worker failed.");
                    }

                    foreach (var result in task.Result)
                    {
                        results.Add(result);
                    }
                }
            }

            return results;
        }
    }
}
