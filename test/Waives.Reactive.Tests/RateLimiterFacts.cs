using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using Microsoft.Reactive.Testing;
using Waives.Pipelines;
using Xunit;

namespace Waives.Reactive.Tests
{
    public class RateLimiterFacts
    {
        [Fact]
        public void Only_output_maximum_allowed_number_of_documents_when_there_are_more_in_input()
        {
            var scheduler = new TestScheduler();
            var sut = new RateLimiter(scheduler);

            // Schedule one more document than the maximum concurrency, all to be scheduled immediately
            var source = scheduler
                .CreateColdObservable(AnArrayOfDocumentNotifications(RateLimiter.MaximumConcurrentDocuments + 1));

            var rateLimitedDocuments = sut.RateLimited(source);

            var testObserver = scheduler.Start(() => rateLimitedDocuments);

            Assert.Equal(RateLimiter.MaximumConcurrentDocuments, testObserver.Messages.Count);
        }

        [Fact]
        public void Outputs_the_documents_as_slots_become_free()
        {
            var scheduler = new TestScheduler();
            var sut = new RateLimiter(scheduler);

            var source = scheduler
                .CreateColdObservable(AnArrayOfDocumentNotifications(RateLimiter.MaximumConcurrentDocuments + 1));

            scheduler.ScheduleAbsolute(sut, TimeSpan.FromSeconds(3).Ticks, (_, rateLimiter) =>
            {
                rateLimiter.MakeDocumentSlotAvailable();
                return Disposable.Empty;
            });

            var testObserver = scheduler.Start(() => sut.RateLimited(source),
                created: 0, subscribed: 0, disposed: TimeSpan.FromSeconds(5).Ticks);

            Assert.Equal(11, testObserver.Messages.Count);
        }

        /// <summary>
        /// Create an array of Document notifications, where each document is scheduled at 1 tick of the
        /// virtual scheduler
        /// </summary>
        /// <param name="numberOfDocuments">The number of document notifications</param>
        /// <returns></returns>
        private static Recorded<Notification<Document>>[] AnArrayOfDocumentNotifications(long numberOfDocuments)
        {
            var notifications = new List<Recorded<Notification<Document>>>();
            notifications.AddRange(Enumerable.Repeat(
                new Recorded<Notification<Document>>(0,
                    Notification.CreateOnNext<Document>(new TestDocument(Generate.Bytes()))), (int) numberOfDocuments));

            return notifications.ToArray();
        }
    }
}