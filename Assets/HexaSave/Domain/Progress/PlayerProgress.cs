using System;
using HexaSave.Domain.Shared;

namespace HexaSave.Domain.Progress
{
    /// <summary>
    /// PlayerProgress é uma entidade responsável pelo progresso salvo de um jogador.
    /// É uma classe (e não struct) porque entidades têm identidade e não devem
    /// ser copiadas campo a campo nem existir em um estado default inválido.
    /// É imutável: Update() devolve um NOVO PlayerProgress.
    /// </summary>
    public sealed class PlayerProgress
    {
        public PlayerId PlayerId {get;}
        public SaveData Data { get; }
        public SaveVersion Version { get; }
        public DateTimeOffset UpdatedAt { get; }
        public DeviceId DeviceId { get; }

        /// <summary>
        /// O construtor é privado para respeitar o isolamento e a regra de só se criar um PlayerProgress pelas fábricas abaixo.
        /// </summary>
        private PlayerProgress(PlayerId playerId, SaveData data, SaveVersion version,
                               DateTimeOffset updatedAt, DeviceId deviceId)
        {
            Guard.AgainstDefault(playerId.IsDefault, nameof(playerId));
            Guard.AgainstDefault(data.IsDefault, nameof(data));
            Guard.AgainstDefault(version.IsDefault, nameof(version));
            Guard.AgainstDefault(deviceId.IsDefault, nameof(deviceId));
 
            PlayerId = playerId;
            Data = data;
            Version = version;
            UpdatedAt = updatedAt.ToUniversalTime();
            DeviceId = deviceId;

        }

        /// <summary>
        /// Primeiro save de um jogador: sempre começa na versão 1.
        /// </summary>
        public static PlayerProgress CreateNew(PlayerId playerId, SaveData data, DateTimeOffset now, DeviceId deviceId)
        {
            return new PlayerProgress(playerId, data, SaveVersion.Initial, now, deviceId);
        }

        /// <summary>
        /// Reconstrói um progresso que já existia (lido do disco, da nuvem ou da API).
        /// Usado pelos adapters, que precisam recriar a entidade com a versão já gravada.
        /// </summary>
        public static PlayerProgress Restore(PlayerId playerId, SaveData data, SaveVersion version,
                               DateTimeOffset updatedAt, DeviceId deviceId)
        {
            return new PlayerProgress(playerId, data, version, updatedAt, deviceId);
        }

        /// <summary>
        /// Nova gravação: mesmo jogador, novo conteúdo, versão + 1.
        /// Não valida se "now" é posterior a UpdatedAt, porque relógios de
        /// dispositivos diferentes podem divergir; conflitos são tratados por versão (R15).
        /// </summary>
        public PlayerProgress Update(SaveData newData, DateTimeOffset now, DeviceId deviceId)
        {
            return new PlayerProgress(PlayerId, newData, Version.Next(), now, deviceId);
        }

        /// <summary>
        /// Entidades se comparam por identidade. Como cada Update() gera um novo objeto,
        /// sobrescrever Equals faria o original e o atualizado parecerem "iguais";
        /// por isso a comparação de identidade é explícita.
        /// </summary>
        public bool HasSameIdentityAs(PlayerProgress other) => other != null && PlayerId == other.PlayerId;

        public override string ToString() => $"PlayerProgress({PlayerId}, {Version}, {UpdatedAt:O}, {DeviceId})";
    }
}
