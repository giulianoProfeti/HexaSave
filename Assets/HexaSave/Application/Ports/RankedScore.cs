using System;
using HexaSave.Domain.Leaderboard;
 
namespace HexaSave.Application.Ports
{
    /// <summary>
    /// Uma linha do ranking: a posição (calculada pelo provedor) e a pontuação.
    /// Fica na Application, e não no Domain, porque é o RESULTADO de uma consulta,
    /// não uma regra de negócio.
    /// </summary>
    public readonly struct RankedScore : IEquatable<RankedScore>
    {
        public int Rank { get; }
        public ScoreEntry Entry { get; }
 
        public RankedScore(int rank, ScoreEntry entry)
        {
            if (rank < 1)
                throw new ArgumentOutOfRangeException(nameof(rank), "A posição no ranking começa em 1.");
            if (entry.IsDefault)
                throw new ArgumentException("A pontuação não foi inicializada.", nameof(entry));
 
            Rank = rank;
            Entry = entry;
        }
 
        public bool Equals(RankedScore other) => Rank == other.Rank && Entry == other.Entry;
        public override bool Equals(object obj) => obj is RankedScore other && Equals(other);
        public override int GetHashCode() => HashCode.Combine(Rank, Entry);
 
        public static bool operator ==(RankedScore left, RankedScore right) => left.Equals(right);
        public static bool operator !=(RankedScore left, RankedScore right) => !left.Equals(right);
 
        public override string ToString() => $"#{Rank} {Entry}";
    }
}