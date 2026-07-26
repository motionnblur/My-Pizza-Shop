using System;
using Engineering.Scripts.Domain.Table;
using NUnit.Framework;

namespace Engineering.Tests
{
    public class TableModelTests
    {
        [Test]
        public void Constructor_RejectsZeroCapacity()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new TableModel(0));
        }

        [Test]
        public void Constructor_RejectsNegativeCapacity()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new TableModel(-1));
        }

        [Test]
        public void Constructor_SetsCapacityAndAvailableSeats()
        {
            var table = new TableModel(2);
            Assert.That(table.SeatCapacity, Is.EqualTo(2));
            Assert.That(table.AvailableSeatCount, Is.EqualTo(2));
            Assert.That(table.IsFull, Is.False);
        }

        [Test]
        public void TryReserveSeat_ReservesFirstAvailableSeat()
        {
            var table = new TableModel(2);
            var result = table.TryReserveSeat();
            Assert.That(result.Reserved, Is.True);
            Assert.That(result.SeatIndex, Is.EqualTo(0));
            Assert.That(table.AvailableSeatCount, Is.EqualTo(1));
            Assert.That(table.IsFull, Is.False);
        }

        [Test]
        public void TryReserveSeat_FillsBothSeats()
        {
            var table = new TableModel(2);
            var first = table.TryReserveSeat();
            var second = table.TryReserveSeat();
            Assert.That(first.Reserved, Is.True);
            Assert.That(first.SeatIndex, Is.EqualTo(0));
            Assert.That(second.Reserved, Is.True);
            Assert.That(second.SeatIndex, Is.EqualTo(1));
            Assert.That(table.AvailableSeatCount, Is.EqualTo(0));
            Assert.That(table.IsFull, Is.True);
        }

        [Test]
        public void TryReserveSeat_FailsWhenAllSeatsOccupied()
        {
            var table = new TableModel(2);
            table.TryReserveSeat();
            table.TryReserveSeat();
            var result = table.TryReserveSeat();
            Assert.That(result.Reserved, Is.False);
            Assert.That(result.SeatIndex, Is.EqualTo(-1));
            Assert.That(table.AvailableSeatCount, Is.EqualTo(0));
            Assert.That(table.IsFull, Is.True);
        }

        [Test]
        public void TryReleaseSeat_ReleasesOccupiedSeat()
        {
            var table = new TableModel(2);
            table.TryReserveSeat();
            table.TryReserveSeat();
            var result = table.TryReleaseSeat(0);
            Assert.That(result.Released, Is.True);
            Assert.That(result.SeatIndex, Is.EqualTo(0));
            Assert.That(table.AvailableSeatCount, Is.EqualTo(1));
            Assert.That(table.IsFull, Is.False);
        }

        [Test]
        public void TryReleaseSeat_RejectsInvalidIndex()
        {
            var table = new TableModel(2);
            var result = table.TryReleaseSeat(-1);
            Assert.That(result.Released, Is.False);
            Assert.That(result.SeatIndex, Is.EqualTo(-1));
        }

        [Test]
        public void TryReleaseSeat_RejectsOutOfBoundsIndex()
        {
            var table = new TableModel(2);
            var result = table.TryReleaseSeat(5);
            Assert.That(result.Released, Is.False);
            Assert.That(result.SeatIndex, Is.EqualTo(-1));
        }

        [Test]
        public void TryReleaseSeat_RejectsUnoccupiedSeat()
        {
            var table = new TableModel(2);
            var result = table.TryReleaseSeat(0);
            Assert.That(result.Released, Is.False);
            Assert.That(result.SeatIndex, Is.EqualTo(-1));
        }

        [Test]
        public void ReleaseThenReserve_ReusesFreedSeat()
        {
            var table = new TableModel(2);
            table.TryReserveSeat();
            table.TryReserveSeat();
            table.TryReleaseSeat(0);
            var result = table.TryReserveSeat();
            Assert.That(result.Reserved, Is.True);
            Assert.That(result.SeatIndex, Is.EqualTo(0));
        }

        [Test]
        public void ReserveSeatResult_Full_HasCorrectValues()
        {
            var result = ReserveSeatResult.Full;
            Assert.That(result.Reserved, Is.False);
            Assert.That(result.SeatIndex, Is.EqualTo(-1));
        }

        [Test]
        public void ReleaseSeatResult_InvalidIndex_HasCorrectValues()
        {
            var result = ReleaseSeatResult.InvalidIndex;
            Assert.That(result.Released, Is.False);
            Assert.That(result.SeatIndex, Is.EqualTo(-1));
        }

        [Test]
        public void ReleaseSeatResult_NotOccupied_HasCorrectValues()
        {
            var result = ReleaseSeatResult.NotOccupied;
            Assert.That(result.Released, Is.False);
            Assert.That(result.SeatIndex, Is.EqualTo(-1));
        }

        [Test]
        public void SingleSeatTable_ReservesAndReleases()
        {
            var table = new TableModel(1);
            var reserve = table.TryReserveSeat();
            Assert.That(reserve.Reserved, Is.True);
            Assert.That(reserve.SeatIndex, Is.EqualTo(0));
            Assert.That(table.IsFull, Is.True);

            var release = table.TryReleaseSeat(0);
            Assert.That(release.Released, Is.True);
            Assert.That(table.IsFull, Is.False);
        }

        [Test]
        public void SingleSeatTable_FailsWhenOccupied()
        {
            var table = new TableModel(1);
            table.TryReserveSeat();
            var result = table.TryReserveSeat();
            Assert.That(result.Reserved, Is.False);
        }
    }
}
