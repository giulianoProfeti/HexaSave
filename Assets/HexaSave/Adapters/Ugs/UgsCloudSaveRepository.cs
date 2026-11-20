using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Services.CloudSave;
using Unity.Services.Core;
using UnityEngine;
using HexaSave.Application.Errors;
using HexaSave.Application.Ports;
using HexaSave.Domain.Progress;
using HexaSave.Domain.Shared;

// O SDK tem classes de opções com o mesmo nome no namespace raiz e em Models.Data.Player.
// Apelidos (alias) deixam explícito qual delas usamos, sem ambiguidade.
using PlayerDeleteOptions = Unity.Services.CloudSave.Models.Data.Player.DeleteOptions;
using PlayerLoadOptions = Unity.Services.CloudSave.Models.Data.Player.LoadOptions;
using PlayerSaveOptions = Unity.Services.CloudSave.Models.Data.Player.SaveOptions;

namespace HexaSave.Adapters.Ugs
{
    /// <summary>
    /// Adapter do port ISaveRepository sobre o UGS Cloud Save (dados do jogador).
    /// Guarda o progresso inteiro em uma única chave, o que torna cada gravação atômica.
    /// </summary>
    public sealed class UgsCloudSaveRepository : ISaveRepository
    {
        internal const string ProgressKey = "hexasave_progress";
        private const string CorruptedKeyPrefix = "hexasave_progress_corrupted_";

        public async Task<PlayerProgress> LoadAsync(PlayerId playerId)
        {
            UgsSession.RequireSessionPlayer(playerId);
            string json;

            try
            {
                var items = await CloudSaveService.Instance.Data.Player.LoadAsync(
                    new HashSet<string> { ProgressKey }, new PlayerLoadOptions());

                if (!items.TryGetValue(ProgressKey, out var item))
                    return null; // contrato: jogador sem save não é erro

                // sem 'item.Value.GetAs<CloudProgressDto>()' pois usa o serializador interno do SDK e quebraria as regras de dependencia
                json = item.Value.GetAsString();
            }
            catch (RequestFailedException exception)
            {
                // Inclui CloudSaveException, que herda de RequestFailedException.
                throw new PersistenceUnavailableException("Não foi possível ler o progresso no Cloud Save.", exception);
            }

            try
            {
                return CloudProgressMapper.ToDomain(json, playerId);
            }
            catch (Exception exception) when (exception is ArgumentException
                                              || exception is FormatException
                                              || exception is DomainException)
            {
                await QuarantineAsync(json, exception);
                return null;
            }
        }

        public async Task SaveAsync(PlayerProgress progress)
        {
            if (progress is null)
                throw new ArgumentNullException(nameof(progress));

            UgsSession.RequireSessionPlayer(progress.PlayerId);

            // O SDK converte o objeto em JSON; no Dashboard, o valor aparece com os campos do DTO.
            var data = new Dictionary<string, object> { { ProgressKey, CloudProgressMapper.ToDto(progress) } };

            try
            {
                await CloudSaveService.Instance.Data.Player.SaveAsync(data, new PlayerSaveOptions());
            }
            catch (RequestFailedException exception)
            {
                throw new PersistenceUnavailableException("Não foi possível gravar o progresso no Cloud Save.", exception);
            }
        }

        /// <summary>
        /// Mesma ideia da quarentena do adapter Local: o valor ilegível é MOVIDO para outra chave
        /// (copiado e depois removido da chave principal). Nunca é descartado.
        /// </summary>
        private static async Task QuarantineAsync(string json, Exception reason)
        {
            // Data + trecho aleatório: a lição do bug da Etapa 5 (duas cópias no mesmo segundo não colidem).
            var backupKey = $"{CorruptedKeyPrefix}{DateTime.UtcNow:yyyyMMddHHmmss}_{Guid.NewGuid().ToString("N").Substring(0, 8)}";
            var player = CloudSaveService.Instance.Data.Player;

            try
            {
                // 1º a cópia, depois a remoção: se cair no meio, o pior caso é uma cópia a mais.
                await player.SaveAsync(new Dictionary<string, object> { { backupKey, json } }, new PlayerSaveOptions());
                await player.DeleteAsync(ProgressKey, new PlayerDeleteOptions());
            }
            catch (RequestFailedException exception)
            {
                throw new PersistenceUnavailableException(
                    "O progresso na nuvem está ilegível e não foi possível movê-lo para a quarentena.", exception);
            }

            Debug.LogWarning($"[HexaSave] Progresso ilegível no Cloud Save movido para a chave '{backupKey}'. Motivo: {reason.Message}");
        }
    }
}
