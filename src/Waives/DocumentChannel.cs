using System;

namespace Waives
{
    public abstract class DocumentChannel : IObservable<IDocumentSource>
    {
        private readonly IObservable<IDocumentSource> _observable;

        protected DocumentChannel(IObservable<IDocumentSource> buffer)
        {
            _observable = buffer;
        }

        public IDisposable Subscribe(IObserver<IDocumentSource> observer)
        {
            return _observable.Subscribe(observer);
        }
    }
}