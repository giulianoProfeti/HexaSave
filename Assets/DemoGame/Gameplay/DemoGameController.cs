using System;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using HexaSave.Application.UseCases;
using HexaSave.Application.Errors;
using HexaSave.Domain.Shared;
using HexaSave.Application.Ports;

namespace DemoGame.Gameplay
{
    /// <summary>
    /// O "maestro" do jogo: reage aos cliques da UI e ao fim das partidas,
    /// e conversa com o HexaSave SOMENTE através dos casos de uso injetados.
    /// </summary>
    public sealed class DemoGameController : MonoBehaviour
    {
        private const int RankingSize = 10;

        [SerializeField] private DemoUi ui;
        [SerializeField] private RunController run;

        private PlayerProfile _profile;
        private SubmitScoreUseCase _submitScore;
        private GetTopScoresUseCase _getTopScores;
        private GetAccountStatusUseCase _getAccountStatus;
        private LinkAccountUseCase _linkAccount;
        private SignInToAccountUseCase _signInToAccount;
        private string _storageLabel = "?";
        private bool _busy;

        public void Construct(DemoGameDependencies dependencies)
        {
            if (dependencies == null)
                throw new ArgumentNullException(nameof(dependencies));

            _profile = new PlayerProfile(dependencies.SignIn, dependencies.LoadProgress, dependencies.SaveProgress);
            _submitScore = dependencies.SubmitScore;
            _getTopScores = dependencies.GetTopScores;
            _getAccountStatus = dependencies.GetAccountStatus;
            _linkAccount = dependencies.LinkAccount;
            _signInToAccount = dependencies.SignInToAccount;
            _storageLabel = dependencies.StorageLabel;
        }

        private void OnEnable()
        {
            ui.PlayClicked += HandlePlay;
            ui.BuyShieldClicked += HandleBuyShield;
            ui.RankingClicked += HandleRanking;
            ui.BackToMenuClicked += HandleBackToMenu;
            ui.NameSubmitted += HandleNameSubmitted;
            ui.AccountClicked += HandleAccount;
            ui.LinkAccountRequested += HandleLinkAccount;
            ui.SignInAccountRequested += HandleSignInAccount;

            run.HudChanged += ui.SetHud;
            run.RunEnded += HandleRunEnded;
        }

        private void OnDisable()
        {
            ui.PlayClicked -= HandlePlay;
            ui.BuyShieldClicked -= HandleBuyShield;
            ui.RankingClicked -= HandleRanking;
            ui.BackToMenuClicked -= HandleBackToMenu;
            ui.NameSubmitted -= HandleNameSubmitted;
            ui.AccountClicked -= HandleAccount;
            ui.LinkAccountRequested -= HandleLinkAccount;
            ui.SignInAccountRequested -= HandleSignInAccount;

            run.HudChanged -= ui.SetHud;
            run.RunEnded -= HandleRunEnded;
        }

        private async void Start()
        {
            ui.Show(UiPanel.Menu);

            if (_profile == null)
            {
                Debug.LogError("[DemoGame] Dependências não injetadas. O GameBootstrapper referencia este componente?", this);
                ui.SetStatus("Erro de configuração: veja o Console.");
                ui.SetInteractable(false);
                return;
            }

            SetBusy(true, "Entrando...");
            try
            {
                await _profile.InitializeAsync();
                Status(_profile.CurrentVersion.HasValue
                    ? $"Progresso carregado (v{_profile.CurrentVersion})"
                    : "Bem-vindo! Nenhum progresso salvo ainda.");
            }
            catch (PersistenceUnavailableException exception)
            {
                Debug.LogWarning($"[DemoGame] {exception.Message}");
                Status("Armazenamento indisponível: jogando sem salvar.");
            }
            catch (Exception exception)
            {
                Debug.LogException(exception, this);
                Status("Erro inesperado ao iniciar: veja o Console.");
            }
            finally
            {
                SetBusy(false);
                RefreshMenu();
            }
        }

        // ---------- Reações aos eventos ----------

        private void HandlePlay()
        {
            if (_busy || run.IsRunning)
                return;

            ui.Show(UiPanel.Hud);
            run.StartRun(_profile.ShieldLevel);
        }

