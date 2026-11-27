using UnityEngine;

namespace Stilargo.Gameplay.Fortification
{
    /// <summary>
    /// The level platform a roof turret stands on. The model's origin is the top of the deck; the deck reaches down
    /// far enough to meet a pitched roof, so it is placed with its top just above the highest tile under it.
    /// </summary>
    public sealed class TurretDeck : MonoBehaviour
    {
        [SerializeField, Min(0.1f)] private float _halfSizeMetres = 0.7f;

        public float HalfSizeMetres => _halfSizeMetres;
    }
}
