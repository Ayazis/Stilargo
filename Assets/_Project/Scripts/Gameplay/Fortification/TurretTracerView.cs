using System;
using Stilargo.Core.Combat;
using Stilargo.Gameplay.Units;
using UnityEngine;

namespace Stilargo.Gameplay.Fortification
{
    /// <summary>Draws a short-lived line from a turret to its target each time a turret fires.</summary>
    [RequireComponent(typeof(LineRenderer))]
    public sealed class TurretTracerView : MonoBehaviour
    {
        private const float DefaultVisibleSeconds = 0.08f;

        [SerializeField, Min(0.01f)] private float _visibleSeconds = DefaultVisibleSeconds;

        private LineRenderer _line;
        private TurretBattery _battery;
        private float _remainingSeconds;

        public void Construct(TurretBattery battery)
        {
            if (_battery != null)
            {
                throw new InvalidOperationException($"{nameof(TurretTracerView)} is already constructed.");
            }

            if (battery == null)
            {
                throw new ArgumentNullException(nameof(battery));
            }

            _battery = battery;
            CacheComponents();
            _battery.Fired += HandleFired;
        }

        private void Awake()
        {
            CacheComponents();
        }

        // Construct can run before Awake when the bootstrapper wires scene objects, so both paths cache.
        private void CacheComponents()
        {
            if (_line == null)
            {
                _line = GetComponent<LineRenderer>();
                _line.positionCount = 2;
                _line.enabled = false;
            }
        }

        private void OnDestroy()
        {
            if (_battery != null)
            {
                _battery.Fired -= HandleFired;
            }
        }

        private void HandleFired(Vector3 muzzlePosition, ICombatTarget target)
        {
            _line.SetPosition(0, muzzlePosition);
            _line.SetPosition(1, target.Position.ToUnity());
            _line.enabled = true;
            _remainingSeconds = _visibleSeconds;
        }

        private void Update()
        {
            if (!_line.enabled)
            {
                return;
            }

            _remainingSeconds -= Time.deltaTime;
            if (_remainingSeconds <= 0f)
            {
                _line.enabled = false;
            }
        }
    }
}
