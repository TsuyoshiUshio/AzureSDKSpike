using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Reactive.Linq;

namespace AzureSDKSpike
{
    public class AsyncEnumeratorSpike
    {
        public async ValueTask ExecuteAsync()
        {
            await foreach (var number in RangeAsync(0, 5))
            {
                Console.WriteLine(number);
            }

            CancellationTokenSource cts = new CancellationTokenSource(400);
            await foreach(var number in RangeWithCancellationAsync(0, 10).WithCancellation(cts.Token))
            {
                // Expect that it will finish at 4.
                Console.WriteLine(number);
            }

            // Reactive support IAsyncEnumerator
            var observable = Observable.Interval(TimeSpan.FromSeconds(1))
                .Take(5)
                .Select(l => (int)l);
            await foreach(var number in observable.ToAsyncEnumerable())
            {
                Console.WriteLine($"from Rx: {number}");
            }

        }

        private async IAsyncEnumerable<int> RangeAsync(int start, int count)
        {
            for (int i = 0; i < count; i++)
            {
                await Task.Delay(100);
                yield return start + i;
            }
        }

        private async IAsyncEnumerable<int> RangeWithCancellationAsync(int start, int count, [EnumeratorCancellation] CancellationToken token = default)
        {
            for (int i = 0; i < count; i++)
            {
                try
                {
                    await Task.Delay(100, token);
                }
                catch (TaskCanceledException)
                {
                    Console.WriteLine("Task was cancelled.");
                    break;  // Without this break, iterator keep on going.
                }
                yield return start + i;
            }
        }
    }
}
