namespace Waives.Http
{
#pragma warning disable CA1034 // Nested types should not be visible
    /// <summary>
    /// A convenience class offering quick access to a number of MIME type strings
    /// supported by the Waives platform.
    /// </summary>
    public static class ContentTypes
    {
        /// <summary>
        /// Gets the MIME type string for PDF files
        /// </summary>
        public static string Pdf { get; } = "application/pdf";

        /// <summary>
        /// Gets the MIME type string for plain text files
        /// </summary>
        public static string Text { get; } = "text/plain";

        /// <summary>
        /// Gets the MIME type string for HTML files
        /// </summary>
        public static string Html { get; } = "text/html";

        /// <summary>
        /// MIME types specific to images
        /// </summary>
        public static class Image
        {
            /// <summary>
            /// Gets the MIME type string for Bitmap images
            /// </summary>
            public static string Bitmap { get; } = "image/bmp";

            /// <summary>
            /// Gets the MIME type string for JPEG images
            /// </summary>
            public static string Jpeg { get; } = "image/jpeg";

            /// <summary>
            /// Gets the MIME type string for TIFF images
            /// </summary>
            public static string Tiff { get; } = "image/tiff";
        }

        /// <summary>
        /// MIME types specific to Microsoft Office files
        /// </summary>
        public static class MicrosoftOfficeOpenXml
        {
            /// <summary>
            /// Gets the MIME type string for Word files
            /// </summary>
            public static string Word { get; } =
                "application/vnd.openxmlformats-officedocument.wordprocessingml.document";

            /// <summary>
            /// Gets the MIME type string for Excel files
            /// </summary>
            public static string Spreadsheet { get; } =
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

            /// <summary>
            /// Gets the MIME type string for PowerPoint files
            /// </summary>
            public static string Presentation { get; } =
                "application/vnd.openxmlformats-officedocument.presentationml.presentation";

            /// <summary>
            /// Gets the MIME type string for legacy (pre-Office 2007) Word files
            /// </summary>
            public static string WordLegacy { get; } = "application/msword";

            /// <summary>
            /// Gets the MIME type string for legacy (pre-Office 2007) Excel files
            /// </summary>
            public static string SpreadsheetLegacy { get; } = "application/vnd.ms-excel";

            /// <summary>
            /// Gets the MIME type string for legacy (pre-Office 2007) PowerPoint files
            /// </summary>
            public static string PresentationLegacy { get; } = "application/vnd.ms-powerpoint";
        }

        /// <summary>
        /// MIME types specific to email files
        /// </summary>
        public static class Email
        {
            /// <summary>
            /// Gets the MIME type string for MIME-format (.eml) emails
            /// </summary>
            // ReSharper disable once InconsistentNaming
            public static string MIME { get; } = "message/rfc822";

            /// <summary>
            /// Gets the MIME type string for Outlook-format (.msg) emails
            /// </summary>
            // ReSharper disable once InconsistentNaming
            public static string MSG { get; } = "application/vnd.ms-outlook";
        }

        /// <summary>
        /// Gets the MIME type string for Waives document recognition (OCR) results
        /// </summary>
        public static string WaivesReadResults { get; } = "application/vnd.waives.resultformats.read+zip";

        /// <summary>
        /// Gets the MIME type string for indeterminate binary files
        /// </summary>
        public static string OctetStream { get; } = "application/octet-stream";

        /// <summary>
        /// Gets the MIME type string for ZIP archive files
        /// </summary>
        public static string Zip { get; } = "application/zip";
    }
#pragma warning restore CA1034 // Nested types should not be visible
}
