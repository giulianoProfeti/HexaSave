using System.Threading.Tasks;
using HexaSave.Domain.Shared;
using HexaSave.Domain.Progress;
using HexaSave.Application.Errors;
using HexaSave.Application.Ports;

namespace HexaSave.Tests.EditMode.Support
{
    /// <summary>
    /// Dublê de teste: um repositório sempre "fora do ar".
    /// Conta as chamadas para que os testes possam verificar se houve tentativa de I/O.
    /// </summary>
    internal sealed class FailingSaveRepository : ISaveRepository
    {
        public int Calls { get; private set; }

        public Task<PlayerProgress> LoadAsync(PlayerId playerId)
        {
            Calls++;
            return Task.FromException<PlayerProgress>(Unavailable());
        }

        public Task SaveAsync(PlayerProgress progress)
        {
            Calls++;
            return Task.FromException(Unavailable());
        }

        private static PersistenceUnavailableException Unavailable()
            => new PersistenceUnavailableException("Provedor fora do ar (simulado).");
    }
}
