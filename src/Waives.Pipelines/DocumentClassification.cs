using System;
using Waives.Http.Responses;

namespace Waives.Pipelines
{
    public class DocumentClassification
    {
        public DocumentClassification(Document document, ClassificationResult classificationResult)
        {
            Document = document ?? throw new ArgumentNullException(nameof(document));
            ClassificationResult = classificationResult ?? throw new ArgumentNullException(nameof(classificationResult));
        }

        public Document Document { get; }

        public ClassificationResult ClassificationResult { get; }

        public override string ToString()
        {
            return $"{Document.SourceId}: {ClassificationResult.DocumentType} ({(ClassificationResult.IsConfident ? "" : "Not ")}Confident)";
        }
    }
}