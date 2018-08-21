using System;
using System.Threading;
using System.Threading.Tasks;

namespace Waives.Reactive
{
    /// <summary>
    /// Wraps an event stream (logical or literal) producing documents
    /// and provides its own <see cref="NewDocument"/> event emitting
    /// <see cref="Document"/>s.
    /// </summary>
    public abstract class DocumentEmitter
    {
        /// <summary>
        /// Raised when a new document has been detected in the underlying
        /// event stream.
        /// </summary>
        public event EventHandler<NewDocumentEventArgs> NewDocument;

        /// <summary>
        /// Signals the emitter will not be sending any further events.
        /// </summary>
        internal event EventHandler EmitterCompleted;

        /// <summary>
        /// Emits a <see cref="NewDocument"/> event, provided
        /// <see cref="EnableRaisingEvents"/> is <c>true</c>. Use this from
        /// derived classes rather than invoking <see cref="NewDocument"/>
        /// directly.
        /// </summary>
        /// <param name="newDocument">The document to emit to listeners.</param>
        protected void EmitDocument(Document newDocument)
        {
            if (EnableRaisingEvents)
            {
                NewDocument?.Invoke(this, new NewDocumentEventArgs(newDocument));
            }
        }

        /// <summary>
        /// Emits a <see cref="Completed"/> event, indicating that no further
        /// events will be coming from this <see cref="DocumentEmitter"/>.
        /// </summary>
        protected void Completed()
        {
            EmitterCompleted?.Invoke(this, new EventArgs());
        }

        /// <summary>
        /// Enable or disable the raising of <see cref="NewDocument"/> events.
        /// Defaults to <c>true</c>. This should be used to temporarily disable
        /// event emission without also signalling completion.
        /// </summary>
        public virtual bool EnableRaisingEvents { get; set; } = true;

        /// <summary>
        /// The process for emitting events.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This method will be run in a task on the task pool. Returning from
        /// this method indicates nothing to the <see cref="Pipeline"/> about the
        /// completion of the source, unless <see cref="Completed"/> is called
        /// first. As such, it can be used to implement long-running tasks (such
        /// as polling), or set-up tasks (for configuring an underlying event source).
        /// In the case of a long-running task, like a polling activity, you should
        /// check whether the <paramref name="token"/> has been cancelled, call
        /// <see cref="Completed"/> and return from this method.
        /// </para>
        /// <para>
        /// When a document is ready to be sent to the pipeline, call
        /// <see cref="EmitDocument"/> with your <see cref="Document"/> instance to
        /// ensure it is emitted correctly.
        /// </para>
        /// </remarks>
        /// <param name="token"></param>
        /// <returns></returns>
        protected abstract Task EmitDocuments(CancellationToken token);

        /// <summary>
        /// Start emitting objects from the underlying event stream, with no
        /// cancellation possible.
        /// </summary>
        public void Start()
        {
            Start(CancellationToken.None);
        }

        /// <summary>
        /// Start emitting objects from the underlying event stream, continuing
        /// until the <paramref name="token"/> is cancelled.
        /// </summary>
        /// <param name="token">A cancellation token that can be used by other
        /// objects or threads to receive notice of cancellation.</param>
        public void Start(CancellationToken token)
        {
            EnableRaisingEvents = true;
            Task.Run(() => { EmitDocuments(token).ConfigureAwait(false); }, token);
        }
    }

    /// <inheritdoc />
    /// <summary>
    /// Event arguments for the <see cref="DocumentEmitter.NewDocument"/> event.
    /// </summary>
    public class NewDocumentEventArgs : EventArgs
    {
        public NewDocumentEventArgs(Document document)
        {
            Document = document;
        }

        /// <summary>
        /// Gets the document that was emitted.
        /// </summary>
        public Document Document { get; }
    }
}