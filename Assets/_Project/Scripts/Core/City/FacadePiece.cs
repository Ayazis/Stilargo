using System.Numerics;

namespace Stilargo.Core.City
{
    /// <summary>
    /// One kit piece on a building's outer wall. <see cref="Position"/> is the bottom centre of its bay on the wall
    /// surface in world metres; the piece faces outwards along <see cref="FacingDegrees"/> (0 = +Z, 90 = +X).
    /// </summary>
    public readonly struct FacadePiece
    {
        public FacadePiece(FacadePieceKind kind, Vector3 position, float facingDegrees)
        {
            Kind = kind;
            Position = position;
            FacingDegrees = facingDegrees;
        }

        public FacadePieceKind Kind { get; }

        public Vector3 Position { get; }

        public float FacingDegrees { get; }
    }
}
