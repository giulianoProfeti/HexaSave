using HexaSave.Adapters.InMemory;
using HexaSave.Application.Ports;
using NUnit.Framework;

namespace HexaSave.Tests.EditMode.Adapters
{
    /// <summary>Roda os testes de contrato do leaderboard contra o adapter em memória.</summary>
    [TestFixture]
    public sealed class InMemoryLeaderboardServiceTests : LeaderboardContractTests
    {
        protected override ILeaderboardService CreateLeaderboard() => new InMemoryLeaderboardService();
    }
}
