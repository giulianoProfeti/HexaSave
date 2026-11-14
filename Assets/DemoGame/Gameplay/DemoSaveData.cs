using System;

namespace DemoGame.Gameplay
{
    /// <summary>
    /// DTO com os dados do JOGO (não do módulo). Viram texto JSON e vão para o HexaSave
    /// como conteúdo opaco (SaveData). O HexaSave nunca abre este formato.
    /// Campos públicos porque o JsonUtility exige.
    /// </summary>
    [Serializable]
    internal sealed class DemoSaveData
    {
        public const int CurrentFormatVersion = 1;
        public const string DefaultPlayerName = "Jogador";

        public int formatVersion = CurrentFormatVersion;
        public string playerName = DefaultPlayerName;
        public int coins;
        public int bestScore;
        public int runsPlayed;
        public int shieldLevel;
    }
}