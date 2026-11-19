using System;

namespace HexaSave.Application.Ports
{
    /// <summary>
    /// Tipo de conta do jogador atual. Unavailable vale 0 de propósito:
    /// um AccountStatus criado com default(...) significa "sem contas", o estado mais seguro.
    /// </summary>
    public enum AccountKind
    {
        Unavailable = 0, // o provedor não oferece contas
        Guest = 1,       // jogador anônimo: o progresso só existe neste aparelho
        Registered = 2,  // jogador com usuário e senha: pode recuperar o progresso em outro aparelho
    }

    /// <summary>Situação da conta do jogador atual, como o jogo precisa exibi-la.</summary>
    public readonly struct AccountStatus
    {
        public AccountKind Kind { get; }
        /// <summary>Nome de usuário; null quando Kind não é Registered.</summary>
        public string Username { get; }
        public static AccountStatus Unavailable => new(AccountKind.Unavailable, null);
        public static AccountStatus Guest => new(AccountKind.Guest, null);

        private AccountStatus(AccountKind kind, string username)
        {
            Kind = kind;
            Username = username;
        }

        public static AccountStatus Registered(string username)
        {
            if (string.IsNullOrWhiteSpace(username))
                throw new ArgumentException("Uma conta registrada precisa de um nome de usuário.", nameof(username));

            return new AccountStatus(AccountKind.Registered, username);
        }

        public override string ToString() => Kind == AccountKind.Registered ? $"Registered({Username})" : Kind.ToString();
    }
}