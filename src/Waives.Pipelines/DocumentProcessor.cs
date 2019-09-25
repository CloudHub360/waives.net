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

        public async Task Run(Document doc)
        {
            try
            {
                var waivesDocument = await _docCreator(doc);
                await PerformDocActions(waivesDocument).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                _onDocumentException(e, doc);
            }
        }

        private async Task PerformDocActions(WaivesDocument doc)
        {
            foreach (var docAction in _docActions)
            {
                doc = await docAction(doc).ConfigureAwait(false);
            }
        }
    }
}