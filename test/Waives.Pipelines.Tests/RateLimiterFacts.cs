using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using Microsoft.Reactive.Testing;
using Xunit;

namespace Waives.Pipelines.Tests
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
                .CreateColdObservable(AnArrayOfDocumentNotifications(RateLimiter.DefaultMaximumConcurrentDocuments + 1));

            var rateLimitedDocuments = sut.RateLimited(source);

            var testObserver = scheduler.Start(() => rateLimitedDocuments);

            Assert.Equal(RateLimiter.DefaultMaximumConcurrentDocuments, testObserver.Messages.Count);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(3)]
        [InlineData(10)]
        public void Outputs_the_documents_as_slots_become_free(int maxConcurrency)
        {
            var scheduler = new TestScheduler();
            var sut = new RateLimiter(scheduler, maxConcurrency);

            var expectedDocsCount = maxConcurrency + 1;
            var source = scheduler
                .CreateColdObservable(AnArrayOfDocumentNotifications(expectedDocsCount));

            scheduler.ScheduleAbsolute(sut, TimeSpan.FromSeconds(3).Ticks, (_, rateLimiter) =>
            {
                rateLimiter.MakeDocumentSlotAvailable();
                return Disposable.Empty;
            });

            var testObserver = scheduler.Start(() => sut.RateLimited(source),
                created: 0, subscribed: 0, disposed: TimeSpan.FromSeconds(5).Ticks);

            Assert.Equal(expectedDocsCount, testObserver.Messages.Count);
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