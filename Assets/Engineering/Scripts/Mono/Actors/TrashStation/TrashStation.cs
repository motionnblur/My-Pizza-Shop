using System.Collections.Generic;
using DG.Tweening;
using Engineering.ScriptableObjects;
using Engineering.Scripts.Mono.Player;
using UnityEngine;
using UnityEngine.Pool;

namespace Engineering.Scripts.Mono.Actors.TrashStation
{
    public class TrashStation : MonoBehaviour
    {
        [SerializeField] private STrashStation sTrashStation;
        [SerializeField] private GameObject pizzaVisualPrefab;
        [SerializeField] private Transform trashTarget;
        [SerializeField] private SVoidEventChannel pizzaTrashedEvent;

        private ObjectPool<GameObject> _pizzaPool;
        private Vector3 _originalScale = Vector3.one;
        private readonly List<GameObject> _activeVisuals = new();
        private readonly List<Tween> _activeTweens = new();
        private bool _isAnimating;

        private const int PoolDefaultCapacity = 10;
        private const int PoolMaxSize = 20;

        private void EnsurePoolInitialized()
        {
            if (_pizzaPool != null)
                return;

            _originalScale = pizzaVisualPrefab.transform.localScale;

            _pizzaPool = new ObjectPool<GameObject>(
                createFunc: () =>
                {
                    var obj = Instantiate(pizzaVisualPrefab);
                    obj.SetActive(false);
                    return obj;
                },
                actionOnGet: null,
                actionOnRelease: OnReleasePizzaVisual,
                actionOnDestroy: obj => Destroy(obj),
                defaultCapacity: PoolDefaultCapacity,
                maxSize: PoolMaxSize);
        }

        private static void OnReleasePizzaVisual(GameObject visual)
        {
            visual.transform.DOKill(false);
            visual.SetActive(false);
        }

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

            EnsurePoolInitialized();

            var pizzaStackAnchor = playerPizzaInventory.PizzaStackAnchor;
            var spacing = playerPizzaInventory.PizzaStackSpacing;
            var target = trashTarget != null ? trashTarget.position : transform.position;

            for (var i = 0; i < count; i++)
            {
                var worldPos = pizzaStackAnchor != null
                    ? pizzaStackAnchor.TransformPoint(Vector3.up * (i * spacing))
                    : playerPizzaInventory.transform.position + Vector3.up * (i * spacing);

                var visual = GetPizzaVisual(worldPos);
                _activeVisuals.Add(visual);
            }

            playerPizzaInventory.TryRemove(count);
            _isAnimating = true;

            for (var i = 0; i < _activeVisuals.Count; i++)
            {
                var visual = _activeVisuals[i];
                var delay = i * sTrashStation.staggerDelay;

                var moveTween = visual.transform
                    .DOMove(target, sTrashStation.animationDuration)
                    .SetDelay(delay)
                    .SetEase(sTrashStation.moveEase);

                visual.transform
                    .DOScale(Vector3.zero, sTrashStation.animationDuration)
                    .SetDelay(delay)
                    .SetEase(sTrashStation.moveEase);

                moveTween.OnComplete(() => ReleasePizzaVisual(visual));

                _activeTweens.Add(moveTween);
            }
        }

        private GameObject GetPizzaVisual(Vector3 position)
        {
            var visual = _pizzaPool.Get();
            visual.transform.SetPositionAndRotation(position, Quaternion.identity);
            visual.transform.localScale = _originalScale;
            visual.SetActive(true);
            return visual;
        }

        private void ReleasePizzaVisual(GameObject visual)
        {
            if (visual == null)
                return;

            _pizzaPool.Release(visual);
            _activeVisuals.Remove(visual);

            if (_activeVisuals.Count == 0)
                CompleteTrashAnimation();
        }

        private void CompleteTrashAnimation()
        {
            _isAnimating = false;
            _activeTweens.Clear();
            pizzaTrashedEvent?.Raise();
        }

        private void OnDestroy()
        {
            foreach (var tween in _activeTweens)
                tween?.Kill();
            _activeTweens.Clear();

            if (_pizzaPool != null)
            {
                foreach (var visual in _activeVisuals)
                {
                    if (visual != null)
                        _pizzaPool.Release(visual);
                }
            }
            _activeVisuals.Clear();

            if (_pizzaPool != null)
            {
                _pizzaPool.Clear();
                _pizzaPool.Dispose();
                _pizzaPool = null;
            }
        }
    }
}
