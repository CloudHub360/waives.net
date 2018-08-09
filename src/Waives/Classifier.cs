using System;
using System.Reactive.Linq;
using System.Threading.Tasks;

namespace Waives
{
    public class Classifier : IObservable<DocumentClassification>
    {
        private readonly string _name;

        private Client.Classifier _classifier;
        private readonly IObservable<Document> _documentChannel;

        public Classifier(string name, IObservable<Document> documentChannel)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentNullException(nameof(name));
            }

            _name = name;
            _documentChannel = documentChannel;
        }

        public IDisposable Subscribe(IObserver<DocumentClassification> observer)
        {
            _classifier = _classifier ?? Task.Run(() => Waives.ApiClient.GetClassifier(_name)).Result;

            return _documentChannel.Select(d => Task.Run(() => ClassifyDocument(d)).Result).Subscribe(observer);
        }

        private async Task<DocumentClassification> ClassifyDocument(Document document)
        {
            using (var documentStream = await document.OpenStream().ConfigureAwait(false))
            {
                var classificationResult = await _classifier.Classify(documentStream).ConfigureAwait(false);
                return new DocumentClassification(document, classificationResult);
            }
        }
    }
}