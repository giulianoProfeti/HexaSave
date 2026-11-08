using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HexaSave.Application.Ports;
using HexaSave.Domain.Leaderboard;
 
namespace HexaSave.Application.UseCases
{
    /// <summary>Caso de uso: consultar as melhores pontuações do ranking configurado.</summary>
    public sealed class GetTopScoresUseCase
    {
        public const int MinCount = 1;
        public const int MaxCount = 100;
        public const int DefaultCount = 10;
 
        private readonly ILeaderboardService _leaderboard;
        private readonly LeaderboardId _leaderboardId;
 
        public GetTopScoresUseCase(ILeaderboardService leaderboard, LeaderboardId leaderboardId)
        {
            _leaderboard = leaderboard ?? throw new ArgumentNullException(nameof(leaderboard));
 
            if (leaderboardId.IsDefault)
                throw new ArgumentException("O LeaderboardId não foi configurado.", nameof(leaderboardId));
            _leaderboardId = leaderboardId;
        }
 
        /// <returns>Lista ordenada da maior para a menor pontuação (vazia se não houver).</returns>
        public async Task<IReadOnlyList<RankedScore>> ExecuteAsync(int count = DefaultCount)
        {
            // Regra de aplicação (limite de consulta), não de domínio.
            if (count < MinCount || count > MaxCount)
                throw new ArgumentOutOfRangeException(nameof(count),
                    $"A quantidade deve estar entre {MinCount} e {MaxCount}.");
 
            return await _leaderboard.GetTopScoresAsync(_leaderboardId, count);
        }
    }
}