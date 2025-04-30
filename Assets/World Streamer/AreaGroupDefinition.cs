using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace RaxterBaxter.WorldStreamer
{
    [CreateAssetMenu(fileName = "NewAreaGroupDefinition", menuName = "World Streamer/Area Group Definition")]
    public class AreaGroupDefinition : ScriptableObject
    {
        [Tooltip("List of area definitions in this group.")] [SerializeField]
        private List<AreaDefinition> areaDefinitions = new List<AreaDefinition>();

        [Tooltip("List of global scenes for this group.")] [SerializeField]
        private List<AssetReference> globalScenes = new List<AssetReference>();

        public List<AreaDefinition> AreaDefinitions => areaDefinitions;

        public List<AssetReference> GlobalScenes => globalScenes;

        private HashSet<AssetReference> _alwaysLoadedScenes = null;

        public IEnumerable<AssetReference> AlwaysLoadedScenes
        {
            get
            {
                if (Application.isPlaying)
                {
                    _alwaysLoadedScenes ??= new HashSet<AssetReference>(
                        areaDefinitions.ConvertAll(a => a.AlwaysLoadedScene),
                        _comparer);
                    return _alwaysLoadedScenes;
                }
                else
                {
                    return new HashSet<AssetReference>(
                        areaDefinitions.ConvertAll(a => a.AlwaysLoadedScene),
                        _comparer);
                }
            }
        }

        #region Custom comparer for HashSet

        private AssetReferenceEqualityComparer _comparer { get; set; } = new AssetReferenceEqualityComparer();


        #endregion

        private HashSet<AreaDefinition> _areaGroups = null;

        public bool Contains(AreaDefinition area)
        {
#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                return areaDefinitions.Contains(area);
            }
            else
#endif
            {
                _areaGroups ??= new HashSet<AreaDefinition>(areaDefinitions);

                return _areaGroups.Contains(area);
            }
        }

        public bool ContainsScene(AssetReference scene)
        {
            foreach (var area in areaDefinitions)
            {
                if (area.Contains(scene))
                    return true;
            }

            return false;
        }
    }
}