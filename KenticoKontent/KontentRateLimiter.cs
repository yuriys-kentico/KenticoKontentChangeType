using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

using Core.KenticoKontent.Services;

namespace KenticoKontent
{
    public class KontentRateLimiter : IKontentRateLimiter
    {
        private const int buffer = 1;
        private const int secondsMax = 10 - buffer;
        private static readonly Queue<DateTime> secondsQueue = new Queue<DateTime>(secondsMax);
        private const int minutesMax = 400 - buffer;
        private static readonly Queue<DateTime> minutesQueue = new Queue<DateTime>(minutesMax);
        private const int hoursMax = 15000 - buffer;
        private static readonly Queue<DateTime> hoursQueue = new Queue<DateTime>(hoursMax);

        private readonly IKontentApiTracker kontentApiTracker;

        public KontentRateLimiter(IKontentApiTracker kontentApiTracker)
        {
            this.kontentApiTracker = kontentApiTracker;
        }

        public async Task<HttpResponseMessage> WithRetry(Func<Task<HttpResponseMessage>> doRequest)
        {
            static async Task CheckQueue(Queue<DateTime> queue, TimeSpan span, int max)
            {
                queue.Enqueue(DateTime.Now.Add(span));

                if (queue.Count == max)
                {
                    var now = DateTime.Now;
                    var next = queue.Dequeue();
                    var flushed = false;

                    while (now > next)
                    {
                        next = queue.Dequeue();
                        flushed = true;
                    }

                    if (!flushed)
                    {
                        await Task.Delay((int)(next - now).TotalMilliseconds);
                    }
                }
            }

            await CheckQueue(hoursQueue, TimeSpan.FromHours(1), hoursMax);
            await CheckQueue(minutesQueue, TimeSpan.FromMinutes(1), minutesMax);
            await CheckQueue(secondsQueue, TimeSpan.FromSeconds(1), secondsMax);

            var response = await doRequest();

            if (response.StatusCode == HttpStatusCode.TooManyRequests && response.Headers.RetryAfter.Delta != null)
            {
                await Task.Delay(response.Headers.RetryAfter.Delta.Value);

                return await WithRetry(doRequest);
            }

            kontentApiTracker.ApiCalls++;

            return response;
        }
    }
}