using System;

namespace Waives.Pipelines
{
    /// <summary>
    /// Provides a set of documents to be processed. Use <see cref="Subscribe"/>
    /// to receive each document in turn.
    /// </summary>
    public abstract class DocumentSource : IObservable<Document>
    {
        private readonly IObservable<Document> _observable;

        protected DocumentSource(IObservable<Document> buffer)
        {
            _observable = buffer;
        }

        public IDisposable Subscribe(IObserver<Document> observer)
        {
            return _observable.Subscribe(observer);
        }
    }
}