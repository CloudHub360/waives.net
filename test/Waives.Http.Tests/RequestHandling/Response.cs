using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using Waives.Http.RequestHandling;

namespace Waives.Http.Tests.RequestHandling
{
    internal static class Response
    {
        public static HttpResponseMessage From(
            HttpStatusCode statusCode,
            HttpRequestMessageTemplate requestTemplate,
            HttpContent content = null)
        {
            return new HttpResponseMessage(statusCode)
            {
                Content = content,
                RequestMessage = requestTemplate?.CreateRequest()
            };
        }

        public static HttpResponseMessage Success(HttpRequestMessageTemplate requestTemplate)
        {
            return From(HttpStatusCode.OK, requestTemplate);
        }

        public static HttpResponseMessage SuccessFrom(HttpStatusCode statusCode, HttpRequestMessageTemplate request)
        {
            return From(statusCode, request, new StringContent(GetAllDocumentsResponse)
            {
                Headers = { ContentType = new MediaTypeHeaderValue("application/json" )}
            });
        }

        public static HttpResponseMessage ErrorFrom(HttpStatusCode statusCode, HttpRequestMessageTemplate requestTemplate)
        {
            return From(statusCode, requestTemplate, new StringContent(ErrorResponse)
            {
                Headers = { ContentType = new MediaTypeHeaderValue("application/json") }
            });
        }

        public const string ErrorMessage = "The error message";

        public static HttpResponseMessage ErrorWithMessage(HttpRequestMessageTemplate requestTemplate)
        {
            return From(HttpStatusCode.NotFound, requestTemplate, new StringContent(ErrorResponse)
            {
                Headers = { ContentType = new MediaTypeHeaderValue("application/json") }
            });
        }

        public static HttpResponseMessage CreateDocument(HttpRequestMessageTemplate requestTemplate)
        {
            return From(HttpStatusCode.OK, requestTemplate, new StringContent(CreateDocumentResponse)
            {
                Headers = { ContentType = new MediaTypeHeaderValue("application/json") }
            });
        }

        public static HttpResponseMessage GetDocument(HttpRequestMessageTemplate requestTemplate)
        {
            return From(HttpStatusCode.OK, requestTemplate, new StringContent(GetDocumentResponse)
            {
                Headers = { ContentType = new MediaTypeHeaderValue("application/json") }
            });
        }

        public static HttpResponseMessage GetAllDocuments(HttpRequestMessageTemplate requestTemplate)
        {
            return From(HttpStatusCode.OK, requestTemplate, new StringContent(GetAllDocumentsResponse)
            {
                Headers = {ContentType = new MediaTypeHeaderValue("application/json")}
            });
        }

        public static HttpResponseMessage GetToken(HttpRequestMessageTemplate requestTemplate)
        {
            return From(HttpStatusCode.OK, requestTemplate, new StringContent(GetTokenResponse)
            {
                Headers = {ContentType = new MediaTypeHeaderValue("application/json")}
            });
        }

        public static HttpResponseMessage GetReadResults(HttpRequestMessageTemplate requestTemplate, string content)
        {
            return From(HttpStatusCode.OK, requestTemplate, new StringContent(content)
            {
                Headers = {ContentType = new MediaTypeHeaderValue("text/plain")}
            });
        }

        public static HttpResponseMessage Classify(HttpRequestMessageTemplate requestTemplate)
        {
            return From(HttpStatusCode.OK, requestTemplate, new StringContent(ClassifyResponse)
            {
                Headers = {ContentType = new MediaTypeHeaderValue("application/json")}
            });
        }

        public static HttpResponseMessage Extract(HttpRequestMessageTemplate requestTemplate)
        {
            return From(HttpStatusCode.OK, requestTemplate, new StringContent(ExtractResponse)
            {
                Headers = {ContentType = new MediaTypeHeaderValue("application/json")}
            });
        }

        private const string ClassifyResponse = @"{
	        ""document_id"": ""expectedDocumentId"",
            ""classification_results"": {
                ""document_type"": ""expectedDocumentType"",
                ""relative_confidence"": 2.85512137,
                ""is_confident"": true,
                ""document_type_scores"": [
                {
                    ""document_type"": ""Assignment of Deed of Trust"",
                    ""score"": 61.4187

                },
                {
                    ""document_type"": ""Notice of Default"",
                    ""score"": 32.94312
                },
                {
                    ""document_type"": ""Correspondence"",
                    ""score"": 28.2860489
                },
                {
                    ""document_type"": ""Deed of Trust"",
                    ""score"": 28.0011711
                },
                {
                    ""document_type"": ""Notice of Lien"",
                    ""score"": 27.9561481
                }
                ]
            }
        }";

