using System;
using HexaSave.Domain.Leaderboard;
using HexaSave.Domain.Progress;
using HexaSave.Domain.Shared;

namespace HexaSave.Tests.EditMode.Support
{
    /// <summary>
    /// Dados de teste reutilizáveis ("Object Mother"): valores fixos e fábricas com padrões
    /// sensatos. Cada teste informa só o que importa para ele e herda o resto.
    /// </summary>
    internal static class TestData
    {
        public static readonly DateTimeOffset T0 = new DateTimeOffset(2026, 11, 2, 12, 0, 0, TimeSpan.Zero);

        public static readonly PlayerId PlayerA = new PlayerId("player-a");
        public static readonly PlayerId PlayerB = new PlayerId("player-b");
        public static readonly PlayerId PlayerC = new PlayerId("player-c");

        public static readonly DeviceId Pc = new DeviceId("pc");
        public static readonly DeviceId Phone = new DeviceId("phone");

        public static readonly LeaderboardId Board = new LeaderboardId("pontuacao-geral");

        public const string Content = "{\"nivel\":1}";

        public static PlayerProgress Progress(PlayerId? player = null, string content = Content, int version = 1,
                                              DateTimeOffset? at = null, DeviceId? device = null)
            => PlayerProgress.Restore(player ?? PlayerA, new SaveData(content), new SaveVersion(version),
                                      at ?? T0, device ?? Pc);

        public static ScoreEntry Entry(PlayerId player, int points, string name = "Jogador", DateTimeOffset? at = null)
            => new ScoreEntry(player, new DisplayName(name), new Score(points), at ?? T0);
    }
}
