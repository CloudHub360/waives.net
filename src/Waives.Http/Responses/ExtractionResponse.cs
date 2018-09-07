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
        public IEnumerable<FieldResult> FieldResults { get; internal set; }

        /// <summary>
        /// Gets extraction-specific document metadata.
        /// </summary>
        [JsonProperty("document")]
        public ExtractionDocumentMetadata DocumentMetadata { get; internal set; }
    }

    public class FieldResult
    {
        /// <summary>
        /// Gets the name of the field
        /// </summary>
        [JsonProperty("field_name")]
        public string FieldName { get; internal set; }

        /// <summary>
        /// Gets a flag indicating whether the field results should be considered potentially invalid
        /// </summary>
        [JsonProperty("rejected")]
        public bool Rejected { get; internal set; }

        /// <summary>
        /// Gets a message indicating the reason for rejection of the field
        /// </summary>
        [JsonProperty("reject_reason")]
        public string RejectReason { get; internal set; }

        /// <summary>
        /// Gets the primary result for the field (null for a table field)
        /// </summary>
        [JsonProperty("result")]
        public ExtractionResult Result { get; internal set; }

        /// <summary>
        /// Gets a collection of secondary (alternative) results for the field
        /// </summary>
        [JsonProperty("alternatives")]
        public IEnumerable<ExtractionResult> Alternatives { get; internal set; }
    }

    /// <summary>
    /// Represents an individual extraction result.
    /// </summary>
    public class ExtractionResult
    {
        /// <summary>
        /// Gets the text of the result
        /// </summary>
        [JsonProperty("text")]
        public string Text { get; internal set; }

        /// <summary>
        /// Gets the value of the result as a non-text type (e.g. Decimal or DateTime), if available
        /// </summary>
        [JsonProperty("value")]
        public string Value { get; internal set; }

        /// <summary>
        /// Gets a score indicating how well any proximity rules have been met
        /// </summary>
        [JsonProperty("proximity_score")]
        public double ProximityScore { get; internal set; }

        /// <summary>
        /// Gets a score indicating how well the text matched any search criteria
        /// </summary>
        [JsonProperty("match_score")]
        public double MatchScore { get; internal set; }

        /// <summary>
        /// Gets a score indicating the OCR confidence assigned to the actual text that was extracted
        /// </summary>
        [JsonProperty("text_score")]
        public double TextScore { get; internal set; }

        /// <summary>
        /// Gets a list of areas from which the result originated
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
