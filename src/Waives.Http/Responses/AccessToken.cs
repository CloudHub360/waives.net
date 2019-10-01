using System;
using Newtonsoft.Json;

namespace Waives.Http.Responses
{
    // ReSharper disable once ClassNeverInstantiated.Global
    internal class AccessToken
    {
        private readonly string _token;
        private readonly string _type;

        internal TimeSpan LifeTime { get; }

        [JsonConstructor] // ReSharper disable InconsistentNaming
        public AccessToken(string access_token, string token_type, int expires_in)
        {
            _token = access_token;
            _type = token_type;
            LifeTime = TimeSpan.FromSeconds(expires_in);
        }

        public override string ToString()
        {
            return $"{_type} {_token}";
        }
    }
}