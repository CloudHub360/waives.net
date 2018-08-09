using System;

namespace Waives
{
    public static class WaivesObserver
    {
        public static IDisposable SubscribeTo<T>(this IObserver<T> observer, IObservable<T> observable)
        {
            return observable.Subscribe(observer);
        }
    }
}