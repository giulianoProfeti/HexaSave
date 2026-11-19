using System;
using System.Threading.Tasks;
using HexaSave.Application.Errors;
using HexaSave.Application.Ports;

namespace HexaSave.Application.UseCases
{
    /// <summary>
    /// Caso de uso: transformar o convidado atual em uma conta com usuário e senha,
    /// mantendo o mesmo jogador (e o progresso). Regra de orquestração: só convidados podem vincular.
    /// </summary>
    public sealed class LinkAccountUseCase
    {
        private readonly IAccountService _account;
        private readonly IAuthService _auth;

        public LinkAccountUseCase(IAccountService account, IAuthService auth)
        {
            _account = account ?? throw new ArgumentNullException(nameof(account));
            _auth = auth ?? throw new ArgumentNullException(nameof(auth));
        }

        /// <returns>A nova situação da conta (Registered).</returns>
        public async Task<AccountStatus> ExecuteAsync(string username, string password)
        {
            AuthGuard.RequireSignedIn(_auth);
            var credentials = new AccountCredentials(username, password);

            var status = await _account.GetStatusAsync();

            if (status.Kind == AccountKind.Unavailable)
                throw new AccountException(AccountError.NotSupported, "Contas de jogador não estão disponíveis neste provedor.");

            if (status.Kind == AccountKind.Registered)
                throw new AccountException(AccountError.AlreadyRegistered, $"Este jogador já tem a conta '{status.Username}'.");

            await _account.LinkUsernamePasswordAsync(credentials);
            return await _account.GetStatusAsync();
        }
    }
}
