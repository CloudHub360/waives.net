using System;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading;
using NSubstitute;
using Waives.Http;
using Xunit;

namespace Waives.Reactive.Tests
{
    public class PipelineFacts
    {
        private readonly Pipeline _sut = new Pipeline(Substitute.For<IWaivesClient>());

        [Fact]
        public void OnPipelineCompleted_is_run_at_the_end_of_a_successful_pipeline()
        {
            var pipelineCompleted = false;
            _sut.OnPipelineCompleted(() => pipelineCompleted = true);

            _sut.Start();

            Assert.True(pipelineCompleted);
        }

        [Fact]
        public void WithDocumentsFrom_projects_the_document_source_into_the_pipeline()
        {
            var source = Observable.Repeat<Document>(new TestDocument(), 3);
            var pipeline = _sut.WithDocumentsFrom(source);

            source.Zip(pipeline, (s, p) => (s, p.Source)).Subscribe(
                Observer.Create<ValueTuple<Document, Document>>(
                    t =>
                    {
                        var (expected, actual) = t;
                        Assert.Same(expected, actual);
                    }));
        }
    }
}
