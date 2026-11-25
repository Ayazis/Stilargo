using System;
using System.Linq;
using NUnit.Framework;
using Stilargo.Core.Hud;

namespace Stilargo.Tests.EditMode.Hud
{
    public sealed class CameraControlHintsTests
    {
        [Test]
        public void All_ListsEveryCameraAction()
        {
            string[] actions = CameraControlHints.All.Select(hint => hint.Action).ToArray();

            Assert.That(actions, Is.EqualTo(new[] { "Move", "Move (edge scroll)", "Rotate", "Orbit", "Zoom" }));
        }

        [Test]
        public void Constructor_WithoutKeys_Throws()
        {
            Assert.Throws<ArgumentException>(() => new ControlHint("Zoom", " "));
        }

        [Test]
        public void Constructor_WithoutAction_Throws()
        {
            Assert.Throws<ArgumentException>(() => new ControlHint(string.Empty, "Mouse wheel"));
        }
    }
}
