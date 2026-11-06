using System;
using HexaSave.Domain.Shared;

namespace HexaSave.Domain.Leaderboard
{
    public readonly struct LeaderboardId : IEquatable<LeaderboardId>
    {
        public string Value { get; }
        public bool IsDefault => Value is null;

        public LeaderboardId(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new DomainException("LeaderboardId não pode ser vazio.");
 
            Value = value;
        }
        
        public bool Equals(LeaderboardId other) => string.Equals(Value, other.Value, StringComparison.Ordinal);
        public override bool Equals(object obj) => obj is LeaderboardId other && Equals(other);
        public override int GetHashCode() => Value is null ? 0 : StringComparer.Ordinal.GetHashCode(Value);
 
        public static bool operator ==(LeaderboardId left, LeaderboardId right) => left.Equals(right);
        public static bool operator !=(LeaderboardId left, LeaderboardId right) => !left.Equals(right);
 
        public override string ToString() => Value ?? "<default>";
    }
}
