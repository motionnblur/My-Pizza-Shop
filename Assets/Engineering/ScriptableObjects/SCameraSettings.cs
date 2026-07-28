using UnityEngine;

namespace Engineering.ScriptableObjects
{
    [CreateAssetMenu(fileName = "SCameraSettings", menuName = "Scriptables/SCameraSettings", order = 0)]
    public class SCameraSettings : ScriptableObject
    {
        [SerializeField] private float _xDamping = 0.18f;
        [SerializeField] private float _yDamping = 0.25f;
        [SerializeField] private float _zDamping = 0.18f;
        [SerializeField] private float _maxFollowSpeed;

        public float XDamping => _xDamping;
        public float YDamping => _yDamping;
        public float ZDamping => _zDamping;
        public float MaxFollowSpeed => _maxFollowSpeed;

        private void OnValidate()
        {
            _xDamping = Mathf.Max(_xDamping, 0.001f);
            _yDamping = Mathf.Max(_yDamping, 0.001f);
            _zDamping = Mathf.Max(_zDamping, 0.001f);
        }
    }
}
