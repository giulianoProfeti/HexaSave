using System;
using System.Threading.Tasks;
using HexaSave.Application.Errors;
using HexaSave.Application.Ports;
using HexaSave.Domain.Shared;

namespace HexaSave.Adapters.InMemory
{
    /// <summary>
    /// Adapter do port IAccountService em memória, para testes e para o provedor InMemory.
    /// Trabalha em par com o InMemoryAuthService: entrar em uma conta troca o jogador da sessão.
    /// Instâncias que compartilham um InMemoryAccountDirectory simulam aparelhos diferentes.
    /// Guarda a senha em texto puro, o que só é aceitável em memória e em testes;
    /// provedores reais armazenam apenas um hash da senha, no servidor.
    /// </summary>
    public sealed class InMemoryAccountService : IAccountService
    {
        private readonly InMemoryAuthService _auth;
        private readonly InMemoryAccountDirectory _directory;

        public InMemoryAccountService(InMemoryAuthService auth, InMemoryAccountDirectory directory = null)
        {
            _auth = auth ?? throw new ArgumentNullException(nameof(auth));
            _directory = directory ?? new InMemoryAccountDirectory();
        }

        public Task<AccountStatus> GetStatusAsync()
        {
            var player = _auth.CurrentPlayer;

            if (player.HasValue && _directory.Usernames.TryGetValue(player.Value, out var username))
                return Task.FromResult(AccountStatus.Registered(username));

            return Task.FromResult(AccountStatus.Guest);
        }

        public Task LinkUsernamePasswordAsync(AccountCredentials credentials)
        {
            if (!_auth.CurrentPlayer.HasValue)
                return Task.FromException(new NotSignedInException());

            var player = _auth.CurrentPlayer.Value;

            // TODO: Possível fonte de erro na ordem dos retornos
            if (_directory.Usernames.ContainsKey(player))
                return Task.FromException(new AccountException(AccountError.AlreadyRegistered, "Este jogador já tem uma conta."));

            if (_directory.Accounts.ContainsKey(credentials.Username))
                return Task.FromException(new AccountException(AccountError.UsernameUnavailable,
                    $"O nome de usuário '{credentials.Username}' já está em uso."));

            _directory.Usernames[player] = credentials.Username;
            _directory.Accounts[credentials.Username] = (credentials.Password, player);
            return Task.CompletedTask;
        }

        public Task<PlayerId> SignInWithUsernamePasswordAsync(AccountCredentials credentials)
        {
            if (!_directory.Accounts.TryGetValue(credentials.Username, out var account) || account.Password != credentials.Password)
                return Task.FromException<PlayerId>(new AccountException(AccountError.InvalidCredentials,
                 "Usuário ou senha incorretos."));

            _auth.SwitchTo(account.Player);
            return Task.FromResult(account.Player);
        }
    }
}