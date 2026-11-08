using System;
using HexaSave.Application.Ports;

namespace HexaSave.Adapters.Platform
{
    /// <summary>
    /// Adapter do port IClock que lê o relógio real do sistema.
    /// </summary>
    public sealed class SystemClock : IClock
    {
        public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
    }
}
