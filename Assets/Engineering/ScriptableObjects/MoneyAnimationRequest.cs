using UnityEngine;

namespace Engineering.ScriptableObjects
{
    public class MoneyAnimationRequest
    {
        public Vector3 SourcePosition { get; set; }
        public Vector3 DestinationPosition { get; set; }
        public Transform DestinationTransform { get; set; }

        public bool HasTransformDestination => DestinationTransform != null;
    }
}
