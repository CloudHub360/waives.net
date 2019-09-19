using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Polly;
using Polly.Caching;
using Polly.Caching.Memory;
using Waives.Http.RequestHandling;

namespace Waives.Http.Responses
{
    internal class AccessTokenService : IDisposable
    {
        private readonly IHttpRequestSender _requestSender;
        private readonly MemoryCache _cache;
        private readonly AsyncCachePolicy<AccessToken> _cachePolicy;

        internal AccessTokenService(IHttpRequestSender requestSender)
        {
            _requestSender = requestSender;
            _cache = new MemoryCache(new MemoryCacheOptions());

            _cachePolicy = Policy.CacheAsync(
                new MemoryCacheProvider(_cache).AsyncFor<AccessToken>(),
                new ResultTtl<AccessToken>(t => new Ttl(t.LifeTime)),
                (_, __, ___) => { });
        }

        internal async Task<AccessToken> FetchAccessToken(string clientId, string clientSecret)
        {
            return await _cachePolicy.ExecuteAsync(async () =>
            {
                var request = new HttpRequestMessageTemplate(HttpMethod.Post, new Uri("/oauth/token", UriKind.Relative))
                {
                    Content = new FormUrlEncodedContent(new Dictionary<string, string>
                    {
                        {"client_id", clientId},
                        {"client_secret", clientSecret}
                    })
                };

                var response = await _requestSender.Send(request).ConfigureAwait(false);
                return await response.Content.ReadAsAsync<AccessToken>().ConfigureAwait(false);
            }).ConfigureAwait(false);
        }

        public void Dispose()
        {
            _cache.Dispose();
        }
    }
}