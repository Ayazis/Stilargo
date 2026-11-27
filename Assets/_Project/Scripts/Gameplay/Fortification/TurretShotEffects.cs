using System;
using UnityEngine;

namespace Stilargo.Gameplay.Fortification
{
    /// <summary>
    /// Per-shot feedback on a turret model: a brief muzzle flash (light and glow) and the barrels kicking back
    /// along their axis, then easing forward again.
    /// </summary>
    public sealed class TurretShotEffects : MonoBehaviour
    {
        private const float DefaultFlashSeconds = 0.05f;
        private const float DefaultRecoilMetres = 0.12f;
        private const float DefaultRecoverySeconds = 0.18f;
        private const float FullTurnDegrees = 360f;

        [SerializeField] private Transform _barrels;
        [SerializeField] private Transform _muzzle;
        [SerializeField] private GameObject _flash;
        [SerializeField, Min(0.01f)] private float _flashSeconds = DefaultFlashSeconds;
        [SerializeField, Min(0f)] private float _recoilMetres = DefaultRecoilMetres;
        [SerializeField, Min(0.01f)] private float _recoverySeconds = DefaultRecoverySeconds;

        private Vector3 _barrelsRest;
        private Vector3 _kickDirection;
        private float _flashRemaining;
        private float _recoilRemaining;

        private void Awake()
        {
            if (_barrels == null || _muzzle == null || _flash == null)
            {
                throw new InvalidOperationException($"{nameof(TurretShotEffects)} on '{name}' needs its barrels, muzzle and flash.");
            }

            _barrelsRest = _barrels.localPosition;
            _flash.SetActive(false);
        }

        public void Play()
        {
            // Measured per shot in the barrels' parent space, because the barrels' tilt changes between shots.
            Vector3 backward = _barrels.position - _muzzle.position;
            _kickDirection = _barrels.parent.InverseTransformDirection(backward).normalized;
            _recoilRemaining = _recoverySeconds;
            _flashRemaining = _flashSeconds;
            _flash.transform.Rotate(Vector3.forward, UnityEngine.Random.Range(0f, FullTurnDegrees), Space.Self);
            _flash.SetActive(true);
        }

        private void LateUpdate()
        {
            if (_flashRemaining > 0f)
            {
                _flashRemaining -= Time.deltaTime;
                _flash.SetActive(_flashRemaining > 0f);
            }

            if (_recoilRemaining <= 0f)
            {
                return;
            }

            _recoilRemaining = Mathf.Max(0f, _recoilRemaining - Time.deltaTime);
            float kick = _recoilMetres * (_recoilRemaining / _recoverySeconds);
            _barrels.localPosition = _barrelsRest + (_kickDirection * kick);
        }
    }
}
