using System.Collections.Generic;
using Newtonsoft.Json;

namespace Waives.Http.Responses
{
    public class ExtractionResponse
    {
        [JsonProperty("field_results")]
        public IEnumerable<ExtractionResult> ExtractionResults { get; internal set; }

        [JsonProperty("document")]
        public ExtractionDocumentMetadata DocumentMetadata { get; internal set; }
    }

    public class ExtractionResult
    {
        [JsonProperty("field_name")]
        public string FieldName { get; internal set; }

        [JsonProperty("rejected")]
        public bool Rejected { get; internal set; }

        [JsonProperty("reject_reason")]
        public string RejectReason { get; internal set; }

        [JsonProperty("result")]
        public ExtractionResult FieldResult { get; internal set; }

        [JsonProperty("alternatives")]
        public IEnumerable<ExtractionResult> AlternativeMatches { get; internal set; }

        [JsonProperty("text")]
        public string Text { get; internal set; }

        [JsonProperty("value")]
        public string Value { get; internal set; }

        [JsonProperty("proximity_score")]
        public double ProximityScore { get; internal set; }

        [JsonProperty("match_score")]
        public double MatchScore { get; internal set; }

        [JsonProperty("text_score")]
        public double TextScore { get; internal set; }

        [JsonProperty("areas")]
        public IEnumerable<ExtractionResultArea> ResultAreas { get; internal set; }
    }

    public class ExtractionResultArea
    {
        [JsonProperty("top")]
        public double Top { get; internal set; }

        [JsonProperty("left")]
        public double Left { get; internal set; }

        [JsonProperty("bottom")]
        public double Bottom { get; internal set; }

        [JsonProperty("right")]
        public double Right { get; internal set; }

        [JsonProperty("page_number")]
        public int PageNumber { get; internal set; }
    }

    public class ExtractionDocumentMetadata
    {
        [JsonProperty("page_count")]
        public int PageCount { get; internal set; }

        [JsonProperty("pages")]
        public IEnumerable<ExtractionPage> Pages { get; internal set; }
    }

    public class ExtractionPage
    {
        [JsonProperty("page_number")]
        public int PageNumber { get; internal set; }

        [JsonProperty("width")]
        public double Width { get; internal set; }

        [JsonProperty("height")]
        public double Height { get; internal set; }
    }
}
