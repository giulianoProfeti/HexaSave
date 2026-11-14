using UnityEngine;

namespace DemoGame.Gameplay
{
    public enum FallingObjectKind
    {
        Hazard = 0,
        Coin = 1,
    }

    /// <summary>Um bloco perigoso ou uma moeda que cai em linha reta.</summary>
    public sealed class FallingObject : MonoBehaviour
    {
        [SerializeField] private FallingObjectKind kind = FallingObjectKind.Hazard;

        [Tooltip("Raio usado na detecção de colisão, em unidades do mundo.")]
        [SerializeField] private float radius = 0.3f;

        private float _speed;

        public FallingObjectKind Kind => kind;
        public float Radius => radius;

        public void Launch(float speed) => _speed = speed;

        private void Update()
        {
            transform.position += Vector3.down * (_speed * Time.deltaTime);
        }

        void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.limeGreen;
            Gizmos.DrawWireSphere(transform.localPosition, radius);
        }
    }
}