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
        private readonly IObservable<IDocumentSource> _documentChannel;

        public ClassificationResultChannel(Classifier classifier, IObservable<IDocumentSource> documentChannel)
        {
            _classifier = classifier;
            _documentChannel = documentChannel;
        }

        public IDisposable Subscribe(IObserver<DocumentClassification> observer)
        {
            var subscription = _documentChannel.Select(d => Task.Run(() => ClassifyDocument(d)).Result).Subscribe(observer);
            return Disposable.Create(() => subscription.Dispose());
        }

        private async Task<DocumentClassification> ClassifyDocument(IDocumentSource document)
        {
            using (var documentStream = await document.OpenStream().ConfigureAwait(false))
            {
                var classificationResult = await _classifier.Classify(documentStream).ConfigureAwait(false);
                return new DocumentClassification(document, classificationResult);
            }
        }
    }
}