using System;

namespace HexaSave.Adapters.Ugs
{
    /// <summary>
    /// Formato do progresso guardado no Cloud Save. É propositalmente uma cópia do formato do
    /// adapter Local, e não um formato compartilhado: cada adapter é dono do próprio armazenamento
    /// e pode evoluí-lo sem afetar os outros.
    /// </summary>
    [Serializable]
    internal sealed class CloudProgressDto
    {
        public const int CurrentSchemaVersion = 1;

        public int schemaVersion;
        public string playerId;
        public string content;
        public int version;
        public string updatedAtUtc;
        public string deviceId;
    }
}
