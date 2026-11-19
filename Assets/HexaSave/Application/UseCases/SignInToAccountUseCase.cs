using System;
using System.Threading.Tasks;
using HexaSave.Application.Errors;
using HexaSave.Application.Ports;
using HexaSave.Domain.Shared;

namespace HexaSave.Application.UseCases
{
    /// <summary>
    /// Caso de uso: entrar em uma conta existente (ex.: em um aparelho novo).
    /// Não exige sessão prévia: o jogador pode ter aberto o jogo sem conseguir entrar como convidado.
    /// </summary>
    public sealed class SignInToAccountUseCase
    {
        private readonly IAccountService _account;

        public SignInToAccountUseCase(IAccountService account)
        {
            _account = account ?? throw new ArgumentNullException(nameof(account));
        }

        /// <returns>O jogador da conta, que passa a ser o jogador atual.</returns>
        public async Task<PlayerId> ExecuteAsync(string username, string password)
        {
            var credentials = new AccountCredentials(username, password);

            var status = await _account.GetStatusAsync();
            if (status.Kind == AccountKind.Unavailable)
                throw new AccountException(AccountError.NotSupported, "Contas de jogador não estão disponíveis neste provedor.");

            return await _account.SignInWithUsernamePasswordAsync(credentials);
        }
    }
}
