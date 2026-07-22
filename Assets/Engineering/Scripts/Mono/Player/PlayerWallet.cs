using UnityEngine;

namespace Engineering.Scripts.Mono.Player
{
    public class PlayerWallet : MonoBehaviour
    {
        [SerializeField] private int money = 100;
        [SerializeField] private Transform moneyAnimationOrigin;

        public int Money
        {
            get => money;
            set => money = value;
        }

        public Vector3 MoneyAnimationOriginPosition => moneyAnimationOrigin != null
            ? moneyAnimationOrigin.position
            : transform.position;
    }
}