using System.Numerics;

namespace Stilargo.Core.Audio
{
    public interface ISoundPlayer
    {
        /// <summary>Plays a sound heard the same everywhere, such as a button click or the victory chime.</summary>
        void Play(SoundCue cue);

        /// <summary>Plays a sound from a point in the world, so it fades with distance from the camera.</summary>
        void PlayAt(SoundCue cue, Vector3 position);
    }
}
