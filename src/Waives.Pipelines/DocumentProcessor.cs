using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Waives.Pipelines
{
    internal class DocumentProcessor : IDocumentProcessor
    {
        private readonly Func<Document, CancellationToken, Task<WaivesDocument>> _documentCreator;
        private readonly IEnumerable<Func<WaivesDocument, CancellationToken, Task<WaivesDocument>>> _documentActions;
        private readonly Func<WaivesDocument, Task> _documentDeleter;
        private readonly Action<Exception, Document> _onDocumentException;

        public DocumentProcessor(Func<Document, CancellationToken, Task<WaivesDocument>> documentCreator,
            IEnumerable<Func<WaivesDocument, CancellationToken, Task<WaivesDocument>>> documentActions,
            Func<WaivesDocument, Task> documentDeleter,
            Action<Exception, Document> onDocumentException)
        {
            _documentCreator = documentCreator ?? throw new ArgumentNullException(nameof(documentCreator));
            _documentActions = documentActions ?? throw new ArgumentNullException(nameof(documentActions));
            _documentDeleter = documentDeleter ?? throw new ArgumentNullException(nameof(documentDeleter));
            _onDocumentException = onDocumentException ?? throw new ArgumentNullException(nameof(onDocumentException));
        }

        public async Task RunAsync(Document document, CancellationToken cancellationToken = default)
        {
            WaivesDocument waivesDocument = null;
            try
            {
                waivesDocument = await _documentCreator(document, cancellationToken);
                await RunActionsAsync(waivesDocument, cancellationToken).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                _onDocumentException(e, document);
            }
            finally
            {
                if (waivesDocument != null)
                {
                    await _documentDeleter(waivesDocument);
                }
            }
        }

        private async Task RunActionsAsync(WaivesDocument document, CancellationToken cancellationToken = default)
        {
            foreach (var documentAction in _documentActions)
            {
                document = await documentAction(document, cancellationToken).ConfigureAwait(false);
            }
        }
    }
}