using System;
using Newtonsoft.Json;

namespace Waives.Http.Responses
{
    // ReSharper disable once ClassNeverInstantiated.Global
    internal class AccessToken
    {
        internal string Token { get; }

        internal string Type { get; }

        internal TimeSpan LifeTime { get; }

        [JsonConstructor] // ReSharper disable InconsistentNaming
        public AccessToken(string access_token, string token_type, int expires_in)
        {
            Token = access_token;
            Type = token_type;
            LifeTime = TimeSpan.FromSeconds(expires_in);
        }

        public override string ToString()
        {
            return Token;
        }
    }
}