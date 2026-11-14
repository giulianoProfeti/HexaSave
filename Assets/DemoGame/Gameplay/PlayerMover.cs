using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

namespace DemoGame.Gameplay
{
    /// <summary>
    /// Move o jogador na horizontal. Teclado (A/D ou setas) no PC;
    /// mouse ou toque segurado: o hexágono segue o dedo.
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer))]
    public sealed class PlayerMover : MonoBehaviour
    {
        [SerializeField] private float speed = 9f;
        [SerializeField] private float radius = 0.35f;
        [SerializeField] private float bottomMargin = 1.5f;
        [SerializeField] private Color shieldHitColor = Color.white;

        private Camera _camera;
        private SpriteRenderer _renderer;
        private Color _baseColor;
        private bool _controlEnabled;

        public float Radius => radius;

        private void Awake()
        {
            _camera = Camera.main;
            _renderer = GetComponent<SpriteRenderer>();
            _baseColor = _renderer.color;
        }

        public void ResetPosition()
        {
            var half = CameraBounds.HalfSize(_camera);
            transform.position = new Vector3(0f, -half.y + bottomMargin, 0f);
            _renderer.color = _baseColor;
        }

        public void SetControlEnabled(bool isEnabled) => _controlEnabled = isEnabled;

        public void ShowShieldHit()
        {
            StopAllCoroutines();
            StartCoroutine(FlashRoutine());
        }

        private void Update()
        {
            if (!_controlEnabled)
                return;

            var position = transform.position;
            var step = speed * Time.deltaTime;
            var pointer = Pointer.current;

            if (pointer != null && pointer.press.isPressed)
            {
                var target = _camera.ScreenToWorldPoint(pointer.position.ReadValue());
                position.x = Mathf.MoveTowards(position.x, target.x, step);
            }
            else
            {
                position.x += ReadKeyboardDirection() * step;
            }

            var limit = CameraBounds.HalfSize(_camera).x - radius;
            position.x = Mathf.Clamp(position.x, -limit, limit);
            transform.position = position;
        }

        void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.limeGreen;
            Gizmos.DrawWireSphere(transform.localPosition, radius);
        }

        private static float ReadKeyboardDirection()
        {
            var keyboard = Keyboard.current;
            if (keyboard == null)
                return 0f;

            var direction = 0f;
            if (keyboard.leftArrowKey.isPressed || keyboard.aKey.isPressed) direction -= 1f;
            if (keyboard.rightArrowKey.isPressed || keyboard.dKey.isPressed) direction += 1f;
            return direction;
        }

        private IEnumerator FlashRoutine()
        {
            _renderer.color = shieldHitColor;
            yield return new WaitForSeconds(0.15f);
            _renderer.color = _baseColor;
        }
    }
}