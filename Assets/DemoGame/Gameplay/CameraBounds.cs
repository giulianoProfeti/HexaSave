using UnityEngine;

namespace DemoGame.Gameplay
{
    /// <summary>Metade da largura e da altura visíveis por uma câmera ortográfica, em unidades do mundo.</summary>
    internal static class CameraBounds
    {
        public static Vector2 HalfSize(Camera camera)
            => new(camera.orthographicSize * camera.aspect, camera.orthographicSize);
    }
}