using System;
using Waives.Http;

namespace Waives.Pipelines
{
    /// <summary>
    /// Options for configuring the interaction with the Waives API during
    /// pipeline processing.
    /// </summary>
    public class WaivesOptions
    {
        /// <summary>
        /// The Waives API Client ID to use when authenticating with the service.
        /// </summary>
        public string ClientId { get; set; }

        /// <summary>
        /// The Waives API Client Secret to use when authenticating with the service.
        /// </summary>
        public string ClientSecret { get; set; }

        /// <summary>
        /// The URL on which the Waives API is running. This only needs overriding for
        /// connections to instances other than the public cloud-hosted service.
        /// </summary>
        public Uri ApiUrl { get; set; } = new Uri(WaivesClient.DefaultUrl);

        /// <summary>
        /// If set to <c>true</c>, it will (immediately) delete all documents
        /// in existence in the Waives account; if set to <c>false</c>, no such clean up will be completed.
        /// Defaults to <c>true</c>.
        /// </summary>
        public bool DeleteExistingDocuments { get; set; } = true;

        /// <summary>
        /// The maximum number of documents to process concurrently.
        /// </summary>
        public int MaxConcurrency { get; set; } = RateLimiter.DefaultMaximumConcurrentDocuments;
    }
}