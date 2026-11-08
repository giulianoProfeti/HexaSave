using System.Threading.Tasks;
using HexaSave.Application.Errors;
using HexaSave.Domain.Shared;
 
namespace HexaSave.Application.Ports
{
    /// <summary>
    /// Port de saída: identidade do jogador.
    /// </summary>
    /// <remarks>
    /// Contrato:
    /// <list type="bullet">
    /// <item>SignInAsync cria uma sessão (ou retoma a existente) e devolve o PlayerId.</item>
    /// <item>CurrentPlayer é null enquanto ninguém entrou.</item>
    /// <item>IsSignedIn é sempre equivalente a CurrentPlayer.HasValue.</item>
    /// <item>Falhas de infraestrutura são lançadas como <see cref="PersistenceUnavailableException"/>.</item>
    /// </list>
    /// </remarks>
    public interface IAuthService
    {
        bool IsSignedIn { get; }
 
        /// <summary>O jogador da sessão atual, ou null se ninguém entrou.</summary>
        PlayerId? CurrentPlayer { get; }
 
        Task<PlayerId> SignInAsync();
    }
}