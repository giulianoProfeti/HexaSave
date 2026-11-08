using System;
using System.Threading.Tasks;
using HexaSave.Application.Ports;
using HexaSave.Domain.Progress;
 
namespace HexaSave.Application.UseCases
{
    /// <summary>Caso de uso: carregar o progresso do jogador autenticado.</summary>
    public sealed class LoadProgressUseCase
    {
        private readonly ISaveRepository _repository;
        private readonly IAuthService _auth;
 
        public LoadProgressUseCase(ISaveRepository repository, IAuthService auth)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _auth = auth ?? throw new ArgumentNullException(nameof(auth));
        }
 
        /// <returns>O progresso salvo, ou null se o jogador nunca salvou.</returns>
        public async Task<PlayerProgress> ExecuteAsync()
        {
            var playerId = AuthGuard.RequireSignedIn(_auth);
            return await _repository.LoadAsync(playerId);
        }
    }
}