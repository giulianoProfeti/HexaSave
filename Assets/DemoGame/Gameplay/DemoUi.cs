using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DemoGame.Gameplay
{
    public enum UiPanel
    {
        Menu,
        Hud,
        GameOver,
        Ranking,
        Account,
    }

    /// <summary>Tudo o que a tela de menu precisa mostrar, num único pacote.</summary>
    public readonly struct MenuViewData
    {
        public MenuViewData(string playerName, int coins, int bestScore, int runsPlayed,
                            int shieldLevel, int maxShieldLevel, int? nextShieldCost, bool canBuyShield)
        {
            PlayerName = playerName;
            Coins = coins;
            BestScore = bestScore;
            RunsPlayed = runsPlayed;
            ShieldLevel = shieldLevel;
            MaxShieldLevel = maxShieldLevel;
            NextShieldCost = nextShieldCost;
            CanBuyShield = canBuyShield;
        }

        public string PlayerName { get; }
        public int Coins { get; }
        public int BestScore { get; }
        public int RunsPlayed { get; }
        public int ShieldLevel { get; }
        public int MaxShieldLevel { get; }
        public int? NextShieldCost { get; }
        public bool CanBuyShield { get; }
    }

    /// <summary>
    /// View passiva: mostra o que mandam e avisa quando um botão é clicado.
    /// Não toma decisões; quem decide é o DemoGameController.
    /// </summary>
    public sealed class DemoUi : MonoBehaviour
    {
        [Header("Painéis")]
        [SerializeField] private GameObject menuPanel;
        [SerializeField] private GameObject hudPanel;
        [SerializeField] private GameObject gameOverPanel;
        [SerializeField] private GameObject rankingPanel;
        [SerializeField] private GameObject accountPanel; 

        [Header("Menu")]
        [SerializeField] private TMP_InputField nameInput;
        [SerializeField] private TMP_Text walletText;
        [SerializeField] private TMP_Text statsText;
        [SerializeField] private TMP_Text shieldText;
        [SerializeField] private Button buyShieldButton;
        [SerializeField] private Button playButton;
        [SerializeField] private Button rankingButton;
        [SerializeField] private Button accountButton;

        [Header("HUD")]
        [SerializeField] private TMP_Text hudText;

        [Header("Fim de partida")]
        [SerializeField] private TMP_Text resultText;
        [SerializeField] private Button playAgainButton;
        [SerializeField] private Button menuButton;

        [Header("Ranking")]
        [SerializeField] private TMP_Text rankingText;
        [SerializeField] private Button backButton;

        [Header("Conta")]
        [SerializeField] private TMP_Text accountStatusText;
        [Tooltip("Objeto que agrupa os campos e botões de conta; fica oculto quando não há o que fazer.")]
        [SerializeField] private GameObject accountForm;
        [SerializeField] private TMP_InputField usernameInput;
        [Tooltip("Configure o Content Type deste campo como Password.")]
        [SerializeField] private TMP_InputField passwordInput;
        [SerializeField] private Button linkAccountButton;
        [SerializeField] private Button signInAccountButton;
        [SerializeField] private Button accountBackButton;

        [Header("Rodapé (sempre visível)")]
        [SerializeField] private TMP_Text statusText;

        private bool _interactable = true;
        private bool _canBuyShield;

        public event Action PlayClicked;
        public event Action BuyShieldClicked;
        public event Action RankingClicked;
        public event Action BackToMenuClicked;
        public event Action<string> NameSubmitted;
        public event Action AccountClicked;
        public event Action<string, string> LinkAccountRequested;
        public event Action<string, string> SignInAccountRequested;

        // private void Awake()
        // {
        //     playButton.onClick.AddListener(() => PlayClicked?.Invoke());
        //     playAgainButton.onClick.AddListener(() => PlayClicked?.Invoke());
        //     buyShieldButton.onClick.AddListener(() => BuyShieldClicked?.Invoke());
        //     rankingButton.onClick.AddListener(() => RankingClicked?.Invoke());
        //     menuButton.onClick.AddListener(() => BackToMenuClicked?.Invoke());
        //     backButton.onClick.AddListener(() => BackToMenuClicked?.Invoke());
        //     nameInput.onEndEdit.AddListener(value => NameSubmitted?.Invoke(value));
        // }

        void OnEnable()
        {
            playButton.onClick.AddListener(() => PlayClicked?.Invoke());
            playAgainButton.onClick.AddListener(() => PlayClicked?.Invoke());
            buyShieldButton.onClick.AddListener(() => BuyShieldClicked?.Invoke());
            rankingButton.onClick.AddListener(() => RankingClicked?.Invoke());
            menuButton.onClick.AddListener(() => BackToMenuClicked?.Invoke());
            backButton.onClick.AddListener(() => BackToMenuClicked?.Invoke());
            accountButton.onClick.AddListener(() => AccountClicked?.Invoke());
            accountBackButton.onClick.AddListener(() => BackToMenuClicked?.Invoke());
            linkAccountButton.onClick.AddListener(() => LinkAccountRequested?.Invoke(usernameInput.text, passwordInput.text));
            signInAccountButton.onClick.AddListener(() => SignInAccountRequested?.Invoke(usernameInput.text, passwordInput.text));
            nameInput.onEndEdit.AddListener(value => NameSubmitted?.Invoke(value));
        }

        void OnDisable()
        {
            playButton.onClick.RemoveAllListeners();
            playAgainButton.onClick.RemoveAllListeners();
            buyShieldButton.onClick.RemoveAllListeners();
            rankingButton.onClick.RemoveAllListeners();
            menuButton.onClick.RemoveAllListeners();
            backButton.onClick.RemoveAllListeners();
            accountButton.onClick.RemoveAllListeners();
            accountBackButton.onClick.RemoveAllListeners();
            linkAccountButton.onClick.RemoveAllListeners();
            signInAccountButton.onClick.RemoveAllListeners();
            nameInput.onEndEdit.RemoveAllListeners();
        }

        public void Show(UiPanel panel)
        {
            menuPanel.SetActive(panel == UiPanel.Menu);
            hudPanel.SetActive(panel == UiPanel.Hud);
            gameOverPanel.SetActive(panel == UiPanel.GameOver);
            rankingPanel.SetActive(panel == UiPanel.Ranking);
            accountPanel.SetActive(panel == UiPanel.Account);
        }

        public void SetMenu(MenuViewData data)
        {
            nameInput.SetTextWithoutNotify(data.PlayerName);
            walletText.text = $"Moedas: {data.Coins}";
            statsText.text = $"Recorde: {data.BestScore}   |   Partidas: {data.RunsPlayed}";
            shieldText.text = data.NextShieldCost.HasValue
                ? $"Escudo nível {data.ShieldLevel}/{data.MaxShieldLevel}  -  próximo: {data.NextShieldCost} moedas"
                : $"Escudo nível {data.ShieldLevel}/{data.MaxShieldLevel} (máximo)";

            _canBuyShield = data.CanBuyShield;
            buyShieldButton.interactable = _interactable && _canBuyShield;
        }

        public void SetHud(int score, int coins, int shields)
        {
            hudText.text = $"Pontos: {score}\nMoedas: {coins}    Escudos: {shields}";
        }

        public void SetGameOver(RunResult result, bool isNewBest)
        {
            var header = isNewBest ? "NOVO RECORDE!\n\n" : string.Empty;
            resultText.text = header +
                              $"Pontos: {result.Score}\n" +
                              $"Moedas coletadas: {result.CoinsCollected}\n" +
                              $"Tempo: {result.DurationSeconds:0.0} s";
        }

        public void SetRanking(string text) => rankingText.text = text;

        /// <param name="showForm">Mostra os campos de usuário e senha (só faz sentido para convidados).</param>
        public void SetAccount(string statusText, bool showForm)
        {
            accountStatusText.text = statusText;
            accountForm.SetActive(showForm);
        }

        /// <summary>Apaga a senha da tela depois de cada tentativa: ela não deve ficar à vista.</summary>
        public void ClearPassword() => passwordInput.SetTextWithoutNotify(string.Empty);

        public void SetStatus(string text) => statusText.text = text;

        /// <summary>Trava ou libera todos os controles (usado enquanto algo está sendo salvo).</summary>
        public void SetInteractable(bool interactable)
        {
            _interactable = interactable;
            playButton.interactable = interactable;
            rankingButton.interactable = interactable;
            playAgainButton.interactable = interactable;
            menuButton.interactable = interactable;
            backButton.interactable = interactable;
            nameInput.interactable = interactable;
            accountButton.interactable = interactable;
            accountBackButton.interactable = interactable;
            usernameInput.interactable = interactable;
            passwordInput.interactable = interactable;
            linkAccountButton.interactable = interactable;
            signInAccountButton.interactable = interactable;
            buyShieldButton.interactable = interactable && _canBuyShield;
        }
    }
}