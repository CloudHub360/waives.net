using System;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Waives.Client;

namespace Waives
{
    public class ClassificationResultChannel : IObservable<DocumentClassification>
    {
        private readonly Classifier _classifier;
        private readonly IObservable<Document> _documentChannel;

        public ClassificationResultChannel(Classifier classifier, IObservable<Document> documentChannel)
        {
            _classifier = classifier;
            _documentChannel = documentChannel;
        }

        public IDisposable Subscribe(IObserver<DocumentClassification> observer)
        {
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