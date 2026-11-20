using System;

namespace HexaSave.Adapters.Ugs
{
    /// <summary>
    /// Dados extras gravados junto com a pontuação no UGS Leaderboards (campo "metadata").
    /// O UGS não tem o conceito de DisplayName por pontuação, então ele viaja aqui.
    /// </summary>
    [Serializable]
    internal sealed class ScoreMetadataDto
    {
        public string displayName;
    }
}
