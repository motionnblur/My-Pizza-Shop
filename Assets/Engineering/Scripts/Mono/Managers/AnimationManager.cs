using System.Collections.Generic;
using DG.Tweening;
using Engineering.ScriptableObjects;
using UnityEngine;
using UnityEngine.Pool;

namespace Engineering.Scripts.Mono.Managers
{
    public class AnimationManager : MonoBehaviour
    {
        [SerializeField] private SEconomy sEconomy;

        [Header("Money Animation")]
        [SerializeField] private SAnimation _sAnimation;
        [SerializeField] private SMoneyAnimationEventChannel moneyAnimationRequested;

        [Header("Pooling")]
        [SerializeField] private int _initialPoolSize = 8;
        [SerializeField] private int _maxPoolSize = 16;

        private ObjectPool<GameObject> _moneyPool;
        private HashSet<GameObject> _activeMoneyObjects = new();
        private Vector3 _moneyPrefabScale;

        private void OnEnable()
        {
            if (moneyAnimationRequested != null)
                moneyAnimationRequested.RegisterListener(OnMoneyAnimationRequested);
        }

        private void OnDisable()
        {
            if (moneyAnimationRequested != null)
                moneyAnimationRequested.UnregisterListener(OnMoneyAnimationRequested);
        }

        private void OnDestroy()
        {
            foreach (var go in new List<GameObject>(_activeMoneyObjects))
            {
                if (go != null)
                    go.transform.DOKill();
            }

            _activeMoneyObjects.Clear();
            _moneyPool?.Dispose();
            _moneyPool = null;
        }

        private void OnMoneyAnimationRequested(MoneyAnimationRequest request)
        {
            if (request == null) return;

            if (request.HasTransformDestination)
            {
                DoMoneyAnimation(request.SourcePosition, request.DestinationTransform);
            }
            else
            {
                DoMoneyAnimation(request.SourcePosition, request.DestinationPosition);
            }
        }

        private ObjectPool<GameObject> MoneyPool
        {
            get
            {
                if (_moneyPool == null)
                    InitializeMoneyPool();
                return _moneyPool;
            }
        }

        private void InitializeMoneyPool()
        {
            if (sEconomy == null || sEconomy.moneyPrefab == null) return;

            _moneyPrefabScale = sEconomy.moneyPrefab.transform.localScale;

            _moneyPool = new ObjectPool<GameObject>(
                createFunc: () =>
                {
                    var go = Instantiate(sEconomy.moneyPrefab, transform);
                    go.transform.localScale = _moneyPrefabScale;
                    go.SetActive(false);
                    return go;
                },
                actionOnGet: go =>
                {
                    go.transform.localScale = _moneyPrefabScale;
                    go.SetActive(true);
                    _activeMoneyObjects.Add(go);
                },
                actionOnRelease: go =>
                {
                    if (go == null) return;
                    go.transform.DOKill();
                    go.transform.localScale = _moneyPrefabScale;
                    go.transform.rotation = Quaternion.identity;
                    go.SetActive(false);
                    _activeMoneyObjects.Remove(go);
                },
                actionOnDestroy: Destroy,
                collectionCheck: true,
                defaultCapacity: _initialPoolSize,
                maxSize: _maxPoolSize);

            var buffer = new GameObject[_initialPoolSize];
            for (var i = 0; i < _initialPoolSize; i++)
                buffer[i] = _moneyPool.Get();
            for (var i = 0; i < _initialPoolSize; i++)
                _moneyPool.Release(buffer[i]);
        }

        public void DoMoneyAnimation(Vector3 positionFrom, Vector3 positionTo)
        {
            PlayMoneyAnimation(
                positionFrom,
                go => go.transform.DOJump(positionTo, _sAnimation.jumpPower, 1, _sAnimation.duration));
        }

        public void DoMoneyAnimation(Vector3 positionFrom, Transform targetTransform)
        {
            if (targetTransform == null) return;

            PlayMoneyAnimation(positionFrom, go =>
            {
                var animationStartPosition = go.transform.position;
                var lastTargetPosition = targetTransform.position;

                return DOVirtual.Float(0f, 1f, _sAnimation.duration, progress =>
                {
                    if (targetTransform != null)
                        lastTargetPosition = targetTransform.position;

                    var position = Vector3.Lerp(animationStartPosition, lastTargetPosition, progress);
                    position.y += _sAnimation.jumpPower * 4f * progress * (1f - progress);
                    go.transform.position = position;
                });
            });
        }

        private void PlayMoneyAnimation(Vector3 positionFrom, System.Func<GameObject, Tween> createMovementTween)
        {
            if (sEconomy == null || sEconomy.moneyPrefab == null || _sAnimation == null) return;

            var pool = MoneyPool;
            if (pool == null) return;

            Vector3 randomStartOffset = new Vector3(
                Random.Range(-_sAnimation.randomOffsetRadius, _sAnimation.randomOffsetRadius),
                0f,
                Random.Range(-_sAnimation.randomOffsetRadius, _sAnimation.randomOffsetRadius));
            positionFrom += randomStartOffset;

            GameObject go = pool.Get();
            go.transform.position = positionFrom;

            go.transform.localRotation = Quaternion.Euler(
                Random.Range(0f, 360f),
                Random.Range(0f, 360f),
                Random.Range(0f, 360f));

            Sequence sequence = null;
            var returnedToPool = false;
            void ReturnToPool()
            {
                if (returnedToPool)
                    return;

                returnedToPool = true;
                if (sequence != null && sequence.IsActive())
                    sequence.Kill(false);

                pool.Release(go);
            }

            sequence = DOTween.Sequence();
            sequence.SetRecyclable(true).SetTarget(go.transform);

            sequence.Append(createMovementTween(go)
                .SetEase(_sAnimation.moveEase)
                .OnKill(ReturnToPool));

            go.transform.localScale = Vector3.zero;
            sequence.Join(go.transform.DOScale(_moneyPrefabScale, _sAnimation.duration * 0.3f)
                .OnKill(ReturnToPool));

            sequence.Join(go.transform.DORotate(
                new Vector3(0f, _sAnimation.rotationAmount, 0f),
                _sAnimation.duration,
                RotateMode.LocalAxisAdd)
                .SetEase(Ease.Linear)
                .OnKill(ReturnToPool));

            sequence.OnComplete(ReturnToPool);
            sequence.OnKill(ReturnToPool);
        }
    }
}
