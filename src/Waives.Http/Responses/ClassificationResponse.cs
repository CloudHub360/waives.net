using System.Collections.Generic;
using Newtonsoft.Json;

namespace Waives.Http.Responses
{
    internal class ClassificationResponse
    {
        [JsonProperty("document_id")]
        internal string DocumentId {get; set; }

        [JsonProperty("classification_results")]
        internal ClassificationResult ClassificationResults { get; set; }
    }

    /// <summary>
    /// Represents the results of a <see cref="Document.ClassifyAsync"/> operation.
    /// </summary>
    /// <remarks>
    /// <para>
    /// In most cases, the <see cref="DocumentType"/> and <see cref="IsConfident"/> flags are the only ones you should use
    /// in production.
    /// </para>
    /// <para>
    /// If <see cref="IsConfident"/> is false, you should not trust <see cref="DocumentType"/>. Depending on your
    /// scenario, you may want to route this document for a person to review, or handle differently in some other way.
    /// </para>
    /// <para>In general, unconfident classifications indicate that the content of the document is substantially different
    /// from any documents contained in your sample set. Therefore you may also wish to capture these documents, analyze them
    /// and if appropriate add some to your samples and retrain your classifier.
    /// </para>
    /// </remarks>
    public class ClassificationResult
    {
        /// <summary>
        /// Gets the document type that Waives believes the document to be.
        /// </summary>
        [JsonProperty("document_type")]
        public string DocumentType { get; internal set; }

        /// <summary>
        /// Gets a value giving an indication of how confident Waives is that the
        /// document type is correct.
        /// </summary>
        [JsonProperty("relative_confidence")]
        public decimal RelativeConfidence { get; internal set; }

        /// <summary>
        /// Gets a value indicating whether Waives is confident that document type
        /// is correct. If the value is <c>false</c>, you should not trust the
        /// value of <see cref="DocumentType"/>.
        /// </summary>
        [JsonProperty("is_confident")]
        public bool IsConfident { get; internal set; }

        /// <summary>
        /// Gets a collection of scores for each possible document type in the classifier.
        /// </summary>
        [JsonProperty("document_type_scores")]
        public IEnumerable<DocumentTypeScore> DocumentTypeScores { get; internal set; }
    }

    /// <summary>
    /// Represents a classification score for a given document type.
    /// </summary>
    public class DocumentTypeScore
    {
        /// <summary>
        /// Gets the document type associated with this score.
        /// </summary>
        [JsonProperty("document_type")]
        public string DocumentType { get; internal set; }

        /// <summary>
        /// Gets the score associated with this document type.
        /// </summary>
        [JsonProperty("score")]
        public decimal Score { get; internal set; }
    }
}
