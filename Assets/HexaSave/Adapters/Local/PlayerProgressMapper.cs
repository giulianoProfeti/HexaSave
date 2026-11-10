using System;
using System.Globalization;
using HexaSave.Domain.Progress;
using HexaSave.Domain.Shared;
 
namespace HexaSave.Adapters.Local
{
    /// <summary>
    /// Converte entre a entidade do domínio e o formato do arquivo.
    /// Na leitura, toda a validação do domínio é reaplicada: um arquivo com dados
    /// inválidos gera exceção em vez de produzir uma entidade inválida.
    /// </summary>
    internal static class PlayerProgressMapper
    {
        // "O" é o formato ISO 8601 de ida e volta: preserva todos os dígitos do instante.
        private const string DateFormat = "O";
 
        public static PlayerProgressDto ToDto(PlayerProgress progress) => new()
        {
            schemaVersion = PlayerProgressDto.CurrentSchemaVersion,
            playerId = progress.PlayerId.Value,
            content = progress.Data.Content,
            version = progress.Version.Number,
            updatedAtUtc = progress.UpdatedAt.ToString(DateFormat, CultureInfo.InvariantCulture),
            deviceId = progress.DeviceId.Value,
        };
 
        public static PlayerProgress ToDomain(PlayerProgressDto dto)
        {
            if (dto is null)
                throw new FormatException("O arquivo de save está vazio.");
 
            if (dto.schemaVersion != PlayerProgressDto.CurrentSchemaVersion)
                throw new FormatException($"Versão de formato não suportada: {dto.schemaVersion}.");
 
            var updatedAt = DateTimeOffset.ParseExact(
                dto.updatedAtUtc, DateFormat, CultureInfo.InvariantCulture, DateTimeStyles.None);
 
            return PlayerProgress.Restore(
                new PlayerId(dto.playerId),
                new SaveData(dto.content),
                new SaveVersion(dto.version),
                updatedAt,
                new DeviceId(dto.deviceId));
        }
    }
}