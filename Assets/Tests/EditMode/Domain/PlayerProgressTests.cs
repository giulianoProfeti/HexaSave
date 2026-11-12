using System;
using NUnit.Framework;
using HexaSave.Domain.Progress;
using HexaSave.Domain.Shared;
using HexaSave.Tests.EditMode.Support;

namespace HexaSave.Tests.EditMode.Domain
{
    [TestFixture]
    public sealed class PlayerProgressTests
    {
        [Test]
        public void CreateNew_StartsAtVersionOne()
        {
            var progress = PlayerProgress.CreateNew(TestData.PlayerA, new SaveData(TestData.Content),
                                                    TestData.T0, TestData.Pc);

            Assert.That(progress.Version.Number, Is.EqualTo(1));
        }

        [Test]
        public void CreateNew_StoresUpdatedAtInUtc()
        {
            var inBrasilia = new DateTimeOffset(2026, 11, 2, 9, 0, 0, TimeSpan.FromHours(-3));

            var progress = PlayerProgress.CreateNew(TestData.PlayerA, new SaveData(TestData.Content),
                                                    inBrasilia, TestData.Pc);

            Assert.That(progress.UpdatedAt.Offset, Is.EqualTo(TimeSpan.Zero));
            Assert.That(progress.UpdatedAt, Is.EqualTo(inBrasilia)); // mesmo instante
        }

        [Test]
        public void Update_IncrementsVersionByOne()
        {
            var original = TestData.Progress(version: 7);

            var updated = original.Update(new SaveData("{\"nivel\":2}"), TestData.T0.AddMinutes(1), TestData.Phone);

            Assert.That(updated.Version.Number, Is.EqualTo(8));
        }

        [Test]
        public void Update_KeepsIdentityAndReplacesData()
        {
            var original = TestData.Progress();
            var later = TestData.T0.AddMinutes(5);

            var updated = original.Update(new SaveData("{\"nivel\":2}"), later, TestData.Phone);

            Assert.That(updated.PlayerId, Is.EqualTo(original.PlayerId));
            Assert.That(updated.Data.Content, Is.EqualTo("{\"nivel\":2}"));
            Assert.That(updated.UpdatedAt, Is.EqualTo(later));
            Assert.That(updated.DeviceId, Is.EqualTo(TestData.Phone));
        }

        [Test]
        public void Update_DoesNotChangeOriginal()
        {
            var original = TestData.Progress();

            var updated = original.Update(new SaveData("{\"nivel\":2}"), TestData.T0.AddMinutes(1), TestData.Phone);

            Assert.That(updated, Is.Not.SameAs(original));
            Assert.That(original.Version.Number, Is.EqualTo(1));
            Assert.That(original.Data.Content, Is.EqualTo(TestData.Content));
            Assert.That(original.DeviceId, Is.EqualTo(TestData.Pc));
        }

        [Test]
        public void Restore_KeepsStoredVersion()
        {
            var restored = PlayerProgress.Restore(TestData.PlayerA, new SaveData(TestData.Content),
                                                  new SaveVersion(42), TestData.T0, TestData.Pc);

            Assert.That(restored.Version.Number, Is.EqualTo(42));
        }

        [Test]
        public void CreateNew_WithDefaultValueObjects_IsRejected()
        {
            Assert.Throws<DomainException>(() => PlayerProgress.CreateNew(
                default, new SaveData(TestData.Content), TestData.T0, TestData.Pc));

            Assert.Throws<DomainException>(() => PlayerProgress.CreateNew(
                TestData.PlayerA, default, TestData.T0, TestData.Pc));

            Assert.Throws<DomainException>(() => PlayerProgress.CreateNew(
                TestData.PlayerA, new SaveData(TestData.Content), TestData.T0, default));
        }

        [Test]
        public void HasSameIdentityAs_ComparesOnlyThePlayer()
        {
            var original = TestData.Progress();
            var updated = original.Update(new SaveData("x"), TestData.T0, TestData.Pc);
            var otherPlayer = TestData.Progress(player: TestData.PlayerB);

            Assert.That(updated.HasSameIdentityAs(original), Is.True);
            Assert.That(otherPlayer.HasSameIdentityAs(original), Is.False);
            Assert.That(original.HasSameIdentityAs(null), Is.False);
        }
    }
}
