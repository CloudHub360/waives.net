using System;
using System.Collections.Concurrent;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading;

namespace Waives.Reactive
{
    //Inspired by http://granitestatehacker.kataire.com/2016/01/rate-limiting-observables-with-reactive.html
    public class RateLimiter : IRateLimiter
    {
        private readonly IScheduler _scheduler;
        private long _availableDocumentSlots;
        public const byte MaximumConcurrentDocuments = 10;

        public RateLimiter(IScheduler scheduler = null)
        {
            _scheduler = scheduler ?? Scheduler.Default;
            _availableDocumentSlots = MaximumConcurrentDocuments;
        }

        public void MakeDocumentSlotAvailable()
        {
            if (Interlocked.Read(ref _availableDocumentSlots) < MaximumConcurrentDocuments)
            {
                Interlocked.Increment(ref _availableDocumentSlots);
            }
        }

        /// <summary>
        /// Transforms an observable sequence of documents into one where the documents are only
        /// emitted to the observer when slots are available within the maximum concurrency limit.
        /// </summary>
        /// <param name="sourceDocuments">An observable sequence of documents to be rate limited</param>
        /// <returns>An observable sequence of documents which is rate limited to maximum concurrency</returns>
        public IObservable<TSource> RateLimited<TSource>(IObservable<TSource> source)
        {
            var timeSpan = TimeSpan.FromMilliseconds(500);
            var sourceCompleted = false;

            void EmitIfSlotAvailable(ConcurrentQueue<TSource> buffer, IObserver<TSource> observer)
            {
                while (Interlocked.Read(ref _availableDocumentSlots) > 0)
                {
                    if (!buffer.TryDequeue(out var item))
                    {
                        if (sourceCompleted)
                        {
                            observer.OnCompleted();
                        }

                        break;
                    }

                    observer.OnNext(item);
                    Interlocked.Decrement(ref _availableDocumentSlots);
                }
            }

            return Observable.Create<TSource>(
                observer =>
                {
                    var buffer = new ConcurrentQueue<TSource>();
                    var sourceSub = source
                        .Subscribe(x =>
                        {
                            buffer.Enqueue(x);
                            EmitIfSlotAvailable(buffer, observer);
                        }, () =>
                        {
                            sourceCompleted = true;
                        });

                    var timer = Observable.Interval(timeSpan, _scheduler)
                        .Subscribe(x =>
                        {
                            EmitIfSlotAvailable(buffer, observer);
                        }, observer.OnError, observer.OnCompleted);
                    return new CompositeDisposable(sourceSub, timer);
                });
        }
    }
}