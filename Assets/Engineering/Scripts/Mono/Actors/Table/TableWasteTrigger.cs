using Engineering.Scripts.Mono.Player;
using UnityEngine;

namespace Engineering.Scripts.Mono.Actors.Table
{
    [RequireComponent(typeof(Collider))]
    public class TableWasteTrigger : MonoBehaviour
    {
        [SerializeField] private Table table;

        private bool _hasPlayerInside;
        private bool _isProcessing;

        private void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag("Player") || table == null || _isProcessing)
                return;

            _hasPlayerInside = true;
            TryCollectWaste(other);
        }

        private void OnTriggerStay(Collider other)
        {
            if (!other.CompareTag("Player") || table == null || _isProcessing || !_hasPlayerInside)
                return;

            TryCollectWaste(other);
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.CompareTag("Player"))
                _hasPlayerInside = false;
        }

        private void TryCollectWaste(Collider playerCollider)
        {
            var wasteInventory = playerCollider.GetComponentInParent<PlayerWasteInventory>();
            if (wasteInventory == null)
                return;

            var currentLeftovers = table.LeftoverCount;
            if (currentLeftovers <= 0)
                return;

            var playerCapacity = wasteInventory.RemainingCapacity;
            if (playerCapacity <= 0)
                return;

            var transferAmount = currentLeftovers < playerCapacity ? currentLeftovers : playerCapacity;

            _isProcessing = true;

            var removed = table.TryRemoveLeftovers(transferAmount);
            if (removed > 0)
                wasteInventory.TryAdd(removed);

            _isProcessing = false;
        }
    }
}
