using System.Threading.Tasks;
using HexaSave.Application.Ports;
using HexaSave.Domain.Shared;

namespace HexaSave.Adapters.InMemory
{
    /// <summary>
    /// Adapter do port IAuthService que "autentica" sempre o mesmo jogador local.
    /// </summary>
    public sealed class InMemoryAuthService : IAuthService
    {
        public const string DefaultPlayerId = "jogador-local";
        private readonly PlayerId _playerToSignIn;

        public PlayerId? CurrentPlayer { get; private set; }
        public bool IsSignedIn => CurrentPlayer.HasValue;

        public InMemoryAuthService() : this(DefaultPlayerId) { }
 
        public InMemoryAuthService(string playerId)
        {
            _playerToSignIn = new PlayerId(playerId);
        }
        
        public Task<PlayerId> SignInAsync()
        {
            if (!CurrentPlayer.HasValue)
                CurrentPlayer = _playerToSignIn;
 
            return Task.FromResult(CurrentPlayer.Value);
        }

        /// <summary>
        /// Troca o jogador da sessão. Usado pelo InMemoryAccountService ao entrar em outra conta.
        /// "internal": só o próprio assembly pode trocar a sessão.
        /// </summary>
        internal void SwitchTo(PlayerId playerId) => CurrentPlayer = playerId;
    }
}