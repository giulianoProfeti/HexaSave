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
        private string _storageLabel = "?";
        private bool _busy;

        public void Construct(SignInUseCase signIn, SaveProgressUseCase saveProgress,
                              LoadProgressUseCase loadProgress, SubmitScoreUseCase submitScore,
                              GetTopScoresUseCase getTopScores, string storageLabel)
        {
            _profile = new PlayerProfile(signIn, loadProgress, saveProgress);
            _submitScore = submitScore ?? throw new ArgumentNullException(nameof(submitScore));
            _getTopScores = getTopScores ?? throw new ArgumentNullException(nameof(getTopScores));
            _storageLabel = storageLabel;
        }

        private void OnEnable()
        {
            ui.PlayClicked += HandlePlay;
            ui.BuyShieldClicked += HandleBuyShield;
            ui.RankingClicked += HandleRanking;
            ui.BackToMenuClicked += HandleBackToMenu;
            ui.NameSubmitted += HandleNameSubmitted;
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