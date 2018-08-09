using System.IO;
using System.Threading.Tasks;

namespace Waives
{
    /// <summary>
    /// Represents a document which will be processed.
    /// </summary>
    public abstract class Document
    {
        protected Document(string sourceId)
        {
            if (string.IsNullOrWhiteSpace(sourceId))
            {
                throw new System.ArgumentException("message", nameof(sourceId));
            }

            SourceId = sourceId;
        }

        /// <summary>
        /// E.g. File path, database row ID, etc.
        /// </summary>
        public string SourceId { get; }

        /// <summary>
        ///
        /// </summary>
        /// <returns></returns>
        public abstract Task<Stream> OpenStream();

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

        public override int GetHashCode()
        {
            return SourceId.GetHashCode();
        }

        public override string ToString()
        {
            return SourceId;
        }
    }
}