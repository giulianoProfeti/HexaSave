using System;
using HexaSave.Domain.Shared;

namespace HexaSave.Domain.Leaderboard
{
    /// <summary>
    /// Fato imutável: "o jogador X, com o nome público Y, fez Z pontos no instante T".
    /// É um Value Object composto: uma nova pontuação é sempre um novo ScoreEntry.
    /// </summary>
    public readonly struct ScoreEntry : IEquatable<ScoreEntry>
    {
        public PlayerId PlayerId { get; }
        public DisplayName DisplayName { get; }
        public Score Score { get; }
        public DateTimeOffset AchievedAt { get; }

        /// <summary>
        /// default(ScoreEntry) não passa pelo construtor e fica sem jogador.
        /// </summary>
        public bool IsDefault => PlayerId.IsDefault;
 
        public ScoreEntry(PlayerId playerId, DisplayName displayName, Score score, DateTimeOffset achievedAt)
        {
            Guard.AgainstDefault(playerId.IsDefault, nameof(playerId));
            Guard.AgainstDefault(displayName.IsDefault, nameof(displayName));
 
            PlayerId = playerId;
            DisplayName = displayName;
            Score = score;
            AchievedAt = achievedAt.ToUniversalTime();
        }
 
        public bool Equals(ScoreEntry other)
            => PlayerId == other.PlayerId
            && DisplayName == other.DisplayName
            && Score == other.Score
            && AchievedAt.Equals(other.AchievedAt);
 
        public override bool Equals(object obj) => obj is ScoreEntry other && Equals(other);
        public override int GetHashCode() => HashCode.Combine(PlayerId, DisplayName, Score, AchievedAt);
 
        public static bool operator ==(ScoreEntry left, ScoreEntry right) => left.Equals(right);
        public static bool operator !=(ScoreEntry left, ScoreEntry right) => !left.Equals(right);
 
        public override string ToString() => $"{DisplayName}: {Score} ({AchievedAt:O})";
    }
}
