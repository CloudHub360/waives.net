using System;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Waives.Client;

namespace Waives.NET
{
    public class ClassificationResultStream : IObservable<DocumentClassification>
    {
        private readonly Classifier _classifier;
        private readonly IObservable<IDocumentSource> _documentStream;

        public ClassificationResultStream(Classifier classifier, IObservable<IDocumentSource> documentStream)
        {
            _classifier = classifier;
            _documentStream = documentStream;
        }

        public IDisposable Subscribe(IObserver<DocumentClassification> observer)
        {
            var subscription = _documentStream.Select(d => Task.Run(() => ClassifyDocument(d)).Result).Subscribe(observer);
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