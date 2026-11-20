using System;
using Stilargo.Core.Combat;
using Stilargo.Gameplay.Units;
using UnityEngine;

namespace Stilargo.Gameplay.Combat
{
    /// <summary>Draws a brief tracer from this transform (the muzzle) to whatever the unit shot.</summary>
    public sealed class HitEffectView : MonoBehaviour
    {
        private const int TracerPointCount = 2;
        private const float DefaultTracerSeconds = 0.08f;

        [SerializeField] private LineRenderer _lineRenderer;
        [SerializeField, Min(0f)] private float _tracerSeconds = DefaultTracerSeconds;

        private UnitCombat _combat;
        private float _hideAtTime;
        private bool _isSubscribed;

        private bool IsConstructed => _combat != null;

        private void Awake()
        {
            HideTracer();
        }

        private void OnEnable()
        {
            Subscribe();
        }

        private void OnDisable()
        {
            Unsubscribe();
            HideTracer();
        }

        public void Construct(UnitCombat combat)
        {
            if (IsConstructed)
            {
                throw new InvalidOperationException($"{name} is already constructed.");
            }

            if (_lineRenderer == null)
            {
                throw new InvalidOperationException($"{name} needs a LineRenderer assigned.");
            }

            _combat = combat ?? throw new ArgumentNullException(nameof(combat));
            Subscribe();
        }

        private void Update()
        {
            if (_lineRenderer == null || !_lineRenderer.enabled || Time.time < _hideAtTime)
            {
                return;
            }

            HideTracer();
        }

        private void OnFired(ICombatTarget target)
        {
            _lineRenderer.positionCount = TracerPointCount;
            _lineRenderer.SetPosition(0, transform.position);
            _lineRenderer.SetPosition(1, target.Position.ToUnity());
            _lineRenderer.enabled = true;
            _hideAtTime = Time.time + _tracerSeconds;
        }

        private void HideTracer()
        {
            if (_lineRenderer != null)
            {
                _lineRenderer.enabled = false;
            }
        }

        private void Subscribe()
        {
            if (!IsConstructed || _isSubscribed)
            {
                return;
            }

            _combat.Fired += OnFired;
            _isSubscribed = true;
        }

        private void Unsubscribe()
        {
            if (!_isSubscribed)
            {
                return;
            }

            _combat.Fired -= OnFired;
            _isSubscribed = false;
        }
    }
}
