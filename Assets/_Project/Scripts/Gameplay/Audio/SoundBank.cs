using System;
using System.Collections.Generic;
using Stilargo.Core.Audio;
using UnityEngine;

namespace Stilargo.Gameplay.Audio
{
    /// <summary>
    /// Renders every sound in the <see cref="SoundLibrary"/> into an AudioClip once, at startup, at the output's
    /// sample rate. The game ships no audio files: everything it plays is synthesised here.
    /// </summary>
    public sealed class SoundBank : IDisposable
    {
        private const int MonoChannels = 1;

        private readonly Dictionary<SoundCue, AudioClip> _clips = new Dictionary<SoundCue, AudioClip>();
        private readonly Dictionary<SoundCue, SoundChannel> _channels = new Dictionary<SoundCue, SoundChannel>();

        public SoundBank(SoundLibrary library, int sampleRate)
        {
            if (library == null)
            {
                throw new ArgumentNullException(nameof(library));
            }

            if (sampleRate <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(sampleRate), sampleRate, "Sample rate must be positive.");
            }

            foreach (SoundCue cue in Enum.GetValues(typeof(SoundCue)))
            {
                ISoundRecipe recipe = library.RecipeFor(cue);
                float[] samples = recipe.Render(sampleRate);
                AudioClip clip = AudioClip.Create(cue.ToString(), samples.Length, MonoChannels, sampleRate, false);
                clip.SetData(samples, 0);
                _clips.Add(cue, clip);
                _channels.Add(cue, recipe.Channel);
            }
        }

        public AudioClip ClipFor(SoundCue cue)
        {
            return _clips[cue];
        }

        public SoundChannel ChannelOf(SoundCue cue)
        {
            return _channels[cue];
        }

        public void Dispose()
        {
            foreach (AudioClip clip in _clips.Values)
            {
                UnityEngine.Object.Destroy(clip);
            }

            _clips.Clear();
        }
    }
}
