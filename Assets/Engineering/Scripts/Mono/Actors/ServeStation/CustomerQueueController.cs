using System.Collections.Generic;
using Engineering.Scripts.Domain.CustomerQueue;
using Engineering.Scripts.Mono.Actors.CustomerQueue;
using UnityEngine;

namespace Engineering.Scripts.Mono.Actors.ServeStation
{
    public class CustomerQueueController : MonoBehaviour
    {
        [SerializeField] private Transform[] queueSlots;

        private readonly List<CustomerBot> _customers = new List<CustomerBot>();
        private CustomerQueueModel _model;

        public int CustomerCount => _model?.Count ?? 0;
        public CustomerBot FrontCustomer => _customers.Count > 0 ? _customers[0] : null;

        public void Initialize(int capacity)
        {
            if (_model == null)
                _model = new CustomerQueueModel(capacity);
            else
                _model.UpdateCapacity(capacity);
        }

        public bool TryRegister(CustomerBot customer)
        {
            if (customer == null || _model == null)
                return false;

            var orderModel = customer.OrderModel;
            if (orderModel == null)
                return false;

            var slotIndex = _customers.Count;
            if (queueSlots == null || slotIndex >= queueSlots.Length || queueSlots[slotIndex] == null)
                return false;

            var enqueueResult = _model.TryEnqueue(orderModel);
            if (!enqueueResult.Accepted)
                return false;

            _customers.Add(customer);
            customer.AssignQueueSlot(queueSlots[slotIndex], slotIndex);
            return true;
        }

        public void RemoveFrontCustomer()
        {
            if (_customers.Count == 0)
                return;

            _model.TryRemoveFront();
            var front = _customers[0];
            _customers.RemoveAt(0);
            Destroy(front.gameObject);

            ReassignSlots();
        }

        public CustomerBot DequeueFrontCustomer()
        {
            if (_customers.Count == 0)
                return null;

            _model.TryRemoveFront();
            var front = _customers[0];
            _customers.RemoveAt(0);
            ReassignSlots();
            return front;
        }

        public CustomerBot GetFirstWaitingCustomer()
        {
            for (var i = 0; i < _customers.Count; i++)
            {
                if (_customers[i].IsWaitingForTable)
                    return _customers[i];
            }
            return null;
        }

        private void ReassignSlots()
        {
            if (queueSlots == null)
                return;

            for (var i = 0; i < _customers.Count; i++)
            {
                if (i < queueSlots.Length && queueSlots[i] != null)
                    _customers[i].AssignQueueSlot(queueSlots[i], i);
            }
        }
    }
}
