using System.Threading.Tasks;
using HexaSave.Application.Errors;
using HexaSave.Domain.Progress;
using HexaSave.Domain.Shared;
 
namespace HexaSave.Application.Ports
{
    /// <summary>
    /// Port de saída: persistência do progresso do jogador (padrão Repository).
    /// Implementado pelos adapters Local, UGS, HTTP e em memória.
    /// </summary>
    /// <remarks>
    /// Contrato que TODOS os adapters devem respeitar:
    /// <list type="bullet">
    /// <item>LoadAsync devolve null quando o jogador ainda não tem save (não é erro).</item>
    /// <item>SaveAsync substitui o progresso existente daquele PlayerId.</item>
    /// <item>Falhas de infraestrutura são lançadas como <see cref="PersistenceUnavailableException"/>.</item>
    /// </list>
    /// </remarks>
    public interface ISaveRepository
    {
        /// <returns>O progresso salvo, ou null se o jogador nunca salvou.</returns>
        /// <exception cref="PersistenceUnavailableException">Provedor indisponível.</exception>
        Task<PlayerProgress> LoadAsync(PlayerId playerId);
 
        /// <exception cref="PersistenceUnavailableException">Provedor indisponível.</exception>
        Task SaveAsync(PlayerProgress progress);
    }
}