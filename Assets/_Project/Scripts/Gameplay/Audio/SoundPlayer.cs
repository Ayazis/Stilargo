using System;
using System.Collections.Generic;
using Stilargo.Core.Audio;
using Stilargo.Gameplay.Units;
using UnityEngine;
using GameAudioSettings = Stilargo.Core.Audio.AudioSettings;

namespace Stilargo.Gameplay.Audio
{
    /// <summary>
    /// Plays the game's sounds: world sounds from a pool of 3D sources so they fade with distance from the camera,
    /// and interface sounds flat. Each world sound gets a slightly different pitch,
    /// so a firefight never sounds like one sample on repeat.
    /// </summary>
    public sealed class SoundPlayer : MonoBehaviour, ISoundPlayer
    {
        private const int DefaultWorldVoices = 24;
        private const float DefaultPitchSpread = 0.06f;
        private const float FlatBlend = 0f;
        private const float WorldBlend = 1f;
        private const string WorldVoiceName = "World Voice";

        [SerializeField, Min(1)] private int _worldVoices = DefaultWorldVoices;
        [SerializeField, Range(0f, 0.5f)] private float _pitchSpread = DefaultPitchSpread;

        private readonly List<AudioSource> _voices = new List<AudioSource>();
        private readonly System.Random _random = new System.Random();
        private SoundBank _bank;
        private GameAudioSettings _settings;
        private AudioSource _flat;
        private int _nextVoice;

        public void Construct(SoundBank bank, GameAudioSettings settings)
        {
            if (_bank != null)
            {
                throw new InvalidOperationException($"{name} is already constructed.");
            }

            _bank = bank ?? throw new ArgumentNullException(nameof(bank));
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));
            _flat = CreateSource(gameObject, FlatBlend);
            for (int index = 0; index < _worldVoices; index++)
            {
                var voice = new GameObject(WorldVoiceName);
                voice.transform.SetParent(transform, false);
                _voices.Add(CreateSource(voice, WorldBlend));
            }
        }

        public void Play(SoundCue cue)
        {
            _flat.PlayOneShot(_bank.ClipFor(cue), _settings.VolumeOf(_bank.ChannelOf(cue)));
        }

        public void PlayAt(SoundCue cue, System.Numerics.Vector3 position)
        {
            AudioSource voice = _voices[_nextVoice];
            _nextVoice = (_nextVoice + 1) % _voices.Count;
            voice.transform.position = position.ToUnity();
            voice.clip = _bank.ClipFor(cue);
            voice.volume = _settings.VolumeOf(_bank.ChannelOf(cue));
            voice.pitch = 1f + (((float)_random.NextDouble() - 0.5f) * 2f * _pitchSpread);
            voice.Play();
        }

        private void OnDestroy()
        {
            if (_bank != null)
            {
                _bank.Dispose();
            }
        }

        private AudioSource CreateSource(GameObject owner, float spatialBlend)
        {
            var source = owner.AddComponent<AudioSource>();
            source.playOnAwake = false;
            source.spatialBlend = spatialBlend;
            source.rolloffMode = AudioRolloffMode.Linear;
            source.minDistance = _settings.FullVolumeDistanceMetres;
            source.maxDistance = _settings.SilentDistanceMetres;
            source.dopplerLevel = 0f;
            return source;
        }
    }
}
