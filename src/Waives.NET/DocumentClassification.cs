using System;
using Waives.Client.Responses;

namespace Waives.NET
{
    public class DocumentClassification
    {
        public DocumentClassification(IDocumentSource document, ClassificationResult classificationResult)
        {
            Document = document ?? throw new ArgumentNullException(nameof(document));
            ClassificationResult = classificationResult ?? throw new ArgumentNullException(nameof(classificationResult));
        }

        public IDocumentSource Document { get; }

        public ClassificationResult ClassificationResult { get; }

        public override string ToString()
        {
            return $"{ClassificationResult.DocumentType} ({(ClassificationResult.IsConfident ? "" : "Not ")}Confident)";
        }
    }
}