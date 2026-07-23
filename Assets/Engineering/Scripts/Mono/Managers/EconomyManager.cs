using System.Collections;
using Engineering.Engineering.Scripts.Mono.Items;
using Engineering.ScriptableObjects;
using Engineering.Scripts.Mono.Areas;
using Engineering.Scripts.Mono.Player;
using UnityEngine;

namespace Engineering.Scripts.Mono.Managers
{
    public class EconomyManager : MonoBehaviour
    {
        public static EconomyManager Instance { get; private set; }
        [SerializeField] private SEconomy sEconomy;
        [SerializeField] private SAnimation _sAnimation;
        private PlayerWallet _pWallet;
        private Coroutine _activePaymentCoroutine;

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

        private void Start()
        {
            _pWallet = FindFirstObjectByType<PlayerWallet>();
            UpdateMoneyText();
        }

        private void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }

        public void ProcessPayment(BuyingArea ba)
        {
            if (_sAnimation == null || _sAnimation.moneySpendSpeed <= 0) return;
            if (_activePaymentCoroutine != null) return;
            _activePaymentCoroutine = StartCoroutine(DelayedPayment(ba));
        }

        public void CancelPayment()
        {
            if (_activePaymentCoroutine != null)
            {
                StopCoroutine(_activePaymentCoroutine);
                _activePaymentCoroutine = null;
            }
        }

        private IEnumerator DelayedPayment(BuyingArea ba)
        {
            if (ba == null || _pWallet == null) yield break;

            var pay = sEconomy.playerMoneySpendRate;
            var delay = 1f / _sAnimation.moneySpendSpeed;

            yield return new WaitForSeconds(_sAnimation.moneySpendDelay);

            while (ba != null && _pWallet != null)
            {
                var afterMoneyInPlayerPocket = _pWallet.Money - pay;

                if (afterMoneyInPlayerPocket >= 0)
                {
                    var animationTargetPosition = ba.transform.position;
                    _pWallet.Money = afterMoneyInPlayerPocket;
                    ba.AddPayment(pay);

                    yield return new WaitForSeconds(delay);

                    if (_pWallet == null)
                        break;

                    if (AnimationManager.Instance != null)
                    {
                        AnimationManager.Instance.DoMoneyAnimation(
                            _pWallet.MoneyAnimationOriginPosition,
                            animationTargetPosition);
                    }
                    
                    UpdateMoneyText();
                }
                else
                {
                    break;
                }
            }

            _activePaymentCoroutine = null;
        }

        private void UpdateMoneyText()
        {
            if (UIManager.Instance != null && _pWallet != null)
                UIManager.Instance.UpdateMoneyText(_pWallet.Money);
        }

        public void PlayerBuyBuyingArea(BuyingArea ba)
        {
            if (ba == null || ba.gameObject == null) return;
            Destroy(ba.gameObject);
        }

        public void CollectMoneyFromGround(MoneyToCollect mc, int money)
        {
            if (mc == null || _pWallet == null) return;

            var animationOriginPosition = mc.transform.position;
            _pWallet.Money += money;
            UpdateMoneyText();

            if (AnimationManager.Instance != null)
            {
                AnimationManager.Instance.DoMoneyAnimation(
                    animationOriginPosition,
                    _pWallet.MoneyAnimationOrigin);
            }

            mc.Destroy();
        }
    }
}
