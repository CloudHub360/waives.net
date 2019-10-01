using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Waives.Pipelines
{
    internal class DocumentProcessor : IDocumentProcessor
    {
        private readonly Func<Document, CancellationToken, Task<WaivesDocument>> _docCreator;
        private readonly IEnumerable<Func<WaivesDocument, CancellationToken, Task<WaivesDocument>>> _docActions;
        private readonly Func<WaivesDocument, Task> _docDeleter;
        private readonly Action<Exception, Document> _onDocumentException;

        public DocumentProcessor(Func<Document, CancellationToken, Task<WaivesDocument>> docCreator,
            IEnumerable<Func<WaivesDocument, CancellationToken, Task<WaivesDocument>>> docActions,
            Func<WaivesDocument, Task> docDeleter,
            Action<Exception, Document> onDocumentException)
        {
            _docCreator = docCreator ?? throw new ArgumentNullException(nameof(docCreator));
            _docActions = docActions ?? throw new ArgumentNullException(nameof(docActions));
            _docDeleter = docDeleter ?? throw new ArgumentNullException(nameof(docDeleter));
            _onDocumentException = onDocumentException ?? throw new ArgumentNullException(nameof(onDocumentException));
        }

        public async Task RunAsync(Document document, CancellationToken cancellationToken = default)
        {
            WaivesDocument waivesDocument = null;
            try
            {
                waivesDocument = await _docCreator(document, cancellationToken);
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
                    await _docDeleter(waivesDocument);
                }
            }
        }

        private async Task RunActionsAsync(WaivesDocument document, CancellationToken cancellationToken = default)
        {
            foreach (var docAction in _docActions)
            {
                document = await docAction(document, cancellationToken).ConfigureAwait(false);
            }
        }
    }
}