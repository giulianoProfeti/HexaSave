using System;
using System.Globalization;
using UnityEngine;
using HexaSave.Domain.Progress;
using HexaSave.Domain.Shared;

namespace HexaSave.Adapters.Ugs
{
    /// <summary>
    /// Converte entre a entidade do domínio e o JSON guardado no Cloud Save.
    /// Fica separado do repositório para poder ser testado sem o SDK e sem rede
    /// (o repositório fica "humilde": só conversa com o SDK).
    /// </summary>
    internal static class CloudProgressMapper
    {
        private const string DateFormat = "O";

        public static CloudProgressDto ToDto(PlayerProgress progress) => new CloudProgressDto
        {
            schemaVersion = CloudProgressDto.CurrentSchemaVersion,
            playerId = progress.PlayerId.Value,
            content = progress.Data.Content,
            version = progress.Version.Number,
            updatedAtUtc = progress.UpdatedAt.ToString(DateFormat, CultureInfo.InvariantCulture),
            deviceId = progress.DeviceId.Value,
        };

        /// <exception cref="ArgumentException">JSON ilegível.</exception>
        /// <exception cref="FormatException">Formato desconhecido, data inválida ou dono diferente.</exception>
        /// <exception cref="DomainException">Dados recusados pelas regras do domínio.</exception>
        public static PlayerProgress ToDomain(string json, PlayerId expectedPlayer)
        {
            var dto = JsonUtility.FromJson<CloudProgressDto>(json);

            if (dto is null)
                throw new FormatException("O progresso na nuvem está vazio.");

            if (dto.schemaVersion != CloudProgressDto.CurrentSchemaVersion)
                throw new FormatException($"Versão de formato não suportada: {dto.schemaVersion}.");

            var updatedAt = DateTimeOffset.ParseExact(dto.updatedAtUtc, DateFormat,
                                                      CultureInfo.InvariantCulture, DateTimeStyles.None);

            var progress = PlayerProgress.Restore(
                new PlayerId(dto.playerId),
                new SaveData(dto.content),
                new SaveVersion(dto.version),
                updatedAt,
                new DeviceId(dto.deviceId));

            if (progress.PlayerId != expectedPlayer)
                throw new FormatException("O progresso na nuvem pertence a outro jogador.");

            return progress;
        }
    }
}
