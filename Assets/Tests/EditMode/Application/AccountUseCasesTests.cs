using System.Threading.Tasks;
using HexaSave.Adapters.InMemory;
using HexaSave.Application.Errors;
using HexaSave.Application.Ports;
using HexaSave.Application.UseCases;
using HexaSave.Tests.EditMode.Support;
using NUnit.Framework;

namespace HexaSave.Tests.EditMode.Application
{
    /// <summary>
    /// Casos de uso de conta. Dois "aparelhos" compartilham o mesmo InMemoryAccountDirectory,
    /// que faz o papel do servidor de contas.
    /// </summary>
    [TestFixture]
    public sealed class AccountUseCasesTests
    {
        private const string Username = "rafael";
        private const string Password = "Senha#123";

        private InMemoryAccountDirectory _server;

        // Aparelho A (PC): convidado "player-a"
        private InMemoryAuthService _authA;
        private LinkAccountUseCase _linkA;
        private GetAccountStatusUseCase _statusA;

        // Aparelho B (celular): convidado "player-b"
        private InMemoryAuthService _authB;
        private InMemoryAccountService _accountB;
        private SignInToAccountUseCase _signInB;
        private GetAccountStatusUseCase _statusB;

        [SetUp]
        public void SetUp()
        {
            _server = new InMemoryAccountDirectory();

            _authA = new InMemoryAuthService(TestData.PlayerA.Value);
            var accountA = new InMemoryAccountService(_authA, _server);
            _linkA = new LinkAccountUseCase(accountA, _authA);
            _statusA = new GetAccountStatusUseCase(accountA);

            _authB = new InMemoryAuthService(TestData.PlayerB.Value);
            _accountB = new InMemoryAccountService(_authB, _server);
            _signInB = new SignInToAccountUseCase(_accountB);
            _statusB = new GetAccountStatusUseCase(_accountB);
        }

        [Test]
        public async Task Status_NewPlayer_IsGuest()
        {
            await _authA.SignInAsync();

            var status = await _statusA.ExecuteAsync();

            Assert.That(status.Kind, Is.EqualTo(AccountKind.Guest));
            Assert.That(status.Username, Is.Null);
        }

        [Test]
        public async Task Link_BeforeSignIn_ThrowsNotSignedIn()
        {
            await AsyncAssert.ThrowsAsync<NotSignedInException>(() => _linkA.ExecuteAsync(Username, Password));
        }

        [Test]
        public async Task Link_Guest_BecomesRegisteredAndKeepsThePlayer()
        {
            var before = await _authA.SignInAsync();

            var status = await _linkA.ExecuteAsync("  rafael  ", Password);

            Assert.That(status.Kind, Is.EqualTo(AccountKind.Registered));
            Assert.That(status.Username, Is.EqualTo(Username));
            Assert.That(_authA.CurrentPlayer, Is.EqualTo(before));
        }

        [Test]
        public async Task Link_KeepsTheProgressAlreadySaved()
        {
            var repository = new InMemorySaveRepository();
            var save = new SaveProgressUseCase(repository, _authA, new ManualClock(TestData.T0), new FixedDeviceInfo(TestData.Pc));
            var load = new LoadProgressUseCase(repository, _authA);
            await _authA.SignInAsync();
            await save.ExecuteAsync(TestData.Content);

            await _linkA.ExecuteAsync(Username, Password);
            var loaded = await load.ExecuteAsync();

            Assert.That(loaded, Is.Not.Null);
            Assert.That(loaded.Data.Content, Is.EqualTo(TestData.Content));
        }

        [Test]
        public async Task Link_Twice_ThrowsAlreadyRegistered()
        {
            await _authA.SignInAsync();
            await _linkA.ExecuteAsync(Username, Password);

            var error = await AsyncAssert.ThrowsAsync<AccountException>(() => _linkA.ExecuteAsync("outro", Password));

            Assert.That(error.Reason, Is.EqualTo(AccountError.AlreadyRegistered));
        }

