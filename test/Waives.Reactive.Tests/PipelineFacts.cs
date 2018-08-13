using System;
using System.IO;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
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
    }
}
