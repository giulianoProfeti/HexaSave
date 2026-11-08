using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HexaSave.Application.Ports;
using HexaSave.Domain.Progress;
using HexaSave.Domain.Shared;
 
namespace HexaSave.Adapters.InMemory
{
    /// <summary>
    /// Adapter do port ISaveRepository que guarda o progresso em um dicionário na memória.
    /// Os dados somem quando o jogo é encerrado: serve para validar a composição e para testes.
    /// </summary>
    public sealed class InMemorySaveRepository : ISaveRepository
    {
        private readonly Dictionary<PlayerId, PlayerProgress> _store = new();
 
        public Task<PlayerProgress> LoadAsync(PlayerId playerId)
        {
            // Se não encontrar, "progress" fica null, exatamente o que o contrato pede.
            _store.TryGetValue(playerId, out var progress);
            return Task.FromResult(progress);
        }
 
        public Task SaveAsync(PlayerProgress progress)
        {
            if (progress is null)
                throw new ArgumentNullException(nameof(progress));
 
            _store[progress.PlayerId] = progress;
            return Task.CompletedTask;
        }
    }
}