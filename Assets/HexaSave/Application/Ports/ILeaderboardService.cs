using System.Collections.Generic;
using System.Threading.Tasks;
using HexaSave.Application.Errors;
using HexaSave.Domain.Leaderboard;
 
namespace HexaSave.Application.Ports
{
    /// <summary>
    /// Port de saída: envio de pontuações e consulta de rankings.
    /// </summary>
    /// <remarks>
    /// Contrato:
    /// <list type="bullet">
    /// <item>GetTopScoresAsync devolve a lista ordenada da MAIOR para a MENOR pontuação.</item>
    /// <item>Um ranking sem pontuações devolve uma lista vazia (nunca null).</item>
    /// <item>Um adapter pode ignorar campos que seu provedor não suporta
    /// (ex.: o UGS usa o jogador da sessão em vez de Entry.PlayerId).</item>
    /// <item>Falhas de infraestrutura são lançadas como <see cref="PersistenceUnavailableException"/>.</item>
    /// </list>
    /// </remarks>
    public interface ILeaderboardService
    {
        Task SubmitScoreAsync(LeaderboardId leaderboardId, ScoreEntry entry);
 
        Task<IReadOnlyList<RankedScore>> GetTopScoresAsync(LeaderboardId leaderboardId, int count);
    }
}