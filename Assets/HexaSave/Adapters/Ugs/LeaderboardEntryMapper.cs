using System;
using UnityEngine;
using HexaSave.Application.Ports;
using HexaSave.Domain.Leaderboard;
using HexaSave.Domain.Shared;

namespace HexaSave.Adapters.Ugs
{
    /// <summary>
    /// Converte entre o domínio e as linhas do UGS Leaderboards. Recebe valores simples
    /// (e não as classes do SDK) para ser testável sem o pacote do Leaderboards.
    /// </summary>
    internal static class LeaderboardEntryMapper
    {
        /// <summary>Nome usado quando nem o metadata nem o PlayerName do UGS são válidos.</summary>
        internal const string FallbackName = "Jogador";

        public static ScoreMetadataDto ToMetadata(ScoreEntry entry) =>
            new ScoreMetadataDto { displayName = entry.DisplayName.Value };

        /// <summary>
        /// Monta uma linha do ranking. Devolve false quando a linha não cabe nas regras do
        /// domínio (ex.: pontuação negativa inserida pelo Dashboard): ela é omitida.
        /// </summary>
        /// <param name="rank">Posição calculada pela ordem da lista (1 = primeiro).</param>
        public static bool TryToRankedScore(int rank, string playerId, string playerName, double score,
                                            DateTime updatedTime, string metadataJson, out RankedScore ranked)
        {
            ranked = default;

            // O jogo envia inteiros; um valor fora da faixa do Score não é uma pontuação nossa.
            if (double.IsNaN(score) || score < 0 || score > int.MaxValue || string.IsNullOrEmpty(playerId))
                return false;

            var entry = new ScoreEntry(
                new PlayerId(playerId),
                ChooseDisplayName(metadataJson, playerName),
                new Score((int)Math.Round(score)),
                new DateTimeOffset(AsUtc(updatedTime)));

            ranked = new RankedScore(rank, entry);
            return true;
        }

        /// <summary>1º o nome do metadata; 2º o PlayerName do UGS; 3º um nome padrão.</summary>
        private static DisplayName ChooseDisplayName(string metadataJson, string playerName)
        {
            if (TryCreateName(ReadMetadataName(metadataJson), out var fromMetadata))
                return fromMetadata;

            if (TryCreateName(playerName, out var fromUgs))
                return fromUgs;

            return new DisplayName(FallbackName);
        }

        private static string ReadMetadataName(string metadataJson)
        {
            if (string.IsNullOrWhiteSpace(metadataJson))
                return null;

            try
            {
                return JsonUtility.FromJson<ScoreMetadataDto>(metadataJson)?.displayName;
            }
            catch (ArgumentException)
            {
                return null; // metadata ilegível: tenta o próximo nome
            }
        }

        // As regras de nome continuam só no domínio: aqui apenas perguntamos se o valor passa.
        private static bool TryCreateName(string value, out DisplayName name)
        {
            name = default;
            if (string.IsNullOrWhiteSpace(value))
                return false;

            try
            {
                name = new DisplayName(value);
                return true;
            }
            catch (DomainException)
            {
                return false;
            }
        }

        // O SDK entrega DateTime; o domínio usa DateTimeOffset em UTC.
        private static DateTime AsUtc(DateTime time) =>
            time.Kind == DateTimeKind.Local ? time.ToUniversalTime() : DateTime.SpecifyKind(time, DateTimeKind.Utc);
    }
}
