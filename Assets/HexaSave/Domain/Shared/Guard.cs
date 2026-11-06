namespace HexaSave.Domain.Shared
{
    /// <summary>
    /// Verificações reutilizadas pelas entidades e Value Objects compostos.
    /// "internal": visível apenas dentro do assembly HexaSave.Domain.
    /// </summary>
    internal static class Guard
    {
        /// <summary>
        /// Structs sempre podem ser criados com default(T), sem passar pelo construtor.
        /// Quem recebe um Value Object como parâmetro usa este método para recusá-lo.
        /// </summary>
        public static void AgainstDefault(bool isDefault, string parameterName)
        {
            if (isDefault)
            {
                throw new DomainException($"{parameterName} não foi inicializado (valor default).");
            }
        }
    }
}