        [Test]
        public async Task Link_UsernameAlreadyTaken_ThrowsUsernameUnavailable()
        {
            await _authA.SignInAsync();
            await _linkA.ExecuteAsync(Username, Password);
            await _authB.SignInAsync();
            var linkB = new LinkAccountUseCase(_accountB, _authB);

            var error = await AsyncAssert.ThrowsAsync<AccountException>(() => linkB.ExecuteAsync(Username, "Outra#456"));

            Assert.That(error.Reason, Is.EqualTo(AccountError.UsernameUnavailable));
        }

        [Test]
        public async Task SignIn_OnAnotherDevice_SwitchesToTheAccountPlayer()
        {
            await _authA.SignInAsync();
            await _linkA.ExecuteAsync(Username, Password);
            await _authB.SignInAsync(); // aparelho novo: começa como outro convidado

            var player = await _signInB.ExecuteAsync(Username, Password);

            Assert.That(player, Is.EqualTo(TestData.PlayerA));
            Assert.That(_authB.CurrentPlayer, Is.EqualTo(TestData.PlayerA));
            Assert.That((await _statusB.ExecuteAsync()).Kind, Is.EqualTo(AccountKind.Registered));
        }

        [Test]
        public async Task SignIn_WrongPassword_ThrowsInvalidCredentialsAndKeepsTheSession()
        {
            await _authA.SignInAsync();
            await _linkA.ExecuteAsync(Username, Password);
            await _authB.SignInAsync();

            var error = await AsyncAssert.ThrowsAsync<AccountException>(() => _signInB.ExecuteAsync(Username, "errada"));

            Assert.That(error.Reason, Is.EqualTo(AccountError.InvalidCredentials));
            Assert.That(_authB.CurrentPlayer, Is.EqualTo(TestData.PlayerB));
        }

        [Test]
        public async Task SignIn_UsernameWithOtherCase_FindsTheSameAccount()
        {
            await _authA.SignInAsync();
            await _linkA.ExecuteAsync(Username, Password);
            await _authB.SignInAsync();

            var player = await _signInB.ExecuteAsync(Username.ToUpperInvariant(), Password);

            Assert.That(player, Is.EqualTo(TestData.PlayerA));
        }

        [TestCase("", Password)]
        [TestCase("   ", Password)]
        [TestCase(Username, "")]
        public async Task SignIn_EmptyCredentials_ThrowsInvalidCredentials(string username, string password)
        {
            var error = await AsyncAssert.ThrowsAsync<AccountException>(() => _signInB.ExecuteAsync(username, password));

            Assert.That(error.Reason, Is.EqualTo(AccountError.InvalidCredentials));
        }

        [Test]
        public async Task UnavailableProvider_ReportsUnavailableAndRejectsOperations()
        {
            var auth = new InMemoryAuthService();
            var account = new UnavailableAccountService();
            await auth.SignInAsync();

            var status = await new GetAccountStatusUseCase(account).ExecuteAsync();
            var linkError = await AsyncAssert.ThrowsAsync<AccountException>(
                () => new LinkAccountUseCase(account, auth).ExecuteAsync(Username, Password));
            var signInError = await AsyncAssert.ThrowsAsync<AccountException>(
                () => new SignInToAccountUseCase(account).ExecuteAsync(Username, Password));

            Assert.That(status.Kind, Is.EqualTo(AccountKind.Unavailable));
            Assert.That(linkError.Reason, Is.EqualTo(AccountError.NotSupported));
            Assert.That(signInError.Reason, Is.EqualTo(AccountError.NotSupported));
        }

        [Test]
        public void Credentials_ToString_NeverShowsThePassword()
        {
            var text = new AccountCredentials(Username, Password).ToString();

            Assert.That(text, Does.Contain(Username));
            Assert.That(text.Contains(Password), Is.False);
        }

        [Test]
        public void Status_Default_IsUnavailable()
        {
            Assert.That(default(AccountStatus).Kind, Is.EqualTo(AccountKind.Unavailable));
        }
    }
}
