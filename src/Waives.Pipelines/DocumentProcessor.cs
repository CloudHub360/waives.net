using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Waives.Pipelines
{
    internal class DocumentProcessor : IDocumentProcessor
    {
        private readonly Func<Document, Task<WaivesDocument>> _docCreator;
        private readonly IEnumerable<Func<WaivesDocument, Task<WaivesDocument>>> _docActions;
        private readonly Action<Exception, Document> _onDocumentException;

        public DocumentProcessor(Func<Document, Task<WaivesDocument>> docCreator,
            IEnumerable<Func<WaivesDocument, Task<WaivesDocument>>> docActions,
            Action<Exception, Document> onDocumentException)
        {
            _docCreator = docCreator ?? throw new ArgumentNullException(nameof(docCreator));
            _docActions = docActions ?? throw new ArgumentNullException(nameof(docActions));
            _onDocumentException = onDocumentException ?? throw new ArgumentNullException(nameof(onDocumentException));
        }

        public async Task RunAsync(Document document)
        {
            try
            {
                var waivesDocument = await _docCreator(document);
                await RunActions(waivesDocument).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                _onDocumentException(e, document);
            }
        }

        private async Task RunActions(WaivesDocument document)
        {
            foreach (var docAction in _docActions)
            {
                document = await docAction(document).ConfigureAwait(false);
            }
        }
    }
}