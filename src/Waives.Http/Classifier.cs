using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Waives.Http.Responses;

namespace Waives.Http
{
    public class Classifier
    {
        private readonly WaivesClient _waivesClient;
        private readonly string _name;
        private readonly IDictionary<string, HalUri> _behaviours;

        internal Classifier(WaivesClient httpClient, string name, IDictionary<string, HalUri> behaviours)
        {
            _waivesClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));

            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("message", nameof(name));
            }

            _name = name;
            _behaviours = behaviours ?? throw new ArgumentNullException(nameof(behaviours));
        }

        public async Task AddSamplesFromZip(string path)
        {
            var streamContent = new StreamContent(File.OpenRead(path));
            streamContent.Headers.ContentType = new MediaTypeHeaderValue("application/zip");

            var response = await _waivesClient.HttpClient.PostAsync(
                _behaviours["classifier:add_samples_from_zip"].CreateUri(),
                streamContent).ConfigureAwait(false);

            if (!response.IsSuccessStatusCode)
            {
                throw new WaivesApiException();
            }
        }

        public async Task<ClassificationResult> Classify(Stream documentStream)
        {
            var document = await _waivesClient.CreateDocument(documentStream).ConfigureAwait(false);
            try
            {
                return await document.Classify(_name).ConfigureAwait(false);
            }
            finally
            {
                await document.Delete().ConfigureAwait(false);
            }
        }
    }
}
