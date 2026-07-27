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
        [Header("Pizza Trash")]
        [SerializeField] private STrashStation sTrashStation;
        [SerializeField] private GameObject pizzaVisualPrefab;
        [SerializeField] private Transform trashTarget;
        [SerializeField] private SVoidEventChannel pizzaTrashedEvent;

        [Header("Waste Trash")]
        [SerializeField] private GameObject wasteVisualPrefab;
        [SerializeField] private SVoidEventChannel wasteDisposedEvent;

        private ObjectPool<GameObject> _pizzaPool;
        private Vector3 _originalScale = Vector3.one;
        private readonly List<GameObject> _activePizzaVisuals = new();
        private readonly List<Tween> _activePizzaTweens = new();
        private bool _isPizzaAnimating;

        private ObjectPool<GameObject> _wastePool;
        private Vector3 _wasteOriginalScale = Vector3.one;
        private readonly List<GameObject> _activeWasteVisuals = new();
        private readonly List<Tween> _activeWasteTweens = new();
        private bool _isWasteAnimating;

        private const int PoolDefaultCapacity = 10;
        private const int PoolMaxSize = 20;

        private void EnsurePizzaPoolInitialized()
        {
            if (_pizzaPool != null)
                return;

            if (pizzaVisualPrefab == null)
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

        private void EnsureWastePoolInitialized()
        {
            if (_wastePool != null)
                return;

            if (wasteVisualPrefab == null)
                return;

            _wasteOriginalScale = wasteVisualPrefab.transform.localScale;

            _wastePool = new ObjectPool<GameObject>(
                createFunc: () =>
                {
                    var obj = Instantiate(wasteVisualPrefab);
                    obj.SetActive(false);
                    return obj;
                },
                actionOnGet: null,
                actionOnRelease: OnReleaseWasteVisual,
                actionOnDestroy: obj => Destroy(obj),
                defaultCapacity: PoolDefaultCapacity,
                maxSize: PoolMaxSize);
        }

        private static void OnReleasePizzaVisual(GameObject visual)
        {
            visual.transform.DOKill(false);
            visual.SetActive(false);
        }

        private static void OnReleaseWasteVisual(GameObject visual)
        {
            visual.transform.DOKill(false);
            visual.SetActive(false);
        }

        public void TrashAllPizzas(PlayerPizzaInventory playerPizzaInventory)
        {
            if (playerPizzaInventory == null || _isPizzaAnimating)
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

            EnsurePizzaPoolInitialized();

            var pizzaStackAnchor = playerPizzaInventory.PizzaStackAnchor;
            var spacing = playerPizzaInventory.PizzaStackSpacing;
            var target = trashTarget != null ? trashTarget.position : transform.position;

            for (var i = 0; i < count; i++)
            {
                var worldPos = pizzaStackAnchor != null
                    ? pizzaStackAnchor.TransformPoint(Vector3.up * (i * spacing))
                    : playerPizzaInventory.transform.position + Vector3.up * (i * spacing);

                var visual = GetPizzaVisual(worldPos);
                _activePizzaVisuals.Add(visual);
            }

            playerPizzaInventory.TryRemove(count);
            _isPizzaAnimating = true;

            for (var i = 0; i < _activePizzaVisuals.Count; i++)
            {
                var visual = _activePizzaVisuals[i];
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

                _activePizzaTweens.Add(moveTween);
            }
        }

        public void TryDisposeWaste(PlayerWasteInventory wasteInventory)
        {
            if (wasteInventory == null || _isWasteAnimating)
                return;

            var count = wasteInventory.Count;
            if (count <= 0)
                return;

            if (wasteVisualPrefab == null)
            {
                wasteInventory.TryRemove(count);
                wasteDisposedEvent?.Raise();
                return;
            }

            EnsureWastePoolInitialized();

            var wasteStackAnchor = wasteInventory.WasteStackAnchor;
            var spacing = wasteInventory.WasteStackSpacing;
            var target = trashTarget != null ? trashTarget.position : transform.position;

            for (var i = 0; i < count; i++)
            {
                var worldPos = wasteStackAnchor != null
                    ? wasteStackAnchor.TransformPoint(Vector3.up * (i * spacing))
                    : wasteInventory.transform.position + Vector3.up * (i * spacing);

                var visual = GetWasteVisual(worldPos);
                _activeWasteVisuals.Add(visual);
            }

            wasteInventory.TryRemove(count);
            _isWasteAnimating = true;

            var duration = sTrashStation != null ? sTrashStation.animationDuration : 0.4f;
            var stagger = sTrashStation != null ? sTrashStation.staggerDelay : 0.05f;
            var ease = sTrashStation != null ? sTrashStation.moveEase : Ease.InBack;

            for (var i = 0; i < _activeWasteVisuals.Count; i++)
            {
                var visual = _activeWasteVisuals[i];
                var delay = i * stagger;

                var moveTween = visual.transform
                    .DOMove(target, duration)
                    .SetDelay(delay)
                    .SetEase(ease);

                visual.transform
                    .DOScale(Vector3.zero, duration)
                    .SetDelay(delay)
                    .SetEase(ease);

                moveTween.OnComplete(() => ReleaseWasteVisual(visual));

                _activeWasteTweens.Add(moveTween);
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

        private GameObject GetWasteVisual(Vector3 position)
        {
            var visual = _wastePool.Get();
            visual.transform.SetPositionAndRotation(position, Quaternion.identity);
            visual.transform.localScale = _wasteOriginalScale;
            visual.SetActive(true);
            return visual;
        }

        private void ReleasePizzaVisual(GameObject visual)
        {
            if (visual == null)
                return;

            _pizzaPool.Release(visual);
            _activePizzaVisuals.Remove(visual);

            if (_activePizzaVisuals.Count == 0)
                CompletePizzaTrashAnimation();
        }

        private void ReleaseWasteVisual(GameObject visual)
        {
            if (visual == null)
                return;

            _wastePool.Release(visual);
            _activeWasteVisuals.Remove(visual);

            if (_activeWasteVisuals.Count == 0)
                CompleteWasteDisposeAnimation();
        }

        private void CompletePizzaTrashAnimation()
        {
            _isPizzaAnimating = false;
            _activePizzaTweens.Clear();
            pizzaTrashedEvent?.Raise();
        }

        private void CompleteWasteDisposeAnimation()
        {
            _isWasteAnimating = false;
            _activeWasteTweens.Clear();
            wasteDisposedEvent?.Raise();
        }

        private void OnDestroy()
        {
            foreach (var tween in _activePizzaTweens)
                tween?.Kill();
            _activePizzaTweens.Clear();

            foreach (var tween in _activeWasteTweens)
                tween?.Kill();
            _activeWasteTweens.Clear();

            if (_pizzaPool != null)
            {
                foreach (var visual in _activePizzaVisuals)
                {
                    if (visual != null)
                        _pizzaPool.Release(visual);
                }
            }
            _activePizzaVisuals.Clear();

            if (_wastePool != null)
            {
                foreach (var visual in _activeWasteVisuals)
                {
                    if (visual != null)
                        _wastePool.Release(visual);
                }
            }
            _activeWasteVisuals.Clear();

            if (_pizzaPool != null)
            {
                _pizzaPool.Clear();
                _pizzaPool.Dispose();
                _pizzaPool = null;
            }

            if (_wastePool != null)
            {
                _wastePool.Clear();
                _wastePool.Dispose();
                _wastePool = null;
            }
        }
    }
}

