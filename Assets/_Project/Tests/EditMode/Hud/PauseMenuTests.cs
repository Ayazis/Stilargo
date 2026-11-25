using System;
using NUnit.Framework;
using Stilargo.Core.Economy;
using Stilargo.Core.Hud;

namespace Stilargo.Tests.EditMode.Hud
{
    public sealed class PauseMenuTests
    {
        private Wallet _wallet;
        private FakeGameQuitter _quitter;
        private PauseMenu _menu;

        [SetUp]
        public void SetUp()
        {
            _wallet = new Wallet();
            _quitter = new FakeGameQuitter();
            _menu = new PauseMenu(_wallet, _quitter);
        }

        [Test]
        public void NewMenu_IsClosed()
        {
            Assert.IsFalse(_menu.IsOpen);
        }

        [Test]
        public void HandleEscape_TogglesOpenAndRaisesEvent()
        {
            bool? reported = null;
            _menu.OpenChanged += isOpen => reported = isOpen;

            _menu.HandleEscape();
            Assert.IsTrue(_menu.IsOpen);
            Assert.AreEqual(true, reported);

            _menu.HandleEscape();
            Assert.IsFalse(_menu.IsOpen);
            Assert.AreEqual(false, reported);
        }

        [Test]
        public void HandleEscape_OnSettings_GoesBackToMainAndStaysOpen()
        {
            _menu.HandleEscape();
            _menu.ShowSettings();

            _menu.HandleEscape();

            Assert.IsTrue(_menu.IsOpen);
            Assert.AreEqual(PauseMenuPage.Main, _menu.Page);
        }

        [Test]
        public void Reopening_StartsOnMainPage()
        {
            _menu.HandleEscape();
            _menu.ShowSettings();
            _menu.Resume();

            _menu.HandleEscape();

            Assert.AreEqual(PauseMenuPage.Main, _menu.Page);
        }

        [Test]
        public void ShowSettings_WhileClosed_Throws()
        {
            Assert.Throws<InvalidOperationException>(() => _menu.ShowSettings());
        }

        [Test]
        public void ApplyMoney_ValidAmount_SetsBalance()
        {
            _wallet.Deposit(50);

            _menu.ApplyMoney("1234");

            Assert.AreEqual(1234, _wallet.Balance);
        }

        [Test]
        public void ApplyMoney_Zero_EmptiesWallet()
        {
            _wallet.Deposit(50);

            _menu.ApplyMoney("0");

            Assert.AreEqual(0, _wallet.Balance);
        }

        [TestCase("")]
        [TestCase("abc")]
        [TestCase("-5")]
        [TestCase("1.5")]
        public void ApplyMoney_Invalid_KeepsBalanceAndExplains(string typed)
        {
            _wallet.Deposit(50);

            string message = _menu.ApplyMoney(typed);

            Assert.AreEqual(50, _wallet.Balance);
            Assert.AreEqual(PauseMenu.InvalidMoneyMessage, message);
        }

        [Test]
        public void Quit_AsksTheQuitter()
        {
            _menu.Quit();

            Assert.IsTrue(_quitter.WasQuitCalled);
        }

        private sealed class FakeGameQuitter : IGameQuitter
        {
            public bool WasQuitCalled { get; private set; }

            public void Quit()
            {
                WasQuitCalled = true;
            }
        }
    }
}
