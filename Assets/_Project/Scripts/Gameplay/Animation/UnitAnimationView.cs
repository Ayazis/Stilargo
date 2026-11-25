using System;
using Stilargo.Core.Animation;
using Stilargo.Core.Combat;
using UnityEngine;
using UnityEngine.AI;

namespace Stilargo.Gameplay.Animation
{
    /// <summary>
    /// Plays the unit's pose on its avatar's Animator. Sits on the unit root next to the NavMeshAgent;
    /// the pose rules live in <see cref="UnitPosePicker"/>. Animator states are named after <see cref="UnitPose"/>.
    /// </summary>
    [RequireComponent(typeof(NavMeshAgent))]
    public sealed class UnitAnimationView : MonoBehaviour
    {
        // Below a slow walk the unit reads as standing; aiming lingers after each shot so bursts do not flicker.
        private const float DefaultRunThresholdMetresPerSecond = 0.5f;
        private const float DefaultAimHoldSeconds = 2f;
        private const float DefaultCrossFadeSeconds = 0.2f;

        [SerializeField] private Animator _animator;
        [SerializeField, Min(0.01f)] private float _runThresholdMetresPerSecond = DefaultRunThresholdMetresPerSecond;
        [SerializeField, Min(0f)] private float _aimHoldSeconds = DefaultAimHoldSeconds;
        [SerializeField, Min(0f)] private float _crossFadeSeconds = DefaultCrossFadeSeconds;

        private NavMeshAgent _agent;
        private UnitPosePicker _picker;
        private UnitCombat _combat;
        private float _lastShotTime = float.NegativeInfinity;
        private UnitPose _pose = UnitPose.Idle;
        private bool _isSubscribed;

        private bool IsConstructed => _combat != null;

        private void Awake()
        {
            _agent = GetComponent<NavMeshAgent>();
            _picker = new UnitPosePicker(_runThresholdMetresPerSecond, _aimHoldSeconds);
        }

        private void OnEnable()
        {
            Subscribe();
        }

        private void OnDisable()
        {
            Unsubscribe();
        }

        public void Construct(UnitCombat combat)
        {
            if (IsConstructed)
            {
                throw new InvalidOperationException($"{name} is already constructed.");
            }

            if (_animator == null)
            {
                throw new InvalidOperationException($"{name} needs an Animator assigned.");
            }

            _combat = combat ?? throw new ArgumentNullException(nameof(combat));
            Subscribe();
        }

        private void Update()
        {
            if (!IsConstructed)
            {
                return;
            }

            UnitPose pose = _picker.Pick(CurrentInput());
            if (pose == _pose)
            {
                return;
            }

            _pose = pose;
            _animator.CrossFadeInFixedTime(pose.ToString(), _crossFadeSeconds);
        }

        private UnitPoseInput CurrentInput()
        {
            float speed = _agent.enabled ? _agent.velocity.magnitude : 0f;
            bool isInCover = _combat.Protection.HitChanceMultiplier < 1f;
            return new UnitPoseInput(speed, Time.time - _lastShotTime, isInCover);
        }

        private void OnShot(ICombatTarget target)
        {
            _lastShotTime = Time.time;
        }

        private void Subscribe()
        {
            if (!IsConstructed || _isSubscribed)
            {
                return;
            }

            _combat.Fired += OnShot;
            _combat.Missed += OnShot;
            _isSubscribed = true;
        }

        private void Unsubscribe()
        {
            if (!_isSubscribed)
            {
                return;
            }

            _combat.Fired -= OnShot;
            _combat.Missed -= OnShot;
            _isSubscribed = false;
        }
    }
}
