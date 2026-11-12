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
    public sealed class ProgressUseCasesTests
    {
        private InMemorySaveRepository _repository;
        private InMemoryAuthService _auth;
        private ManualClock _clock;
        private SignInUseCase _signIn;
        private SaveProgressUseCase _save;
        private LoadProgressUseCase _load;

        [SetUp]
        public void SetUp()
        {
            // Um "mini composition root" para cada teste: peças novas, sem estado compartilhado.
            _repository = new InMemorySaveRepository();
            _auth = new InMemoryAuthService(TestData.PlayerA.Value);
            _clock = new ManualClock(TestData.T0);

            _signIn = new SignInUseCase(_auth);
            _save = new SaveProgressUseCase(_repository, _auth, _clock, new FixedDeviceInfo(TestData.Pc));
            _load = new LoadProgressUseCase(_repository, _auth);
        }

        [Test]
        public async Task Save_BeforeSignIn_ThrowsNotSignedIn()
        {
            await AsyncAssert.ThrowsAsync<NotSignedInException>(() => _save.ExecuteAsync(TestData.Content));
        }

        [Test]
        public async Task Load_BeforeSignIn_ThrowsNotSignedIn()
        {
            await AsyncAssert.ThrowsAsync<NotSignedInException>(() => _load.ExecuteAsync());
        }

        [Test]
        public async Task SignIn_Twice_ReturnsSamePlayer()
        {
            var first = await _signIn.ExecuteAsync();
            var second = await _signIn.ExecuteAsync();

            Assert.That(first, Is.EqualTo(TestData.PlayerA));
            Assert.That(second, Is.EqualTo(first));
        }

        [Test]
        public async Task Load_WithoutSave_ReturnsNull()
        {
            await _signIn.ExecuteAsync();

            var loaded = await _load.ExecuteAsync();

            Assert.That(loaded, Is.Null);
        }

        [Test]
        public async Task Save_FirstTime_CreatesVersionOneAndPersists()
        {
            await _signIn.ExecuteAsync();

            var saved = await _save.ExecuteAsync(TestData.Content);
            var loaded = await _load.ExecuteAsync();

            Assert.That(saved.Version.Number, Is.EqualTo(1));
            Assert.That(loaded, Is.SameAs(saved));
        }

        [Test]
        public async Task Save_UsesInjectedClockAndDevice()
        {
            await _signIn.ExecuteAsync();
            _clock.Advance(TimeSpan.FromMinutes(30));

            var saved = await _save.ExecuteAsync(TestData.Content);

            Assert.That(saved.UpdatedAt, Is.EqualTo(TestData.T0.AddMinutes(30)));
            Assert.That(saved.DeviceId, Is.EqualTo(TestData.Pc));
        }

        [Test]
        public async Task Save_Twice_IncrementsVersion()
        {
            await _signIn.ExecuteAsync();

            await _save.ExecuteAsync("{\"nivel\":1}");
            var second = await _save.ExecuteAsync("{\"nivel\":2}");

            Assert.That(second.Version.Number, Is.EqualTo(2));
            Assert.That(second.Data.Content, Is.EqualTo("{\"nivel\":2}"));
        }

        [Test]
        public async Task Save_FromAnotherDeviceSharingTheRepository_ContinuesTheVersion()
        {
            // Dois aparelhos = duas montagens que compartilham o mesmo repositório e a mesma conta.
            var saveFromPhone = new SaveProgressUseCase(_repository, _auth, _clock, new FixedDeviceInfo(TestData.Phone));
            await _signIn.ExecuteAsync();

            await _save.ExecuteAsync("{\"nivel\":1}");
            _clock.Advance(TimeSpan.FromHours(1));
            var fromPhone = await saveFromPhone.ExecuteAsync("{\"nivel\":2}");

            Assert.That(fromPhone.Version.Number, Is.EqualTo(2));
            Assert.That(fromPhone.DeviceId, Is.EqualTo(TestData.Phone));
        }

        [Test]
        public async Task Save_InvalidContent_FailsBeforeAnyIO()
        {
            var failing = new FailingSaveRepository();
            var save = new SaveProgressUseCase(failing, _auth, _clock, new FixedDeviceInfo(TestData.Pc));
            await _signIn.ExecuteAsync();

            await AsyncAssert.ThrowsAsync<DomainException>(() => save.ExecuteAsync(""));

            Assert.That(failing.Calls, Is.EqualTo(0));
        }

        [Test]
        public async Task Save_WhenProviderIsDown_PropagatesPersistenceUnavailable()
        {
            var save = new SaveProgressUseCase(new FailingSaveRepository(), _auth, _clock, new FixedDeviceInfo(TestData.Pc));
            await _signIn.ExecuteAsync();

            await AsyncAssert.ThrowsAsync<PersistenceUnavailableException>(() => save.ExecuteAsync(TestData.Content));
        }

        [Test]
        public void Constructor_NullDependency_ThrowsArgumentNull()
        {
            var device = new FixedDeviceInfo(TestData.Pc);

            Assert.Throws<ArgumentNullException>(() => new SaveProgressUseCase(null, _auth, _clock, device));
            Assert.Throws<ArgumentNullException>(() => new SaveProgressUseCase(_repository, null, _clock, device));
            Assert.Throws<ArgumentNullException>(() => new SaveProgressUseCase(_repository, _auth, null, device));
            Assert.Throws<ArgumentNullException>(() => new SaveProgressUseCase(_repository, _auth, _clock, null));
            Assert.Throws<ArgumentNullException>(() => new LoadProgressUseCase(null, _auth));
            Assert.Throws<ArgumentNullException>(() => new SignInUseCase(null));
        }
    }
}
