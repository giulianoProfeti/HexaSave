using System;
using HexaSave.Application.UseCases;

namespace DemoGame.Gameplay
{
    /// <summary>
    /// Tudo o que o jogo recebe do HexaSave, num único pacote ("objeto de parâmetros").
    /// Evita um método Construct com nove parâmetros e deixa explícito, num só lugar,
    /// de quais casos de uso o jogo depende. O jogo continua sem conhecer nenhum adapter.
    /// </summary>
    public sealed class DemoGameDependencies
    {
        public SignInUseCase SignIn { get; }
        public SaveProgressUseCase SaveProgress { get; }
        public LoadProgressUseCase LoadProgress { get; }
        public SubmitScoreUseCase SubmitScore { get; }
        public GetTopScoresUseCase GetTopScores { get; }
        public GetAccountStatusUseCase GetAccountStatus { get; }
        public LinkAccountUseCase LinkAccount { get; }
        public SignInToAccountUseCase SignInToAccount { get; }

        /// <summary>Texto que descreve a composição, só para exibir no rodapé.</summary>
        public string StorageLabel { get; }

        public DemoGameDependencies(SignInUseCase signIn, SaveProgressUseCase saveProgress,
                                    LoadProgressUseCase loadProgress, SubmitScoreUseCase submitScore,
                                    GetTopScoresUseCase getTopScores, GetAccountStatusUseCase getAccountStatus,
                                    LinkAccountUseCase linkAccount, SignInToAccountUseCase signInToAccount,
                                    string storageLabel)
        {
            SignIn = signIn ?? throw new ArgumentNullException(nameof(signIn));
            SaveProgress = saveProgress ?? throw new ArgumentNullException(nameof(saveProgress));
            LoadProgress = loadProgress ?? throw new ArgumentNullException(nameof(loadProgress));
            SubmitScore = submitScore ?? throw new ArgumentNullException(nameof(submitScore));
            GetTopScores = getTopScores ?? throw new ArgumentNullException(nameof(getTopScores));
            GetAccountStatus = getAccountStatus ?? throw new ArgumentNullException(nameof(getAccountStatus));
            LinkAccount = linkAccount ?? throw new ArgumentNullException(nameof(linkAccount));
            SignInToAccount = signInToAccount ?? throw new ArgumentNullException(nameof(signInToAccount));
            StorageLabel = string.IsNullOrWhiteSpace(storageLabel) ? "?" : storageLabel;
        }
    }
}
