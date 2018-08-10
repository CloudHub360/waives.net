namespace Waives.Http
{
    public static class ContentTypes
    {
        public static readonly string Pdf = "application/pdf";
        public static readonly string Text = "text/plain";
        public static readonly string Html = "text/html";

        public static class Image
        {
            public static readonly string Bitmap = "image/bmp";
            public static readonly string Jpeg = "image/jpeg";
            public static readonly string Tiff = "image/tiff";
        }

        public static class MicrosoftOfficeOpenXml
        {
            public static readonly string Word =
                "application/vnd.openxmlformats-officedocument.wordprocessingml.document";

            public static readonly string Spreadsheet =
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

            public static readonly string Presentation =
                "application/vnd.openxmlformats-officedocument..presentationml.presentation";
        }

        public static class MicrosoftOffice
        {
            public static readonly string Word = "application/msword";
            public static readonly string Spreadsheet = "application/vnd.ms-excel";
            public static readonly string Presentation = "application/vnd.ms-powerpoint";
        }

        public static class Email
        {
            // ReSharper disable once InconsistentNaming
            public static readonly string MIME = "message/rfc822";
            public static readonly string MSG = "application/vnd.ms-outlook";
        }

        public static readonly string WaivesReadResults = "application/vnd.waives.resultformats.read+zip";
        public static readonly string OctetStream = "application/octet-stream";
        public static readonly string Zip = "application/zip";
    }
}