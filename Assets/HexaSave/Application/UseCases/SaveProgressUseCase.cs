using System;
using System.Threading.Tasks;
using HexaSave.Application.Errors;
using HexaSave.Application.Ports;
using HexaSave.Domain.Progress;
using HexaSave.Domain.Shared;

namespace HexaSave.Application.UseCases
{
    /// <summary>
    /// Caso de uso: salvar o progresso do jogador.
    /// Orquestra domínio e ports; não contém regra de negócio (o incremento de
    /// versão está em PlayerProgress.Update) nem infraestrutura (está nos adapters).
    /// </summary>
    public sealed class SaveProgressUseCase
    {
        private readonly ISaveRepository _repository;
        private readonly IAuthService _auth;
        private readonly IClock _clock;
        private readonly IDeviceInfo _device;
 
        public SaveProgressUseCase(ISaveRepository repository, IAuthService auth,
                                   IClock clock, IDeviceInfo device)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _auth = auth ?? throw new ArgumentNullException(nameof(auth));
            _clock = clock ?? throw new ArgumentNullException(nameof(clock));
            _device = device ?? throw new ArgumentNullException(nameof(device));
        }
 
        /// <param name="gameContent">Dados do jogo já convertidos em texto (ex.: JSON).</param>
        /// <returns>O progresso efetivamente gravado, com a nova versão.</returns>
        public async Task<PlayerProgress> ExecuteAsync(string gameContent)
        {
            // 1. Validações sem custo primeiro: falha rápido, antes de qualquer I/O.
            var playerId = AuthGuard.RequireSignedIn(_auth);
            var data = new SaveData(gameContent);
 
            // 2. Carrega o atual para saber em qual versão estamos.
            var existing = await _repository.LoadAsync(playerId);
 
            // 3. Decide qual operação do domínio chamar. Lê o relógio uma única vez.
            var now = _clock.UtcNow;
            var updated = existing is null
                ? PlayerProgress.CreateNew(playerId, data, now, _device.DeviceId)
                : existing.Update(data, now, _device.DeviceId);
 
            // 4. Persiste e devolve o que foi gravado.
            await _repository.SaveAsync(updated);
            return updated;
        }
    }

    /// <summary>
    /// Checagem compartilhada pelos casos de uso que exigem um jogador autenticado.
    /// </summary>
    // "internal": é um detalhe da camada Application, invisível para o jogo.
    internal static class AuthGuard
    {
        public static PlayerId RequireSignedIn(IAuthService auth)
            => auth.CurrentPlayer ?? throw new NotSignedInException();
    }
}