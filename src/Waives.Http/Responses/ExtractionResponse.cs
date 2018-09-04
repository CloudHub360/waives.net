using System.Collections.Generic;
using Newtonsoft.Json;

namespace Waives.Http.Responses
{
    /// <summary>
    /// Represents an extraction result response.
    /// </summary>
    public class ExtractionResponse
    {
        /// <summary>
        /// Gets the extraction results for the document.
        /// </summary>
        [JsonProperty("field_results")]
        public IEnumerable<ExtractionResult> ExtractionResults { get; internal set; }

        /// <summary>
        /// Gets extraction-specific document metadata.
        /// </summary>
        [JsonProperty("document")]
        public ExtractionDocumentMetadata DocumentMetadata { get; internal set; }
    }

    /// <summary>
    /// Represents an individual extraction result.
    /// </summary>
    public class ExtractionResult
    {
        /// <summary>
        /// Gets the field name for this extraction result.
        /// </summary>
        [JsonProperty("field_name")]
        public string FieldName { get; internal set; }

        /// <summary>
        /// Gets a flag indicating whether or not this extraction result was rejected.
        /// </summary>
        [JsonProperty("rejected")]
        public bool Rejected { get; internal set; }

        /// <summary>
        /// Gets a message indicating why this extraction result was rejected.
        /// </summary>
        [JsonProperty("reject_reason")]
        public string RejectReason { get; internal set; }

        /// <summary>
        /// Gets the field value for this extraction result.
        /// </summary>
        [JsonProperty("result")]
        public ExtractionResult FieldResult { get; internal set; }

        /// <summary>
        /// Gets a collection of alternative extraction results for this field.
        /// </summary>
        [JsonProperty("alternatives")]
        public IEnumerable<ExtractionResult> AlternativeMatches { get; internal set; }

        /// <summary>
        /// Gets the document text matching this extraction result
        /// </summary>
        [JsonProperty("text")]
        public string Text { get; internal set; }

        /// <summary>
        /// Gets the value of this extraction result
        /// </summary>
        [JsonProperty("value")]
        public string Value { get; internal set; }

        /// <summary>
        ///
        /// </summary>
        [JsonProperty("proximity_score")]
        public double ProximityScore { get; internal set; }

        /// <summary>
        ///
        /// </summary>
        [JsonProperty("match_score")]
        public double MatchScore { get; internal set; }

        /// <summary>
        ///
        /// </summary>
        [JsonProperty("text_score")]
        public double TextScore { get; internal set; }

        /// <summary>
        ///
        /// </summary>
        [JsonProperty("areas")]
        public IEnumerable<ExtractionResultArea> ResultAreas { get; internal set; }
    }

    /// <summary>
    /// Represents the area on a page where an extraction result occurs.
    /// </summary>
    public class ExtractionResultArea
    {
        /// <summary>
        /// Gets the top boundary of the extraction result area.
        /// </summary>
        [JsonProperty("top")]
        public double Top { get; internal set; }

        /// <summary>
        /// Gets the left boundary of the extraction result area.
        /// </summary>
        [JsonProperty("left")]
        public double Left { get; internal set; }

        /// <summary>
        /// Gets the bottom boundary of the extraction result area.
        /// </summary>
        [JsonProperty("bottom")]
        public double Bottom { get; internal set; }

        /// <summary>
        /// Gets the right boundary of the extraction result area.
        /// </summary>
        [JsonProperty("right")]
        public double Right { get; internal set; }

        /// <summary>
        /// Gets the page number where the extraction result appears.
        /// </summary>
        [JsonProperty("page_number")]
        public int PageNumber { get; internal set; }
    }

    /// <summary>
    /// Provides access to extraction-specific metadata on a document.
    /// </summary>
    public class ExtractionDocumentMetadata
    {
        /// <summary>
        /// Gets the number of pages in a document.
        /// </summary>
        [JsonProperty("page_count")]
        public int PageCount { get; internal set; }

        /// <summary>
        /// Gets a collection of all the pages' metadata for a document.
        /// </summary>
        [JsonProperty("pages")]
        public IEnumerable<ExtractionPage> Pages { get; internal set; }
    }

    /// <summary>
    /// Provides access to extraction-specific metadata on a document's page.
    /// </summary>
    public class ExtractionPage
    {
        /// <summary>
        /// Gets the page number of the page.
        /// </summary>
        [JsonProperty("page_number")]
        public int PageNumber { get; internal set; }

        /// <summary>
        /// Gets the width of the page.
        /// </summary>
        [JsonProperty("width")]
        public double Width { get; internal set; }

        /// <summary>
        /// Gets the height of the page.
        /// </summary>
        [JsonProperty("height")]
        public double Height { get; internal set; }
    }
}
