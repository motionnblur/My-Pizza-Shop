using UnityEngine;

namespace Engineering.ScriptableObjects
{
    [CreateAssetMenu(fileName = "SAnimation", menuName = "Scriptables/SAnimation", order = 0)]
    public class SAnimation : ScriptableObject
    {
        public float moneySpendDelay = 1f;
        public float moneySpendSpeed = 1f;
        public float jumpPower = 3f;
        public float duration = 0.5f;
        public float rotationAmount = 360f;

        public float randomOffsetRadius = 0.5f;
        
        public DG.Tweening.Ease moveEase = DG.Tweening.Ease.OutQuad;
    }
}