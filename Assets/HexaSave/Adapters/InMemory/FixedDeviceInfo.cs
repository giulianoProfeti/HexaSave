using System;
using HexaSave.Domain.Shared;
using HexaSave.Application.Ports;

namespace HexaSave.Adapters.InMemory
{
    /// <summary>
    /// Adapter do port IDeviceInfo com um identificador fixo.
    /// Nos testes, duas instâncias diferentes simulam dois aparelhos.
    /// </summary>
    public sealed class FixedDeviceInfo : IDeviceInfo
    {
        public DeviceId DeviceId { get; }

        public FixedDeviceInfo(DeviceId deviceId)
        {
            if (deviceId.IsDefault)
                throw new ArgumentException("O DeviceId não foi inicializado.", nameof(deviceId));

            DeviceId = deviceId;
        }
    }
}
