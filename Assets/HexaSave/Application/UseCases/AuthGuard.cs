using HexaSave.Application.Errors;
using HexaSave.Application.Ports;
using HexaSave.Domain.Shared;

namespace HexaSave.Application.UseCases
{
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