using UnityEngine;

namespace DemoGame.Composition
{
    /// <summary>
    /// Configuração do HexaSave guardada como asset no projeto.
    /// Trocar o provedor aqui não exige recompilar: basta alterar o asset no Inspector.
    /// </summary>
    [CreateAssetMenu(fileName = "HexaSaveSettings", menuName = "HexaSave/Settings")]
    public sealed class HexaSaveSettings : ScriptableObject
    {
        [Tooltip("Qual conjunto de adapters será injetado nos ports.")]
        [SerializeField] private SaveProvider provider = SaveProvider.InMemory;
 
        [Tooltip("Identificador do ranking usado pelo jogo.")]
        [SerializeField] private string leaderboardKey = "pontuacao-geral";
 
        public SaveProvider Provider => provider;
        public string LeaderboardKey => leaderboardKey;
    }
}
