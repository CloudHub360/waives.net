using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using Serilog;
using Serilog.Context;
using Serilog.Events;
using Xunit;

namespace Waives.Http.Tests.Logging
{
    internal static class Logger
    {
        private static readonly Subject<LogEvent> LogEventSubject = new Subject<LogEvent>();
        private const string CaptureCorrelationIdKey = "CaptureCorrelationId";

        static Logger()
        {
            // Capture all log output to an Observable
            Log.Logger = new LoggerConfiguration()
                .WriteTo.Observers(observable => observable.Do(logEvent =>
                {
                    LogEventSubject.OnNext(logEvent);
                }).Subscribe())
                .Enrich.FromLogContext()
                .CreateLogger();
        }

        public static IDisposable CaptureTo(ICollection<LogEvent> events)
        {
            // Open a unique diagnostic context for each test
            var captureId = Guid.NewGuid();
            var pushProperty = LogContext.PushProperty(CaptureCorrelationIdKey, captureId);


            bool MatchesCurrentTest(LogEvent logEvent) =>
                logEvent.Properties.ContainsKey(CaptureCorrelationIdKey) &&
                logEvent.Properties[CaptureCorrelationIdKey].ToString() == captureId.ToString();
            // Filter the logs for this test's logs, and add them to the provided collection
            var subscription = LogEventSubject.Where(MatchesCurrentTest).Subscribe(events.Add);

            // Tidy up the subscriptions
            return Disposable.Create(() =>
            {
                subscription.Dispose();
                pushProperty.Dispose();
            });
        }

        #region Assertions
        // ReSharper disable PossibleMultipleEnumeration

        public static IEnumerable<LogEvent> HasMessage(this IEnumerable<LogEvent> logEvents, string messageTemplate)
        {
            var matchingEvents = logEvents.Where(e => e.MessageTemplate.Text == messageTemplate);
            Assert.NotEmpty(matchingEvents);

            return matchingEvents;
        }

        public static IEnumerable<LogEvent> AtLevel(this IEnumerable<LogEvent> logEvents, LogEventLevel eventLevel)
        {
            var matchingEvents = logEvents.Where(e => e.Level == eventLevel);
            Assert.NotEmpty(matchingEvents);

            return matchingEvents;
        }

        public static IEnumerable<LogEvent> WithPropertyValue(this IEnumerable<LogEvent> logEvents, string propertyName, object expectedValue)
        {
            var properties = new Dictionary<string, LogEventPropertyValue>(logEvents.SelectMany(e => e.Properties));
            var propertyExists = properties.TryGetValue(propertyName, out var propertyValue);

            Assert.True(propertyExists);
            Assert.Equal(expectedValue.ToString(), propertyValue.ToString());

            return logEvents;
        }

        public static IEnumerable<LogEvent> WithProperty(this IEnumerable<LogEvent> logEvents, string propertyName)
        {
            var properties = new Dictionary<string, LogEventPropertyValue>(logEvents.SelectMany(e => e.Properties));
            var propertyExists = properties.TryGetValue(propertyName, out _);

            Assert.True(propertyExists);

            return logEvents;
        }

        // ReSharper restore PossibleMultipleEnumeration
        #endregion
    }
}
