using System;

namespace HexaSave.Application.Errors
{
    /// <summary>Motivos pelos quais uma operação de conta pode ser recusada.</summary>
    public enum AccountError
    {
        InvalidCredentials = 0,   // usuário ou senha incorretos, vazios ou fora do formato do provedor
        UsernameUnavailable = 1,  // o nome de usuário já pertence a outro jogador
        AlreadyRegistered = 2,    // o jogador atual já tem uma conta vinculada
        NotSupported = 3,         // o provedor não oferece contas
        Rejected = 4,             // o provedor recusou por outro motivo (detalhe na mensagem)
    }

    /// <summary>
    /// Falha de conta que o JOGADOR pode corrigir (ex.: senha errada).
    /// Diferente de PersistenceUnavailableException, que é falha de infraestrutura.
    /// A mensagem é escrita para ser exibida na tela.
    /// </summary>
    public sealed class AccountException : Exception
    {
        public AccountException(AccountError reason, string message, Exception innerException = null)
            : base(message, innerException)
        {
            Reason = reason;
        }

        public AccountError Reason { get; }
    }
}
