using System;
using Waives.Client.Responses;

namespace Waives
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
            return $"{ClassificationResult.DocumentType} ({(ClassificationResult.IsConfident ? "" : "Not ")}Confident)";
        }
    }
}