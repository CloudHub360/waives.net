using System.Collections.Generic;
using Newtonsoft.Json;

namespace Waives.Http.Responses
{
    internal class ExtractionResponse
    {
        [JsonProperty("field_results")]
        public IEnumerable<ExtractionResult> ExtractionResults { get; set; }

        [JsonProperty("document")]
        public ExtractionDocument DocumentMetadata { get; set; }
    }

    public class ExtractionResult
    {
        [JsonProperty("field_name")]
        public string FieldName { get; set; }

        [JsonProperty("rejected")]
        public bool Rejected { get; set; }

        [JsonProperty("reject_reason")]
        public string RejectReason { get; set; }

        [JsonProperty("result")]
        public ExtractionResult Result { get; set; }

        [JsonProperty("alternatives")]
        public IEnumerable<ExtractionResult> AlternativeMatches { get; set; }

        [JsonProperty("text")]
        public string Text { get; set; }

        [JsonProperty("value")]
        public string Value { get; set; }

        [JsonProperty("proximity_score")]
        public string ProximityScore { get; set; }

        [JsonProperty("match_score")]
        public string MatchScore { get; set; }

        [JsonProperty("text_score")]
        public string TextScore { get; set; }

        [JsonProperty("areas")]
        public IEnumerable<ExtractionResultArea> ResultAreas { get; set; }
    }

    public class ExtractionResultArea
    {
        [JsonProperty("top")]
        public double Top { get; set; }

        [JsonProperty("left")]
        public double Left { get; set; }

        [JsonProperty("bottom")]
        public double Bottom { get; set; }

        [JsonProperty("right")]
        public double Right { get; set; }

        [JsonProperty("page_number")]
        public int PageNumber { get; set; }
    }

    public class ExtractionDocument
    {
        [JsonProperty("page_count")]
        public int PageCount { get; set; }

        [JsonProperty("pages")]
        public IEnumerable<ExtractionPage> Pages { get; set; }
    }

    public class ExtractionPage
    {
        [JsonProperty("page_number")]
        public int PageNumber { get; set; }

        [JsonProperty("width")]
        public double Width { get; set; }

        [JsonProperty("height")]
        public double Height { get; set; }
    }
}
