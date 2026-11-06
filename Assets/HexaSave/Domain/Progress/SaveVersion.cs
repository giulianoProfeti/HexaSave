using System;
using HexaSave.Domain.Shared;

namespace HexaSave.Domain.Progress
{
    /// <summary>
    /// Contador que aumenta a cada gravação. Começa em 1 e nunca diminui.
    /// Chamado SaveVersion (e não Version) para não colidir com System.Version.
    /// </summary>
    public readonly struct SaveVersion : IEquatable<SaveVersion>, IComparable<SaveVersion>
    {
        public const int FirstNumber = 1;
        public static SaveVersion Initial => new(FirstNumber);
 
        public int Number { get; }
        public bool IsDefault => Number == 0;
 
        public SaveVersion(int number)
        {
            if (number < FirstNumber)
                throw new DomainException($"SaveVersion deve ser maior ou igual a {FirstNumber}.");
 
            Number = number;
        }

        public SaveVersion Next() => new(checked(Number + 1));

        public bool IsNewerThan(SaveVersion other) => Number > other.Number;

        public int CompareTo(SaveVersion other) => Number.CompareTo(other.Number);

        public bool Equals(SaveVersion other) => Number == other.Number;
        public override bool Equals(object obj) => obj is SaveVersion other && Equals(other);
        public override int GetHashCode() => Number;

        public static bool operator == (SaveVersion left, SaveVersion right) => left.Equals(right);
        public static bool operator != (SaveVersion left, SaveVersion right) => !left.Equals(right);
        public static bool operator > (SaveVersion left, SaveVersion right) => left.Number > right.Number;
        public static bool operator < (SaveVersion left, SaveVersion right) => left.Number < right.Number;
        public static bool operator >= (SaveVersion left, SaveVersion right) => left.Number >= right.Number;
        public static bool operator <= (SaveVersion left, SaveVersion right) => left.Number <= right.Number;
 
        public override string ToString() => $"v{Number}";
    }
}
