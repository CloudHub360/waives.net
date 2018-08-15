using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Threading.Tasks;
using Microsoft.Reactive.Testing;
using NSubstitute;
using Waives.Pipelines.HttpAdapters;
using Xunit;

namespace Waives.Pipelines.Tests
{
    public class ObservableProcessExtensionFacts
    {
        private readonly TestScheduler _scheduler = new TestScheduler();
        private readonly WaivesDocument _successfulDocument;
        private readonly WaivesDocument _errorDocument;
        private readonly WaivesDocument[] _documents;
        private readonly IObservable<WaivesDocument> _documentsObservable;
        private readonly Exception _errorDocumentError;
        private readonly Exception[] _errors;
        private readonly IObservable<Exception> _errorsObservable;

        public ObservableProcessExtensionFacts()
        {
            _successfulDocument = new WaivesDocument(
                new TestDocument(Generate.Bytes(), "dontThrow"),
                Substitute.For<IHttpDocument>());

            _errorDocument = new WaivesDocument(
                new TestDocument(Generate.Bytes(), "throw"),
                Substitute.For<IHttpDocument>());

            _documents = new[]
            {
                _successfulDocument,
                _errorDocument
            };

            _errorDocumentError = new NullReferenceException("testError");
            _errors = new[]
            {
                _errorDocumentError
            };

            _documentsObservable = _scheduler.CreateColdObservable(
                _documents.Select(d =>
                    new Recorded<Notification<WaivesDocument>>(0,
                        Notification.CreateOnNext(d)))
                .ToArray());


            _errorsObservable = _scheduler.CreateColdObservable(
                _errors.Select(e =>
                    new Recorded<Notification<Exception>>(0,
                        Notification.CreateOnNext(e)))
                .ToArray());
        }

        private async Task OnProcessingError(ProcessingError value)
        {
        }

        [Fact]
        public void Returns_only_documents_where_process_action_succeeds()
        {
            var documents = _documentsObservable.Process(
                ProcessAndThrowForErrorDocument,
                OnProcessingError);

            var documentsObserver = _scheduler.Start(() => documents);

            var receivedDocuments = documentsObserver
                .Messages
                .Where(m => m.Value.Kind == NotificationKind.OnNext)
                .Select(m => m.Value.Value)
                .ToArray();

            Assert.Equal(
                _documents.Count(d => ReferenceEquals(d, _successfulDocument)),
                receivedDocuments.Length);
            Assert.Same(_successfulDocument, receivedDocuments.First());
        }

        [Fact]
        public void Does_not_call_error_action_for_documents_where_process_action_succeeds()
        {
            var errorActionCalledFor = new List<ProcessingError>();

            var documents = _documentsObservable.Process(
                ProcessAndThrowForErrorDocument,
                (e) =>
                {
                    errorActionCalledFor.Add(e);
                    return Task.CompletedTask;
                });

            _scheduler.Start(() => documents);

            Assert.Equal(1, errorActionCalledFor.Count);
            Assert.False(errorActionCalledFor.Any(pe =>
                ReferenceEquals(pe.Document, _successfulDocument) ));
        }

        [Fact]
        public void Calls_error_action_for_documents_where_process_action_throws()
        {
            var errorActionCalledFor = new List<ProcessingError>();

            var documents = _documentsObservable.Process(
                ProcessAndThrowForErrorDocument,
                (e) =>
                {
                    errorActionCalledFor.Add(e);
                    return Task.CompletedTask;
                });

            _scheduler.Start(() => documents);

            Assert.Equal(1, errorActionCalledFor.Count);
            Assert.True(errorActionCalledFor.Any(pe =>
                ReferenceEquals(pe.Document, _errorDocument)));
        }

        /// <summary>
        /// Process a WaivesDocument and throw an exception if it
        /// is the _errorDocument
        /// </summary>
        /// <param name="document"></param>
        /// <returns></returns>
        private async Task<WaivesDocument> ProcessAndThrowForErrorDocument(WaivesDocument document)
        {
            if (ReferenceEquals(document, _errorDocument))
            {
                throw _errorDocumentError;
            }

            return document;
        }
    }
}