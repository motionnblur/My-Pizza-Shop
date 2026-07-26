using System.Collections.Generic;
using UnityEngine;

namespace Engineering.Scripts.Mono.Actors.ServeStation
{
    public class ServeStationVisuals : MonoBehaviour
    {
        [SerializeField] private GameObject pizzaVisualPrefab;
        [SerializeField] private BoxCollider plateCollider;
        [SerializeField, Min(0.01f)] private float pizzaStackSpacing = 0.14f;

        private readonly List<GameObject> _pizzaVisuals = new List<GameObject>();
        private Vector3 _cachedStackBasePosition;

        public void Initialize(int capacity)
        {
            UpdateStackBasePosition();
            if (_pizzaVisuals.Count == 0)
                CreateVisualPool(capacity);
        }

        public void Refresh(int activePizzaCount)
        {
            var basePos = _cachedStackBasePosition;

            for (var index = 0; index < _pizzaVisuals.Count; index++)
            {
                if (_pizzaVisuals[index] == null)
                    continue;

                _pizzaVisuals[index].transform.position = basePos + Vector3.up * (index * pizzaStackSpacing);
                _pizzaVisuals[index].transform.rotation = Quaternion.identity;
                _pizzaVisuals[index].SetActive(index < activePizzaCount);
            }
        }

        private void CreateVisualPool(int capacity)
        {
            if (capacity <= 0 || pizzaVisualPrefab == null)
                return;

            for (var index = 0; index < capacity; index++)
            {
                var pizzaVisual = Instantiate(pizzaVisualPrefab, transform);
                pizzaVisual.SetActive(false);
                _pizzaVisuals.Add(pizzaVisual);
            }
        }

        private void UpdateStackBasePosition()
        {
            if (plateCollider == null)
                _cachedStackBasePosition = transform.position;
            else
            {
                var bounds = plateCollider.bounds;
                _cachedStackBasePosition = new Vector3(bounds.center.x, bounds.max.y, bounds.center.z);
            }
        }
    }
}
