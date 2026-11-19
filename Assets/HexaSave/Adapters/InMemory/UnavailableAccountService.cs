using System.Threading.Tasks;
using HexaSave.Application.Errors;
using HexaSave.Application.Ports;
using HexaSave.Domain.Shared;

namespace HexaSave.Adapters.InMemory
{
    /// <summary>
    /// "Null Object" do port IAccountService: usado por provedores que não têm contas (ex.: Local).
    /// Em vez de o jogo checar "existe serviço de conta?", ele sempre recebe um serviço,
    /// que simplesmente responde "indisponível".
    /// </summary>
    public sealed class UnavailableAccountService : IAccountService
    {
        public Task<AccountStatus> GetStatusAsync() => Task.FromResult(AccountStatus.Unavailable);

        public Task LinkUsernamePasswordAsync(AccountCredentials credentials) => Task.FromException(NotSupported());

        public Task<PlayerId> SignInWithUsernamePasswordAsync(AccountCredentials credentials)
            => Task.FromException<PlayerId>(NotSupported());

        private static AccountException NotSupported()
            => new AccountException(AccountError.NotSupported, "Contas de jogador não estão disponíveis neste provedor.");
    }
}