using System.Linq;
using NUnit.Framework;
using Stilargo.Core.Fog;
using Stilargo.Core.Hud;
using Stilargo.Core.Input;

namespace Stilargo.Tests.EditMode.Hud
{
    public sealed class GameOptionsTests
    {
        private CameraControlChoice _controls;
        private VisibilityGrid _fog;
        private GameOptions _options;

        [SetUp]
        public void SetUp()
        {
            _controls = new CameraControlChoice();
            _fog = new VisibilityGrid(4, 4, 1f);
            _options = new GameOptions(_controls, _fog);
        }

        [Test]
        public void ToggleFog_TurnsItOffAndRevealsEveryCell()
        {
            _options.ToggleFog();
            _fog.Refresh();

            Assert.IsFalse(_fog.IsFogEnabled);
            Assert.AreEqual(VisibilityState.Visible, _fog.GetState(3, 3));
            StringAssert.EndsWith("off", _options.FogLabel);
        }

        [Test]
        public void ToggleFog_Twice_BringsItBack()
        {
            _options.ToggleFog();
            _fog.Refresh();
            _options.ToggleFog();
            _fog.Refresh();

            Assert.IsTrue(_fog.IsFogEnabled);
            Assert.AreEqual(VisibilityState.Explored, _fog.GetState(3, 3));
        }

        [Test]
        public void NextCameraPreset_CyclesThroughAllAndWraps()
        {
            for (int index = 0; index < CameraControlPreset.All.Count; index++)
            {
                Assert.AreSame(CameraControlPreset.All[index], _controls.Current);
                _options.NextCameraPreset();
            }

            Assert.AreSame(CameraControlPreset.All[0], _controls.Current);
        }

        [Test]
        public void Presets_IncludeARightMouseOrbit()
        {
            Assert.IsTrue(CameraControlPreset.All.Any(preset => preset.OrbitButton == OrbitButton.Right));
        }

        [Test]
        public void CameraHints_FollowThePreset()
        {
            _options.NextCameraPreset();

            Assert.IsTrue(_options.CameraHints.Any(hint => hint.Keys.Contains("right mouse")));
            StringAssert.Contains(CameraControlPreset.RightMouse.Name, _options.CameraPresetLabel);
        }

        [Test]
        public void CameraHints_WithoutEdgeScroll_LeaveItOut()
        {
            Assert.IsFalse(CameraControlHints.For(CameraControlPreset.ClassicNoEdgeScroll).Any(hint => hint.Action.Contains("edge")));
        }
    }
}
