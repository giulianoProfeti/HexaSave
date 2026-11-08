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
        private readonly PlayerId _playerToSignIn;

        public const string DefaultPlayerId = "jogador-local";
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
    }
}