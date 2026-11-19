using System;
using System.Threading.Tasks;
using HexaSave.Application.UseCases;
using HexaSave.Domain.Leaderboard;
using HexaSave.Domain.Shared;

namespace DemoGame.Gameplay
{
    /// <summary>
    /// O perfil do jogador: as regras de progressão do jogo (moedas, recorde, escudo)
    /// e a ponte com os casos de uso de entrar, carregar e salvar do HexaSave.
    /// C# puro: nenhuma dependência de cena, fácil de entender e de testar.
    /// </summary>
    internal sealed class PlayerProfile
    {
        public const int MaxShieldLevel = 3;
        private const int ShieldBaseCost = 20;

        private readonly SignInUseCase _signIn;
        private readonly LoadProgressUseCase _loadProgress;
        private readonly SaveProgressUseCase _saveProgress;

        private DemoSaveData _data = new DemoSaveData();

        public PlayerProfile(SignInUseCase signIn, LoadProgressUseCase loadProgress, SaveProgressUseCase saveProgress)
        {
            _signIn = signIn ?? throw new ArgumentNullException(nameof(signIn));
            _loadProgress = loadProgress ?? throw new ArgumentNullException(nameof(loadProgress));
            _saveProgress = saveProgress ?? throw new ArgumentNullException(nameof(saveProgress));
        }

        public PlayerId? PlayerId { get; private set; }

        /// <summary>Versão do último progresso carregado ou salvo; null se ainda não existe save.</summary>
        public int? CurrentVersion { get; private set; }

        public string PlayerName => _data.playerName;
        public int Coins => _data.coins;
        public int BestScore => _data.bestScore;
        public int RunsPlayed => _data.runsPlayed;
        public int ShieldLevel => _data.shieldLevel;

        /// <summary>Custo do próximo nível de escudo (20, 40, 60) ou null se já está no máximo.</summary>
        public int? NextShieldCost => ShieldLevel >= MaxShieldLevel ? (int?)null : ShieldBaseCost * (ShieldLevel + 1);

        public bool CanBuyShield => NextShieldCost.HasValue && Coins >= NextShieldCost.Value;

        /// <summary>
        /// Entra e carrega o progresso. Pode ser chamado de novo após trocar de conta:
        /// começa sempre do zero, para que nada do jogador anterior sobre na memória.
        /// </summary>
        public async Task InitializeAsync()
        {
            _data = new DemoSaveData();
            CurrentVersion = null;
            PlayerId = null;

            PlayerId = await _signIn.ExecuteAsync();

            var progress = await _loadProgress.ExecuteAsync();
            if (progress is null)
                return; // jogador novo: fica com os dados padrão

            _data = DemoSaveCodec.Decode(progress.Data.Content);
            CurrentVersion = progress.Version.Number;
        }

        /// <returns>O número da versão gravada.</returns>
        public async Task<int> SaveAsync()
        {
            var saved = await _saveProgress.ExecuteAsync(DemoSaveCodec.Encode(_data));
            CurrentVersion = saved.Version.Number;
            return saved.Version.Number;
        }

        /// <returns>Verdadeiro se a partida bateu o recorde pessoal.</returns>
        public bool ApplyRun(RunResult result)
        {
            var isNewBest = result.Score > _data.bestScore;

            _data.coins += result.CoinsCollected;
            _data.bestScore = Math.Max(_data.bestScore, result.Score);
            _data.runsPlayed++;

            return isNewBest;
        }

        public bool TryBuyShield()
        {
            if (!CanBuyShield)
                return false;

            var cost = NextShieldCost.Value;
            _data.coins -= cost;
            _data.shieldLevel++;
            return true;
        }

        /// <summary>Usa a regra do domínio do HexaSave: lança DomainException se o nome for inválido.</summary>
        public void SetPlayerName(string name)
        {
            var displayName = new DisplayName(name);
            _data.playerName = displayName.Value;
        }
    }
}