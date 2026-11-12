using HexaSave.Adapters.InMemory;
using HexaSave.Application.Ports;
using NUnit.Framework;

namespace HexaSave.Tests.EditMode.Adapters
{
    /// <summary>Roda todos os testes de contrato contra o adapter em memória.</summary>
    [TestFixture]
    public sealed class InMemorySaveRepositoryTests : SaveRepositoryContractTests
    {
        protected override ISaveRepository CreateRepository() => new InMemorySaveRepository();
    }
}
