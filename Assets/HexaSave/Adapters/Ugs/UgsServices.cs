using System.Threading.Tasks;
using Unity.Services.Core;
using UnityEngine;

namespace HexaSave.Adapters.Ugs
{
    /// <summary>
    /// Inicializa o Unity Gaming Services uma única vez, sob demanda, para TODOS os adapters UGS
    /// Se a inicialização anterior falhou (ex.: sem rede), a próxima chamada tenta de novo.
    /// </summary>
    internal static class UgsServices
    {
        private static Task _initialization;

        // Todo serviço do UGS exige que UnityServices.InitializeAsync() termine antes do primeiro uso.
        public static Task EnsureInitializedAsync()
        {
            if (UnityServices.State == ServicesInitializationState.Initialized)
                return Task.CompletedTask;

            // Reaproveita uma inicialização em andamento; recomeça se a anterior falhou.
            if (_initialization == null || _initialization.IsFaulted || _initialization.IsCanceled)
                _initialization = UnityServices.InitializeAsync();

            return _initialization;
        }

        /// <summary>
        /// Zera o estado estático ao entrar no Play Mode. Necessário quando o projeto desativa
        /// o "Reload Domain" (Enter Play Mode Options): sem isso, o campo sobreviveria entre sessões.
        /// </summary>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics() => _initialization = null;
    }
}
