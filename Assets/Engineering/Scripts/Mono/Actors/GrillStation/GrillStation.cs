using System.Collections;
using System.Collections.Generic;
using Engineering.ScriptableObjects;
using Engineering.Scripts.Domain.GrillStation;
using Engineering.Scripts.Mono.Player;
using UnityEngine;

namespace Engineering.Scripts.Mono.Actors.GrillStation
{
    public class GrillStation : MonoBehaviour
    {
        [SerializeField] private SGrillStation sGrillStation;
        [SerializeField] private GrillPlate grillPlate;
        [SerializeField] private Transform pizzaStackAnchor;
        [SerializeField] private GameObject pizzaVisualPrefab;
        [SerializeField, Min(0.01f)] private float pizzaStackSpacing = 0.14f;

        private readonly List<GameObject> _pizzaVisuals = new List<GameObject>();
        private Coroutine _productionCoroutine;
        private float _pizzaHalfHeight;
        private GrillStationModel _model;

        public int ReadyPizzaCount => _model?.ReadyPizzaCount ?? 0;

        private void OnEnable()
        {
            TryPrepareModel();
            PositionStackAnchor();
            CreateVisualPool();
            _productionCoroutine = StartCoroutine(ProducePizzas());
        }

        private void OnDisable()
        {
            if (_productionCoroutine == null)
                return;

            StopCoroutine(_productionCoroutine);
            _productionCoroutine = null;
        }

        public int TryCollectAll(PlayerPizzaInventory playerPizzaInventory)
        {
            if (playerPizzaInventory == null || !TryPrepareModel())
                return 0;

            var collectedAmount = playerPizzaInventory.TryAdd(_model.ReadyPizzaCount);
            if (collectedAmount <= 0)
                return 0;

            _model.RemoveReady(collectedAmount);
            RefreshVisuals();
            return collectedAmount;
        }

        private bool TryPrepareModel()
        {
            if (sGrillStation == null)
                return false;

            if (_model == null)
            {
                _model = new GrillStationModel(sGrillStation.maxReadyPizzas);
            }
            else
            {
                _model.UpdateConfiguration(sGrillStation.maxReadyPizzas);
            }

            return true;
        }

        private IEnumerator ProducePizzas()
        {
            while (true)
            {
                yield return new WaitUntil(CanProduce);
                yield return new WaitForSeconds(sGrillStation.productionInterval);

                if (CanProduce() && _model.TryProduceOne())
                    RefreshVisuals();
            }
        }

        private bool CanProduce()
        {
            return TryPrepareModel() && _model.CanProduce;
        }

        private void CreateVisualPool()
        {
            if (_pizzaVisuals.Count > 0 || sGrillStation == null || pizzaVisualPrefab == null || pizzaStackAnchor == null)
                return;

            for (var index = 0; index < sGrillStation.maxReadyPizzas; index++)
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
            PositionStackAnchor();

            for (var index = 0; index < _pizzaVisuals.Count; index++)
                _pizzaVisuals[index].SetActive(index < ReadyPizzaCount);
        }

        private void PositionStackAnchor()
        {
            if (pizzaStackAnchor == null || grillPlate == null)
                return;

            pizzaStackAnchor.position = grillPlate.PizzaStackBasePosition;
            pizzaStackAnchor.rotation = Quaternion.identity;
        }

        private float GetPizzaHalfHeight(GameObject pizzaVisual)
        {
            var pizzaRenderer = pizzaVisual.GetComponentInChildren<Renderer>();
            return pizzaRenderer != null ? pizzaRenderer.bounds.extents.y : pizzaStackSpacing * 0.5f;
        }
    }
}
