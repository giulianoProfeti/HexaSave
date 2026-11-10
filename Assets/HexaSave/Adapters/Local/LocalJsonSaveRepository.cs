using System;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using HexaSave.Application.Errors;
using HexaSave.Application.Ports;
using HexaSave.Domain.Progress;
using HexaSave.Domain.Shared;
 
namespace HexaSave.Adapters.Local
{
    /// <summary>
    /// Adapter do port ISaveRepository que grava um arquivo JSON por jogador no disco.
    /// Recebe o diretório base pelo construtor (injeção), o que permite testá-lo
    /// com uma pasta temporária, sem depender da Unity para saber onde gravar.
    /// </summary>
    public sealed class LocalJsonSaveRepository : ISaveRepository
    {
        private const string SavesFolderName = "saves";
        private const string FileExtension = ".json";
        private const string TempSuffix = ".tmp";
 
        private readonly string _savesDirectory;
 
        public LocalJsonSaveRepository(string baseDirectory)
        {
            if (string.IsNullOrWhiteSpace(baseDirectory))
                throw new ArgumentException("O diretório base não pode ser vazio.", nameof(baseDirectory));
 
            _savesDirectory = Path.Combine(baseDirectory, SavesFolderName);
        }
 
        public async Task<PlayerProgress> LoadAsync(PlayerId playerId)
        {
            var path = FilePathFor(playerId);
            string json;
 
            try
            {
                RecoverInterruptedWrite(path);
 
                if (!File.Exists(path))
                    return null; // contrato: jogador sem save não é erro
 
                json = await File.ReadAllTextAsync(path);
            }
            catch (Exception exception) when (exception is IOException || exception is UnauthorizedAccessException)
            {
                throw new PersistenceUnavailableException($"Não foi possível ler o save local em '{path}'.", exception);
            }
 
            try
            {
                var progress = PlayerProgressMapper.ToDomain(JsonUtility.FromJson<PlayerProgressDto>(json));
 
                if (progress.PlayerId != playerId)
                    throw new FormatException("O arquivo pertence a outro jogador.");
 
                return progress;
            }
            catch (Exception exception) when (exception is ArgumentException
                                              || exception is FormatException
                                              || exception is DomainException)
            {
                QuarantineCorruptedFile(path, exception);
                return null;
            }
        }
 
        public async Task SaveAsync(PlayerProgress progress)
        {
            if (progress is null)
                throw new ArgumentNullException(nameof(progress));
 
            var path = FilePathFor(progress.PlayerId);
            var tempPath = path + TempSuffix;
            var json = JsonUtility.ToJson(PlayerProgressMapper.ToDto(progress), prettyPrint: true);
 
            try
            {
                Directory.CreateDirectory(_savesDirectory);
 
                // Escrita atômica: o arquivo definitivo nunca fica pela metade.
                await File.WriteAllTextAsync(tempPath, json); // 1. grava tudo no temporário
                if (File.Exists(path))
                    File.Delete(path);                        // 2. remove a versão anterior
                File.Move(tempPath, path);                    // 3. promove o temporário
            }
            catch (Exception exception) when (exception is IOException || exception is UnauthorizedAccessException)
            {
                throw new PersistenceUnavailableException($"Não foi possível gravar o save local em '{path}'.", exception);
            }
        }
 
        private string FilePathFor(PlayerId playerId)
        {
            // Escapa caracteres que não são válidos em nomes de arquivo (/, :, *, ? ...).
            var fileName = Uri.EscapeDataString(playerId.Value) + FileExtension;
            return Path.Combine(_savesDirectory, fileName);
        }
 
        /// <summary>
        /// Se o jogo foi interrompido entre os passos 2 e 3 da escrita, o temporário completo
        /// existe e o definitivo não: promove o temporário para não perder o último save.
        /// </summary>
        private static void RecoverInterruptedWrite(string path)
        {
            var tempPath = path + TempSuffix;
 
            if (!File.Exists(path) && File.Exists(tempPath))
                File.Move(tempPath, path);
        }
 
        /// <summary>
        /// Um arquivo ilegível é renomeado (nunca apagado), para que o jogo continue
        /// funcionando e o conteúdo original possa ser investigado depois.
        /// </summary>
        private static void QuarantineCorruptedFile(string path, Exception reason)
        {
            // Data + trecho aleatório: dois arquivos isolados no mesmo segundo não colidem.
            var quarantinePath = 
                $"{path}.corrupted-{DateTime.UtcNow:yyyyMMdd-HHmmss}-{Guid.NewGuid().ToString("N").Substring(0, 8)}";
 
            try
            {
                File.Move(path, quarantinePath);
            }
            catch (Exception exception) when (exception is IOException || exception is UnauthorizedAccessException)
            {
                throw new PersistenceUnavailableException($"Save corrompido em '{path}' e não foi possível isolá-lo.", exception);
            }
 
            Debug.LogWarning($"[HexaSave] Save local ilegível isolado em '{quarantinePath}'. Motivo: {reason.Message}");
        }
    }
}