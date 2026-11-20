using System;
using NUnit.Framework;
using UnityEngine;
using HexaSave.Tests.EditMode.Support;
using HexaSave.Adapters.Ugs;
using HexaSave.Domain.Shared;

namespace HexaSave.Tests.EditMode.Adapters
{
    /// <summary>
    /// O repositório do Cloud Save precisa de rede e de um projeto vinculado, então não roda em
    /// EditMode. A parte que pode quebrar sem rede (a conversão) fica no mapper e é testada aqui.
    /// </summary>
    public class CloudProgressMapperTests
    {
        [Test]
        public void ToDto_ThenToDomain_ReturnsEquivalentProgress()
        {
            var original = TestData.Progress(version: 4, at: TestData.T0.AddTicks(1234567), device: TestData.Phone);

            var json = JsonUtility.ToJson(CloudProgressMapper.ToDto(original));
            var restored = CloudProgressMapper.ToDomain(json, TestData.PlayerA);

            Assert.That(restored.PlayerId, Is.EqualTo(original.PlayerId));
            Assert.That(restored.Data, Is.EqualTo(original.Data));
            Assert.That(restored.Version, Is.EqualTo(original.Version));
            Assert.That(restored.UpdatedAt.UtcTicks, Is.EqualTo(original.UpdatedAt.UtcTicks));
            Assert.That(restored.DeviceId, Is.EqualTo(original.DeviceId));
        }

        [Test]
        public void ToDto_WritesTheCurrentSchemaVersion()
        {
            var dto = CloudProgressMapper.ToDto(TestData.Progress());

            Assert.That(dto.schemaVersion, Is.EqualTo(CloudProgressDto.CurrentSchemaVersion));
        }

        [Test]
        public void ToDomain_InvalidJson_ThrowsArgumentException()
        {
            Assert.Throws<ArgumentException>(() => CloudProgressMapper.ToDomain("isto não é JSON", TestData.PlayerA));
        }

        [Test]
        public void ToDomain_UnknownSchemaVersion_ThrowsFormatException()
        {
            var dto = CloudProgressMapper.ToDto(TestData.Progress());
            dto.schemaVersion = 99;

            Assert.Throws<FormatException>(() => CloudProgressMapper.ToDomain(JsonUtility.ToJson(dto), TestData.PlayerA));
        }

        [Test]
        public void ToDomain_ProgressOfAnotherPlayer_ThrowsFormatException()
        {
            var json = JsonUtility.ToJson(CloudProgressMapper.ToDto(TestData.Progress(player: TestData.PlayerB)));

            Assert.Throws<FormatException>(() => CloudProgressMapper.ToDomain(json, TestData.PlayerA));
        }

        [Test]
        public void ToDomain_DataRejectedByTheDomain_ThrowsDomainException()
        {
            var dto = CloudProgressMapper.ToDto(TestData.Progress());
            dto.version = 0; // SaveVersion começa em 1

            Assert.Throws<DomainException>(() => CloudProgressMapper.ToDomain(JsonUtility.ToJson(dto), TestData.PlayerA));
        }
    }
}
