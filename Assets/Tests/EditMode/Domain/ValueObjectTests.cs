using System;
using HexaSave.Domain.Leaderboard;
using HexaSave.Domain.Progress;
using HexaSave.Domain.Shared;
using HexaSave.Tests.EditMode.Support;
using NUnit.Framework;

namespace HexaSave.Tests.EditMode.Domain
{
    [TestFixture]
    public sealed class ValueObjectTests
    {
        // ---------- PlayerId ----------

        [TestCase(null)]
        [TestCase("")]
        [TestCase("   ")]
        public void PlayerId_EmptyValue_IsRejected(string value)
        {
            Assert.Throws<DomainException>(() => new PlayerId(value));
        }

        [Test]
        public void PlayerId_SameText_AreEqualWithSameHashCode()
        {
            var first = new PlayerId("abc");
            var second = new PlayerId("abc");

            Assert.That(first == second, Is.True);
            Assert.That(first.Equals(second), Is.True);
            Assert.That(first.GetHashCode(), Is.EqualTo(second.GetHashCode()));
        }

        [Test]
        public void PlayerId_DifferentCase_AreDifferent()
        {
            Assert.That(new PlayerId("Player1") != new PlayerId("player1"), Is.True);
        }

        [Test]
        public void PlayerId_Default_IsDetected()
        {
            Assert.That(default(PlayerId).IsDefault, Is.True);
            Assert.That(new PlayerId("abc").IsDefault, Is.False);
        }

        // ---------- SaveData ----------

        [TestCase(null)]
        [TestCase("")]
        public void SaveData_EmptyContent_IsRejected(string content)
        {
            Assert.Throws<DomainException>(() => new SaveData(content));
        }

        // ---------- SaveVersion ----------

        [TestCase(0)]
        [TestCase(-1)]
        public void SaveVersion_BelowOne_IsRejected(int number)
        {
            Assert.Throws<DomainException>(() => new SaveVersion(number));
        }

        [Test]
        public void SaveVersion_Next_ReturnsNewVersionAndKeepsOriginal()
        {
            var original = new SaveVersion(3);

            var next = original.Next();

            Assert.That(next.Number, Is.EqualTo(4));
            Assert.That(original.Number, Is.EqualTo(3));
            Assert.That(next > original, Is.True);
            Assert.That(next.IsNewerThan(original), Is.True);
        }

        // ---------- Score ----------

        [Test]
        public void Score_Negative_IsRejected()
        {
            Assert.Throws<DomainException>(() => new Score(-1));
        }

        [Test]
        public void Score_Zero_IsValidAndEqualsDefault()
        {
            Assert.That(new Score(0), Is.EqualTo(Score.Zero));
            Assert.That(default(Score), Is.EqualTo(Score.Zero));
        }

        // ---------- DisplayName ----------

        [TestCase("ab")]
        [TestCase("123456789012345678901")]
        [TestCase("  ab  ")]
        [TestCase("   ")]
        public void DisplayName_InvalidLength_IsRejected(string value)
        {
            Assert.Throws<DomainException>(() => new DisplayName(value));
        }

        [TestCase("abc")]
        [TestCase("12345678901234567890")]
        public void DisplayName_LengthLimits_AreAccepted(string value)
        {
            Assert.That(new DisplayName(value).Value, Is.EqualTo(value));
        }

        [Test]
        public void DisplayName_SurroundingSpaces_AreTrimmed()
        {
            Assert.That(new DisplayName("  Ana  ").Value, Is.EqualTo("Ana"));
        }

        // ---------- ScoreEntry ----------

        [Test]
        public void ScoreEntry_SameInstantWithDifferentOffsets_AreEqual()
        {
            var inBrasilia = new DateTimeOffset(2026, 11, 2, 9, 0, 0, TimeSpan.FromHours(-3));
            var inUtc = new DateTimeOffset(2026, 11, 2, 12, 0, 0, TimeSpan.Zero);

            var first = TestData.Entry(TestData.PlayerA, 100, at: inBrasilia);
            var second = TestData.Entry(TestData.PlayerA, 100, at: inUtc);

            Assert.That(first, Is.EqualTo(second));
            Assert.That(first.AchievedAt.Offset, Is.EqualTo(TimeSpan.Zero));
        }

        [Test]
        public void ScoreEntry_DefaultPlayer_IsRejected()
        {
            Assert.Throws<DomainException>(
                () => new ScoreEntry(default, new DisplayName("Ana"), new Score(1), TestData.T0));
        }
    }
}
