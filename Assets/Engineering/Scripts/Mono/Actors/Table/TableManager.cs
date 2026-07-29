using System;
using UnityEngine;

namespace Engineering.Scripts.Mono.Actors.Table
{
    public class TableManager : MonoBehaviour
    {
        [SerializeField] private Table[] tables;

        private bool _leftoversSubscribed;

        public event Action SeatReleased;
        public event Action LeftoversRemoved;

        private void EnsureSubscribed()
        {
            if (_leftoversSubscribed || tables == null)
                return;
            foreach (var table in tables)
            {
                if (table != null)
                    table.LeftoverStateChanged += OnTableLeftoverStateChanged;
            }
            _leftoversSubscribed = true;
        }

        private void OnTableLeftoverStateChanged()
        {
            LeftoversRemoved?.Invoke();
        }

        public bool TryReserveSeat(out int tableIndex, out int seatIndex)
        {
            EnsureSubscribed();
            if (tables != null)
            {
                for (var i = 0; i < tables.Length; i++)
                {
                    if (tables[i] != null && tables[i].HasAvailableSeat)
                    {
                        var result = tables[i].TryReserveSeat();
                        if (result.Reserved)
                        {
                            tableIndex = i;
                            seatIndex = result.SeatIndex;
                            return true;
                        }
                    }
                }
            }

            tableIndex = -1;
            seatIndex = -1;
            return false;
        }

        public bool TryReserveSeat(int expectedLeftoverCount, out int tableIndex, out int seatIndex)
        {
            EnsureSubscribed();
            if (tables != null)
            {
                for (var i = 0; i < tables.Length; i++)
                {
                    if (tables[i] != null && tables[i].HasAvailableSeat && tables[i].CanAcceptLeftovers(expectedLeftoverCount))
                    {
                        var result = tables[i].TryReserveSeat();
                        if (result.Reserved)
                        {
                            tableIndex = i;
                            seatIndex = result.SeatIndex;
                            return true;
                        }
                    }
                }
            }

            tableIndex = -1;
            seatIndex = -1;
            return false;
        }

        public void ReleaseSeat(int tableIndex, int seatIndex)
        {
            EnsureSubscribed();
            if (tables == null || tableIndex < 0 || tableIndex >= tables.Length || tables[tableIndex] == null)
                return;

            var result = tables[tableIndex].TryReleaseSeat(seatIndex);
            if (result.Released)
                SeatReleased?.Invoke();
        }

        public Transform GetSeatTransform(int tableIndex, int seatIndex)
        {
            EnsureSubscribed();
            if (tables == null || tableIndex < 0 || tableIndex >= tables.Length || tables[tableIndex] == null)
                return null;
            return tables[tableIndex].GetSeatTransform(seatIndex);
        }

        public void AddLeftoversToTable(int tableIndex, int pizzaCount)
        {
            EnsureSubscribed();
            if (tables == null || tableIndex < 0 || tableIndex >= tables.Length || tables[tableIndex] == null)
                return;

            tables[tableIndex].AddLeftovers(pizzaCount);
        }
    }
}
