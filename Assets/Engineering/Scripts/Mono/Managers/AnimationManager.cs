using DG.Tweening;
using Engineering.ScriptableObjects;
using Engineering.Scripts.Class;
using Engineering.Scripts.Mono.Areas;
using UnityEngine;

namespace Engineering.Scripts.Mono.Managers
{
    public class AnimationManager : MonoBehaviour
    {
        public static AnimationManager Instance { get; private set; }
        [SerializeField] private SEconomy sEconomy;

        [Header("Money Animation")]
        [SerializeField] private float _jumpPower = 3f;
        [SerializeField] private float _duration = 0.5f;
        [SerializeField] private float _randomOffsetRadius = 0.5f;
        [SerializeField] private Ease _moveEase = Ease.OutQuad;
        [SerializeField] private float _scalePunch = 0.3f;
        [SerializeField] private float _rotationAmount = 360f;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        public void DoMoneyAnimation(Vector3 positionFrom, Vector3 positionTo)
        {
            if (sEconomy == null || sEconomy.moneyPrefab == null) return;

            Vector3 randomStartOffset = new Vector3(
                Random.Range(-_randomOffsetRadius, _randomOffsetRadius),
                0f,
                Random.Range(-_randomOffsetRadius, _randomOffsetRadius));
            positionFrom += randomStartOffset;

            GameObject go = Instantiate(sEconomy.moneyPrefab, positionFrom, Quaternion.identity);

            Sequence sequence = DOTween.Sequence();

            sequence.Append(go.transform.DOJump(positionTo, _jumpPower, 1, _duration)
                .SetEase(_moveEase));

            Vector3 targetScale = go.transform.localScale;
            go.transform.localScale = Vector3.zero;
            sequence.Join(go.transform.DOScale(targetScale, _duration * 0.3f));

            go.transform.localRotation = Quaternion.Euler(
                Random.Range(0f, 360f),
                Random.Range(0f, 360f),
                Random.Range(0f, 360f));
            sequence.Join(go.transform.DORotate(
                new Vector3(0f, _rotationAmount, 0f),
                _duration,
                RotateMode.LocalAxisAdd)
                .SetEase(Ease.Linear));

            sequence.OnComplete(() => Destroy(go));
        }
    }
}