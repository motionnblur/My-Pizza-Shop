using DG.Tweening;
using UnityEngine;

namespace Engineering.ScriptableObjects
{
    [CreateAssetMenu(fileName = "STrashStation", menuName = "Scriptables/STrashStation", order = 0)]
    public class STrashStation : ScriptableObject
    {
        [Min(0.01f)] public float animationDuration = 0.4f;
        [Min(0f)] public float staggerDelay = 0.05f;
        public Ease moveEase = Ease.InBack;
    }
}
