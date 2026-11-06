using System;
using HexaSave.Domain.Shared;

namespace HexaSave.Domain.Progress
{
    /// <summary>
    /// Conteúdo do jogo em forma de texto (ex.: um JSON gerado pelo próprio jogo).
    /// O módulo NUNCA interpreta este conteúdo: ele é opaco (decisão D4).
    /// </summary>
    public readonly struct SaveData : IEquatable<SaveData>
    {
        public string Content { get; }
        public bool IsDefault => Content is null;
 
        public SaveData(string content)
        {
            if (string.IsNullOrEmpty(content))
                throw new DomainException("SaveData não pode ser vazio.");
 
            Content = content;
        }
 
        public bool Equals(SaveData other) => string.Equals(Content, other.Content, StringComparison.Ordinal);
        public override bool Equals(object obj) => obj is SaveData other && Equals(other);
        public override int GetHashCode() => Content is null ? 0 : StringComparer.Ordinal.GetHashCode(Content);
 
        public static bool operator ==(SaveData left, SaveData right) => left.Equals(right);
        public static bool operator !=(SaveData left, SaveData right) => !left.Equals(right);
 
        // Não despeja o conteúdo inteiro no log: mostra só o tamanho.
        public override string ToString() => Content is null ? "<default>" : $"SaveData({Content.Length} caracteres)";
    }
}
