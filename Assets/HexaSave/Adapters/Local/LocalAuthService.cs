using System;
using System.IO;
using System.Threading.Tasks;
using HexaSave.Application.Errors;
using HexaSave.Application.Ports;
using HexaSave.Domain.Shared;
 
namespace HexaSave.Adapters.Local
{
    /// <summary>
    /// Adapter do port IAuthService para jogo offline: gera um identificador na primeira
    /// execução e o guarda em disco, para que o mesmo jogador seja reconhecido nas próximas.
    /// </summary>
    public sealed class LocalAuthService : IAuthService
    {
        private const string IdentityFileName = "player-id.txt";
        private const string IdPrefix = "local-";
 
        private readonly string _baseDirectory;
        private readonly string _identityPath;

        public PlayerId? CurrentPlayer { get; private set; }
 
        public bool IsSignedIn => CurrentPlayer.HasValue;
 
        public LocalAuthService(string baseDirectory)
        {
            if (string.IsNullOrWhiteSpace(baseDirectory))
                throw new ArgumentException("O diretório base não pode ser vazio.", nameof(baseDirectory));
 
            _baseDirectory = baseDirectory;
            _identityPath = Path.Combine(baseDirectory, IdentityFileName);
        }
 
        public async Task<PlayerId> SignInAsync()
        {
            if (CurrentPlayer.HasValue)
                return CurrentPlayer.Value; // sessão já existe: retoma
 
            try
            {
                Directory.CreateDirectory(_baseDirectory);
 
                var storedId = File.Exists(_identityPath)
                    ? (await File.ReadAllTextAsync(_identityPath)).Trim()
                    : null;
 
                if (string.IsNullOrEmpty(storedId))
                {
                    storedId = IdPrefix + Guid.NewGuid().ToString("N");
                    await File.WriteAllTextAsync(_identityPath, storedId);
                }
 
                CurrentPlayer = new PlayerId(storedId);
                return CurrentPlayer.Value;
            }
            catch (Exception exception) when (exception is IOException || exception is UnauthorizedAccessException)
            {
                throw new PersistenceUnavailableException("Não foi possível ler ou criar a identidade local do jogador.", exception);
            }
        }
    }
}