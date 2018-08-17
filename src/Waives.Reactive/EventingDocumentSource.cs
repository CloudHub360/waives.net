using System;
using System.Reactive.Linq;
using System.Threading;

namespace Waives.Reactive
{
    /// <summary>
    /// Adapter class converting an event stream from a <see cref="DocumentEmitter"/>
    /// into a <see cref="DocumentSource"/> for use as the starting point of a
    /// <see cref="Pipeline"/>.
    /// </summary>
    public sealed class EventingDocumentSource : DocumentSource, IDisposable
    {
        private readonly IDisposable _connection;

        private EventingDocumentSource(IObservable<Document> buffer, IDisposable connection) : base(buffer)
        {
            _connection = connection;
        }

        /// <summary>
        /// Create an <see cref="EventingDocumentSource" /> from the events raised by
        /// the given <see cref="DocumentEmitter"/>. Events raised will be cached from
        /// the point at which this method is called to ensure the pipeline does not
        /// miss any documents.
        /// </summary>
        /// <param name="emitter">The source of new document events.</param>
        /// <param name="token">A token for cancelling the <paramref name="emitter"/>'s work.</param>
        /// <returns>A new <see cref="EventingDocumentSource"/> configured to capture new document
        /// events from the <paramref name="emitter"/>.</returns>
        public static EventingDocumentSource Create(DocumentEmitter emitter, CancellationToken token)
        {
            var documents = Observable
                .FromEventPattern<NewDocumentEventArgs>(emitter, nameof(emitter.NewDocument))
                .Select(e => e.EventArgs.Document)
                .Replay();

            var documentSource = new EventingDocumentSource(documents, documents.Connect());
            emitter.EmitterCompleted += (s, e) => { documentSource.Dispose(); };
            emitter.Start(token);

            return documentSource;
        }

        public void Dispose()
        {
            _connection.Dispose();
        }
    }
}