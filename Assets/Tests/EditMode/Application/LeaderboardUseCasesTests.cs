using System;
using System.Threading.Tasks;
using HexaSave.Adapters.InMemory;
using HexaSave.Application.Errors;
using HexaSave.Application.UseCases;
using HexaSave.Domain.Shared;
using HexaSave.Tests.EditMode.Support;
using NUnit.Framework;

namespace HexaSave.Tests.EditMode.Application
{
    [TestFixture]
    public sealed class LeaderboardUseCasesTests
    {
        private InMemoryLeaderboardService _board;
        private InMemoryAuthService _auth;
        private ManualClock _clock;
        private SubmitScoreUseCase _submit;
        private GetTopScoresUseCase _getTop;

        [SetUp]
        public void SetUp()
        {
            _board = new InMemoryLeaderboardService();
            _auth = new InMemoryAuthService(TestData.PlayerA.Value);
            _clock = new ManualClock(TestData.T0);

            _submit = new SubmitScoreUseCase(_board, _auth, _clock, TestData.Board);
            _getTop = new GetTopScoresUseCase(_board, TestData.Board);
        }

        [Test]
        public async Task Submit_BeforeSignIn_ThrowsNotSignedIn()
        {
            await AsyncAssert.ThrowsAsync<NotSignedInException>(() => _submit.ExecuteAsync(100, "Ana"));
        }

        [Test]
        public async Task Submit_BuildsEntryFromSessionNameAndClock()
        {
            await _auth.SignInAsync();

            var entry = await _submit.ExecuteAsync(500, "  Ana  ");

            Assert.That(entry.PlayerId, Is.EqualTo(TestData.PlayerA));
            Assert.That(entry.DisplayName.Value, Is.EqualTo("Ana"));
            Assert.That(entry.Score.Value, Is.EqualTo(500));
            Assert.That(entry.AchievedAt, Is.EqualTo(TestData.T0));
        }

        [Test]
        public async Task Submit_NegativePoints_ThrowsDomainException()
        {
            await _auth.SignInAsync();

            await AsyncAssert.ThrowsAsync<DomainException>(() => _submit.ExecuteAsync(-1, "Ana"));
        }

        [Test]
        public async Task Submit_InvalidName_ThrowsDomainException()
        {
            await _auth.SignInAsync();

            await AsyncAssert.ThrowsAsync<DomainException>(() => _submit.ExecuteAsync(10, "A"));
        }

        [Test]
        public async Task GetTop_ReturnsScoresOrderedFromHighestToLowest()
        {
            await _auth.SignInAsync();
            await _submit.ExecuteAsync(500, "Ana");
            await _board.SubmitScoreAsync(TestData.Board, TestData.Entry(TestData.PlayerB, 900, "Bia"));
            await _board.SubmitScoreAsync(TestData.Board, TestData.Entry(TestData.PlayerC, 100, "Caio"));

            var top = await _getTop.ExecuteAsync();

            Assert.That(top.Count, Is.EqualTo(3));
            Assert.That(top[0].Rank, Is.EqualTo(1));
            Assert.That(top[0].Entry.Score.Value, Is.EqualTo(900));
            Assert.That(top[2].Entry.Score.Value, Is.EqualTo(100));
        }

        [TestCase(0)]
        [TestCase(101)]
        public async Task GetTop_CountOutOfRange_ThrowsArgumentOutOfRange(int count)
        {
            await AsyncAssert.ThrowsAsync<ArgumentOutOfRangeException>(() => _getTop.ExecuteAsync(count));
        }

        [Test]
        public void Constructors_WithoutLeaderboardId_ThrowArgumentException()
        {
            Assert.Throws<ArgumentException>(() => new GetTopScoresUseCase(_board, default));
            Assert.Throws<ArgumentException>(() => new SubmitScoreUseCase(_board, _auth, _clock, default));
        }
    }
}
