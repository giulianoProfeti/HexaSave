using System;
using System.IO;
using System.Threading.Tasks;
using HexaSave.Adapters.Local;
using NUnit.Framework;

namespace HexaSave.Tests.EditMode.Adapters
{
    [TestFixture]
    public sealed class LocalAuthServiceTests
    {
        private string _baseDirectory;

        [SetUp]
        public void SetUp()
        {
            _baseDirectory = Path.Combine(Path.GetTempPath(), "HexaSaveTests", Guid.NewGuid().ToString("N"));
        }

        [TearDown]
        public void TearDown()
        {
            if (Directory.Exists(_baseDirectory))
                Directory.Delete(_baseDirectory, recursive: true);
        }

        [Test]
        public void BeforeSignIn_NobodyIsSignedIn()
        {
            var auth = new LocalAuthService(_baseDirectory);

            Assert.That(auth.IsSignedIn, Is.False);
            Assert.That(auth.CurrentPlayer, Is.Null);
        }

        [Test]
        public async Task SignIn_FirstTime_CreatesLocalIdentityFile()
        {
            var auth = new LocalAuthService(_baseDirectory);

            var playerId = await auth.SignInAsync();

            Assert.That(playerId.Value, Does.StartWith("local-"));
            Assert.That(File.ReadAllText(Path.Combine(_baseDirectory, "player-id.txt")), Is.EqualTo(playerId.Value));
            Assert.That(auth.IsSignedIn, Is.True);
        }

        [Test]
        public async Task SignIn_InANewSession_RecognizesTheSamePlayer()
        {
            var firstSession = await new LocalAuthService(_baseDirectory).SignInAsync();
            var secondSession = await new LocalAuthService(_baseDirectory).SignInAsync();

            Assert.That(secondSession, Is.EqualTo(firstSession));
        }

        [Test]
        public async Task SignIn_EmptyIdentityFile_GeneratesANewIdentity()
        {
            Directory.CreateDirectory(_baseDirectory);
            File.WriteAllText(Path.Combine(_baseDirectory, "player-id.txt"), "   ");

            var playerId = await new LocalAuthService(_baseDirectory).SignInAsync();

            Assert.That(playerId.Value, Does.StartWith("local-"));
        }
    }
}
