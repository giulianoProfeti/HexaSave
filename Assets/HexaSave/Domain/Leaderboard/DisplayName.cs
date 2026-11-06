using System;
using HexaSave.Domain.Shared;

namespace HexaSave.Domain.Leaderboard
{
    /// <summary>
    /// Nome público exibido no ranking, escolhido pelo jogador (nunca o nome real: LGPD).
    /// Espaços nas pontas são removidos antes de validar o tamanho.
    /// </summary>
    public readonly struct DisplayName : IEquatable<DisplayName>
    {
        public const int MinLength = 3;
        public const int MaxLength = 20;
 
        public string Value { get; }
        public bool IsDefault => Value is null;
 
        public DisplayName(string value)
        {
            var trimmed = value?.Trim();
 
            if (string.IsNullOrEmpty(trimmed))
                throw new DomainException("DisplayName não pode ser vazio.");
 
            if (trimmed.Length < MinLength || trimmed.Length > MaxLength)
                throw new DomainException($"DisplayName deve ter entre {MinLength} e {MaxLength} caracteres.");
 
            Value = trimmed;
        }
 
        public bool Equals(DisplayName other) => string.Equals(Value, other.Value, StringComparison.Ordinal);
        public override bool Equals(object obj) => obj is DisplayName other && Equals(other);
        public override int GetHashCode() => Value is null ? 0 : StringComparer.Ordinal.GetHashCode(Value);
 
        public static bool operator ==(DisplayName left, DisplayName right) => left.Equals(right);
        public static bool operator !=(DisplayName left, DisplayName right) => !left.Equals(right);
 
        public override string ToString() => Value ?? "<default>";
    }
}
