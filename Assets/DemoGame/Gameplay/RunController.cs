using System;
using System.Collections.Generic;
using UnityEngine;

namespace DemoGame.Gameplay
{
    /// <summary>
    /// Regras de uma partida: cria blocos e moedas, aumenta a dificuldade,
    /// detecta colisões, conta pontos e avisa quando a partida termina.
    /// Não sabe nada sobre salvar: só entrega um RunResult.
    /// </summary>
    public sealed class RunController : MonoBehaviour
    {
        [Header("Referências")]
        [SerializeField] private PlayerMover player;
        [SerializeField] private FallingObject hazardPrefab;
        [SerializeField] private FallingObject coinPrefab;

        [Header("Dificuldade")]
        [SerializeField] private float startSpawnInterval = 0.8f;
        [SerializeField] private float minSpawnInterval = 0.25f;
        [SerializeField] private float secondsToMaxDifficulty = 60f;
        [SerializeField] private float startFallSpeed = 3f;
        [SerializeField] private float fallSpeedGainPerSecond = 0.12f;
        [Range(0f, 1f)]
        [SerializeField] private float coinChance = 0.3f;

        [Header("Pontuação")]
        [SerializeField] private int pointsPerSecond = 10;
        [SerializeField] private int pointsPerCoin = 50;

        private readonly List<FallingObject> _active = new List<FallingObject>();
        private Camera _camera;
        private float _elapsed;
        private float _spawnTimer;
        private int _coins;
        private int _shields;
        private int _lastScore = -1, _lastCoins = -1, _lastShields = -1;

        /// <summary>Pontos, moedas e escudos atuais; disparado só quando algum valor muda.</summary>
        public event Action<int, int, int> HudChanged;

        public event Action<RunResult> RunEnded;

        public bool IsRunning { get; private set; }

        private int Score => Mathf.FloorToInt(_elapsed * pointsPerSecond) + _coins * pointsPerCoin;

        private void Awake()
        {
            _camera = Camera.main;
        }

        public void StartRun(int shields)
        {
            ClearObjects();
            _elapsed = 0f;
            _spawnTimer = 0f;
            _coins = 0;
            _shields = shields;
            _lastScore = _lastCoins = _lastShields = -1;

            player.ResetPosition();
            player.SetControlEnabled(true);
            IsRunning = true;
            ReportHud();
        }

        private void Update()
        {
            if (!IsRunning)
                return;

            _elapsed += Time.deltaTime;
            SpawnIfDue();
            ResolveObjects();

            if (IsRunning)
                ReportHud();
        }

        private void SpawnIfDue()
        {
            _spawnTimer -= Time.deltaTime;
            if (_spawnTimer > 0f)
                return;

            // Dificuldade de 0 a 1: o intervalo entre objetos diminui com o tempo.
            var difficulty = Mathf.Clamp01(_elapsed / secondsToMaxDifficulty);
            _spawnTimer = Mathf.Lerp(startSpawnInterval, minSpawnInterval, difficulty);

            var prefab = UnityEngine.Random.value < coinChance ? coinPrefab : hazardPrefab;
            var half = CameraBounds.HalfSize(_camera);
            var x = UnityEngine.Random.Range(-half.x + prefab.Radius, half.x - prefab.Radius);
            var spawnPosition = new Vector3(x, half.y + prefab.Radius, 0f);

            var spawned = Instantiate(prefab, spawnPosition, Quaternion.identity, transform);
            spawned.Launch(startFallSpeed + fallSpeedGainPerSecond * _elapsed);
            _active.Add(spawned);
        }

        private void ResolveObjects()
        {
            var bottom = -CameraBounds.HalfSize(_camera).y;
            Vector2 playerPosition = player.transform.position;

            // De trás para frente: assim dá para remover itens da lista dentro do laço.
            for (var i = _active.Count - 1; i >= 0; i--)
            {
                var item = _active[i];
                Vector2 itemPosition = item.transform.position;

                var touched = Vector2.Distance(itemPosition, playerPosition) <= item.Radius + player.Radius;

                if (touched)
                {
                    DestroyAt(i);

                    if (item.Kind == FallingObjectKind.Coin)
                    {
                        _coins++;
                    }
                    else if (_shields > 0)
                    {
                        _shields--;
                        player.ShowShieldHit();
                    }
                    else
                    {
                        EndRun();
                        return;
                    }
                }
                else if (itemPosition.y < bottom - item.Radius)
                {
                    DestroyAt(i); // saiu da tela por baixo
                }
            }
        }

        private void EndRun()
        {
            IsRunning = false;
            player.SetControlEnabled(false);

            var result = new RunResult(Score, _coins, _elapsed);
            ClearObjects();
            RunEnded?.Invoke(result);
        }

        private void ReportHud()
        {
            var score = Score;
            if (score == _lastScore && _coins == _lastCoins && _shields == _lastShields)
                return;

            _lastScore = score;
            _lastCoins = _coins;
            _lastShields = _shields;
            HudChanged?.Invoke(score, _coins, _shields);
        }

        private void DestroyAt(int index)
        {
            Destroy(_active[index].gameObject);
            _active.RemoveAt(index);
        }

        private void ClearObjects()
        {
            foreach (var item in _active)
                Destroy(item.gameObject);

            _active.Clear();
        }
    }
}