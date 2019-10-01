using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NSubstitute;
using Waives.Pipelines.HttpAdapters;
using Xunit;

namespace Waives.Pipelines.Tests
{
    public class DocumentProcessorFacts
    {
        private readonly Func<Document, CancellationToken, Task<WaivesDocument>> _documentCreator;
        private readonly Func<WaivesDocument, Task> _documentDeleter;
        private readonly Action<Exception, Document> _onDocumentException;
        private readonly TestDocument _testDocument;

        public DocumentProcessorFacts()
        {
            _documentCreator = (document, cancellationToken) =>
            {
                var waivesDocument = new WaivesDocument(document, Substitute.For<IHttpDocument>());
                return Task.FromResult(waivesDocument);
            };
            _documentDeleter = _ => Task.CompletedTask;
            _onDocumentException = (exception, document) => { };
            _testDocument = new TestDocument(Generate.Bytes());
        }

        [Fact]
        public async Task Create_WaivesDocument_from_Document()
        {
            Document capturedDoc = null;
            var documentActions = new List<Func<WaivesDocument, CancellationToken, Task<WaivesDocument>>>
            {
                (waivesDoc, cancellationToken) => {
                    capturedDoc = waivesDoc.Source;
                    return Task.FromResult(waivesDoc);
                }
            };
            var sut = new DocumentProcessor(
                _documentCreator,
                documentActions,
                _documentDeleter,
                _onDocumentException);

            await sut.RunAsync(_testDocument);

            Assert.Same(_testDocument, capturedDoc);
        }

        [Fact]
        public async Task Runs_all_provided_actions_on_docs()
        {
            var fakeDocumentActions = FakeDocumentAction.AListOfDocumentActions(3);
            var sut = new DocumentProcessor(
                _documentCreator,
                fakeDocumentActions.Select<FakeDocumentAction, Func<WaivesDocument, CancellationToken, Task<WaivesDocument>>>(f => f.Run),
                _documentDeleter,
                _onDocumentException);

            await sut.RunAsync(_testDocument);

            Assert.All(fakeDocumentActions, documentAction =>
            {
                Assert.True(documentAction.HasRun);
            });
        }

        [Fact]
        public async Task Fires_error_handler_on_error()
        {
            var fakeDocumentActions = FakeDocumentAction.AListOfDocumentActions(1);
            var errorHandlerRun = false;
            var sut = new DocumentProcessor(
                _documentCreator,
                fakeDocumentActions.Select<FakeDocumentAction, Func<WaivesDocument, CancellationToken, Task<WaivesDocument>>>(f => f.ThrowError),
                _documentDeleter,
                (exception, document) => { errorHandlerRun = true; });

            await sut.RunAsync(_testDocument);

            Assert.True(errorHandlerRun);
        }

        [Fact]
        public async Task Deletes_document_after_error()
        {
            var documentDeleted = false;

            Task DocumentDeleter(WaivesDocument document)
            {
                documentDeleted = true;
                return Task.CompletedTask;
            }

            var fakeDocumentActions = FakeDocumentAction.AListOfDocumentActions(1);

            var sut = new DocumentProcessor(
                _documentCreator,
                fakeDocumentActions.Select<FakeDocumentAction, Func<WaivesDocument, CancellationToken, Task<WaivesDocument>>>(f => f.ThrowError),
                DocumentDeleter,
                _onDocumentException);
            await sut.RunAsync(_testDocument);

            Assert.True(documentDeleted);
        }

        [Fact]
        public async Task Deletes_document_after_error_in_error_handler()
        {
            var documentDeleted = false;

            Task DocumentDeleter(WaivesDocument document)
            {
                documentDeleted = true;
                return Task.CompletedTask;
            }

            var fakeDocumentActions = FakeDocumentAction.AListOfDocumentActions(1);

            void OnDocumentException(Exception exception, Document document) => throw new Exception();

            var sut = new DocumentProcessor(
                _documentCreator,
                fakeDocumentActions.Select<FakeDocumentAction, Func<WaivesDocument, CancellationToken, Task<WaivesDocument>>>(f => f.ThrowError),
                DocumentDeleter,
                OnDocumentException);

            await Assert.ThrowsAsync<Exception>(() => sut.RunAsync(_testDocument));
            Assert.True(documentDeleted);
        }

        private class FakeDocumentAction
        {
            public bool HasRun;
            public Task<WaivesDocument> Run(WaivesDocument input, CancellationToken cancellationToken)
            {
                HasRun = true;

                return Task.FromResult(input);
            }

            public Task<WaivesDocument> ThrowError(WaivesDocument input, CancellationToken cancellationToken)
            {
                HasRun = true;

                throw new Exception();
            }

            public static List<FakeDocumentAction> AListOfDocumentActions(int count)
            {
                return Enumerable.Range(0, count)
                    .Select(_ => new FakeDocumentAction())
                    .ToList();
            }
        }
    }
}