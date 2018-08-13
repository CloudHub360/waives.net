namespace Waives.Reactive
{
    /// <summary>
    /// Represents a document within the Waives API.
    /// </summary>
    public class WaivesDocument
    {
        private readonly Http.Document _waivesDocument;

        internal WaivesDocument(Document source, Http.Document waivesDocument)
        {
            Source = source;
            _waivesDocument = waivesDocument;
        }

        public Document Source { get; }
    }
}