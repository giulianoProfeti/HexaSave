using System;
using System.Threading.Tasks;
using HexaSave.Application.Ports;
using HexaSave.Domain.Leaderboard;
 
namespace HexaSave.Application.UseCases
{
    /// <summary>
    /// Caso de uso: enviar uma pontuação para o ranking configurado.
    /// O LeaderboardId é injetado no construtor: valores de configuração
    /// também podem ser dependências.
    /// </summary>
    public sealed class SubmitScoreUseCase
    {
        private readonly ILeaderboardService _leaderboard;
        private readonly IAuthService _auth;
        private readonly IClock _clock;
        private readonly LeaderboardId _leaderboardId;
 
        public SubmitScoreUseCase(ILeaderboardService leaderboard, IAuthService auth,
                                  IClock clock, LeaderboardId leaderboardId)
        {
            _leaderboard = leaderboard ?? throw new ArgumentNullException(nameof(leaderboard));
            _auth = auth ?? throw new ArgumentNullException(nameof(auth));
            _clock = clock ?? throw new ArgumentNullException(nameof(clock));
 
            if (leaderboardId.IsDefault)
                throw new ArgumentException("O LeaderboardId não foi configurado.", nameof(leaderboardId));
            _leaderboardId = leaderboardId;
        }
 
        /// <returns>A pontuação enviada, como fato do domínio.</returns>
        public async Task<ScoreEntry> ExecuteAsync(int points, string displayName)
        {
            var playerId = AuthGuard.RequireSignedIn(_auth);
 
            // Os Value Objects validam: pontos negativos ou nome inválido geram DomainException.
            var entry = new ScoreEntry(playerId, new DisplayName(displayName),
                                       new Score(points), _clock.UtcNow);
 
            await _leaderboard.SubmitScoreAsync(_leaderboardId, entry);
            return entry;
        }
    }
}