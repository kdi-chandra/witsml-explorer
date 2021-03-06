using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using WitsmlExplorer.Api.Jobs;
using WitsmlExplorer.Api.Jobs.Common;
using WitsmlExplorer.Api.Services;
using WitsmlExplorer.Api.Workers;
using Xunit;

namespace WitsmlExplorer.IntegrationTests.Api.Workers
{
    [SuppressMessage("ReSharper", "xUnit1004")]
    public class DeleteTrajectoryWorkerTests
    {
        private readonly DeleteTrajectoryWorker worker;

        public DeleteTrajectoryWorkerTests()
        {
            var configuration = ConfigurationReader.GetConfig();
            var witsmlClientProvider = new WitsmlClientProvider(configuration);
            worker = new DeleteTrajectoryWorker(witsmlClientProvider);
        }

        [Fact(Skip = "Should only be run manually")]
        public async Task DeleteTrajectory()
        {
            var job = new DeleteTrajectoryJob
            {
                TrajectoryReference = new TrajectoryReference
                {
                    WellUid = "fa53698b-0a19-4f02-bca5-001f5c31c0ca",
                    WellboreUid = "eea43bf8-e3b7-42b6-b328-21b34cb505eb",
                    TrajectoryUid = "1YJFL7"
                }
            };
            await worker.Execute(job);
        }
    }
}
