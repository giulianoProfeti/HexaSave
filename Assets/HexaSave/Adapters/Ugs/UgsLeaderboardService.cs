using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Services.Core;
using Unity.Services.Leaderboards;
using Unity.Services.Leaderboards.Exceptions;
using Unity.Services.Leaderboards.Models;
using UnityEngine;
using HexaSave.Application.Errors;
using HexaSave.Application.Ports;
using HexaSave.Domain.Leaderboard;

namespace HexaSave.Adapters.Ugs
{
    /// <summary>
    /// Adapter do port ILeaderboardService sobre o UGS Leaderboards.
    /// O ranking precisa existir no Dashboard com o mesmo ID e a estratégia "Keep best"
    /// (manter a melhor), que é a regra do contrato.
    /// </summary>
    public sealed class UgsLeaderboardService : ILeaderboardService
    {
        public async Task SubmitScoreAsync(LeaderboardId leaderboardId, ScoreEntry entry)
        {
            if (entry.IsDefault)
                throw new ArgumentException("A pontuação não foi inicializada.", nameof(entry));

            // O UGS grava sempre no jogador da sessão: recusamos um PlayerId diferente em vez de ignorá-lo.
            UgsSession.RequireSessionPlayer(entry.PlayerId);

            var options = new AddPlayerScoreOptions { Metadata = LeaderboardEntryMapper.ToMetadata(entry) };

            try
            {
                await LeaderboardsService.Instance.AddPlayerScoreAsync(leaderboardId.Value, entry.Score.Value, options);
            }
            catch (RequestFailedException exception)
            {
                throw Translate(exception, leaderboardId, "enviar a pontuação");
            }
        }

        public async Task<IReadOnlyList<RankedScore>> GetTopScoresAsync(LeaderboardId leaderboardId, int count)
        {
            // O caso de uso de consulta não exige login, mas o UGS exige uma sessão até para ler.
            if (!UgsSession.IsSignedIn)
                throw new PersistenceUnavailableException("O ranking do UGS só pode ser consultado com um jogador autenticado.");

            var options = new GetScoresOptions { Limit = count, IncludeMetadata = true };
            LeaderboardScoresPage page;

            try
            {
                page = await LeaderboardsService.Instance.GetScoresAsync(leaderboardId.Value, options);
            }
            catch (RequestFailedException exception)
            {
                throw Translate(exception, leaderboardId, "consultar o ranking");
            }

            return ToRanking(page?.Results);
        }

        private static IReadOnlyList<RankedScore> ToRanking(List<LeaderboardEntry> results)
        {
            if (results is null || results.Count == 0)
                return Array.Empty<RankedScore>(); // contrato: lista vazia, nunca null

            var ranking = new List<RankedScore>(results.Count);

            for (var index = 0; index < results.Count; index++)
            {
                var row = results[index];

                // A posição vem da ORDEM da lista (a base do campo Rank do SDK não é documentada).
                if (LeaderboardEntryMapper.TryToRankedScore(index + 1, row.PlayerId, row.PlayerName, row.Score,
                                                            row.UpdatedTime, row.Metadata, out var ranked))
                    ranking.Add(ranked);
                else
                    Debug.LogWarning($"[HexaSave] Linha do ranking ignorada (fora das regras do domínio): {row.PlayerId} = {row.Score}.");
            }

            return ranking;
        }

        private static PersistenceUnavailableException Translate(RequestFailedException exception,
                                                                 LeaderboardId leaderboardId, string action)
        {
            // Ranking que não existe no projeto é erro de CONFIGURAÇÃO: a mensagem diz como corrigir.
            if (exception is LeaderboardsException leaderboards
                && leaderboards.Reason == LeaderboardsExceptionReason.LeaderboardNotFound)
                return new PersistenceUnavailableException(
                    $"O ranking '{leaderboardId}' não existe no projeto UGS. Crie-o no Dashboard (Leaderboards) com esse mesmo ID.",
                    exception);

            return new PersistenceUnavailableException(
                $"Não foi possível {action} no UGS Leaderboards (código {exception.ErrorCode}).", exception);
        }
    }
}
