using System;
using System.IO;
using HexaSave.Adapters.InMemory;
using HexaSave.Adapters.Local;
using HexaSave.Adapters.Platform;
using HexaSave.Adapters.Ugs;
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

        /// <summary>Qual adapter atende cada port, em texto, para exibir no jogo e no Console.</summary>
        public string Description { get; }
 
        private HexaSaveModule(SignInUseCase signIn, SaveProgressUseCase saveProgress,
                               LoadProgressUseCase loadProgress, SubmitScoreUseCase submitScore,
                               GetTopScoresUseCase getTopScores, string description)
        {
            SignIn = signIn;
            SaveProgress = saveProgress;
            LoadProgress = loadProgress;
            SubmitScore = submitScore;
            GetTopScores = getTopScores;
            Description = description;
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
            string description;
 
            switch (provider)
            {
                case SaveProvider.InMemory:
                    saveRepository = new InMemorySaveRepository();
                    leaderboard = new InMemoryLeaderboardService();
                    auth = new InMemoryAuthService();
                    description = "Memória (login, save e ranking)";
                    break;

                case SaveProvider.Local:
                    var localDirectory = LocalDirectory();
                    saveRepository = new LocalJsonSaveRepository(localDirectory);
                    auth = new LocalAuthService(localDirectory);
                    leaderboard = new InMemoryLeaderboardService();
                    description = "Local (login e save) · Memória (ranking)";
                    break;

                case SaveProvider.Ugs:
                    auth = new UgsAuthService();
                    saveRepository = new LocalJsonSaveRepository(LocalDirectory());
                    leaderboard = new InMemoryLeaderboardService();
                    description = "UGS (login) · Local (save) · Memória (ranking)";
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(provider), provider,
                        "Provedor ainda não suportado pelo composition root.");
            }
 
            // 3. Casos de uso: recebem as dependências pelo construtor.
            return new HexaSaveModule
            (
                new SignInUseCase(auth),
                new SaveProgressUseCase(saveRepository, auth, clock, device),
                new LoadProgressUseCase(saveRepository, auth),
                new SubmitScoreUseCase(leaderboard, auth, clock, leaderboardId),
                new GetTopScoresUseCase(leaderboard, leaderboardId),
                description
            );
        }

        private static string LocalDirectory()
        {
            var directory = Path.Combine(UnityEngine.Application.persistentDataPath, LocalFolderName);
            UnityEngine.Debug.Log($"[HexaSave] Saves locais em: {directory}");
            return directory;
        }
    }
}