        private async void HandleRunEnded(RunResult result)
        {
            var isNewBest = _profile.ApplyRun(result);
            ui.SetGameOver(result, isNewBest);
            ui.Show(UiPanel.GameOver);

            SetBusy(true, "Salvando progresso...");
            try
            {
                // Progresso primeiro (é o mais valioso); ranking depois.
                var saveMessage = await SaveWithFeedbackAsync();
                var rankingMessage = await SubmitScoreWithFeedbackAsync(result.Score);
                Status($"{saveMessage}  |  {rankingMessage}");
            }
            catch (Exception exception)
            {
                Debug.LogException(exception, this);
            }
            finally
            {
                SetBusy(false);
                RefreshMenu();
            }
        }

        private async void HandleBuyShield()
        {
            if (_busy || !_profile.TryBuyShield())
                return;

            RefreshMenu();
            SetBusy(true, "Salvando compra...");
            try
            {
                Status(await SaveWithFeedbackAsync());
            }
            catch (Exception exception)
            {
                Debug.LogException(exception, this);
            }
            finally
            {
                SetBusy(false);
                RefreshMenu();
            }
        }

        private async void HandleRanking()
        {
            if (_busy)
                return;

            ui.SetRanking("Carregando...");
            ui.Show(UiPanel.Ranking);
            SetBusy(true, "Consultando ranking...");
            try
            {
                var top = await _getTopScores.ExecuteAsync(RankingSize);
                ui.SetRanking(FormatRanking(top));
                Status("Ranking atualizado.");
            }
            catch (PersistenceUnavailableException exception)
            {
                Debug.LogWarning($"[DemoGame] {exception.Message}");
                ui.SetRanking("Ranking indisponível no momento.");
                Status("Não foi possível consultar o ranking.");
            }
            catch (Exception exception)
            {
                Debug.LogException(exception, this);
            }
            finally
            {
                SetBusy(false);
            }
        }

        private async void HandleNameSubmitted(string newName)
        {
            if (_busy || newName == _profile.PlayerName)
                return;

            try
            {
                _profile.SetPlayerName(newName);
            }
            catch (DomainException exception)
            {
                Status($"Nome inválido: {exception.Message}");
                RefreshMenu(); // devolve o nome antigo ao campo
                return;
            }

            SetBusy(true, "Salvando nome...");
            try
            {
                Status(await SaveWithFeedbackAsync());
            }
            catch (Exception exception)
            {
                Debug.LogException(exception, this);
            }
            finally
            {
                SetBusy(false);
                RefreshMenu();
            }
        }

        private void HandleBackToMenu()
        {
            RefreshMenu();
            ui.Show(UiPanel.Menu);
        }

        private async void HandleAccount()
        {
            if (_busy)
                return;

            ui.SetAccount("Consultando conta...", showForm: false);
            ui.Show(UiPanel.Account);
            SetBusy(true, "Consultando conta...");
            try
            {
                await RefreshAccountAsync();
                Status("Conta consultada.");
            }
            catch (PersistenceUnavailableException exception)
            {
                Debug.LogWarning($"[DemoGame] {exception.Message}");
                ui.SetAccount("Não foi possível consultar a conta agora.", showForm: false);
                Status("Serviço de contas indisponível.");
            }
            catch (Exception exception)
            {
                Debug.LogException(exception, this);
            }
            finally
            {
                SetBusy(false);
            }
        }

        private async void HandleLinkAccount(string username, string password)
        {
            if (_busy)
                return;

            SetBusy(true, "Criando conta...");
            try
            {
                var status = await _linkAccount.ExecuteAsync(username, password);
                await RefreshAccountAsync();
                Status($"Conta '{status.Username}' criada. Seu progresso continua o mesmo.");
            }
            catch (AccountException exception)
            {
                Status(exception.Message);
            }
            catch (NotSignedInException)
            {
                Status("Sem sessão: não é possível criar a conta agora.");
            }
            catch (PersistenceUnavailableException exception)
            {
                Debug.LogWarning($"[DemoGame] {exception.Message}");
                Status("Serviço de contas indisponível. Tente novamente mais tarde.");
            }
            catch (Exception exception)
            {
                Debug.LogException(exception, this);
            }
            finally
            {
                ui.ClearPassword();
                SetBusy(false);
            }
        }

