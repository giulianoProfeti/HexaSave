using System;
using System.IO;
using System.Threading.Tasks;
using HexaSave.Adapters.Local;
using HexaSave.Application.Errors;
using HexaSave.Application.Ports;
using HexaSave.Domain.Shared;
using HexaSave.Tests.EditMode.Support;
using NUnit.Framework;

namespace HexaSave.Tests.EditMode.Adapters
{
    /// <summary>
    /// Roda os testes de contrato contra o adapter local e acrescenta os testes
    /// específicos dele (escrita atômica, arquivos corrompidos, nomes de arquivo).
    /// Cada teste usa uma pasta temporária própria, apagada no final.
    /// </summary>
    [TestFixture]
    public sealed class LocalJsonSaveRepositoryTests : SaveRepositoryContractTests
    {
        private string _baseDirectory;

        protected override ISaveRepository CreateRepository()
        {
            _baseDirectory = Path.Combine(Path.GetTempPath(), "HexaSaveTests", Guid.NewGuid().ToString("N"));
            return new LocalJsonSaveRepository(_baseDirectory);
        }

        [TearDown]
        public void DeleteTemporaryFolder()
        {
            if (Directory.Exists(_baseDirectory))
                Directory.Delete(_baseDirectory, recursive: true);
        }

        [Test]
        public async Task Save_WritesOneJsonFileInsideSavesFolder()
        {
            await Repository.SaveAsync(TestData.Progress());

            Assert.That(File.Exists(FileFor(TestData.PlayerA)), Is.True);
            Assert.That(File.ReadAllText(FileFor(TestData.PlayerA)), Does.Contain("\"schemaVersion\""));
        }

        [Test]
        public async Task Save_LeavesNoTemporaryFileBehind()
        {
            await Repository.SaveAsync(TestData.Progress(version: 1));
            await Repository.SaveAsync(TestData.Progress(version: 2));

            Assert.That(File.Exists(FileFor(TestData.PlayerA) + ".tmp"), Is.False);
        }

        [Test]
        public async Task Load_AfterInterruptedWrite_RecoversTheTemporaryFile()
        {
            await Repository.SaveAsync(TestData.Progress(version: 5));
            var path = FileFor(TestData.PlayerA);
            File.Move(path, path + ".tmp"); // simula queda entre "apagar o antigo" e "promover o temporário"

            var loaded = await Repository.LoadAsync(TestData.PlayerA);

            Assert.That(loaded.Version.Number, Is.EqualTo(5));
            Assert.That(File.Exists(path), Is.True);
            Assert.That(File.Exists(path + ".tmp"), Is.False);
        }

        [Test]
        public async Task Load_CorruptedJson_ReturnsNullAndQuarantinesTheFile()
        {
            WriteRaw(TestData.PlayerA, "{ isto não é json ");

            var loaded = await Repository.LoadAsync(TestData.PlayerA);

            Assert.That(loaded, Is.Null);
            Assert.That(File.Exists(FileFor(TestData.PlayerA)), Is.False);
            Assert.That(QuarantinedFiles().Length, Is.EqualTo(1));
        }

        [Test]
        public async Task Load_TwoCorruptionsInTheSameSecond_KeepsBothQuarantinedFiles()
        {
            // Teste de regressão: a primeira versão do adapter falhava neste cenário.
            WriteRaw(TestData.PlayerA, "{ quebrado 1 ");
            await Repository.LoadAsync(TestData.PlayerA);
            WriteRaw(TestData.PlayerA, "{ quebrado 2 ");
            await Repository.LoadAsync(TestData.PlayerA);

            Assert.That(QuarantinedFiles().Length, Is.EqualTo(2));
        }

        [Test]
        public async Task Load_DataRejectedByTheDomain_IsQuarantined()
        {
            WriteRaw(TestData.PlayerA, ValidJson(TestData.PlayerA.Value, version: 0));

            Assert.That(await Repository.LoadAsync(TestData.PlayerA), Is.Null);
            Assert.That(QuarantinedFiles().Length, Is.EqualTo(1));
        }

        [Test]
        public async Task Load_UnknownSchemaVersion_IsQuarantined()
        {
            WriteRaw(TestData.PlayerA, ValidJson(TestData.PlayerA.Value, schemaVersion: 99));

            Assert.That(await Repository.LoadAsync(TestData.PlayerA), Is.Null);
        }

        [Test]
        public async Task Load_FileBelongingToAnotherPlayer_IsQuarantined()
        {
            WriteRaw(TestData.PlayerA, ValidJson(TestData.PlayerB.Value));

            Assert.That(await Repository.LoadAsync(TestData.PlayerA), Is.Null);
        }

        [Test]
        public async Task Load_HandWrittenValidFile_IsRestored()
        {
            WriteRaw(TestData.PlayerA, ValidJson(TestData.PlayerA.Value, version: 50));

            var loaded = await Repository.LoadAsync(TestData.PlayerA);

            Assert.That(loaded.Version.Number, Is.EqualTo(50));
            Assert.That(loaded.DeviceId, Is.EqualTo(TestData.Pc));
        }

        [Test]
        public async Task PlayerIdWithForbiddenFileNameCharacters_RoundTrips()
        {
            var unusualId = new PlayerId("a/b:c*d?e");

            await Repository.SaveAsync(TestData.Progress(player: unusualId));
            var loaded = await Repository.LoadAsync(unusualId);

            Assert.That(loaded.PlayerId, Is.EqualTo(unusualId));
        }

        [Test]
        public async Task Save_WhenTheFolderCannotBeCreated_ThrowsPersistenceUnavailable()
        {
            // Um ARQUIVO no lugar onde deveria haver uma pasta torna a gravação impossível.
            Directory.CreateDirectory(Path.GetDirectoryName(_baseDirectory));
            File.WriteAllText(_baseDirectory, "não sou uma pasta");
            var repository = new LocalJsonSaveRepository(_baseDirectory);

            try
            {
                var error = await AsyncAssert.ThrowsAsync<PersistenceUnavailableException>(
                    () => repository.SaveAsync(TestData.Progress()));

                Assert.That(error.InnerException, Is.InstanceOf<IOException>());
            }
            finally
            {
                File.Delete(_baseDirectory);
            }
        }

        // ---------- auxiliares ----------

        private string SavesFolder => Path.Combine(_baseDirectory, "saves");

        private string FileFor(PlayerId playerId)
            => Path.Combine(SavesFolder, Uri.EscapeDataString(playerId.Value) + ".json");

        private void WriteRaw(PlayerId playerId, string text)
        {
            Directory.CreateDirectory(SavesFolder);
            File.WriteAllText(FileFor(playerId), text);
        }

        private string[] QuarantinedFiles()
            => Directory.GetFiles(SavesFolder, "*.corrupted-*");

        private static string ValidJson(string playerId, int version = 1, int schemaVersion = 1)
            => "{\"schemaVersion\":" + schemaVersion +
               ",\"playerId\":\"" + playerId + "\"" +
               ",\"content\":\"{}\"" +
               ",\"version\":" + version +
               ",\"updatedAtUtc\":\"2026-11-02T12:00:00.0000000+00:00\"" +
               ",\"deviceId\":\"pc\"}";
    }
}
