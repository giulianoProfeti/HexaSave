using System;
using NUnit.Framework;
using UnityEngine;
using HexaSave.Adapters.Ugs;
using HexaSave.Domain.Leaderboard;
using HexaSave.Tests.EditMode.Support;

namespace HexaSave.Tests.EditMode.Adapters
{
    /// <summary>
    /// Conversões do adapter de Leaderboards do UGS, testadas sem rede e sem o SDK:
    /// o mapper recebe valores simples, exatamente os campos de uma linha do UGS.
    /// </summary>
    public class LeaderboardEntryMapperTests
    {
        private static readonly DateTime UpdatedUtc = new DateTime(2026, 11, 17, 12, 0, 0, DateTimeKind.Utc);

        private static string Metadata(string name) => JsonUtility.ToJson(new ScoreMetadataDto { displayName = name });

        [Test]
        public void ToMetadata_CarriesTheDisplayName()
        {
            var metadata = LeaderboardEntryMapper.ToMetadata(TestData.Entry(TestData.PlayerA, 100, name: "Rafael"));

            Assert.That(metadata.displayName, Is.EqualTo("Rafael"));
        }

        [Test]
        public void TryToRankedScore_ValidRow_KeepsPlayerScoreRankAndName()
        {
            var ok = LeaderboardEntryMapper.TryToRankedScore(2, "player-a", "Brave#1234", 750, UpdatedUtc,
                                                             Metadata("Rafael"), out var ranked);

            Assert.That(ok, Is.True);
            Assert.That(ranked.Rank, Is.EqualTo(2));
            Assert.That(ranked.Entry.PlayerId, Is.EqualTo(TestData.PlayerA));
            Assert.That(ranked.Entry.Score.Value, Is.EqualTo(750));
            Assert.That(ranked.Entry.DisplayName, Is.EqualTo(new DisplayName("Rafael")));
        }

        [TestCase(null)]
        [TestCase("{ilegível")]
        [TestCase("{\"displayName\":\"x\"}")]   // nome curto demais para o domínio
        public void TryToRankedScore_MetadataMissingOrInvalid_UsesTheUgsPlayerName(string metadata)
        {
            LeaderboardEntryMapper.TryToRankedScore(1, "player-a", "Brave#1234", 10, UpdatedUtc, metadata, out var ranked);

            Assert.That(ranked.Entry.DisplayName, Is.EqualTo(new DisplayName("Brave#1234")));
        }

        [Test]
        public void TryToRankedScore_NoValidName_UsesTheFallbackName()
        {
            var tooLong = new string('x', DisplayName.MaxLength + 1);

            LeaderboardEntryMapper.TryToRankedScore(1, "player-a", tooLong, 10, UpdatedUtc, null, out var ranked);

            Assert.That(ranked.Entry.DisplayName, Is.EqualTo(new DisplayName(LeaderboardEntryMapper.FallbackName)));
        }

        [Test]
        public void TryToRankedScore_ConvertsTheServerTimeToUtc()
        {
            var unspecified = new DateTime(2026, 11, 17, 12, 0, 0, DateTimeKind.Unspecified);

            LeaderboardEntryMapper.TryToRankedScore(1, "player-a", "Brave#1234", 10, unspecified, null, out var ranked);

            Assert.That(ranked.Entry.AchievedAt, Is.EqualTo(new DateTimeOffset(UpdatedUtc)));
            Assert.That(ranked.Entry.AchievedAt.Offset, Is.EqualTo(TimeSpan.Zero));
        }

        [TestCase(-1d)]
        [TestCase(3e9)]
        [TestCase(double.NaN)]
        public void TryToRankedScore_ScoreOutsideTheDomain_IsRejected(double score)
        {
            var ok = LeaderboardEntryMapper.TryToRankedScore(1, "player-a", "Brave#1234", score, UpdatedUtc, null, out _);

            Assert.That(ok, Is.False);
        }
    }
}
