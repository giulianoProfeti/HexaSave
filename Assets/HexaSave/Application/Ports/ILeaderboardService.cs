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
    /// <item>Pré-condição: Entry.PlayerId é o do jogador autenticado (os casos de uso garantem).
    /// Provedores de nuvem recusam outro PlayerId com ArgumentException.</item>
    /// <item>Cada jogador aparece no máximo UMA vez por ranking, com sua MELHOR pontuação;
    /// um envio menor que a melhor não a substitui.</item>
    /// <item>GetTopScoresAsync devolve a lista ordenada da MAIOR para a MENOR pontuação, com
    /// posições a partir de 1. A ordem entre EMPATADOS é definida pelo provedor
    /// (memória: quem atingiu primeiro; UGS: pelo PlayerId).</item>
    /// <item>>Um ranking sem pontuações devolve uma lista vazia (nunca null). Provedores que exigem
    /// rankings criados previamente tratam um ID desconhecido como falha de configuração
    /// (<see cref="PersistenceUnavailableException"/> com a explicação).</item>
    /// <item>Campos que o provedor controla podem ser substituídos por ele
    /// (ex.: o UGS registra o horário do servidor no lugar de Entry.AchievedAt).</item>
    /// <item>Falhas de infraestrutura são lançadas como <see cref="PersistenceUnavailableException"/>.</item>
    /// </list>
    /// </remarks>
    public interface ILeaderboardService
    {
        Task SubmitScoreAsync(LeaderboardId leaderboardId, ScoreEntry entry);
 
        Task<IReadOnlyList<RankedScore>> GetTopScoresAsync(LeaderboardId leaderboardId, int count);
    }
}