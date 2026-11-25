using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Stilargo.Core.City;

namespace Stilargo.Tests.EditMode.City
{
    public sealed class FacadeLayoutTests
    {
        private const float CellSize = 2f;
        private const float StoreyHeight = 3f;

        [Test]
        public void For_PlacesOneDoorPerSide()
        {
            IReadOnlyList<FacadePiece> pieces = Layout(width: 3, depth: 4, storeys: 2);

            Assert.AreEqual(4, pieces.Count(p => p.Kind == FacadePieceKind.Door));
        }

        [Test]
        public void For_PlacesOneCornicePerBayAtTheEaves()
        {
            IReadOnlyList<FacadePiece> pieces = Layout(width: 3, depth: 4, storeys: 2);
            FacadePiece[] cornices = pieces.Where(p => p.Kind == FacadePieceKind.Cornice).ToArray();

            Assert.AreEqual(2 * (3 + 4), cornices.Length);
            Assert.That(cornices.All(c => Math.Abs(c.Position.Y - (2 * StoreyHeight)) < 0.0001f));
        }

        [Test]
        public void For_FillsUpperStoreysWithWindows()
        {
            IReadOnlyList<FacadePiece> pieces = Layout(width: 3, depth: 3, storeys: 3);
            int bays = 4 * 3;

            Assert.AreEqual((bays * 3) - 4, pieces.Count(p => p.Kind == FacadePieceKind.Window));
        }

        [Test]
        public void For_SouthWallFacesSouthOnTheWall()
        {
            IReadOnlyList<FacadePiece> pieces = Layout(width: 1, depth: 1, storeys: 1);
            FacadePiece south = pieces.Single(p => p.FacingDegrees == 180f && p.Kind == FacadePieceKind.Door);

            Assert.AreEqual(2f * CellSize, south.Position.Z, 0.0001f);
            Assert.AreEqual(2.5f * CellSize, south.Position.X, 0.0001f);
        }

        private static IReadOnlyList<FacadePiece> Layout(int width, int depth, int storeys)
        {
            var plot = new Plot(new GridRect(new GridPosition(2, 2), width, depth), storeys);
            return FacadeLayout.For(plot, CellSize, StoreyHeight);
        }
    }
}
