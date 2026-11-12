using System;
using HexaSave.Application.Ports;

namespace HexaSave.Adapters.InMemory
{
    /// <summary>
    /// Adapter do port IClock controlado manualmente: o tempo só muda quando alguém manda.
    /// </summary>
    public sealed class ManualClock : IClock
    {
        public DateTimeOffset UtcNow { get; private set; }

        public ManualClock(DateTimeOffset start)
        {
            UtcNow = start.ToUniversalTime();
        }

        /// <summary>Avança o relógio. Não aceita valores negativos.</summary>
        public void Advance(TimeSpan amount)
        {
            if (amount < TimeSpan.Zero)
                throw new ArgumentOutOfRangeException(nameof(amount), "Use Set() para voltar no tempo.");

            UtcNow = UtcNow.Add(amount);
        }

        /// <summary>
        /// Coloca o relógio em qualquer instante, inclusive no passado.
        /// Útil para simular relógios desajustados entre dispositivos.
        /// </summary>
        public void Set(DateTimeOffset instant)
        {
            UtcNow = instant.ToUniversalTime();
        }
    }
}
