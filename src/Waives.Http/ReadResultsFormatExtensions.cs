namespace Waives.Http
{
    public static class ReadResultsFormatExtensions
    {
        public static string ToMimeType(this ReadResultsFormat format)
        {
            switch (format)
            {
                case ReadResultsFormat.Text:
                    return "text/plain";
                case ReadResultsFormat.Pdf:
                    return "application/pdf";
                default:
                    return "application/vnd.waives.resultformats.read+zip";
            }
        }
    }
}