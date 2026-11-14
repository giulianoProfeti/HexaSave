using System;
using UnityEngine;

namespace DemoGame.Gameplay
{
    /// <summary>
    /// Converte os dados do jogo em texto (para salvar) e de volta (ao carregar).
    /// A leitura é tolerante: dados ilegíveis ou fora dos limites não quebram o jogo.
    /// </summary>
    internal static class DemoSaveCodec
    {
        public static string Encode(DemoSaveData data) => JsonUtility.ToJson(data);

        public static DemoSaveData Decode(string json)
        {
            DemoSaveData data;

            try
            {
                data = JsonUtility.FromJson<DemoSaveData>(json);
            }
            catch (ArgumentException exception)
            {
                Debug.LogWarning($"[DemoGame] Dados do jogo ilegíveis; começando do zero. Motivo: {exception.Message}");
                return new DemoSaveData();
            }

            if (data is null)
                return new DemoSaveData();

            Sanitize(data);
            return data;
        }

        // Corrige valores impossíveis (ex.: alguém editou o arquivo à mão).
        private static void Sanitize(DemoSaveData data)
        {
            data.coins = Mathf.Max(0, data.coins);
            data.bestScore = Mathf.Max(0, data.bestScore);
            data.runsPlayed = Mathf.Max(0, data.runsPlayed);
            data.shieldLevel = Mathf.Clamp(data.shieldLevel, 0, PlayerProfile.MaxShieldLevel);

            if (string.IsNullOrWhiteSpace(data.playerName))
                data.playerName = DemoSaveData.DefaultPlayerName;
        }
    }
}