using System;

namespace HexaSave.Application.Errors
{
    /// <summary>
    /// Lançada quando um caso de uso que exige um jogador autenticado
    /// é chamado antes do SignInUseCase.
    /// </summary>
    public sealed class NotSignedInException : Exception
    {
        public NotSignedInException()
            : base("É preciso entrar (SignInUseCase) antes de executar este caso de uso.") { }
 
        public NotSignedInException(string message) : base(message) { }
    }
}
