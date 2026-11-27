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

        // Measured from the run clip by the scene creator; this default is only a stand-in until then.
        private const float DefaultRunClipMetresPerSecond = 3.6f;

        // Outside this range a sped-up or slowed-down run looks wrong, so the feet slip a little instead.
        private const float MinRunPlaybackRate = 0.5f;
        private const float MaxRunPlaybackRate = 1.6f;

        /// <summary>The Animator float that sets how fast the run state plays.</summary>
        public const string RunSpeedParameter = "RunSpeed";

        [SerializeField] private Animator _animator;
        [SerializeField, Min(0.01f)] private float _runThresholdMetresPerSecond = DefaultRunThresholdMetresPerSecond;
        [SerializeField, Min(0f)] private float _aimHoldSeconds = DefaultAimHoldSeconds;
        [SerializeField, Min(0f)] private float _crossFadeSeconds = DefaultCrossFadeSeconds;
        [SerializeField, Min(0.1f)] private float _runClipMetresPerSecond = DefaultRunClipMetresPerSecond;

        private NavMeshAgent _agent;
        private UnitPosePicker _picker;
        private UnitCombat _combat;
        private float _lastShotTime = float.NegativeInfinity;
        private UnitPose _pose = UnitPose.Idle;
        private bool _isSubscribed;

        /// <summary>The avatar that is seen; it stays behind as a body when the unit dies.</summary>
        public Transform Body => _animator.transform;

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

            float speed = CurrentSpeed();
            MatchRunToSpeed(speed);
            UnitPose pose = _picker.Pick(CurrentInput(speed));
            if (pose == _pose)
            {
                return;
            }

            _pose = pose;
            _animator.CrossFadeInFixedTime(pose.ToString(), _crossFadeSeconds);
        }

        // Plays the run as fast as the unit really moves, so its feet stay planted instead of gliding.
        private void MatchRunToSpeed(float speedMetresPerSecond)
        {
            float rate = Mathf.Clamp(speedMetresPerSecond / _runClipMetresPerSecond, MinRunPlaybackRate, MaxRunPlaybackRate);
            _animator.SetFloat(RunSpeedParameter, rate);
        }

        private float CurrentSpeed()
        {
            return _agent.enabled ? _agent.velocity.magnitude : 0f;
        }

        private UnitPoseInput CurrentInput(float speedMetresPerSecond)
        {
            bool isInCover = _combat.Protection.HitChanceMultiplier < 1f;
            return new UnitPoseInput(speedMetresPerSecond, Time.time - _lastShotTime, isInCover);
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
