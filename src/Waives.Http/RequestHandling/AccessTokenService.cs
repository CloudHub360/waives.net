using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Polly;
using Polly.Caching;
using Polly.Caching.Memory;
using Waives.Http.Logging;
using Waives.Http.Responses;

namespace Waives.Http.RequestHandling
{
    internal class AccessTokenService : IDisposable
    {
        private static readonly ILog Logger = LogProvider.GetCurrentClassLogger();

        private readonly string _clientId;
        private readonly string _clientSecret;
        private readonly IHttpRequestSender _requestSender;
        private readonly MemoryCache _cache;
        private readonly AsyncCachePolicy<AccessToken> _cachePolicy;

        internal AccessTokenService(string clientId, string clientSecret, IHttpRequestSender requestSender)
        {
            _clientId = clientId;
            _clientSecret = clientSecret;
            _requestSender = requestSender;

            _cache = new MemoryCache(new MemoryCacheOptions());
            _cachePolicy = Policy.CacheAsync(
                new MemoryCacheProvider(_cache).AsyncFor<AccessToken>(),
                new ResultTtl<AccessToken>(t => new Ttl(t.LifeTime - TimeSpan.FromHours(1))),
                onCacheError: (ctx, _, ex) =>
                {
                    Logger.ErrorException($"Could not retrieve access token: '{ex.Message}'", ex);
                });
        }

        internal async Task<AccessToken> FetchAccessTokenAsync()
        {
            return await _cachePolicy.ExecuteAsync(async context =>
            {
                var request = new HttpRequestMessageTemplate(HttpMethod.Post, new Uri("/oauth/token", UriKind.Relative))
                {
                    Content = new FormUrlEncodedContent(new Dictionary<string, string>
                    {
                        { "client_id", _clientId },
                        { "client_secret", _clientSecret }
                    })
                };

                var response = await _requestSender.SendAsync(request).ConfigureAwait(false);
                return await response.Content.ReadAsAsync<AccessToken>().ConfigureAwait(false);
            }, new Context(nameof(FetchAccessTokenAsync))).ConfigureAwait(false);
        }

        public void Dispose()
        {
            _cache.Dispose();
        }
    }
}