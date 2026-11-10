using System;
 
namespace HexaSave.Adapters.Local
{
    /// <summary>
    /// Formato do progresso DENTRO do arquivo JSON (DTO = Data Transfer Object).
    /// O JsonUtility da Unity exige [Serializable] e campos públicos; por isso este formato
    /// fica separado da entidade PlayerProgress do domínio, que continua imutável e encapsulada.
    /// "internal": detalhe do adapter, invisível fora do assembly HexaSave.Adapters.Local.
    /// </summary>
    [Serializable]
    internal sealed class PlayerProgressDto
    {
        public const int CurrentSchemaVersion = 1; // const não é gravado no arquivo: só os campos comuns vão para o JSON.
 
        // camelCase: os nomes dos campos viram os nomes das chaves no JSON. Por isso seguem o estilo comum de JSON, e não o padrão de propriedades do C#.
        public int schemaVersion; // guarda a versão do formato do arquivo, que é diferente da versão do progresso.
        public string playerId;
        public string content;
        public int version;
        public string updatedAtUtc; // é string: o JsonUtility não sabe gravar DateTimeOffset, então a data é guardada como texto no padrão ISO 8601.
        public string deviceId;
    }
}