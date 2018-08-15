using System;

namespace Waives.Reactive
{
    public interface IRateLimiter
    {
        void MakeDocumentSlotAvailable();
        IObservable<TSource> RateLimited<TSource>(IObservable<TSource> source);
    }
}