using System;
using System.Threading.Tasks;

namespace Waives.Pipelines
{
    public interface IRateLimiter
    {
        void MakeDocumentSlotAvailable();
        IObservable<TSource> RateLimited<TSource>(IObservable<TSource> source);
    }
}