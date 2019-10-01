using System;
using System.Collections.Generic;
using System.Reactive.Linq;

namespace Waives.Pipelines
{
    /// <summary>
    /// Adapter class converting an <see cref="IEnumerable{Document}"/> into a
    /// <see cref="DocumentSource"/> for use as the starting point of a <see cref="Pipeline"/>.
    /// </summary>
    public class EnumerableDocumentSource : DocumentSource
    {
        public EnumerableDocumentSource(IEnumerable<Document> source) : base(source.ToObservable())
        {
        }
    }
}