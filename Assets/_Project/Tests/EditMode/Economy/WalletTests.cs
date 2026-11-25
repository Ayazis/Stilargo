using System;
using NUnit.Framework;
using Stilargo.Core.Economy;

namespace Stilargo.Tests.EditMode.Economy
{
    public sealed class WalletTests
    {
        [Test]
        public void NewWallet_IsEmpty()
        {
            Assert.AreEqual(0, new Wallet().Balance);
        }

        [Test]
        public void Deposit_AddsToBalanceAndRaisesEvent()
        {
            Wallet wallet = new Wallet();
            int reported = -1;
            wallet.BalanceChanged += balance => reported = balance;

            wallet.Deposit(30);
            wallet.Deposit(12);

            Assert.AreEqual(42, wallet.Balance);
            Assert.AreEqual(42, reported);
        }

        [Test]
        public void TrySpend_Affordable_DeductsAndReturnsTrue()
        {
            Wallet wallet = new Wallet();
            wallet.Deposit(50);

            Assert.IsTrue(wallet.TrySpend(20));
            Assert.AreEqual(30, wallet.Balance);
        }

        [Test]
        public void TrySpend_TooExpensive_LeavesBalanceAndReturnsFalse()
        {
            Wallet wallet = new Wallet();
            wallet.Deposit(10);
            bool raised = false;
            wallet.BalanceChanged += _ => raised = true;

            Assert.IsFalse(wallet.TrySpend(11));
            Assert.AreEqual(10, wallet.Balance);
            Assert.IsFalse(raised);
        }

        [TestCase(0)]
        [TestCase(-5)]
        public void NonPositiveAmounts_Throw(int amount)
        {
            Wallet wallet = new Wallet();

            Assert.Throws<ArgumentOutOfRangeException>(() => wallet.Deposit(amount));
            Assert.Throws<ArgumentOutOfRangeException>(() => wallet.TrySpend(amount));
        }

        [Test]
        public void SetBalance_ReplacesBalanceAndRaisesEvent()
        {
            Wallet wallet = new Wallet();
            wallet.Deposit(30);
            int reported = -1;
            wallet.BalanceChanged += balance => reported = balance;

            wallet.SetBalance(500);

            Assert.AreEqual(500, wallet.Balance);
            Assert.AreEqual(500, reported);
        }

        [Test]
        public void SetBalance_Negative_Throws()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new Wallet().SetBalance(-1));
        }
    }
}
