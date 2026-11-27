using System;
using System.Linq;
using NUnit.Framework;
using Stilargo.Core.Audio;

namespace Stilargo.Tests.EditMode.Audio
{
    public sealed class SoundRecipeTests
    {
        private const int SampleRate = 22050;

        private static readonly SoundCue[] AllCues = (SoundCue[])Enum.GetValues(typeof(SoundCue));

        [TestCaseSource(nameof(AllCues))]
        public void EveryCue_RendersAudibleSamplesWithinRange(SoundCue cue)
        {
            float[] samples = new SoundLibrary().RecipeFor(cue).Render(SampleRate);

            Assert.IsNotEmpty(samples);
            Assert.IsTrue(samples.All(sample => !float.IsNaN(sample) && sample >= -1f && sample <= 1f));
            Assert.Greater(samples.Max(Math.Abs), 0.1f);
        }

        [TestCaseSource(nameof(AllCues))]
        public void EveryCue_SoundsTheSameEveryTime(SoundCue cue)
        {
            var library = new SoundLibrary();

            CollectionAssert.AreEqual(library.RecipeFor(cue).Render(SampleRate), library.RecipeFor(cue).Render(SampleRate));
        }

        [Test]
        public void ChimeRecipe_RejectsNoNotes()
        {
            Assert.That(() => new ChimeRecipe(new float[0], 0.1f), Throws.ArgumentException);
        }

        [Test]
        public void VolumeOf_MultipliesMasterAndChannel()
        {
            var settings = new AudioSettings().WithVolumes(0.5f, 0.8f, 0.4f);

            Assert.AreEqual(0.4f, settings.VolumeOf(SoundChannel.Effects), 0.0001f);
            Assert.AreEqual(0.2f, settings.VolumeOf(SoundChannel.Interface), 0.0001f);
        }

        [Test]
        public void WithFalloff_RejectsSilenceCloserThanFullVolume()
        {
            Assert.That(() => new AudioSettings().WithFalloff(50f, 40f), Throws.InstanceOf<ArgumentOutOfRangeException>());
        }

        [Test]
        public void WithVolumes_RejectsVolumeAboveOne()
        {
            Assert.That(() => new AudioSettings().WithVolumes(1.5f, 1f, 1f), Throws.InstanceOf<ArgumentOutOfRangeException>());
        }
    }
}
