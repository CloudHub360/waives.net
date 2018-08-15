namespace Waives.Pipelines
{
    internal class ProcessingResult
    {
        public WaivesDocument Document { get; }
        public bool ProcessedSuccessfully { get; }

        public ProcessingResult(WaivesDocument document, bool processedSuccessfully)
        {
            Document = document;
            ProcessedSuccessfully = processedSuccessfully;
        }
    }
}