        private const string GetTokenResponse = @"{
	        ""access_token"": ""token"",
	        ""token_type"": ""Bearer"",
	        ""expires_in"": 86400}";

        private const string CreateDocumentResponse = @"{
            ""id"": ""expectedDocumentId"",
            ""_links"": {
                ""document:read"": {
                    ""href"": ""/documents/LAHV1hoYikqukLpuhiFpAw/reads""
                },
                ""document:classify"": {
                    ""href"": ""/documents/LAHV1hoYikqukLpuhiFpAw/classify/{classifier_name}"",
                    ""templated"": true
                },
                ""self"": {
                    ""href"": ""/documents/LAHV1hoYikqukLpuhiFpAw""
                }
            },
            ""_embedded"": {
                ""files"": [
                {
                    ""id"": ""HEE7UnX680y7yecR-yXsPA"",
                    ""file_type"": ""Image:TIFF"",
                    ""size"": 41203,
                    ""sha256"": ""eeea8dbbf4f0da70bf3dcc25ee0ecf5c6f8a4eae2817fe782a59589cbd4cb9fd""
                }]
            }
        }";

        private const string GetDocumentResponse = @"{
    ""id"": ""expectedDocumentId1"",
    ""_links"": {
        ""document:read"": {
            ""href"": ""/documents/expectedDocumentId1/reads""
        },
        ""document:classify"": {
            ""href"": ""/documents/expectedDocumentId1/classify/{classifier_name}"",
            ""templated"": true
        },
        ""self"": {
            ""href"": ""/documents/expectedDocumentId1""
        }
    },
    ""_embedded"": {
        ""files"": [
        {
            ""id"": ""HEE7UnX680y7yecR-yXsPA"",
            ""file_type"": ""Image:TIFF"",
            ""size"": 41203,
            ""sha256"": ""eeea8dbbf4f0da70bf3dcc25ee0ecf5c6f8a4eae2817fe782a59589cbd4cb9fd""
        }]
    }
}";

        private const string GetAllDocumentsResponse = @"{
	        ""documents"": [
              {
                ""id"": ""expectedDocumentId1"",
                ""_links"": {
                    ""document:read"": {
                        ""href"": ""/documents/expectedDocumentId1/reads""
                    },
                    ""document:classify"": {
                        ""href"": ""/documents/expectedDocumentId1/classify/{classifier_name}"",
                        ""templated"": true
                    },
                    ""self"": {
                        ""href"": ""/documents/expectedDocumentId1""
                    }
                },
                ""_embedded"": {
                    ""files"": [
                    {
                        ""id"": ""HEE7UnX680y7yecR-yXsPA"",
                        ""file_type"": ""Image:TIFF"",
                        ""size"": 41203,
                        ""sha256"": ""eeea8dbbf4f0da70bf3dcc25ee0ecf5c6f8a4eae2817fe782a59589cbd4cb9fd""

                    }
                    ]
                 }
               },
               {
                 ""id"": ""expectedDocumentId2"",
                 ""_links"": {
                    ""document:read"": {
                        ""href"": ""/documents/expectedDocumentId2/reads""
                    },
                    ""document:classify"": {
                        ""href"": ""/documents/expectedDocumentId2/classify/{classifier_name}"",
                        ""templated"": true
                    },
                    ""self"": {
                        ""href"": ""/documents/expectedDocumentId2""
                    }
                 },
                 ""_embedded"": {
                    ""files"": [
                    {
                        ""id"": ""YY-WZbHuukCOXMalCZ3rBA"",
                        ""file_type"": ""Image:TIFF"",
                        ""size"": 41203,
                        ""sha256"": ""eeea8dbbf4f0da70bf3dcc25ee0ecf5c6f8a4eae2817fe782a59589cbd4cb9fd""
                    }
                    ]
                }
            }
            ]
        }";

        private const string ErrorResponse = @"{
	        ""message"": """ + ErrorMessage + "\"}";

        private const string ExtractResponse = @"{
""field_results"": [
{
    ""field_name"": ""Amount"",
    ""rejected"": false,
    ""reject_reason"": ""None"",
    ""result"": {
        ""text"": ""$5.50"",
        ""value"": null,
        ""rejected"": false,
        ""reject_reason"": ""None"",
        ""proximity_score"": 100.0,
        ""match_score"": 100.0,
        ""text_score"": 100.0,
        ""areas"": [
        {
            ""top"": 558.7115,
            ""left"": 276.48,
            ""bottom"": 571.1989,
            ""right"": 298.58,
            ""page_number"": 1
        }]
    },
    ""alternatives"": [
    {
            ""text"": ""$10.50"",
            ""value"": null,
            ""rejected"": false,
            ""reject_reason"": ""None"",
            ""proximity_score"": 100.0,
            ""match_score"": 100.0,
            ""text_score"": 100.0,
            ""areas"": [
            {
                ""top"": 123.4567,
                ""left"": 276.48,
                ""bottom"": 571.1989,
                ""right"": 298.58,
                ""page_number"": 1
            }]
        }
    ],
    ""tabular_results"": null
}],
""document"": {
    ""page_count"": 1,
    ""pages"": [
        {
            ""page_number"": 1,
            ""width"": 611.0,
            ""height"": 1008.0
        }]
    }
}";
    }
}