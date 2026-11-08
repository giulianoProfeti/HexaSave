using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HexaSave.Application.Ports;
using HexaSave.Domain.Leaderboard;
using HexaSave.Domain.Shared;
 
namespace HexaSave.Adapters.InMemory
{
    /// <summary>
    /// Adapter do port ILeaderboardService que mantém os rankings na memória.
    /// Segue o contrato: uma entrada por jogador (a melhor), ordenada da maior para a menor.
    /// </summary>
    public sealed class InMemoryLeaderboardService : ILeaderboardService
    {
        // Para cada ranking, a melhor pontuação de cada jogador.
        private readonly Dictionary<LeaderboardId, Dictionary<PlayerId, ScoreEntry>> _boards = new();
 
        public Task SubmitScoreAsync(LeaderboardId leaderboardId, ScoreEntry entry)
        {
            if (!_boards.TryGetValue(leaderboardId, out var board))
            {
                board = new Dictionary<PlayerId, ScoreEntry>();
                _boards[leaderboardId] = board;
            }
 
            // Só substitui se for a primeira pontuação do jogador ou se for maior que a melhor.
            if (!board.TryGetValue(entry.PlayerId, out var best) || entry.Score.IsHigherThan(best.Score))
                board[entry.PlayerId] = entry;
 
            return Task.CompletedTask;
        }
 
        public Task<IReadOnlyList<RankedScore>> GetTopScoresAsync(LeaderboardId leaderboardId, int count)
        {
            if (!_boards.TryGetValue(leaderboardId, out var board))
                return Task.FromResult<IReadOnlyList<RankedScore>>(Array.Empty<RankedScore>());
 
            var ranked = board.Values
                .OrderByDescending(entry => entry.Score.Value)   // maior pontuação primeiro
                .ThenBy(entry => entry.AchievedAt)               // empate: quem fez primeiro
                .Take(count)
                .Select((entry, index) => new RankedScore(index + 1, entry))
                .ToList();
 
            return Task.FromResult<IReadOnlyList<RankedScore>>(ranked);
        }
    }
}