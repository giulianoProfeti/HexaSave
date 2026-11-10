using System;
using System.IO;
using HexaSave.Adapters.InMemory;
using HexaSave.Adapters.Local;
using HexaSave.Adapters.Platform;
using HexaSave.Application.Ports;
using HexaSave.Application.UseCases;
using HexaSave.Domain.Leaderboard;
 
namespace DemoGame.Composition
{
    /// <summary>
    /// O grafo de objetos montado: os casos de uso prontos para o jogo usar.
    /// Compose() é o ÚNICO lugar do projeto onde adapters são criados com "new".
    /// </summary>
    public sealed class HexaSaveModule
    {
        private const string LocalFolderName = "HexaSave";

        public SignInUseCase SignIn { get; }
        public SaveProgressUseCase SaveProgress { get; }
        public LoadProgressUseCase LoadProgress { get; }
        public SubmitScoreUseCase SubmitScore { get; }
        public GetTopScoresUseCase GetTopScores { get; }
 
        private HexaSaveModule(SignInUseCase signIn, SaveProgressUseCase saveProgress,
                               LoadProgressUseCase loadProgress, SubmitScoreUseCase submitScore,
                               GetTopScoresUseCase getTopScores)
        {
            SignIn = signIn;
            SaveProgress = saveProgress;
            LoadProgress = loadProgress;
            SubmitScore = submitScore;
            GetTopScores = getTopScores;
        }
 
        public static HexaSaveModule Compose(SaveProvider provider, LeaderboardId leaderboardId)
        {
            // 1. Adapters de plataforma: os mesmos para qualquer provedor.
            IClock clock = new SystemClock();
            IDeviceInfo device = new UnityDeviceInfo();
 
            // 2. Adapters do provedor escolhido. Cada um é criado UMA vez e compartilhado.
            ISaveRepository saveRepository;
            ILeaderboardService leaderboard;
            IAuthService auth;
 
            switch (provider)
            {
                case SaveProvider.InMemory:
                    saveRepository = new InMemorySaveRepository();
                    leaderboard = new InMemoryLeaderboardService();
                    auth = new InMemoryAuthService();
                    break;

                case SaveProvider.Local:
                    var baseDirectory = Path.Combine(UnityEngine.Application.persistentDataPath, LocalFolderName);
                    saveRepository = new LocalJsonSaveRepository(baseDirectory);
                    auth = new LocalAuthService(baseDirectory);
                    leaderboard = new InMemoryLeaderboardService(); // ranking persistente chega com o UGS (Sprint 2)
                    UnityEngine.Debug.Log($"[HexaSave] Saves locais em: {baseDirectory}");
                    break;
 
                default:
                    throw new ArgumentOutOfRangeException(nameof(provider), provider,
                        "Provedor ainda não suportado pelo composition root.");
            }
 
            // 3. Casos de uso: recebem as dependências pelo construtor.
            return new HexaSaveModule(
                new SignInUseCase(auth),
                new SaveProgressUseCase(saveRepository, auth, clock, device),
                new LoadProgressUseCase(saveRepository, auth),
                new SubmitScoreUseCase(leaderboard, auth, clock, leaderboardId),
                new GetTopScoresUseCase(leaderboard, leaderboardId));
        }
    }
}