        private async void HandleSignInAccount(string username, string password)
        {
            if (_busy)
                return;

            SetBusy(true, "Entrando na conta...");
            try
            {
                await _signInToAccount.ExecuteAsync(username, password);

                // Outro jogador: o progresso em memória é do anterior e precisa ser recarregado.
                await _profile.InitializeAsync();
                await RefreshAccountAsync();
                Status(_profile.CurrentVersion.HasValue
                    ? $"Conta carregada: progresso v{_profile.CurrentVersion}."
                    : "Conta carregada: ainda não há progresso salvo.");
            }
            catch (AccountException exception)
            {
                Status(exception.Message);
            }
            catch (PersistenceUnavailableException exception)
            {
                Debug.LogWarning($"[DemoGame] {exception.Message}");
                Status("Serviço de contas indisponível. Tente novamente mais tarde.");
            }
            catch (Exception exception)
            {
                Debug.LogException(exception, this);
            }
            finally
            {
                ui.ClearPassword();
                SetBusy(false);
                RefreshMenu();
            }
        }

        private async Task RefreshAccountAsync()
        {
            var status = await _getAccountStatus.ExecuteAsync();

            switch (status.Kind)
            {
                case AccountKind.Registered:
                    ui.SetAccount($"Conta: {status.Username}\nSeu progresso está vinculado a esta conta " +
                                  "e pode ser recuperado em outro aparelho.", showForm: false);
                    break;

                case AccountKind.Guest:
                    ui.SetAccount("Você está jogando como CONVIDADO.\nCrie uma conta para não perder seu progresso, " +
                                  "ou entre em uma conta existente (isso substitui o progresso de convidado " +
                                  "deste aparelho).", showForm: true);
                    break;

                default:
                    ui.SetAccount("Contas de jogador não estão disponíveis neste provedor.", showForm: false);
                    break;
            }
        }

        // ---------- Comunicação com o HexaSave (falhas viram mensagens) ----------

        private async Task<string> SaveWithFeedbackAsync()
        {
            try
            {
                var version = await _profile.SaveAsync();
                return $"Progresso salvo (v{version})";
            }
            catch (PersistenceUnavailableException exception)
            {
                Debug.LogWarning($"[DemoGame] {exception.Message}");
                return "Não foi possível salvar agora";
            }
            catch (NotSignedInException)
            {
                return "Sem sessão: progresso não salvo";
            }
        }

        private async Task<string> SubmitScoreWithFeedbackAsync(int score)
        {
            try
            {
                await _submitScore.ExecuteAsync(score, _profile.PlayerName);
                return "Pontuação enviada ao ranking";
            }
            catch (PersistenceUnavailableException exception)
            {
                Debug.LogWarning($"[DemoGame] {exception.Message}");
                return "Ranking indisponível";
            }
            catch (NotSignedInException)
            {
                return "Sem sessão: ranking não atualizado";
            }
        }

        // ---------- Auxiliares ----------

        private string FormatRanking(IReadOnlyList<RankedScore> top)
        {
            if (top.Count == 0)
                return "Ninguém pontuou ainda.\nSeja o primeiro!";

            var builder = new StringBuilder();
            foreach (var row in top)
            {
                var you = row.Entry.PlayerId == _profile.PlayerId ? "   (você)" : string.Empty;
                builder.AppendLine($"#{row.Rank}   {row.Entry.DisplayName}   {row.Entry.Score}{you}");
            }

            return builder.ToString();
        }

        private void RefreshMenu()
        {
            ui.SetMenu(new MenuViewData(_profile.PlayerName, _profile.Coins, _profile.BestScore,
                                        _profile.RunsPlayed, _profile.ShieldLevel, PlayerProfile.MaxShieldLevel,
                                        _profile.NextShieldCost, _profile.CanBuyShield));
        }

        private void SetBusy(bool busy, string message = null)
        {
            _busy = busy;
            ui.SetInteractable(!busy);

            if (message != null)
                Status(message);
        }

        private void Status(string message)
        {
            ui.SetStatus($"{message}\nArmazenamento: {_storageLabel}");
        }
    }
}