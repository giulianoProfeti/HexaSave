using System;

namespace HexaSave.Domain.Shared
{
    /// <summary>
    /// Lançada quando uma regra do domínio é violada (ex.: Score negativo).
    /// Exception de um tipo próprio para poder diferenciar
    /// "regra de negócio violada" de "erro inesperado".
    /// </summary>
    public sealed class DomainException : Exception
    {
        public DomainException(string message) : base(message) { }
    }
}
