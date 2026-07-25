using System;
using System.IO;
using Engineering.Scripts.Domain.ServeStation;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace Engineering.Tests
{
    public class ServeStationModelTests
    {
        [Test]
        public void Constructor_RejectsNegativeMaximumStorage()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new ServeStationModel(-1, 10));
        }

        [Test]
        public void Constructor_RejectsNegativePrice()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new ServeStationModel(10, -1));
        }

        [Test]
        public void ZeroMaximumStorage_RejectsDeposits()
        {
            var model = new ServeStationModel(0, 10);
            Assert.That(model.CalculateDepositAmount(5), Is.EqualTo(0));
            Assert.That(model.Deposit(5), Is.EqualTo(0));
            Assert.That(model.StoredPizzaCount, Is.EqualTo(0));
        }

        [Test]
        public void Deposit_FullyAcceptsWithinCapacity()
        {
            var model = new ServeStationModel(10, 10);
            var accepted = model.Deposit(5);
            Assert.That(accepted, Is.EqualTo(5));
            Assert.That(model.StoredPizzaCount, Is.EqualTo(5));
        }

        [Test]
        public void Deposit_CapsAtRemainingCapacity()
        {
            var model = new ServeStationModel(5, 10);
            var accepted = model.Deposit(10);
            Assert.That(accepted, Is.EqualTo(5));
            Assert.That(model.StoredPizzaCount, Is.EqualTo(5));
        }

        [Test]
        public void ZeroDeposit_ChangesNothing()
        {
            var model = new ServeStationModel(10, 10);
            var accepted = model.Deposit(0);
            Assert.That(accepted, Is.EqualTo(0));
            Assert.That(model.StoredPizzaCount, Is.EqualTo(0));
        }

        [Test]
        public void NegativeDeposit_ChangesNothing()
        {
            var model = new ServeStationModel(10, 10);
            var accepted = model.Deposit(-5);
            Assert.That(accepted, Is.EqualTo(0));
            Assert.That(model.StoredPizzaCount, Is.EqualTo(0));
        }

        [Test]
        public void SequentialDeposits_AccumulateState()
        {
            var model = new ServeStationModel(10, 10);
            Assert.That(model.Deposit(3), Is.EqualTo(3));
            Assert.That(model.Deposit(4), Is.EqualTo(4));
            Assert.That(model.StoredPizzaCount, Is.EqualTo(7));
        }

        [Test]
        public void EmptyStorage_CannotServe()
        {
            var model = new ServeStationModel(10, 10);
            var result = model.TryServe(5);
            Assert.That(result.HasDelivery, Is.False);
            Assert.That(result.DeliveredPizzaCount, Is.EqualTo(0));
            Assert.That(result.MoneyEarned, Is.EqualTo(0));
            Assert.That(result.OrderCompleted, Is.False);
        }

        [Test]
        public void ZeroRemainingOrder_CannotServe()
        {
            var model = new ServeStationModel(10, 10);
            model.Deposit(5);
            var result = model.TryServe(0);
            Assert.That(result.HasDelivery, Is.False);
        }

        [Test]
        public void NegativeRemainingOrder_CannotServe()
        {
            var model = new ServeStationModel(10, 10);
            model.Deposit(5);
            var result = model.TryServe(-1);
            Assert.That(result.HasDelivery, Is.False);
        }

        [Test]
        public void PartialDelivery_ReducesStorageCorrectly()
        {
            var model = new ServeStationModel(10, 10);
            model.Deposit(5);
            var result = model.TryServe(8);
            Assert.That(result.DeliveredPizzaCount, Is.EqualTo(5));
            Assert.That(model.StoredPizzaCount, Is.EqualTo(0));
        }

        [Test]
        public void PartialDelivery_CalculatesRewardCorrectly()
        {
            var model = new ServeStationModel(10, 10);
            model.Deposit(3);
            var result = model.TryServe(5);
            Assert.That(result.MoneyEarned, Is.EqualTo(30));
        }

        [Test]
        public void PartialDelivery_ReportsOrderNotCompleted()
        {
            var model = new ServeStationModel(10, 10);
            model.Deposit(3);
            var result = model.TryServe(5);
            Assert.That(result.OrderCompleted, Is.False);
        }

        [Test]
        public void CompleteDelivery_ReportsOrderCompleted()
        {
            var model = new ServeStationModel(10, 10);
            model.Deposit(5);
            var result = model.TryServe(5);
            Assert.That(result.OrderCompleted, Is.True);
            Assert.That(result.DeliveredPizzaCount, Is.EqualTo(5));
            Assert.That(model.StoredPizzaCount, Is.EqualTo(0));
        }

        [Test]
        public void StorageLargerThanOrder_DeliversOnlyOrderAmount()
        {
            var model = new ServeStationModel(10, 10);
            model.Deposit(10);
            var result = model.TryServe(3);
            Assert.That(result.DeliveredPizzaCount, Is.EqualTo(3));
            Assert.That(model.StoredPizzaCount, Is.EqualTo(7));
        }

        [Test]
        public void ConfigurationUpdate_PreservesStoredPizzas()
        {
            var model = new ServeStationModel(10, 10);
            model.Deposit(5);
            model.UpdateConfiguration(20, 15);
            Assert.That(model.StoredPizzaCount, Is.EqualTo(5));
            Assert.That(model.MaxStoredPizzas, Is.EqualTo(20));
            Assert.That(model.PricePerPizza, Is.EqualTo(15));
        }

        [Test]
        public void LoweringCapacityBelowStoredCount_KeepsStoredPizzas()
        {
            var model = new ServeStationModel(10, 10);
            model.Deposit(8);
            model.UpdateConfiguration(5, 10);
            Assert.That(model.StoredPizzaCount, Is.EqualTo(8));
        }

        [Test]
        public void LoweringCapacityBelowStoredCount_MakesRemainingCapacityZero()
        {
            var model = new ServeStationModel(10, 10);
            model.Deposit(8);
            model.UpdateConfiguration(5, 10);
            Assert.That(model.RemainingCapacity, Is.EqualTo(0));
            Assert.That(model.CalculateDepositAmount(5), Is.EqualTo(0));
        }

        [Test]
        public void UpdatedPrice_AffectsNextServeResult()
        {
            var model = new ServeStationModel(10, 5);
            model.Deposit(5);
            model.UpdateConfiguration(10, 20);
            var result = model.TryServe(3);
            Assert.That(result.MoneyEarned, Is.EqualTo(60));
        }

        [Test]
        public void HasDelivery_IsFalseForNoneResult()
        {
            Assert.That(ServeResult.None.HasDelivery, Is.False);
        }

        [Test]
        public void HasDelivery_IsTrueForPositiveDelivery()
        {
            var result = new ServeResult(1, 10, false);
            Assert.That(result.HasDelivery, Is.True);
        }

        [Test]
        public void ArchitectureGuard_DomainFilesHaveNoUnityEngine()
        {
            var domainDir = "Assets/Engineering/Scripts/Domain/ServeStation";
            var files = Directory.GetFiles(domainDir, "*.cs");
            Assert.That(files.Length, Is.GreaterThanOrEqualTo(2));

            foreach (var file in files)
            {
                var text = File.ReadAllText(file);
                Assert.That(text, Does.Not.Contain("using UnityEngine"),
                    $"{file} must not depend on UnityEngine.");
            }
        }
    }
}
