using System.Collections.Generic;
using Newtonsoft.Json;

namespace Waives.Client.Responses
{
    internal class ClassificationResponse
    {
        [JsonProperty("document_id")]
        internal string DocumentId {get; set; }

        [JsonProperty("classification_results")]
        internal ClassificationResult ClassificationResults { get; set; }
    }

    public class ClassificationResult
    {
        [JsonProperty("document_type")]
        public string DocumentType { get; set; }

        [JsonProperty("relative_confidence")]
        public decimal RelativeConfidence { get; set; }

        [JsonProperty("is_confident")]
        public bool IsConfident { get; set; }

        [JsonProperty("document_type_scores")]
        public IEnumerable<DocumentTypeScore> DocumentTypeScores { get; set; }
    }

    public class DocumentTypeScore
    {
        [JsonProperty("document_type")]
        public string DocumentType { get; set; }

        [JsonProperty("score")]
        public decimal Score { get; set; }
    }
}
