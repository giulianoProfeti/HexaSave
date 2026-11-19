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
        public GetAccountStatusUseCase GetAccountStatus { get; }
        public LinkAccountUseCase LinkAccount { get; }
        public SignInToAccountUseCase SignInToAccount { get; }

        /// <summary>Qual adapter atende cada port, em texto, para exibir no jogo e no Console.</summary>
        public string Description { get; }
 
        private HexaSaveModule(SignInUseCase signIn, SaveProgressUseCase saveProgress,
                               LoadProgressUseCase loadProgress, SubmitScoreUseCase submitScore,
                               GetTopScoresUseCase getTopScores, GetAccountStatusUseCase getAccountStatus,
                               LinkAccountUseCase linkAccount, SignInToAccountUseCase signInToAccount,
                               string description)
        {
            SignIn = signIn;
            SaveProgress = saveProgress;
            LoadProgress = loadProgress;
            SubmitScore = submitScore;
            GetTopScores = getTopScores;
            GetAccountStatus = getAccountStatus;
            LinkAccount = linkAccount;
            SignInToAccount = signInToAccount;
            Description = description;
        }
 
        public static HexaSaveModule Compose(SaveProvider provider, LeaderboardId leaderboardId)
        {
            IClock clock = new SystemClock();
            IDeviceInfo device = new UnityDeviceInfo();

            ISaveRepository saveRepository;
            ILeaderboardService leaderboard;
            IAuthService auth;
            IAccountService account;
            string description;

            switch (provider)
            {
                case SaveProvider.InMemory:
                    var memoryAuth = new InMemoryAuthService();
                    auth = memoryAuth;
                    account = new InMemoryAccountService(memoryAuth); // as duas peças trabalham em par
                    saveRepository = new InMemorySaveRepository();
                    leaderboard = new InMemoryLeaderboardService();
                    description = "Memória (login, conta, save e ranking)";
                    break;

                case SaveProvider.Local:
                    var localDirectory = LocalDirectory();
                    saveRepository = new LocalJsonSaveRepository(localDirectory);
                    auth = new LocalAuthService(localDirectory);
                    account = new UnavailableAccountService(); // Null Object: o provedor local não tem contas
                    leaderboard = new InMemoryLeaderboardService(); // ranking persistente chega com o UGS
                    description = "Local (login e save) · Memória (ranking) · sem contas";
                    break;

                case SaveProvider.Ugs:
                    auth = new UgsAuthService();
                    account = new UgsAccountService();
                    saveRepository = new LocalJsonSaveRepository(LocalDirectory());
                    leaderboard = new InMemoryLeaderboardService();
                    description = "UGS (login e conta) · Local (save) · Memória (ranking)";
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(provider), provider,
                        "Provedor ainda não suportado pelo composition root.");
            }

            return new HexaSaveModule
            (
                new SignInUseCase(auth),
                new SaveProgressUseCase(saveRepository, auth, clock, device),
                new LoadProgressUseCase(saveRepository, auth),
                new SubmitScoreUseCase(leaderboard, auth, clock, leaderboardId),
                new GetTopScoresUseCase(leaderboard, leaderboardId),
                new GetAccountStatusUseCase(account),
                new LinkAccountUseCase(account, auth),
                new SignInToAccountUseCase(account),
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