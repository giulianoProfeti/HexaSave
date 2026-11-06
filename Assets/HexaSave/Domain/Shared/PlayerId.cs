using System;

namespace HexaSave.Domain.Shared
{
    /// <summary>
    /// Identificador do jogador. Value Object: imutável e comparado por valor.
    /// </summary>
    public readonly struct PlayerId : IEquatable<PlayerId>
    {
        public string Value { get; }

        /// <summary>
        /// Verdadeiro quando criado com default(PlayerId), sem passar pelo construtor.
        /// </summary>
        public bool IsDefault => Value is null;
 
        public PlayerId(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new DomainException("PlayerId não pode ser vazio nem ter espaços em branco.");
 
            Value = value;
        }

        public bool Equals(PlayerId other) => string.Equals(Value, other.Value, StringComparison.Ordinal);

        public override bool Equals(object obj) => obj is PlayerId other && Equals(other);

        public override int GetHashCode() => Value is null ? 0 : StringComparer.Ordinal.GetHashCode(Value);

        public static bool operator ==(PlayerId left, PlayerId right) => left.Equals(right);
        
        public static bool operator !=(PlayerId left, PlayerId right) => !left.Equals(right);

        public override string ToString() => Value ?? "<default>";
    }
}
