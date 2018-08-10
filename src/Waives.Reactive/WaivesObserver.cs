using System;

namespace Waives.Reactive
{
    public static class WaivesObserver
    {
        public static IDisposable SubscribeTo<T>(this IObserver<T> observer, IObservable<T> observable)
        {
            return observable.Subscribe(observer);
        }
    }
}