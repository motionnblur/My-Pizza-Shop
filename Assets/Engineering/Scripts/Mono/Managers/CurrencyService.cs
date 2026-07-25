using Engineering.Scripts.Mono.Player;
using UnityEngine;

namespace Engineering.Scripts.Mono.Managers
{
    public class CurrencyService : MonoBehaviour
    {
        public static CurrencyService Instance { get; private set; }

        [SerializeField] private PlayerWallet _playerWallet;

        public PlayerWallet Wallet => _playerWallet;

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
            if (_playerWallet == null)
                _playerWallet = FindFirstObjectByType<PlayerWallet>();
        }

        private void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }

        public bool TrySpend(int amount)
        {
            return _playerWallet != null && _playerWallet.TrySpend(amount);
        }

        public bool Credit(int amount)
        {
            return _playerWallet != null && _playerWallet.Credit(amount);
        }
    }
}
