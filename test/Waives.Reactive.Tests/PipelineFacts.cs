using Xunit;

namespace Waives.Reactive.Tests
{
    public class PipelineFacts
    {
        [Fact]
        public void OnPipelineCompleted_is_run_at_the_end_of_a_successful_pipeline()
        {
            var sut = new Pipeline();
            var pipelineCompleted = false;
            sut.OnPipelineCompleted(() => pipelineCompleted = true);

            sut.Start();

            Assert.True(pipelineCompleted);
        }
    }
}
