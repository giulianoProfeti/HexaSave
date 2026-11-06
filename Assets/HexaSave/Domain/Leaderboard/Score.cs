using System;
using HexaSave.Domain.Shared;

namespace HexaSave.Domain.Leaderboard
{
    /// <summary>
    /// Pontuação. Não pode ser negativa. Aqui default(Score) = 0 é um valor válido.
    /// </summary>
    public readonly struct Score : IEquatable<Score>, IComparable<Score>
    {
        public static Score Zero => new Score(0);
 
        public int Value { get; }
 
        public Score(int value)
        {
            if (value < 0)
                throw new DomainException("Score não pode ser negativo.");
 
            Value = value;
        }
 
        public bool IsHigherThan(Score other) => Value > other.Value;
 
        public int CompareTo(Score other) => Value.CompareTo(other.Value);
        public bool Equals(Score other) => Value == other.Value;
        public override bool Equals(object obj) => obj is Score other && Equals(other);
        public override int GetHashCode() => Value;
 
        public static bool operator ==(Score left, Score right) => left.Equals(right);
        public static bool operator !=(Score left, Score right) => !left.Equals(right);
        public static bool operator >(Score left, Score right) => left.Value > right.Value;
        public static bool operator <(Score left, Score right) => left.Value < right.Value;
        public static bool operator >=(Score left, Score right) => left.Value >= right.Value;
        public static bool operator <=(Score left, Score right) => left.Value <= right.Value;
 
        public override string ToString() => Value.ToString();
    }
}
