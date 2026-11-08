using System;
using UnityEngine;
using HexaSave.Application.UseCases;
 
namespace DemoGame.Gameplay
{
    /// <summary>
    /// Teste de fumaça da composição: chama todos os casos de uso e escreve o resultado no Console.
    /// Recebe as dependências por injeção de MÉTODO (Construct), porque a Unity cria
    /// os MonoBehaviours sozinha e não permite parâmetros no construtor.
    /// </summary>
    public sealed class HexaSaveSmokeTest : MonoBehaviour
    {
        private SignInUseCase _signIn;
        private SaveProgressUseCase _saveProgress;
        private LoadProgressUseCase _loadProgress;
        private SubmitScoreUseCase _submitScore;
        private GetTopScoresUseCase _getTopScores;
 
        public void Construct(SignInUseCase signIn, SaveProgressUseCase saveProgress,
                              LoadProgressUseCase loadProgress, SubmitScoreUseCase submitScore,
                              GetTopScoresUseCase getTopScores)
        {
            _signIn = signIn ?? throw new ArgumentNullException(nameof(signIn));
            _saveProgress = saveProgress ?? throw new ArgumentNullException(nameof(saveProgress));
            _loadProgress = loadProgress ?? throw new ArgumentNullException(nameof(loadProgress));
            _submitScore = submitScore ?? throw new ArgumentNullException(nameof(submitScore));
            _getTopScores = getTopScores ?? throw new ArgumentNullException(nameof(getTopScores));
        }
 
        private async void Start()
        {
            if (_signIn == null)
            {
                Debug.LogError("[HexaSave] Dependências não injetadas. O GameBootstrapper está na cena " +
                               "com este componente referenciado?", this);
                return;
            }
 
            try
            {
                var playerId = await _signIn.ExecuteAsync();
                Debug.Log($"[HexaSave] 1. Entrou como: {playerId}");
 
                var existing = await _loadProgress.ExecuteAsync();
                Debug.Log(existing is null
                    ? "[HexaSave] 2. Nenhum save encontrado (esperado no primeiro uso)."
                    : $"[HexaSave] 2. Save encontrado: {existing}");
 
                var first = await _saveProgress.ExecuteAsync("{\"nivel\":1,\"moedas\":10}");
                Debug.Log($"[HexaSave] 3. Salvo: {first}");
 
                var second = await _saveProgress.ExecuteAsync("{\"nivel\":2,\"moedas\":25}");
                Debug.Log($"[HexaSave] 4. Salvo de novo: {second}");
 
                await _submitScore.ExecuteAsync(500, "Rafael");
                await _submitScore.ExecuteAsync(300, "Rafael"); // menor: não substitui a melhor
 
                var top = await _getTopScores.ExecuteAsync();
                Debug.Log($"[HexaSave] 5. Ranking com {top.Count} entrada(s):");
                foreach (var row in top)
                    Debug.Log($"[HexaSave]    {row}");
 
                Debug.Log("[HexaSave] Composição OK: todos os casos de uso responderam.");
            }
            catch (Exception exception)
            {
                Debug.LogException(exception, this);
            }
        }
    }
}