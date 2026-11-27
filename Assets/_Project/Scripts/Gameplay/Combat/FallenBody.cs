using System;
using UnityEngine;

namespace Stilargo.Gameplay.Combat
{
    /// <summary>
    /// What is left of a soldier: the avatar topples over, lies in the street for a while, then sinks out of sight.
    /// Death reads as an event in the fight instead of a unit blinking out.
    /// </summary>
    public sealed class FallenBody : MonoBehaviour
    {
        private const float FallSeconds = 0.55f;
        private const float FallDegrees = 88f;
        private const float SinkSeconds = 1.5f;
        private const float SinkDepthMetres = 0.6f;

        private float _lieSeconds;
        private float _age;
        private Quaternion _standing;
        private Quaternion _lying;
        private Vector3 _restingPosition;

        /// <summary>Detaches <paramref name="body"/> from its dying unit and lets it fall; it removes itself afterwards.</summary>
        public static void Lay(Transform body, float lieSeconds)
        {
            if (body == null)
            {
                throw new ArgumentNullException(nameof(body));
            }

            if (!(lieSeconds >= 0f))
            {
                throw new ArgumentOutOfRangeException(nameof(lieSeconds), lieSeconds, "Must be a non-negative number.");
            }

            body.SetParent(null, true);
            Animator animator = body.GetComponentInChildren<Animator>();
            if (animator != null)
            {
                // Freezes the last pose, so the body falls as it stood.
                animator.enabled = false;
            }

            body.gameObject.AddComponent<FallenBody>().Begin(lieSeconds);
        }

        private void Begin(float lieSeconds)
        {
            _lieSeconds = lieSeconds;
            _standing = transform.rotation;

            // Falls backwards, away from where it was facing, pivoting on its feet.
            _lying = Quaternion.AngleAxis(-FallDegrees, transform.right) * _standing;
            _restingPosition = transform.position;
        }

        private void Update()
        {
            _age += Time.deltaTime;
            float fall = Mathf.Clamp01(_age / FallSeconds);
            transform.rotation = Quaternion.Slerp(_standing, _lying, fall * fall);

            float sinkStart = FallSeconds + _lieSeconds;
            float sink = Mathf.Clamp01((_age - sinkStart) / SinkSeconds);
            transform.position = _restingPosition + (Vector3.down * (SinkDepthMetres * sink));
            if (sink >= 1f)
            {
                Destroy(gameObject);
            }
        }
    }
}
