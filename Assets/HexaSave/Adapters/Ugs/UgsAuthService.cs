using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Core;
using UnityEngine;
using HexaSave.Application.Errors;
using HexaSave.Application.Ports;
using HexaSave.Domain.Shared;

namespace HexaSave.Adapters.Ugs
{
    /// <summary>
    /// Adapter do port IAuthService sobre o Unity Gaming Services Authentication.
    /// Entra anonimamente: na primeira execução cria um jogador; nas seguintes, o SDK
    /// retoma a sessão guardada no aparelho e devolve o MESMO PlayerId.
    /// </summary>
    public sealed class UgsAuthService : IAuthService
    {
        public bool IsSignedIn => CurrentPlayer.HasValue;

        /// <summary>
        /// Lido diretamente do SDK, que é a fonte da verdade: o adapter não guarda uma cópia.
        /// </summary>
        public PlayerId? CurrentPlayer
        {
            get
            {
                // Antes da inicialização, AuthenticationService.Instance lança exceção.
                if (UnityServices.State != ServicesInitializationState.Initialized)
                    return null;

                var service = AuthenticationService.Instance;

                // O SDK mantém o PlayerId em cache entre sessões: ele só vale com sessão ativa.
                return service.IsSignedIn ? new PlayerId(service.PlayerId) : (PlayerId?)null;
            }
        }

        public async Task<PlayerId> SignInAsync()
        {
            try
            {
                await UgsServices.EnsureInitializedAsync();
                var service = AuthenticationService.Instance;

                if (!service.IsSignedIn)
                {
                    var resumingSession = service.SessionTokenExists;
                    await service.SignInAnonymouslyAsync();

                    Debug.Log(resumingSession
                        ? $"[HexaSave] UGS: sessão retomada para o jogador: {service.PlayerInfo?.Username}({service.PlayerId})."
                        : $"[HexaSave] UGS: novo jogador anônimo criado: {service.PlayerId}.");
                }

                return new PlayerId(service.PlayerId);
            }
            catch (RequestFailedException exception)
            {
                // Inclui AuthenticationException, que herda de RequestFailedException.
                throw new PersistenceUnavailableException(
                    $"Não foi possível entrar no Unity Gaming Services (código {exception.ErrorCode}).", exception);
            }
            catch (ServicesInitializationException exception)
            {
                throw new PersistenceUnavailableException(
                    "O Unity Gaming Services não inicializou. O projeto está vinculado a um projeto do Unity Cloud?",
                    exception);
            }
        }
    }
}
