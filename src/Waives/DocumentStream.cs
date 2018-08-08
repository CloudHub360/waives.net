using System;
using System.Reactive.Disposables;

namespace Waives
{
    public abstract class DocumentStream : IObservable<IDocumentSource>
    {
        private readonly IObservable<IDocumentSource> _observable;

        protected DocumentStream(IObservable<IDocumentSource> buffer)
        {
            _observable = buffer;
        }

        public IDisposable Subscribe(IObserver<IDocumentSource> observer)
        {
            _observable.Subscribe(observer);
            return Disposable.Empty;
        }
    }
}