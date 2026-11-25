using System;
using Stilargo.Core.Income;
using UnityEngine;

namespace Stilargo.Gameplay.Fortification
{
    /// <summary>Advances claimed-house income and wear with game time, so pausing the match pauses both.</summary>
    public sealed class HouseIncomeTicker : MonoBehaviour
    {
        private HouseIncome _income;

        public void Construct(HouseIncome income)
        {
            if (_income != null)
            {
                throw new InvalidOperationException($"{nameof(HouseIncomeTicker)} is already constructed.");
            }

            _income = income ?? throw new ArgumentNullException(nameof(income));
        }

        private void Update()
        {
            if (_income == null)
            {
                return;
            }

            _income.Tick(Time.deltaTime);
        }
    }
}
