using System;
using UnityEngine;
using GameAudioSettings = Stilargo.Core.Audio.AudioSettings;

namespace Stilargo.Infrastructure.Config
{
    /// <summary>The shape of <c>Audio.json</c>. Defaults match a new <see cref="GameAudioSettings"/>.</summary>
    [Serializable]
    public sealed class AudioConfig
    {
        public const string FileName = "Audio.json";

        // JsonUtility maps field names to JSON keys, so these fields are named like the keys instead of _camelCase.
        [SerializeField] private float masterVolume = GameAudioSettings.DefaultMasterVolume;
        [SerializeField] private float effectsVolume = GameAudioSettings.DefaultEffectsVolume;
        [SerializeField] private float interfaceVolume = GameAudioSettings.DefaultInterfaceVolume;
        [SerializeField] private float fullVolumeDistanceMetres = GameAudioSettings.DefaultFullVolumeDistanceMetres;
        [SerializeField] private float silentDistanceMetres = GameAudioSettings.DefaultSilentDistanceMetres;

        public static GameAudioSettings Load()
        {
            return ConfigFile.Read<AudioConfig>(FileName).ToSettings();
        }

        public GameAudioSettings ToSettings()
        {
            try
            {
                return new GameAudioSettings()
                    .WithVolumes(masterVolume, effectsVolume, interfaceVolume)
                    .WithFalloff(fullVolumeDistanceMetres, silentDistanceMetres);
            }
            catch (ArgumentException exception)
            {
                throw new InvalidOperationException($"{FileName} is invalid: {exception.Message}", exception);
            }
        }
    }
}
