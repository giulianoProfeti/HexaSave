using System;

namespace HexaSave.Application.Errors
{
    /// <summary>
    /// Falha de infraestrutura que faz parte do CONTRATO dos ports (decisão D12).
    /// Cada adapter captura os erros específicos do seu provedor (arquivo, UGS, HTTP)
    /// e os relança como esta exceção, guardando o erro original em InnerException.
    /// Assim, o jogo trata um único tipo de falha, seja qual for o provedor.
    /// </summary>
    public sealed class PersistenceUnavailableException : Exception
    {
        public PersistenceUnavailableException(string message) : base(message) { }
 
        public PersistenceUnavailableException(string message, Exception innerException)
            : base(message, innerException) { }
    }
}
