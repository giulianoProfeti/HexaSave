using HexaSave.Domain.Shared;
 
namespace HexaSave.Application.Ports
{
    /// <summary>
    /// Port de saída: identifica o aparelho atual (decisão D13).
    /// Na Unity, o adapter pode usar SystemInfo.deviceUniqueIdentifier;
    /// nos testes, um valor fixo.
    /// </summary>
    public interface IDeviceInfo
    {
        DeviceId DeviceId { get; }
    }
}
