using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace Core.KenticoKontent.Services
{
    public interface IKontentRateLimiter
    {
        Task<HttpResponseMessage> WithRetry(Func<Task<HttpResponseMessage>> doRequest);
    }
}