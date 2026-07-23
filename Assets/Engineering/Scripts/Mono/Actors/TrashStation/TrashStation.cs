using System.Collections.Generic;
using DG.Tweening;
using Engineering.ScriptableObjects;
using Engineering.Scripts.Mono.Player;
using UnityEngine;

namespace Engineering.Engineering.Scripts.Mono.Actors.TrashStation
{
    public class TrashStation : MonoBehaviour
    {
        [SerializeField] private STrashStation sTrashStation;
        [SerializeField] private GameObject pizzaVisualPrefab;
        [SerializeField] private Transform trashTarget;
        [SerializeField] private SVoidEventChannel pizzaTrashedEvent;

        private readonly List<GameObject> _tempVisuals = new List<GameObject>();
        private readonly List<Tween> _activeTweens = new List<Tween>();
        private bool _isAnimating;

        public void TrashAllPizzas(PlayerPizzaInventory playerPizzaInventory)
        {
            if (playerPizzaInventory == null || _isAnimating)
                return;

            var count = playerPizzaInventory.Count;
            if (count <= 0)
                return;

            if (sTrashStation == null || pizzaVisualPrefab == null)
            {
                playerPizzaInventory.TryRemove(count);
                pizzaTrashedEvent?.Raise();
                return;
            }

            var pizzaStackAnchor = playerPizzaInventory.PizzaStackAnchor;
            var spacing = playerPizzaInventory.PizzaStackSpacing;
            var target = trashTarget != null ? trashTarget.position : transform.position;

            for (var i = 0; i < count; i++)
            {
                var worldPos = pizzaStackAnchor != null
                    ? pizzaStackAnchor.TransformPoint(Vector3.up * (i * spacing))
                    : playerPizzaInventory.transform.position + Vector3.up * (i * spacing);

                var visual = Instantiate(pizzaVisualPrefab, worldPos, Quaternion.identity);
                _tempVisuals.Add(visual);
            }

            playerPizzaInventory.TryRemove(count);
            _isAnimating = true;

            var completedCount = 0;
            var totalVisuals = _tempVisuals.Count;

            for (var i = 0; i < _tempVisuals.Count; i++)
            {
                var visual = _tempVisuals[i];
                var delay = i * sTrashStation.staggerDelay;

                var moveTween = visual.transform
                    .DOMove(target, sTrashStation.animationDuration)
                    .SetDelay(delay)
                    .SetEase(sTrashStation.moveEase);

                visual.transform
                    .DOScale(Vector3.zero, sTrashStation.animationDuration)
                    .SetDelay(delay)
                    .SetEase(sTrashStation.moveEase);

                moveTween.OnComplete(() =>
                {
                    completedCount++;
                    if (completedCount >= totalVisuals)
                    {
                        ClearTempVisuals();
                        _isAnimating = false;
                        _activeTweens.Clear();
                        pizzaTrashedEvent?.Raise();
                    }
                });

                _activeTweens.Add(moveTween);
            }
        }

        private void ClearTempVisuals()
        {
            foreach (var visual in _tempVisuals)
            {
                if (visual != null)
                    Destroy(visual);
            }

            _tempVisuals.Clear();
        }

        private void OnDestroy()
        {
            foreach (var tween in _activeTweens)
                tween?.Kill();

            foreach (var visual in _tempVisuals)
            {
                if (visual != null)
                    visual.transform.DOKill();
            }

            _activeTweens.Clear();
            ClearTempVisuals();
        }
    }
}
