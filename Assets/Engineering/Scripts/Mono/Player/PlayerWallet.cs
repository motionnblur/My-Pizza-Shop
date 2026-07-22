using UnityEngine;

namespace Engineering.Scripts.Mono.Player
{
    public class PlayerWallet : MonoBehaviour
    {
        [SerializeField] private int money = 100;

        public int Money
        {
            get => money;
            set => money = value;
        }
    }
}