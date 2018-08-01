using System.Threading.Tasks;
using MockWaivesApi;
using Xunit;

namespace Waives.Client.Tests.IntegrationTests
{
    // ReSharper disable once InconsistentNaming
    public class Classifiers_Client_Should : IntegrationTest
    {
        private const string ClassifierName = "Waives Client Test Classifier";

        public Classifiers_Client_Should(MockWaivesHost host) : base(host)
        {
        }

        [Fact]
        public async Task Create_A_Classifier()
        {
            var classifier = await ApiClient.CreateClassifier(ClassifierName);

            Assert.NotNull(classifier);
        }
    }
}
