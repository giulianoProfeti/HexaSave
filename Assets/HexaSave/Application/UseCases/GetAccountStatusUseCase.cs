using System;
using System.Threading.Tasks;
using HexaSave.Application.Ports;

namespace HexaSave.Application.UseCases
{
    /// <summary>Caso de uso: saber se o jogador é convidado, tem conta ou se contas não estão disponíveis.</summary>
    public sealed class GetAccountStatusUseCase
    {
        private readonly IAccountService _account;

        public GetAccountStatusUseCase(IAccountService account)
        {
            _account = account ?? throw new ArgumentNullException(nameof(account));
        }

        public async Task<AccountStatus> ExecuteAsync()
        {
            return await _account.GetStatusAsync();
        }
    }
}
