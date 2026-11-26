using System;

namespace Stilargo.Core.Audio
{
    /// <summary>Volumes and how far sounds carry, loaded from <c>Audio.json</c>. Volumes are in [0, 1].</summary>
    public sealed class AudioSettings
    {
        public const float DefaultMasterVolume = 0.8f;
        public const float DefaultEffectsVolume = 0.8f;
        public const float DefaultInterfaceVolume = 0.6f;
        public const float DefaultFullVolumeDistanceMetres = 25f;
        public const float DefaultSilentDistanceMetres = 110f;

        public float MasterVolume { get; private set; } = DefaultMasterVolume;

        public float EffectsVolume { get; private set; } = DefaultEffectsVolume;

        public float InterfaceVolume { get; private set; } = DefaultInterfaceVolume;

        /// <summary>World sounds play at full volume within this distance of the camera, which hangs about 30 m up.</summary>
        public float FullVolumeDistanceMetres { get; private set; } = DefaultFullVolumeDistanceMetres;

        /// <summary>World sounds fade out completely at this distance from the camera.</summary>
        public float SilentDistanceMetres { get; private set; } = DefaultSilentDistanceMetres;

        public AudioSettings WithVolumes(float master, float effects, float interfaceVolume)
        {
            MasterVolume = RequireVolume(master, nameof(master));
            EffectsVolume = RequireVolume(effects, nameof(effects));
            InterfaceVolume = RequireVolume(interfaceVolume, nameof(interfaceVolume));
            return this;
        }

        public AudioSettings WithFalloff(float fullVolumeDistanceMetres, float silentDistanceMetres)
        {
            if (!(fullVolumeDistanceMetres > 0f) || float.IsInfinity(fullVolumeDistanceMetres))
            {
                throw new ArgumentOutOfRangeException(nameof(fullVolumeDistanceMetres), fullVolumeDistanceMetres, "Must be a positive number.");
            }

            if (!(silentDistanceMetres > fullVolumeDistanceMetres) || float.IsInfinity(silentDistanceMetres))
            {
                throw new ArgumentOutOfRangeException(nameof(silentDistanceMetres), silentDistanceMetres, "Must be further than the full-volume distance.");
            }

            FullVolumeDistanceMetres = fullVolumeDistanceMetres;
            SilentDistanceMetres = silentDistanceMetres;
            return this;
        }

        /// <summary>The final volume of a sound on <paramref name="channel"/>: the master volume times the channel volume.</summary>
        public float VolumeOf(SoundChannel channel)
        {
            switch (channel)
            {
                case SoundChannel.Effects:
                    return MasterVolume * EffectsVolume;
                case SoundChannel.Interface:
                    return MasterVolume * InterfaceVolume;
                default:
                    throw new ArgumentOutOfRangeException(nameof(channel), channel, "Unknown sound channel.");
            }
        }

        private static float RequireVolume(float value, string name)
        {
            if (!(value >= 0f && value <= 1f))
            {
                throw new ArgumentOutOfRangeException(name, value, "Volume must be in [0, 1].");
            }

            return value;
        }
    }
}
