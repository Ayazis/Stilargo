using System;
using NUnit.Framework;
using Stilargo.Core.Selection;

namespace Stilargo.Tests.EditMode.Selection
{
    public sealed class UnitSelectionTests
    {
        private UnitSelection _selection;
        private FakeSelectable _first;
        private FakeSelectable _second;
        private FakeSelectable _third;
        private int _changedCount;

        [SetUp]
        public void SetUp()
        {
            _selection = new UnitSelection();
            _first = new FakeSelectable();
            _second = new FakeSelectable();
            _third = new FakeSelectable();
            _changedCount = 0;
            _selection.Changed += () => _changedCount++;
        }

        [Test]
        public void New_IsEmpty()
        {
            Assert.That(_selection.Selected, Is.Empty);
        }

        [Test]
        public void Replace_FromEmpty_SelectsAllAndRaisesOnce()
        {
            _selection.Replace(new[] { _first, _second });

            Assert.That(_selection.Selected, Is.EqualTo(new[] { _first, _second }));
            Assert.That(_first.IsSelected && _second.IsSelected, Is.True);
            Assert.That(_changedCount, Is.EqualTo(1));
        }

        [Test]
        public void Replace_DeselectsOnlyUnitsNotInNewSet()
        {
            _selection.Replace(new[] { _first, _second });

            _selection.Replace(new[] { _second, _third });

            Assert.That(_selection.Selected, Is.EquivalentTo(new[] { _second, _third }));
            Assert.That(_first.IsSelected, Is.False);
            Assert.That(_first.DeselectCount, Is.EqualTo(1));
            Assert.That(_second.DeselectCount, Is.Zero);
            Assert.That(_second.SelectCount, Is.EqualTo(1));
            Assert.That(_third.IsSelected, Is.True);
        }

        [Test]
        public void Replace_SameSet_DoesNotRaiseOrReselect()
        {
            _selection.Replace(new[] { _first, _second });
            _changedCount = 0;

            _selection.Replace(new[] { _second, _first });

            Assert.That(_changedCount, Is.Zero);
            Assert.That(_first.SelectCount, Is.EqualTo(1));
            Assert.That(_first.DeselectCount, Is.Zero);
        }

        [Test]
        public void Replace_WithEmpty_ClearsSelection()
        {
            _selection.Replace(new[] { _first });

            _selection.Replace(Array.Empty<ISelectable>());

            Assert.That(_selection.Selected, Is.Empty);
            Assert.That(_first.IsSelected, Is.False);
            Assert.That(_changedCount, Is.EqualTo(2));
        }

        [Test]
        public void Replace_WithDuplicates_KeepsOneEntry()
        {
            _selection.Replace(new[] { _first, _first });

            Assert.That(_selection.Selected.Count, Is.EqualTo(1));
            Assert.That(_first.SelectCount, Is.EqualTo(1));
        }

        [Test]
        public void Add_KeepsExistingAndSkipsDuplicates()
        {
            _selection.Replace(new[] { _first });

            _selection.Add(new[] { _first, _second });

            Assert.That(_selection.Selected, Is.EqualTo(new[] { _first, _second }));
            Assert.That(_first.SelectCount, Is.EqualTo(1));
            Assert.That(_second.IsSelected, Is.True);
            Assert.That(_changedCount, Is.EqualTo(2));
        }

        [Test]
        public void Add_OnlyAlreadySelected_DoesNotRaise()
        {
            _selection.Replace(new[] { _first });
            _changedCount = 0;

            _selection.Add(new[] { _first });

            Assert.That(_changedCount, Is.Zero);
        }

        [Test]
        public void Clear_DeselectsAllAndRaisesOnce()
        {
            _selection.Replace(new[] { _first, _second });
            _changedCount = 0;

            _selection.Clear();

            Assert.That(_selection.Selected, Is.Empty);
            Assert.That(_first.IsSelected || _second.IsSelected, Is.False);
            Assert.That(_changedCount, Is.EqualTo(1));
        }

        [Test]
        public void Clear_WhenEmpty_DoesNotRaise()
        {
            _selection.Clear();

            Assert.That(_changedCount, Is.Zero);
        }

        [Test]
        public void Remove_SelectedUnit_DeselectsItAndRaises()
        {
            _selection.Replace(new[] { _first, _second });
            _changedCount = 0;

            _selection.Remove(_first);

            Assert.That(_selection.Selected, Is.EqualTo(new[] { _second }));
            Assert.That(_first.IsSelected, Is.False);
            Assert.That(_selection.Contains(_first), Is.False);
            Assert.That(_changedCount, Is.EqualTo(1));
        }

        [Test]
        public void Remove_UnselectedUnit_DoesNothing()
        {
            _selection.Replace(new[] { _first });
            _changedCount = 0;

            _selection.Remove(_second);

            Assert.That(_changedCount, Is.Zero);
            Assert.That(_second.DeselectCount, Is.Zero);
        }

        [Test]
        public void NullArguments_Throw()
        {
            Assert.Throws<ArgumentNullException>(() => _selection.Replace(null));
            Assert.Throws<ArgumentNullException>(() => _selection.Add(null));
            Assert.Throws<ArgumentNullException>(() => _selection.Remove(null));
        }

        [Test]
        public void NullUnitInSet_Throws()
        {
            Assert.Throws<ArgumentException>(() => _selection.Replace(new ISelectable[] { _first, null }));
            Assert.Throws<ArgumentException>(() => _selection.Add(new ISelectable[] { null }));
        }
    }
}
