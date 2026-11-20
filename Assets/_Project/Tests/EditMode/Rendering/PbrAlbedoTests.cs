using System;
using NUnit.Framework;
using Stilargo.Editor.Rendering;
using UnityEngine;

namespace Stilargo.Tests.EditMode.Rendering
{
    public sealed class PbrAlbedoTests
    {
        [Test]
        public void MidGrey_IsPlausible()
        {
            Assert.IsTrue(PbrAlbedo.IsPlausible(new Color(0.5f, 0.5f, 0.5f)));
        }

        [Test]
        public void PureWhite_IsNotPlausible()
        {
            Assert.IsFalse(PbrAlbedo.IsPlausible(Color.white));
        }

        [Test]
        public void PureBlack_IsNotPlausible()
        {
            Assert.IsFalse(PbrAlbedo.IsPlausible(Color.black));
        }

        [Test]
        public void OneFullChannel_IsNotPlausible()
        {
            Assert.IsFalse(PbrAlbedo.IsPlausible(new Color(1f, 0.8f, 0.1f)));
        }

        [Test]
        public void SaturatedColourWithADarkChannel_IsPlausible()
        {
            Assert.IsTrue(PbrAlbedo.IsPlausible(new Color(0.9f, 0.7f, 0.1f)));
        }

        [Test]
        public void Bounds_AreInclusive()
        {
            float darkest = PbrAlbedo.DarkestChannel;
            Assert.IsTrue(PbrAlbedo.IsPlausible(new Color(darkest, darkest, darkest)));
            Assert.IsTrue(PbrAlbedo.IsPlausible(new Color(PbrAlbedo.BrightestChannel, 0.5f, 0.5f)));
        }

        [Test]
        public void Require_Implausible_Throws()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => PbrAlbedo.Require(Color.white));
        }
    }
}
