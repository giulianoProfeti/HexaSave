using System.Threading.Tasks;
using NUnit.Framework;
using HexaSave.Adapters.InMemory;
using HexaSave.Application.Ports;
using HexaSave.Tests.EditMode.Support;

namespace HexaSave.Tests.EditMode.Adapters
{
    /// <summary>
    /// Roda os testes de contrato do leaderboard contra o adapter em memória
    /// e acrescenta as regras que são só DESTE adapter.
    /// </summary>
    [TestFixture]
    public sealed class InMemoryLeaderboardServiceTests : LeaderboardContractTests
    {
        protected override ILeaderboardService CreateLeaderboard() => new InMemoryLeaderboardService();

        // Movido do contrato na Etapa 12: o UGS desempata pelo PlayerId, a memória pelo horário (D22).
        [Test]
        public async Task GetTop_Tie_PutsWhoScoredFirstAhead()
        {
            await Leaderboard.SubmitScoreAsync(TestData.Board, TestData.Entry(TestData.PlayerA, 500, at: TestData.T0.AddMinutes(10)));
            await Leaderboard.SubmitScoreAsync(TestData.Board, TestData.Entry(TestData.PlayerB, 500, at: TestData.T0));

            var top = await Leaderboard.GetTopScoresAsync(TestData.Board, 10);

            Assert.That(top[0].Entry.PlayerId, Is.EqualTo(TestData.PlayerB));
        }
    }
}
