using System;
using System.Threading.Tasks;
using Unity.Services.Core;
using Unity.Services.Authentication;
using UnityEngine;
using HexaSave.Application.Ports;
using HexaSave.Application.Errors;
using HexaSave.Domain.Shared;

namespace HexaSave.Adapters.Ugs
{
    /// <summary>
    /// Adapter do port IAccountService sobre o provedor "usuário e senha" do UGS Authentication.
    /// Vincular mantém o PlayerId do convidado; entrar troca a sessão para a conta informada.
    /// </summary>
    public sealed class UgsAccountService : IAccountService
    {
        private const string CredentialRules =
            "Usuário: 3 a 20 caracteres (letras, números e . - @ _). " +
            "Senha: 8 a 30 caracteres, com letra maiúscula, letra minúscula, número e símbolo.";

        public async Task<AccountStatus> GetStatusAsync()
        {
            try
            {
                await UgsServices.EnsureInitializedAsync();
                var service = AuthenticationService.Instance;

                if (!service.IsSignedIn)
                    return AccountStatus.Guest;

                var username = service.PlayerInfo?.Username;

                // Ao retomar a sessão pelo token salvo, o SDK pode não trazer o Username: pergunta ao servidor.
                if (string.IsNullOrEmpty(username))
                    username = (await service.GetPlayerInfoAsync())?.Username;

                return string.IsNullOrEmpty(username) ? AccountStatus.Guest : AccountStatus.Registered(username);
            }
            catch (RequestFailedException exception)
            {
                throw Unavailable("consultar a conta", exception);
            }
            catch (ServicesInitializationException exception)
            {
                throw Unavailable("consultar a conta", exception);
            }
        }

        public async Task LinkUsernamePasswordAsync(AccountCredentials credentials)
        {
            try
            {
                await UgsServices.EnsureInitializedAsync();
                var service = AuthenticationService.Instance;

                await service.AddUsernamePasswordAsync(credentials.Username, credentials.Password);
                Debug.Log($"[HexaSave] UGS: conta '{credentials.Username}' vinculada ao jogador {service.PlayerId}.");
            }
            catch (RequestFailedException exception)
            {
                throw Translate(exception, "vincular a conta");
            }
            catch (ServicesInitializationException exception)
            {
                throw Unavailable("vincular a conta", exception);
            }
        }

        public async Task<PlayerId> SignInWithUsernamePasswordAsync(AccountCredentials credentials)
        {
            try
            {
                await UgsServices.EnsureInitializedAsync();
            }
            catch (ServicesInitializationException exception)
            {
                throw Unavailable("entrar na conta", exception);
            }

            var service = AuthenticationService.Instance;
            var hadSession = service.IsSignedIn;

            // O SDK não permite entrar com a sessão aberta. SignOut() sem apagar credenciais
            // mantém o token salvo, o que permite voltar à sessão anterior se o login falhar.
            if (hadSession)
                service.SignOut(clearCredentials: false);
            try
            {
                await service.SignInWithUsernamePasswordAsync(credentials.Username, credentials.Password);
                Debug.Log($"[HexaSave] UGS: entrou na conta '{credentials.Username}' (jogador {service.PlayerId}).");
                return new PlayerId(service.PlayerId);
            }
            catch (RequestFailedException exception)
            {
                var translated = Translate(exception, "entrar na conta");
                if (hadSession)
                    await RestorePreviousSessionAsync(service);

                throw translated;
            }
        }

        private static async Task RestorePreviousSessionAsync(IAuthenticationService service)
        {
            try
            {
                await service.SignInAnonymouslyAsync(); // retoma a sessão anterior pelo token salvo
                Debug.Log($"[HexaSave] UGS: sessão anterior restaurada (jogador: {service.PlayerInfo?.Username} {service.PlayerId}).");
            }
            catch (RequestFailedException exception)
            {
                Debug.LogWarning($"[HexaSave] UGS: não foi possível restaurar a sessão anterior: {exception.Message}");
            }
        }

        /// <summary>
        /// Traduz os códigos do SDK para o contrato. Os códigos de AuthenticationErrorCodes são
        /// "static readonly", e não "const", por isso a comparação usa if em vez de switch.
        /// </summary>
        private static Exception Translate(RequestFailedException exception, string action)
        {
            var code = exception.ErrorCode;

            if (code == CommonErrorCodes.TransportError || code == CommonErrorCodes.Timeout ||
                code == CommonErrorCodes.ServiceUnavailable || code == CommonErrorCodes.TooManyRequests)
            {
                return Unavailable(action, exception);
            }

            if (code == AuthenticationErrorCodes.AccountAlreadyLinked)
            {
                //Debug.LogWarning($"[HexaSave] UGS: Este nome de usuário já está em uso, ou este jogador já tem uma conta: {exception.Message}");
                return new AccountException(AccountError.UsernameUnavailable,
                    "Este nome de usuário já está em uso, ou este jogador já tem uma conta.", exception);
            }

            if (code == AuthenticationErrorCodes.InvalidParameters)
            {
                //Debug.LogWarning($"[HexaSave] UGS: Usuário ou senha inválidos: {exception.Message}");
                return new AccountException(AccountError.InvalidCredentials,
                    "Usuário ou senha inválidos. " + CredentialRules, exception);
            }

            //Debug.LogWarning($"[HexaSave] UGS: O serviço recusou a operação (código {code}): {exception.Message}");
            return new AccountException(AccountError.Rejected,
                $"O serviço recusou a operação (código {code}): {exception.Message}", exception);
        }

        private static PersistenceUnavailableException Unavailable(string action, Exception exception)
            => new PersistenceUnavailableException($"Não foi possível {action} no Unity Gaming Services.", exception);
    }
}
