using System.Collections.Generic;
using Engineering.ScriptableObjects;
using Engineering.Scripts.Mono.Managers;
using Engineering.Scripts.Mono.Player;
using UnityEngine;

namespace Engineering.Engineering.Scripts.Mono.Actors.ServeStation
{
    public class ServeStation : MonoBehaviour
    {
        [SerializeField] private SServeStation sServeStation;
        [SerializeField] private ServePlate servePlate;
        [SerializeField] private Transform pizzaStackAnchor;
        [SerializeField] private GameObject pizzaVisualPrefab;
        [SerializeField, Min(0.01f)] private float pizzaStackSpacing = 0.14f;
        [SerializeField] private SVoidEventChannel pizzaServedEvent;

        private readonly List<GameObject> _pizzaVisuals = new List<GameObject>();
        private float _pizzaHalfHeight;
        private int _servedPizzaCount;

        public int ServedPizzaCount => _servedPizzaCount;

        private void OnEnable()
        {
            PositionStackAnchor();
            CreateVisualPool();
        }

        public int TryServeAll(PlayerPizzaInventory playerPizzaInventory)
        {
            if (playerPizzaInventory == null || sServeStation == null)
                return 0;

            var remainingStationCapacity = sServeStation.maxPizzas - _servedPizzaCount;
            if (remainingStationCapacity <= 0)
                return 0;

            var servedAmount = playerPizzaInventory.TryRemove(remainingStationCapacity);
            if (servedAmount <= 0)
                return 0;

            _servedPizzaCount += servedAmount;
            PositionStackAnchor();
            RefreshVisuals();

            var moneyEarned = servedAmount * sServeStation.pricePerPizza;
            if (EconomyManager.Instance != null)
                EconomyManager.Instance.AwardMoney(moneyEarned);

            pizzaServedEvent?.Raise();
            return servedAmount;
        }

        private void CreateVisualPool()
        {
            if (_pizzaVisuals.Count > 0 || sServeStation == null || pizzaVisualPrefab == null || pizzaStackAnchor == null)
                return;

            for (var index = 0; index < sServeStation.maxPizzas; index++)
            {
                var pizzaVisual = Instantiate(pizzaVisualPrefab, pizzaStackAnchor);
                if (index == 0)
                    _pizzaHalfHeight = GetPizzaHalfHeight(pizzaVisual);

                pizzaVisual.transform.localPosition = Vector3.up * (_pizzaHalfHeight + index * pizzaStackSpacing);
                pizzaVisual.transform.localRotation = Quaternion.identity;
                pizzaVisual.SetActive(false);
                _pizzaVisuals.Add(pizzaVisual);
            }
        }

        private void RefreshVisuals()
        {
            for (var index = 0; index < _pizzaVisuals.Count; index++)
                _pizzaVisuals[index].SetActive(index < _servedPizzaCount);
        }

        private void PositionStackAnchor()
        {
            if (pizzaStackAnchor == null || servePlate == null)
                return;

            pizzaStackAnchor.position = servePlate.PizzaStackBasePosition;
            pizzaStackAnchor.rotation = Quaternion.identity;
        }

        private float GetPizzaHalfHeight(GameObject pizzaVisual)
        {
            var pizzaRenderer = pizzaVisual.GetComponentInChildren<Renderer>();
            return pizzaRenderer != null ? pizzaRenderer.bounds.extents.y : pizzaStackSpacing * 0.5f;
        }
    }
}
