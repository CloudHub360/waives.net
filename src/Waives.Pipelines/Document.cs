using System.IO;
using System.Threading.Tasks;

namespace Waives.Pipelines
{
    /// <summary>
    /// Represents a document which will be processed.
    /// </summary>
    public abstract class Document
    {
        /// <summary>
        /// Initialises the <see cref="Document"/> with the given <paramref name="sourceId"/>.
        /// </summary>
        /// <param name="sourceId">A value uniquely identifying this doucment.</param>
        /// <seealso cref="SourceId"/>
        protected Document(string sourceId)
        {
            if (string.IsNullOrWhiteSpace(sourceId))
            {
                throw new System.ArgumentException("message", nameof(sourceId));
            }

            SourceId = sourceId;
        }

        /// <summary>
        /// Gets an identifier for the source of the <see cref="Document"/>.
        /// </summary>
        /// <remarks>
        /// How the SourceId is defined is up to applications to decide. It is not used
        /// internally by the Waives SDK, and is provided for the convenience of consuming
        /// applications. Example values for this property might be path to the document's
        /// file on disk, a database row ID, a URI to fetch the document from, etc.
        /// </remarks>
        public string SourceId { get; }

        /// <summary>
        /// Opens a <see cref="Stream"/> onto the underlying Document resource (e.g. file on
        /// disk, blob, etc.).
        /// </summary>
        /// <remarks>
        /// <para>
        /// This method is called by the Waives SDK to create a document resource in Waives.
        /// The SDK expects to be able to use the <see cref="Stream"/> as returned by this
        /// method, and will dispose of it when the document has been created. This is to
        /// ensure the <see cref="Stream"/> is kept open for the shortest possible time.
        /// As such, this method should be implemented to return a new <see cref="Stream"/>
        /// on each call.
        /// </para>
        /// <para>
        /// If you need the document contents for your application's purposes too and the
        /// stream cannot be easily re-established, copy it to a <see cref="MemoryStream"/>
        /// and pass that to your implementation of <see cref="Document"/>.
        /// </para>
        /// </remarks>
        /// <returns>A new <see cref="Stream"/> ready to be opened for reading.</returns>
        public abstract Task<Stream> OpenStream();

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            return Equals(obj as Document);
        }

        public bool Equals(Document other)
        {
            return Equals(this, other);
        }

        public static bool Equals(Document x, Document y)
        {
            if (ReferenceEquals(x, y))
            {
                return true;
            }

            if (x == null || y == null)
            {
                return false;
            }

            return string.Equals(x.SourceId, y.SourceId);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return SourceId.GetHashCode();
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return SourceId;
        }
    }
}