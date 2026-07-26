using System.Collections;
using System.Reflection;
using Engineering.Scripts.Domain.Table;
using Engineering.Scripts.Mono.Actors.CustomerQueue;
using Engineering.Scripts.Mono.Actors.Table;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.TestTools;

namespace Engineering.Tests
{
    public class TablePlayModeTests
    {
        private readonly System.Collections.Generic.List<GameObject> _toCleanup =
            new System.Collections.Generic.List<GameObject>();

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            foreach (var go in _toCleanup)
            {
                if (go != null)
                    Object.Destroy(go);
            }
            _toCleanup.Clear();
            yield return null;
        }

        [UnityTest]
        public IEnumerator Table_AutoInitializesFromSeatTransforms()
        {
            var root = new GameObject("TableAutoInit");
            _toCleanup.Add(root);

            var seatA = new GameObject("SeatA");
            seatA.transform.SetParent(root.transform);
            var seatB = new GameObject("SeatB");
            seatB.transform.SetParent(root.transform);

            var table = root.AddComponent<Table>();
            SetPrivateField(table, "seatTransforms", new[] { seatA.transform, seatB.transform });

            root.SetActive(true);
            yield return null;

            Assert.That(table.SeatCount, Is.EqualTo(2));
            Assert.That(table.HasAvailableSeat, Is.True);

            var r1 = table.TryReserveSeat();
            Assert.That(r1.Reserved, Is.True);
            Assert.That(table.HasAvailableSeat, Is.True);

            var r2 = table.TryReserveSeat();
            Assert.That(r2.Reserved, Is.True);
            Assert.That(table.HasAvailableSeat, Is.False);

            var r3 = table.TryReserveSeat();
            Assert.That(r3.Reserved, Is.False);
        }

        [UnityTest]
        public IEnumerator TableManager_ReservesAcrossMultipleTables()
        {
            var go1 = new GameObject("Table1");
            var seatA1 = new GameObject("SeatA1");
            seatA1.transform.SetParent(go1.transform);
            var seatB1 = new GameObject("SeatB1");
            seatB1.transform.SetParent(go1.transform);
            var t1 = go1.AddComponent<Table>();
            SetPrivateField(t1, "seatTransforms", new[] { seatA1.transform, seatB1.transform });
            _toCleanup.Add(go1);

            var go2 = new GameObject("Table2");
            var seatA2 = new GameObject("SeatA2");
            seatA2.transform.SetParent(go2.transform);
            var seatB2 = new GameObject("SeatB2");
            seatB2.transform.SetParent(go2.transform);
            var t2 = go2.AddComponent<Table>();
            SetPrivateField(t2, "seatTransforms", new[] { seatA2.transform, seatB2.transform });
            _toCleanup.Add(go2);

            var managerGO = new GameObject("TableManager");
            var manager = managerGO.AddComponent<TableManager>();
            SetPrivateField(manager, "tables", new[] { t1, t2 });
            _toCleanup.Add(managerGO);

            go1.SetActive(true);
            go2.SetActive(true);
            managerGO.SetActive(true);
            yield return null;

            Assert.That(manager.TryReserveSeat(out var tIdx1, out var sIdx1), Is.True);
            Assert.That(tIdx1, Is.EqualTo(0));
            Assert.That(sIdx1, Is.EqualTo(0));

            Assert.That(manager.TryReserveSeat(out var tIdx2, out var sIdx2), Is.True);

            Assert.That(manager.TryReserveSeat(out var tIdx3, out var sIdx3), Is.True);

            Assert.That(manager.TryReserveSeat(out var tIdx4, out var sIdx4), Is.True);

            Assert.That(manager.TryReserveSeat(out _, out _), Is.False,
                "All 4 seats across 2 tables should be occupied.");
        }

        [UnityTest]
        public IEnumerator TableManager_ReleaseSeatRaisesEvent()
        {
            var go = new GameObject("Table");
            var seat = new GameObject("Seat");
            seat.transform.SetParent(go.transform);
            var table = go.AddComponent<Table>();
            SetPrivateField(table, "seatTransforms", new[] { seat.transform });
            _toCleanup.Add(go);

            var managerGO = new GameObject("Manager");
            var manager = managerGO.AddComponent<TableManager>();
            SetPrivateField(manager, "tables", new[] { table });
            _toCleanup.Add(managerGO);

            go.SetActive(true);
            managerGO.SetActive(true);
            yield return null;

            var eventRaised = false;
            manager.SeatReleased += () => eventRaised = true;

            manager.TryReserveSeat(out var tIdx, out var sIdx);
            manager.ReleaseSeat(tIdx, sIdx);

            Assert.That(eventRaised, Is.True);
        }

        [UnityTest]
        public IEnumerator TableManager_ReleaseInvalidSeatDoesNotRaiseEvent()
        {
            var go = new GameObject("Table");
            var seat = new GameObject("Seat");
            seat.transform.SetParent(go.transform);
            var table = go.AddComponent<Table>();
            SetPrivateField(table, "seatTransforms", new[] { seat.transform });
            _toCleanup.Add(go);

            var managerGO = new GameObject("Manager");
            var manager = managerGO.AddComponent<TableManager>();
            SetPrivateField(manager, "tables", new[] { table });
            _toCleanup.Add(managerGO);

            go.SetActive(true);
            managerGO.SetActive(true);
            yield return null;

            var eventRaised = false;
            manager.SeatReleased += () => eventRaised = true;

            manager.ReleaseSeat(0, 0);

            Assert.That(eventRaised, Is.False,
                "Releasing an unoccupied seat should not raise SeatReleased.");
        }

        [UnityTest]
        public IEnumerator GetSeatTransform_ReturnsCorrectTransform()
        {
            var go = new GameObject("Table");
            var seatA = new GameObject("SeatA");
            seatA.transform.SetParent(go.transform);
            var seatB = new GameObject("SeatB");
            seatB.transform.SetParent(go.transform);
            var table = go.AddComponent<Table>();
            SetPrivateField(table, "seatTransforms", new[] { seatA.transform, seatB.transform });
            _toCleanup.Add(go);

            go.SetActive(true);
            yield return null;

            Assert.That(table.GetSeatTransform(0), Is.SameAs(seatA.transform));
            Assert.That(table.GetSeatTransform(1), Is.SameAs(seatB.transform));
            Assert.That(table.GetSeatTransform(-1), Is.Null);
            Assert.That(table.GetSeatTransform(2), Is.Null);
        }

        private static void SetPrivateField(object target, string fieldName, object value)
        {
            var field = target.GetType().GetField(fieldName,
                BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(field, Is.Not.Null,
                $"Expected {target.GetType().Name} to define '{fieldName}'.");
            field.SetValue(target, value);
        }
    }
}
