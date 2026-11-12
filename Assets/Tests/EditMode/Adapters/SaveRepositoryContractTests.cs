using System;
using System.Threading.Tasks;
using HexaSave.Application.Ports;
using HexaSave.Domain.Progress;
using HexaSave.Tests.EditMode.Support;
using NUnit.Framework;

namespace HexaSave.Tests.EditMode.Adapters
{
    /// <summary>
    /// Testes de CONTRATO do port ISaveRepository.
    /// A classe é abstrata: cada adapter cria uma subclasse que só diz como construí-lo,
    /// e herda todos estes testes. Se um adapter passa aqui, ele é substituível pelos outros.
    /// </summary>
    public abstract class SaveRepositoryContractTests
    {
        protected ISaveRepository Repository { get; private set; }

        /// <summary>Cada subclasse informa como criar o adapter testado.</summary>
        protected abstract ISaveRepository CreateRepository();

        [SetUp]
        public void CreateSubject()
        {
            Repository = CreateRepository();
        }

        [Test]
        public async Task Load_UnknownPlayer_ReturnsNull()
        {
            Assert.That(await Repository.LoadAsync(TestData.PlayerA), Is.Null);
        }

        [Test]
        public async Task Save_ThenLoad_ReturnsEquivalentProgress()
        {
            var original = TestData.Progress(version: 3, at: TestData.T0.AddTicks(1234567), device: TestData.Phone);

            await Repository.SaveAsync(original);
            var loaded = await Repository.LoadAsync(TestData.PlayerA);

            AssertEquivalent(loaded, original);
        }

        [Test]
        public async Task Save_Twice_KeepsOnlyTheLatest()
        {
            await Repository.SaveAsync(TestData.Progress(content: "{\"nivel\":1}", version: 1));
            await Repository.SaveAsync(TestData.Progress(content: "{\"nivel\":2}", version: 2));

            var loaded = await Repository.LoadAsync(TestData.PlayerA);

            Assert.That(loaded.Version.Number, Is.EqualTo(2));
            Assert.That(loaded.Data.Content, Is.EqualTo("{\"nivel\":2}"));
        }

        [Test]
        public async Task Players_AreStoredSeparately()
        {
            await Repository.SaveAsync(TestData.Progress(player: TestData.PlayerA, content: "{\"de\":\"A\"}"));
            await Repository.SaveAsync(TestData.Progress(player: TestData.PlayerB, content: "{\"de\":\"B\"}"));

            Assert.That((await Repository.LoadAsync(TestData.PlayerA)).Data.Content, Is.EqualTo("{\"de\":\"A\"}"));
            Assert.That((await Repository.LoadAsync(TestData.PlayerB)).Data.Content, Is.EqualTo("{\"de\":\"B\"}"));
        }

        [Test]
        public async Task Save_Null_ThrowsArgumentNull()
        {
            await AsyncAssert.ThrowsAsync<ArgumentNullException>(() => Repository.SaveAsync(null));
        }

        protected static void AssertEquivalent(PlayerProgress actual, PlayerProgress expected)
        {
            Assert.That(actual, Is.Not.Null);
            Assert.That(actual.PlayerId, Is.EqualTo(expected.PlayerId));
            Assert.That(actual.Data, Is.EqualTo(expected.Data));
            Assert.That(actual.Version, Is.EqualTo(expected.Version));
            Assert.That(actual.UpdatedAt.UtcTicks, Is.EqualTo(expected.UpdatedAt.UtcTicks));
            Assert.That(actual.DeviceId, Is.EqualTo(expected.DeviceId));
        }
    }
}
