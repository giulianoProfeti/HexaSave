using System;
using HexaSave.Application.Ports;
using HexaSave.Domain.Shared;
using UnityEngine;
 
namespace HexaSave.Adapters.Platform
{
    /// <summary>
    /// Adapter do port IDeviceInfo que usa o identificador do aparelho fornecido pela Unity.
    /// Em plataformas que não oferecem esse identificador, gera um uma única vez
    /// e o guarda no próprio aparelho, para que ele seja estável entre sessões.
    /// </summary>
    public sealed class UnityDeviceInfo : IDeviceInfo
    {
        private const string FallbackKey = "HexaSave.DeviceId";
 
        public DeviceId DeviceId { get; }
 
        public UnityDeviceInfo()
        {
            DeviceId = new DeviceId(ResolveIdentifier());
        }
 
        private static string ResolveIdentifier()
        {
            var identifier = SystemInfo.deviceUniqueIdentifier;
 
            if (!string.IsNullOrWhiteSpace(identifier) && identifier != SystemInfo.unsupportedIdentifier)
                return identifier;
 
            if (!PlayerPrefs.HasKey(FallbackKey))
            {
                PlayerPrefs.SetString(FallbackKey, Guid.NewGuid().ToString("N"));
                PlayerPrefs.Save();
            }
 
            return PlayerPrefs.GetString(FallbackKey);
        }
    }
}
