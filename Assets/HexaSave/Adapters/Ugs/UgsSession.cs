using System;
using Unity.Services.Core;
using Unity.Services.Authentication;
using HexaSave.Domain.Shared;
using HexaSave.Application.Errors;

namespace HexaSave.Adapters.Ugs
{
    /// <summary>
    /// Pré-condição comum ao Cloud Save e ao Leaderboards: pelo lado do cliente, o UGS só
    /// lê e grava dados do jogador AUTENTICADO. Pedir outro jogador é erro de uso do adapter.
    /// </summary>
    internal static class UgsSession
    {
        public static void RequireSessionPlayer(PlayerId playerId)
        {
            if (UnityServices.State != ServicesInitializationState.Initialized || !AuthenticationService.Instance.IsSignedIn)
                throw new NotSignedInException("É preciso entrar no Unity Gaming Services antes de acessar os dados do jogador.");

            var sessionPlayer = AuthenticationService.Instance.PlayerId;

            if (!string.Equals(playerId.Value, sessionPlayer, StringComparison.Ordinal))
                throw new ArgumentException(
                    $"O UGS só acessa os dados do jogador autenticado ({sessionPlayer}), e não de {playerId}.",
                    nameof(playerId));
        }
    }
}
