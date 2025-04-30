using UnityEngine;

namespace RaxterBaxter.WorldStreamer
{
    public class AreaTag : MonoBehaviour
    {
        [SerializeField] private AreaDefinition area;

        [SerializeField] private bool isNegateTag = false;

        public AreaDefinition Area => area;
        public bool IsNegateTag => isNegateTag;
    }
}