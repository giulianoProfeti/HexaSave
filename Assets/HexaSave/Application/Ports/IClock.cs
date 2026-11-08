using System;
 
namespace HexaSave.Application.Ports
{
    /// <summary>
    /// Port de saída: o relógio (decisão D13).
    /// O tempo é uma dependência externa; 
    /// </summary>
    public interface IClock
    {
        /// <summary>Instante atual, sempre em UTC.</summary>
        DateTimeOffset UtcNow { get; }
    }
}
