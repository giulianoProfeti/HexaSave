using System.Threading.Tasks;
using HexaSave.Application.Errors;
using HexaSave.Domain.Shared;

namespace HexaSave.Application.Ports
{
    /// <summary>
    /// Port de saída: gerenciamento da CONTA do jogador (usuário e senha).
    /// Separado de IAuthService pelo Princípio da Segregação de Interfaces: quem só precisa
    /// saber "quem está jogando" (salvar, enviar pontuação) não depende de operações de conta.
    /// </summary>
    /// <remarks>
    /// Contrato:
    /// <list type="bullet">
    /// <item>GetStatusAsync nunca falha por falta de suporte: devolve AccountStatus.Unavailable.</item>
    /// <item>LinkUsernamePasswordAsync vincula a conta ao jogador ATUAL, mantendo o mesmo PlayerId
    /// e, portanto, o progresso já salvo.</item>
    /// <item>SignInWithUsernamePasswordAsync troca a sessão para a conta informada; depois dela,
    /// IAuthService.CurrentPlayer passa a ser o jogador dessa conta. Se falhar, a sessão anterior
    /// é preservada sempre que possível.</item>
    /// <item>O nome de usuário NÃO diferencia maiúsculas de minúsculas (regra do UGS adotada
    /// pelo contrato); a senha diferencia.</item>
    /// <item>Erros que o jogador pode corrigir são lançados como <see cref="AccountException"/>;
    /// falhas de infraestrutura, como <see cref="PersistenceUnavailableException"/>.</item>
    /// </list>
    /// </remarks>
    public interface IAccountService
    {
        Task<AccountStatus> GetStatusAsync();

        Task LinkUsernamePasswordAsync(AccountCredentials credentials);

        Task<PlayerId> SignInWithUsernamePasswordAsync(AccountCredentials credentials);
    }
}