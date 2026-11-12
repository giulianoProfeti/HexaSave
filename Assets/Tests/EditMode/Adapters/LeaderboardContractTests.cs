using System.Threading.Tasks;
using HexaSave.Application.Ports;
using HexaSave.Domain.Leaderboard;
using HexaSave.Tests.EditMode.Support;
using NUnit.Framework;

namespace HexaSave.Tests.EditMode.Adapters
{
    /// <summary>Testes de contrato do port ILeaderboardService (inclui a decisão D22).</summary>
    public abstract class LeaderboardContractTests
    {
        protected ILeaderboardService Leaderboard { get; private set; }

        protected abstract ILeaderboardService CreateLeaderboard();

        [SetUp]
        public void CreateSubject()
        {
            Leaderboard = CreateLeaderboard();
        }

        [Test]
        public async Task GetTop_UnknownBoard_ReturnsEmptyList()
        {
            var top = await Leaderboard.GetTopScoresAsync(TestData.Board, 10);

            Assert.That(top, Is.Not.Null);
            Assert.That(top, Is.Empty);
        }

        [Test]
        public async Task Submit_ThenGetTop_ReturnsTheEntryInFirstPlace()
        {
            var entry = TestData.Entry(TestData.PlayerA, 300);

            await Leaderboard.SubmitScoreAsync(TestData.Board, entry);
            var top = await Leaderboard.GetTopScoresAsync(TestData.Board, 10);

            Assert.That(top.Count, Is.EqualTo(1));
            Assert.That(top[0].Rank, Is.EqualTo(1));
            Assert.That(top[0].Entry, Is.EqualTo(entry));
        }

        [Test]
        public async Task Submit_LowerScore_DoesNotReplaceTheBest()
        {
            await Leaderboard.SubmitScoreAsync(TestData.Board, TestData.Entry(TestData.PlayerA, 500));
            await Leaderboard.SubmitScoreAsync(TestData.Board, TestData.Entry(TestData.PlayerA, 300));

            var top = await Leaderboard.GetTopScoresAsync(TestData.Board, 10);

            Assert.That(top.Count, Is.EqualTo(1));
            Assert.That(top[0].Entry.Score.Value, Is.EqualTo(500));
        }

        [Test]
        public async Task Submit_HigherScore_ReplacesTheBest()
        {
            await Leaderboard.SubmitScoreAsync(TestData.Board, TestData.Entry(TestData.PlayerA, 500));
            await Leaderboard.SubmitScoreAsync(TestData.Board, TestData.Entry(TestData.PlayerA, 800));

            var top = await Leaderboard.GetTopScoresAsync(TestData.Board, 10);

            Assert.That(top.Count, Is.EqualTo(1));
            Assert.That(top[0].Entry.Score.Value, Is.EqualTo(800));
        }

        [Test]
        public async Task GetTop_OrdersFromHighestToLowestWithSequentialRanks()
        {
            await Leaderboard.SubmitScoreAsync(TestData.Board, TestData.Entry(TestData.PlayerA, 200));
            await Leaderboard.SubmitScoreAsync(TestData.Board, TestData.Entry(TestData.PlayerB, 900));
            await Leaderboard.SubmitScoreAsync(TestData.Board, TestData.Entry(TestData.PlayerC, 500));

            var top = await Leaderboard.GetTopScoresAsync(TestData.Board, 10);

            Assert.That(top[0].Entry.PlayerId, Is.EqualTo(TestData.PlayerB));
            Assert.That(top[1].Entry.PlayerId, Is.EqualTo(TestData.PlayerC));
            Assert.That(top[2].Entry.PlayerId, Is.EqualTo(TestData.PlayerA));
            Assert.That(top[2].Rank, Is.EqualTo(3));
        }

        [Test]
        public async Task GetTop_Tie_PutsWhoScoredFirstAhead()
        {
            await Leaderboard.SubmitScoreAsync(TestData.Board, TestData.Entry(TestData.PlayerA, 500, at: TestData.T0.AddMinutes(10)));
            await Leaderboard.SubmitScoreAsync(TestData.Board, TestData.Entry(TestData.PlayerB, 500, at: TestData.T0));

            var top = await Leaderboard.GetTopScoresAsync(TestData.Board, 10);

            Assert.That(top[0].Entry.PlayerId, Is.EqualTo(TestData.PlayerB));
        }

        [Test]
        public async Task GetTop_RespectsTheRequestedCount()
        {
            await Leaderboard.SubmitScoreAsync(TestData.Board, TestData.Entry(TestData.PlayerA, 100));
            await Leaderboard.SubmitScoreAsync(TestData.Board, TestData.Entry(TestData.PlayerB, 200));
            await Leaderboard.SubmitScoreAsync(TestData.Board, TestData.Entry(TestData.PlayerC, 300));

            var top = await Leaderboard.GetTopScoresAsync(TestData.Board, 2);

            Assert.That(top.Count, Is.EqualTo(2));
        }

        [Test]
        public async Task Boards_AreIndependent()
        {
            var otherBoard = new LeaderboardId("outro-ranking");
            await Leaderboard.SubmitScoreAsync(otherBoard, TestData.Entry(TestData.PlayerA, 100));

            var top = await Leaderboard.GetTopScoresAsync(TestData.Board, 10);

            Assert.That(top, Is.Empty);
        }
    }
}
