using System;

namespace HexaSave.Domain.Shared
{
    /// <summary>
    /// Identifica o aparelho que fez a última gravação. Base para detectar conflitos (R15).
    /// </summary>
    public readonly struct DeviceId : IEquatable<DeviceId>
    {
        public string Value { get; }
        public bool IsDefault => Value is null;

        public DeviceId(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new DomainException("DeviceId não pode ser vazio.");
 
            Value = value;
        }

        public bool Equals(DeviceId other) => string.Equals(Value, other.Value, StringComparison.Ordinal);
        public override bool Equals(object obj) => obj is DeviceId other && Equals(other);
        public override int GetHashCode() => Value is null ? 0 : StringComparer.Ordinal.GetHashCode(Value);
 
        public static bool operator ==(DeviceId left, DeviceId right) => left.Equals(right);
        public static bool operator !=(DeviceId left, DeviceId right) => !left.Equals(right);
 
        public override string ToString() => Value ?? "<default>";
    }
}
