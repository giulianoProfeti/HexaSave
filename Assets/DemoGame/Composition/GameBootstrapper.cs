using UnityEngine;
using HexaSave.Domain.Leaderboard;
using HexaSave.Domain.Shared;
using DemoGame.Gameplay;

namespace DemoGame.Composition
{
    /// <summary>
    /// Composition root na cena. É o primeiro script a rodar: monta o grafo de objetos
    /// e entrega as dependências a cada componente do jogo que precisa delas.
    /// </summary>
    [DefaultExecutionOrder(-1000)]
    public sealed class GameBootstrapper : MonoBehaviour
    {
        [Header("Configuração")]
        [Tooltip("Asset criado em Create > HexaSave > Settings.")]
        [SerializeField] private HexaSaveSettings settings;
 
        [Header("Componentes que recebem dependências")]
        [SerializeField] private HexaSaveSmokeTest smokeTest;
 
        private HexaSaveModule _module;
 
        private void Awake()
        {
            if (settings == null)
            {
                Debug.LogError("[HexaSave] O GameBootstrapper está sem o asset HexaSaveSettings.", this);
                enabled = false;
                return;
            }
 
            try
            {
                var leaderboardId = new LeaderboardId(settings.LeaderboardKey);
                _module = HexaSaveModule.Compose(settings.Provider, leaderboardId);
            }
            catch (DomainException exception)
            {
                Debug.LogError($"[HexaSave] Configuração inválida em '{settings.name}': {exception.Message}", this);
                enabled = false;
                return;
            }
 
            Debug.Log($"[HexaSave] Composição montada com o provedor {settings.Provider}.", this);
 
            if (smokeTest != null)
            {
                smokeTest.Construct(_module.SignIn, _module.SaveProgress, _module.LoadProgress,
                                    _module.SubmitScore, _module.GetTopScores);
            }
            else
            {
                Debug.LogWarning("[HexaSave] Nenhum HexaSaveSmokeTest referenciado no GameBootstrapper.", this);
            }
        }
    }
}