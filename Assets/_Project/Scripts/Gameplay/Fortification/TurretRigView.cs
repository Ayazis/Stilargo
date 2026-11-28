using System;
using Stilargo.Core.Combat;
using Stilargo.Core.Fortification;
using Stilargo.Gameplay.Units;
using UnityEngine;

namespace Stilargo.Gameplay.Fortification
{
    /// <summary>
    /// Swings a turret model's yaw and pitch bones toward the enemy its <see cref="Turret"/> has chosen.
    /// The rest direction is read from the model itself (pitch pivot to muzzle), so any model with a Yaw bone,
    /// a Pitch bone under it and a Muzzle point under that works, whatever axes it was exported with.
    /// </summary>
    [RequireComponent(typeof(TurretShotEffects))]
    public sealed class TurretRigView : MonoBehaviour
    {
        [SerializeField] private Transform _yaw;
        [SerializeField] private Transform _pitch;
        [SerializeField] private Transform _muzzle;

        private Turret _turret;
        private TurretAim _aim;
        private Quaternion _yawRest;
        private Quaternion _pitchRest;
        private Vector3 _yawAxisInParent;
        private Vector3 _pitchAxisInParent;
        private Vector3 _restForward;
        private Vector3 _restRight;
        private Vector3 _up;
        private float _restPitchDegrees;

        public Vector3 MuzzlePosition => _muzzle.position;

        public TurretShotEffects ShotEffects { get; private set; }

        private void Awake()
        {
            ShotEffects = GetComponent<TurretShotEffects>();
        }

        public void Construct(Turret turret, TurretAimSettings aimSettings)
        {
            if (_turret != null)
            {
                throw new InvalidOperationException($"{nameof(TurretRigView)} is already constructed.");
            }

            if (_yaw == null || _pitch == null || _muzzle == null)
            {
                throw new InvalidOperationException($"{nameof(TurretRigView)} on '{name}' needs its yaw, pitch and muzzle transforms.");
            }

            _turret = turret ?? throw new ArgumentNullException(nameof(turret));
            _aim = new TurretAim(aimSettings);
            CaptureRestPose();
        }

        private void CaptureRestPose()
        {
            _up = transform.up;
            Vector3 barrel = _muzzle.position - _pitch.position;
            _restForward = Vector3.ProjectOnPlane(barrel, _up).normalized;
            _restRight = Vector3.Cross(_up, _restForward);
            _restPitchDegrees = Vector3.SignedAngle(_restForward, barrel, -_restRight);
            _yawRest = _yaw.localRotation;
            _pitchRest = _pitch.localRotation;
            _yawAxisInParent = _yaw.parent.InverseTransformDirection(_up);
            _pitchAxisInParent = _pitch.parent.InverseTransformDirection(_restRight);
        }

        private void LateUpdate()
        {
            if (_turret == null)
            {
                return;
            }

            ICombatTarget target = _turret.AimTarget;
            if (target == null || !target.IsAlive)
            {
                return;
            }

            Vector3 toTarget = target.Position.ToUnity() - _pitch.position;
            var local = new System.Numerics.Vector3(
                Vector3.Dot(toTarget, _restRight), Vector3.Dot(toTarget, _up), Vector3.Dot(toTarget, _restForward));
            _aim.TurnToward(local, Time.deltaTime);

            _yaw.localRotation = Quaternion.AngleAxis(_aim.YawDegrees, _yawAxisInParent) * _yawRest;

            // A positive turn about "right" tips the barrel down in Unity's left-handed frame, so elevation is negated.
            float pitchFromRest = _aim.PitchDegrees - _restPitchDegrees;
            _pitch.localRotation = Quaternion.AngleAxis(-pitchFromRest, _pitchAxisInParent) * _pitchRest;
        }
    }
}
