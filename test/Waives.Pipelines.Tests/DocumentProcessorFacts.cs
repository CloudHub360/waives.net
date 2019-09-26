using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NSubstitute;
using Waives.Pipelines.HttpAdapters;
using Xunit;

namespace Waives.Pipelines.Tests
{
    public class DocumentProcessorFacts
    {
        private readonly Func<Document, Task<WaivesDocument>> _docCreator;
        private readonly Action<Exception, Document> _onDocumentException;
        private readonly TestDocument _testDocument;

        public DocumentProcessorFacts()
        {
            _docCreator = document =>
            {
                var waivesDocument = new WaivesDocument(document, Substitute.For<IHttpDocument>());
                return Task.FromResult(waivesDocument);
            };
            _onDocumentException = (exception, document) => { };
            _testDocument = new TestDocument(Generate.Bytes());
        }

        [Fact]
        public async Task Create_WaivesDocument_from_Document()
        {
            Document capturedDoc = null;
            var docActions = new List<Func<WaivesDocument, Task<WaivesDocument>>>
            {
                waivesDoc => {
                    capturedDoc = waivesDoc.Source;
                    return Task.FromResult(waivesDoc);
                }
            };
            var sut = new DocumentProcessor(
                _docCreator,
                docActions,
                _onDocumentException);

            await sut.RunAsync(_testDocument);

            Assert.Same(_testDocument, capturedDoc);
        }

        [Fact]
        public async Task Runs_all_provided_actions_on_docs()
        {
            var fakeDocActions = FakeDocAction.AListOfDocActions(3);
            var sut = new DocumentProcessor(
                _docCreator,
                fakeDocActions.Select<FakeDocAction, Func<WaivesDocument, Task<WaivesDocument>>>(f => f.Run),
                _onDocumentException);

            await sut.RunAsync(_testDocument);

            Assert.All(fakeDocActions, docAction =>
            {
                Assert.True(docAction.HasRun);
            });
        }

        [Fact]
        public async Task Fires_error_handler_on_error()
        {
            var fakeDocActions = FakeDocAction.AListOfDocActions(1);
            var errorHandlerRun = false;
            var sut = new DocumentProcessor(
                _docCreator,
                fakeDocActions.Select<FakeDocAction, Func<WaivesDocument, Task<WaivesDocument>>>(f => f.ThrowError),
                (exception, document) => { errorHandlerRun = true; });

            await sut.RunAsync(_testDocument);

            Assert.True(errorHandlerRun);
        }

        private class FakeDocAction
        {
            public bool HasRun;
            public Task<WaivesDocument> Run(WaivesDocument input)
            {
                HasRun = true;

                return Task.FromResult(input);
            }

            public Task<WaivesDocument> ThrowError(WaivesDocument input)
            {
                HasRun = true;

                throw new Exception();
            }

            public static List<FakeDocAction> AListOfDocActions(int count)
            {
                return Enumerable.Range(0, count)
                    .Select(_ => new FakeDocAction())
                    .ToList();
            }
        }
    }
}